// <copyright file="NotSimple.cs" author="bogdanovaarina">
// under MIT License
// </copyright>

using HW2.Lazy;

namespace Tests;

public class Tests
{
    [Test]
    public void SimpleLazy_Get_CallsSupplierOnlyOnce()
    {
        int callCount = 0;
        var lazy = new SimpleLazy<int>(() => ++callCount);

        var result1 = lazy.Get();
        var result2 = lazy.Get();
        var result3 = lazy.Get();

        Assert.AreEqual(1, result1);
        Assert.AreEqual(1, result2);
        Assert.AreEqual(1, result3);
        Assert.AreEqual(1, callCount);
    }
    
    [Test]
    public void SimpleLazy_Get_WithNullValue()
    {
        var lazy = new SimpleLazy<object>(() => null);

        Assert.IsNull(lazy.Get());
        Assert.IsNull(lazy.Get());
    }
    
    [Test]
    public void SimpleLazy_Constructor_ThrowsOnNullSupplier()
    {
        Assert.Throws<ArgumentNullException>(() => new SimpleLazy<object>(null));
    }
    
    [Test]
    public void ThreadSafeLazy_Get_CallsSupplierOnlyOnce()
    {
        int callCount = 0;
        var lazy = new ThreadSafeLazy<int>(() => ++callCount);

        var result1 = lazy.Get();
        var result2 = lazy.Get();
        var result3 = lazy.Get();

        Assert.AreEqual(1, result1);
        Assert.AreEqual(1, result2);
        Assert.AreEqual(1, result3);
        Assert.AreEqual(1, callCount);
    }
    
    public void ThreadSafeLazy_Multithreaded_NoRaceConditions()
    {
        int callCount = 0;
        var lazy = new ThreadSafeLazy<int>(() =>
        {
            Thread.Sleep(100);
            return Interlocked.Increment(ref callCount);
        });

        const int threadCount = 10;
        var results = new int[threadCount];
        var threads = new Thread[threadCount];

        for (int i = 0; i < threadCount; i++)
        {
            var index = i;
            threads[i] = new Thread(() => results[index] = lazy.Get());
            threads[i].Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        Assert.AreEqual(1, callCount);
        foreach (var result in results)
        {
            Assert.AreEqual(1, result);
        }
    }
}