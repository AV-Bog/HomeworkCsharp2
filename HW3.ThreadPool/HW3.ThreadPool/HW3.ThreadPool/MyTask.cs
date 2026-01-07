// <copyright file="MyTask.cs" author="bogdanovaarina">
// under MIT License
// </copyright>

namespace HW3.ThreadPool;

internal sealed class MyTask<TResult> : IMyTask<TResult>
{
    private readonly Func<TResult> _function;
    private readonly MyThreadPool _threadPool;

    private TResult? _result;
    private Exception? _exception;
    private readonly ManualResetEventSlim _completionEvent = new(false);
    private readonly object _continuationsLock = new();
    private readonly List<Action> _continuations = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="MyTask{TResult}"/> class.
    /// This constructor is intended for internal use by <see cref="MyThreadPool"/>.
    /// </summary>
    /// <param name="function">The function to execute.</param>
    /// <param name="threadPool">The thread pool that will execute this task.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="function"/> or <paramref name="threadPool"/> is null.</exception>
    internal MyTask(Func<TResult> function, MyThreadPool threadPool)
    {
        this._function = function ?? throw new ArgumentNullException(nameof(function));
        this._threadPool = threadPool ?? throw new ArgumentNullException(nameof(threadPool));
    }

    internal void Execute()
    {
        try
        {
            this._result = this._function();
        }
        catch (Exception ex)
        {
            this._exception = ex;
        }
        finally
        {
            List<Action>? toRun = null;
            lock (this._continuationsLock)
            {
                this._completionEvent.Set();
                if (this._continuations.Count > 0)
                {
                    toRun = new List<Action>(this._continuations);
                    this._continuations.Clear();
                }
            }

            if (toRun != null)
            {
                foreach (var action in toRun)
                {
                    this._threadPool.EnqueueContinuation(action);
                }
            }
        }
    }

    internal void CompleteWithResult(TResult result)
    {
        this._result = result;
        this._completionEvent.Set();
    }

    internal void CompleteWithException(Exception ex)
    {
        this._exception = ex;
        this._completionEvent.Set();
    }

    /// <inheritdoc />
    public bool IsCompleted => this._completionEvent.IsSet;

    /// <inheritdoc />
    public TResult Result
    {
        get
        {
            this._completionEvent.Wait();

            if (this._exception != null)
            {
                throw new AggregateException(this._exception);
            }

            return this._result!;
        }
    }

    /// <inheritdoc />
    public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        var next = new MyTask<TNewResult>(() => default!, this._threadPool);

        lock (this._continuationsLock)
        {
            if (!this._completionEvent.IsSet)
            {
                this._continuations.Add(() =>
                {
                    if (this._exception != null)
                    {
                        next.CompleteWithException(this._exception);
                    }
                    else
                    {
                        try
                        {
                            var newResult = func(this._result!);
                            next.CompleteWithResult(newResult);
                        }
                        catch (Exception ex)
                        {
                            next.CompleteWithException(ex);
                        }
                    }
                });
                return next;
            }

            if (this._exception != null)
            {
                next.CompleteWithException(this._exception);
            }
            else
            {
                try
                {
                    var newResult = func(this._result!);
                    next.CompleteWithResult(newResult);
                }
                catch (Exception ex)
                {
                    next.CompleteWithException(ex);
                }
            }
        }

        return next;
    }
}