// <copyright file="ThreadSafeLazy.cs" author="bogdanovaarina">
// under MIT License
// </copyright>

namespace HW2.Lazy;

public class ThreadSafeLazy<T> : ILazy<T>
{
    private Func<T>? supplier;
    private T? value;
    private volatile bool isValueCreated;
    private readonly object lockObject = new object();

    public ThreadSafeLazy(Func<T> supplier)
    {
        this.supplier = supplier ?? throw new ArgumentNullException(nameof(supplier));
    }

    public T Get()
    {
        if (!isValueCreated)
        {
            lock (lockObject)
            {
                if (!isValueCreated)
                {
                    value = supplier!();
                    supplier = null;
                    isValueCreated = true;
                }
            }
        }
        return value!;
    }
}