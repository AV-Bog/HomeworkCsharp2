// <copyright file="MyThreadPool.cs" author="bogdanovaarina">
// under MIT License
// </copyright>

namespace HW3.ThreadPool;

using System.Collections.Concurrent;

/// <summary>
/// Represents a thread pool that executes tasks using a fixed number of worker threads.
/// Provides functionality similar to <see cref="System.Threading.ThreadPool"/> and <see cref="System.Threading.Tasks.TaskFactory"/>,
/// but with a fixed-size thread pool and custom task execution model.
/// </summary>
public sealed class MyThreadPool : IDisposable
{
    private readonly Thread[] _workerThreads;
    private readonly BlockingCollection<Action> _taskQueue = new();
    private bool _isShutdown = false;
    private readonly object _shutdownLock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MyThreadPool"/> class with the specified number of worker threads.
    /// </summary>
    /// <param name="threadCount">The number of worker threads in the pool. Must be greater than zero.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="threadCount"/> is less than or equal to zero.</exception>

    public MyThreadPool(int threadCount)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(threadCount, 0);

        this._workerThreads = new Thread[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            this._workerThreads[i] = new Thread(this.WorkerLoop)
            {
                IsBackground = true,
                Name = $"MyThreadPool.Worker-{i}",
            };
            this._workerThreads[i].Start();
        }
    }

    private void WorkerLoop()
    {
        foreach (var task in this._taskQueue.GetConsumingEnumerable())
        {
            try
            {
                task();
            }
            catch
            {
                // Ignore exceptions
            }
        }
    }

    /// <summary>
    /// Submits a task for execution to the thread pool.
    /// </summary>
    /// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
    /// <param name="func">The function to execute. Must not be null.</param>
    /// <returns>A task representing the pending or completed computation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="func"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the thread pool has already been shut down.</exception>
    public IMyTask<TResult> Submit<TResult>(Func<TResult> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        var task = new MyTask<TResult>(func, this);
        try
        {
            _taskQueue.Add(task.Execute);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("ThreadPool has been shut down.");
        }

        return task;
    }

    /// <summary>
    /// Enqueues a continuation action to be executed by the thread pool.
    /// This method is intended for internal use by <see cref="MyTask{TResult}"/>.
    /// </summary>
    /// <param name="continuation">The action to enqueue.</param>
    internal void EnqueueContinuation(Action continuation)
    {
        this._taskQueue.Add(continuation);
    }

    /// <summary>
    /// Initiates an orderly shutdown of the thread pool.
    /// No new tasks will be accepted, but all previously submitted tasks will be completed.
    /// This method blocks until all worker threads have finished executing pending tasks.
    /// </summary>
    /// <remarks>
    /// This method is idempotent — multiple calls have no additional effect.
    /// </remarks>
    public void Shutdown()
    {
        lock (this._shutdownLock)
        {
            if (this._isShutdown)
            {
                return;
            }

            this._isShutdown = true;
        }

        this._taskQueue.CompleteAdding();

        foreach (var thread in this._workerThreads)
        {
            thread.Join();
        }

        this._taskQueue.Dispose();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this.Shutdown();
        GC.SuppressFinalize(this);
    }
}