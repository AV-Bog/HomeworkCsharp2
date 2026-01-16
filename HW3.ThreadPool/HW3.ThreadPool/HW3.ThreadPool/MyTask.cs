// <copyright file="MyTask.cs" author="bogdanovaarina">
// under MIT License
// </copyright>

namespace HW3.ThreadPool;

/// <summary>
/// Represents an asynchronous computation that produces a result of type <typeparamref name="TResult"/>.
/// This class is not intended for direct instantiation by user code; use <see cref="MyThreadPool.Submit{TResult}"/> instead.
/// </summary>
/// <typeparam name="TResult">The type of the result produced by this task.</typeparam>
internal sealed class MyTask<TResult> : IMyTask<TResult>
{
    private readonly Func<TResult> function;
    private readonly MyThreadPool threadPool;
    private readonly object continuationsLock = new();
    private readonly List<Action> continuations = [];
    private readonly ManualResetEventSlim completionEvent = new(initialState: false);

    private TResult? result;
    private Exception? exception;

    /// <summary>
    /// Initializes a new instance of the <see cref="MyTask{TResult}"/> class.
    /// This constructor is intended for internal use by <see cref="MyThreadPool"/>.
    /// </summary>
    /// <param name="function">The function to execute.</param>
    /// <param name="threadPool">The thread pool that will execute this task.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="function"/> or <paramref name="threadPool"/> is null.</exception>
    internal MyTask(Func<TResult> function, MyThreadPool threadPool)
    {
        this.function = function ?? throw new ArgumentNullException(nameof(function));
        this.threadPool = threadPool ?? throw new ArgumentNullException(nameof(threadPool));
    }

    /// <inheritdoc />
    public bool IsCompleted => this.completionEvent.IsSet;

    /// <inheritdoc />
    public TResult Result
    {
        get
        {
            this.completionEvent.Wait();

            if (this.exception != null)
            {
                throw new AggregateException(this.exception);
            }

            return this.result!;
        }
    }

    /// <inheritdoc />
    public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        var next = new MyTask<TNewResult>(() => throw new InvalidOperationException("Should not be executed directly"), this.threadPool);

        Action continuationAction = () =>
        {
            if (this.exception != null)
            {
                next.CompleteWithExceptionPrivate(this.exception);
            }
            else
            {
                try
                {
                    var newResult = func(this.result!);
                    next.CompleteWithResultPrivate(newResult);
                }
                catch (Exception ex)
                {
                    next.CompleteWithExceptionPrivate(ex);
                }
            }
        };

        lock (this.continuationsLock)
        {
            if (!this.completionEvent.IsSet)
            {
                this.continuations.Add(continuationAction);
                return next;
            }

            this.threadPool.EnqueueContinuationSafe(continuationAction);
            return next;
        }
    }

    internal void Execute()
    {
        try
        {
            this.result = this.function();
        }
        catch (Exception ex)
        {
            this.exception = ex;
        }
        finally
        {
            List<Action>? toRun = null;
            lock (this.continuationsLock)
            {
                this.completionEvent.Set();
                if (this.continuations.Count > 0)
                {
                    toRun = new List<Action>(this.continuations);
                    this.continuations.Clear();
                }
            }

            if (toRun != null)
            {
                foreach (var action in toRun)
                {
                    this.threadPool.EnqueueContinuationSafe(action);
                }
            }
        }
    }

    private void CompleteWithResultPrivate(TResult value)
    {
        this.result = value;
        this.completionEvent.Set();
    }

    private void CompleteWithExceptionPrivate(Exception ex)
    {
        this.exception = ex;
        this.completionEvent.Set();
    }
}