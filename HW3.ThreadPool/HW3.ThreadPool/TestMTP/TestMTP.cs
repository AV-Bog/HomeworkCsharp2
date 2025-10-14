using HW2.ThreadPool;

namespace TestMTP;

public class Tests
{
    [Test]
    public void Submit_SimpleTask_ReturnsCorrectResult()
    {
        using var pool = new MyThreadPool(2);

        var task = pool.Submit(() => 42);
        var result = task.Result;

        Assert.That(task.IsCompleted, Is.True);
        Assert.That(result, Is.EqualTo(42));
    }
    
    [Test]
    public void Submit_TaskWithDelay_CompletesSuccessfully()
    {
        using var pool = new MyThreadPool(2);

        var task = pool.Submit(() =>
        {
            Thread.Sleep(50);
            return "Hello World";
        });
        var result = task.Result;

        Assert.That(task.IsCompleted, Is.True);
        Assert.That(result, Is.EqualTo("Hello World"));
    }
    
    [Test]
    public void ContinueWith_ChainOfTasks_ExecutesInCorrectOrder()
    {
        using var pool = new MyThreadPool(2);

        var task = pool.Submit(() => 10)
            .ContinueWith(x => x * 2)
            .ContinueWith(x => $"Result: {x}");

        var result = task.Result;

        Assert.That(task.IsCompleted, Is.True);
        Assert.That(result, Is.EqualTo("Result: 20"));
    }
}