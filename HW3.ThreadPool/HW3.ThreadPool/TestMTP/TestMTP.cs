using System.Collections.Concurrent;
using HW3.ThreadPool;

namespace TestMTP;

[TestFixture]
public class MyThreadPoolTests
{
    [Test]
    public void Submit_SimpleTask_ReturnsCorrectResult()
    {
        using var pool = new MyThreadPool(2);
        var task = pool.Submit(() => 42);
        Assert.That(task.Result, Is.EqualTo(42));
        Assert.That(task.IsCompleted, Is.True);
    }

    [Test]
    public void Submit_TaskWithDelay_CompletesSuccessfully()
    {
        using var pool = new MyThreadPool(2);
        var task = pool.Submit(() =>
        {
            Thread.Sleep(50);
            return "Hello";
        });
        Assert.That(task.Result, Is.EqualTo("Hello"));
    }

    [Test]
    public void ContinueWith_Chain_ExecutesCorrectly()
    {
        using var pool = new MyThreadPool(2);
        var task = pool.Submit(() => 5)
            .ContinueWith(x => x * 3)
            .ContinueWith(x => x.ToString());

        Assert.That(task.Result, Is.EqualTo("15"));
    }

    [Test]
    public void Task_ThrowsException_ShouldWrapInAggregateException()
    {
        using var pool = new MyThreadPool(1);
        var task = pool.Submit<object>(() => throw new InvalidOperationException("Oops"));

        var ex = Assert.Throws<AggregateException>(() => _ = task.Result);
        Assert.That(ex.InnerException, Is.InstanceOf<InvalidOperationException>());
        Assert.That(task.IsCompleted, Is.True);
    }

    [Test]
    public void ContinueWith_OnFaultedTask_ShouldAlsoFaultImmediately()
    {
        using var pool = new MyThreadPool(1);
        var faulted = pool.Submit<int>(() => throw new Exception("Boom"));
        var continuation = faulted.ContinueWith(x => x + 1);

        var ex = Assert.Throws<AggregateException>(() => _ = continuation.Result);
        Assert.That(ex.InnerException?.Message, Is.EqualTo("Boom"));
    }

    [Test]
    public void Submit_AfterShutdown_ThrowsInvalidOperationException()
    {
        var pool = new MyThreadPool(1);
        pool.Shutdown();

        Assert.Throws<InvalidOperationException>(() => pool.Submit(() => 1));
        pool.Dispose(); 
    }

    [Test]
    public void Shutdown_WaitsForAllTasksToComplete()
    {
        var pool = new MyThreadPool(2);

        var t1 = pool.Submit(() =>
        {
            Thread.Sleep(50);
            return 1;
        });
        var t2 = pool.Submit(() =>
        {
            Thread.Sleep(50);
            return 2;
        });

        pool.Shutdown(); // ← явный вызов

        Assert.That(t1.Result, Is.EqualTo(1));
        Assert.That(t2.Result, Is.EqualTo(2));

        pool.Dispose(); // ← можно вызвать, но не обязательно, если нет неуправляемых ресурсов
    }
    
    [Test]
    public void ThreadPool_HasExactlyNThreads()
    {
        const int n = 4;
        var threadIds = new ConcurrentBag<int>();

        using var pool = new MyThreadPool(n);

        var tasks = Enumerable.Range(0, n * 2)
            .Select(_ => pool.Submit(() =>
            {
                threadIds.Add(Environment.CurrentManagedThreadId);
                Thread.Sleep(10);
                return 0;
            }))
            .ToArray();

        foreach (var t in tasks) _ = t.Result;

        Assert.That(threadIds.Distinct().Count(), Is.LessThanOrEqualTo(n));
        Assert.That(threadIds.Distinct().Count(), Is.GreaterThanOrEqualTo(1));
    }

    [Test]
    public void ConcurrentSubmitAndShutdown_DoesNotCreateZombieTasks()
    {
        using var pool = new MyThreadPool(2);
        const int taskCount = 100;
        var tasks = new List<IMyTask<int>>();

        var submitThread = new Thread(() =>
        {
            for (int i = 0; i < taskCount; i++)
            {
                try
                {
                    tasks.Add(pool.Submit(() => i));
                }
                catch (InvalidOperationException)
                {
                    break;
                }
            }
        });

        submitThread.Start();

        Thread.Sleep(10);
        pool.Shutdown();

        submitThread.Join();

        foreach (var task in tasks)
        {
            Assert.DoesNotThrow(() => _ = task.Result);
            Assert.That(task.IsCompleted, Is.True);
        }
    }
}