// <copyright file="MyTask.cs" author="bogdanovaarina">
// under MIT License
// </copyright>

namespace HW2.ThreadPool;

public class MyTask<TResult> : IMyTask<TResult>
{
    private Func<TResult> _function;
    private TResult _result;
    private Exception _exception;
    private volatile bool _isCompleted;
    private readonly object _lockObject = new object();
    private readonly MyThreadPool _threadPool;
    private ManualResetEvent _completionEvent;
    private readonly CancellationToken _cancellationToken;
    private Queue<Action> _continuations;

    public MyTask(
        Func<TResult> function,
        MyThreadPool threadPool,
        CancellationToken cancellationToken)
    {
        _function = function ?? throw new ArgumentNullException(nameof(function));
        _threadPool = threadPool ?? throw new ArgumentNullException(nameof(threadPool));
        _cancellationToken = cancellationToken;
        _completionEvent = new ManualResetEvent(false);
        _continuations = new Queue<Action>();
    }

    public void Execute()
    {
        try
        {
            _cancellationToken.ThrowIfCancellationRequested();
            TResult localResult = _function();
            lock (_lockObject)
            {
                _result = localResult;
                _isCompleted = true;
                _completionEvent.Set();

                while (_continuations.Count > 0)
                {
                    var continuation = _continuations.Dequeue();
                    _threadPool.EnqueueContinuation(continuation);
                }
            }
        }
        catch (Exception ex)
        {
            lock (_lockObject)
            {
                _exception = ex;
                _isCompleted = true;
                _completionEvent.Set();

                while (_continuations.Count > 0)
                {
                    var continuation = _continuations.Dequeue();
                    _threadPool.EnqueueContinuation(continuation);
                }
            }
        }
    }

    public bool IsCompleted => _isCompleted;

    public TResult Result
    {
        get
        {
            _completionEvent.WaitOne();

            if (_exception != null)
            {
                throw new AggregateException(_exception);
            }
            return _result;
        }
    }

    public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> func)
    {
        if (func == null)
            throw new ArgumentNullException(nameof(func));

        Func<TNewResult> continuationFunction = () =>
        {
            TResult parentResult = this.Result;
            return func(parentResult);
        };

        var continuationTask = new MyTask<TNewResult>(
            continuationFunction,
            _threadPool,
            _cancellationToken);

        lock (_lockObject)
        {
            if (_isCompleted)
            {
                _threadPool.EnqueueContinuation(() => continuationTask.Execute());
            }
            else
            {
                _continuations.Enqueue(() => continuationTask.Execute());
            }
        }

        return continuationTask;
    }
}