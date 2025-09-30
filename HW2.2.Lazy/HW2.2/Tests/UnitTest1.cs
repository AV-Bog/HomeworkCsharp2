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
        [Test]
        public void SimpleLazy_Get_CallsSupplierOnlyOnce()
        {
            int callCount = 0;
            var lazy = new SimpleLazy<int>(() => ++callCount);

            var result1 = lazy.Get();
            var result2 = lazy.Get();
            var result3 = lazy.Get();

            Assert.That(result1, Is.EqualTo(1));
            Assert.That(result2, Is.EqualTo(1));
            Assert.That(result3, Is.EqualTo(1));
            Assert.That(callCount, Is.EqualTo(1));
        }

        [Test]
        public void SimpleLazy_Constructor_ThrowsOnNullSupplier()
        {
            Assert.Throws<ArgumentNullException>(() => new SimpleLazy<object>(null!));
        }

        [Test]
        public void ThreadSafeLazy_Get_CallsSupplierOnlyOnce()
        {
            int callCount = 0;
            var lazy = new ThreadSafeLazy<int>(() => ++callCount);

            var result1 = lazy.Get();
            var result2 = lazy.Get();
            var result3 = lazy.Get();

            Assert.That(result1, Is.EqualTo(1));
            Assert.That(result2, Is.EqualTo(1));
            Assert.That(result3, Is.EqualTo(1));
            Assert.That(callCount, Is.EqualTo(1));
        }

        [Test]
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

            Assert.That(callCount, Is.EqualTo(1));
            foreach (var result in results)
            {
                Assert.That(result, Is.EqualTo(1));
            }
        }
    }