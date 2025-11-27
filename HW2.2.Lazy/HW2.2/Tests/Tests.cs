// <copyright file="NotSimple.cs" author="bogdanovaarina">
// under MIT License
// </copyright>

using System;
using System.Threading;
using HW2.Lazy;
using NUnit.Framework;

namespace Tests;

[TestFixture]
public class Tests
{
    private static readonly object[] LazyImplementations =
    {
        new object[] { new Func<Func<int>, ILazy<int>>(supplier => new SimpleLazy<int>(supplier)) },
        new object[] { new Func<Func<int>, ILazy<int>>(supplier => new ThreadSafeLazy<int>(supplier)) }
    };

    private static readonly object[] StringLazyImplementations =
    {
        new object[] { new Func<Func<string>, ILazy<string>>(supplier => new SimpleLazy<string>(supplier)) },
        new object[] { new Func<Func<string>, ILazy<string>>(supplier => new ThreadSafeLazy<string>(supplier)) }
    };
    
    [Test]
    [TestCaseSource(nameof(LazyImplementations))]
    public void Lazy_Get_CallsSupplierOnlyOnce(Func<Func<int>, ILazy<int>> lazyFactory)
    {
        int callCount = 0;
        var lazy = lazyFactory(() => ++callCount);

        var result1 = lazy.Get();
        var result2 = lazy.Get();
        var result3 = lazy.Get();

        Assert.That(result1, Is.EqualTo(1));
        Assert.That(result2, Is.EqualTo(1));
        Assert.That(result3, Is.EqualTo(1));
        Assert.That(callCount, Is.EqualTo(1));
    }

    [Test]
    [TestCaseSource(nameof(StringLazyImplementations))]
    public void Lazy_Get_WithStringType_WorksCorrectly(Func<Func<string>, ILazy<string>> lazyFactory)
    {
        int callCount = 0;
        var lazy = lazyFactory(() =>
        {
            callCount++;
            return $"Called {callCount} times";
        });

        var result1 = lazy.Get();
        var result2 = lazy.Get();

        Assert.That(result1, Is.EqualTo("Called 1 times"));
        Assert.That(result2, Is.EqualTo("Called 1 times"));
        Assert.That(callCount, Is.EqualTo(1));
    }
    
    [Test]
    [TestCaseSource(nameof(LazyImplementations))]
    public void Lazy_Get_WithNullValue_WorksCorrectly(Func<Func<int?>, ILazy<int?>> lazyFactory)
    {
        int callCount = 0;
        var lazy = lazyFactory(() =>
        {
            callCount++;
            return null;
        });

        var result1 = lazy.Get();
        var result2 = lazy.Get();

        Assert.That(result1, Is.Null);
        Assert.That(result2, Is.Null);
        Assert.That(callCount, Is.EqualTo(1));
    }
    
    [Test]
    [TestCaseSource(nameof(LazyImplementations))]
    public void Lazy_Get_WithComplexObject_WorksCorrectly(Func<Func<object>, ILazy<object>> lazyFactory)
    {
        int callCount = 0;
        var lazy = lazyFactory(() =>
        {
            callCount++;
            return new { Id = callCount, Name = "Test", Created = DateTime.Now };
        });

        var result1 = lazy.Get();
        var result2 = lazy.Get();

        Assert.That(result1, Is.SameAs(result2));
        Assert.That(callCount, Is.EqualTo(1));
    }
    
    [Test]
    public void SimpleLazy_Constructor_ThrowsOnNullSupplier()
    {
        Assert.Throws<ArgumentNullException>(() => new SimpleLazy<object>(null!));
    }

    [Test]
    public void ThreadSafeLazy_Constructor_ThrowsOnNullSupplier()
    {
        Assert.Throws<ArgumentNullException>(() => new ThreadSafeLazy<object>(null!));
    }
    
    [Test]
    public void ThreadSafeLazy_Multithreaded_NoRaceConditions()
    {
        int callCount = 0;
        var startSignal = new ManualResetEventSlim(false);
        var lazy = new ThreadSafeLazy<int>(() =>
        {
            Thread.Sleep(100);
            return Interlocked.Increment(ref callCount);
        });

        const int threadCount = 10;
        var results = new int[threadCount];
        var threads = new Thread[threadCount];
        Exception? overallException = null;

        for (int i = 0; i < threadCount; i++)
        {
            var index = i;
            threads[i] = new Thread(() =>
            {
                try
                {
                    startSignal.Wait();
                    results[index] = lazy.Get();
                }
                catch (Exception ex)
                {
                    Interlocked.CompareExchange(ref overallException, ex, null);
                }
            });
            threads[i].Start();
        }

        Thread.Sleep(100);
        startSignal.Set();
        
        foreach (var thread in threads)
        {
            thread.Join();
        }

        if (overallException != null)
        {
            throw new AssertionException("Исключение в одном из потоков", overallException);
        }
        
        Assert.That(callCount, Is.EqualTo(1));
        foreach (var result in results)
        {
            Assert.That(result, Is.EqualTo(1));
        }
    }
}