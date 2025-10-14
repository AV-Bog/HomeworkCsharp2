// <copyright file="MyThreadPool.cs" author="bogdanovaarina">
// under MIT License
// </copyright>

namespace HW2.ThreadPool;

public class MyThreadPool : IDisposable
{
    private readonly Queue<Action> _taskQueue = new Queue<Action>();
    private readonly Thread[] _workedThreads;
    private readonly object _lockQueue = new object();
    private readonly AutoResetEvent _newTaskEvent = new AutoResetEvent(false);
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private volatile bool _isShutdown = false;
    private int _activeTasksCount = 0;

    public MyThreadPool(int threadCount)
    {
        if (threadCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(threadCount), "кол-во потоков отриц?????");
        }

        _workedThreads = new Thread[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            _workedThreads[i] = new Thread(WorkerThreadProc);
            _workedThreads[i].Name = $"MyThreadPool Worked #{i}";
            _workedThreads[i].IsBackground = true;

            _workedThreads[i].Start();
        }
    }

    private void WorkerThreadProc()
    {
        while (!_isShutdown)
        {
            Action task = null;

            lock (_lockQueue)
            {
                if (_taskQueue.Count > 0)
                {
                    task = _taskQueue.Dequeue();
                }
            }

            if (task != null)
            {
                try
                {
                    Interlocked.Increment(ref _activeTasksCount);
                    task();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка в рабочем потоке: {ex.Message}");
                }
                finally
                {
                    Interlocked.Decrement(ref _activeTasksCount);
                }
            }
            else
            {
                _newTaskEvent.WaitOne(100);
            }
        }
    }

    public IMyTask<TResult> Submit<TResult>(Func<TResult> func)
    {
        if (func == null)
            throw new ArgumentNullException(nameof(func));

        if (_isShutdown)
            throw new InvalidOperationException("лавочка закрыта, избушка на клюшку");

        var task = new MyTask<TResult>(func, this, _cancellationTokenSource.Token);

        EnqueueContinuation(() => task.Execute());

        return task;
    }

    public void EnqueueContinuation(Action continuation)
    {
        if (_isShutdown)
        {
            return;
        }

        lock (_lockQueue)
        {
            _taskQueue.Enqueue(continuation);
        }

        _newTaskEvent.Set();
    }

    public void Dispose()
    {
        Shutdown();
    }

    private void Shutdown()
    {
        throw new NotImplementedException();
    }
}