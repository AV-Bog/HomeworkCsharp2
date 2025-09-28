// <copyright file="ILazy.cs" author="bogdanovaarina">
// under MIT License
// </copyright>

namespace DefaultNamespace;

public interface ILazy<T>
{
    T Get();
}

public class SimpleLazy<T> : ILazy<T>
{
    private Func<T> supplier;
    private T value;
    private bool isValueCreated;

    public SimpleLazy(Func<T> supplier)
    {
        this.supplier = supplier ?? throw new ArgumentNullException(nameof(supplier));
        this.isValueCreated = false;
    }

    public T Get()
    {
        if (!isValueCreated)
        {
            value = supplier();
            supplier = null;
            isValueCreated = true;
        }
        return value;
    }
}

public class ThreadSafeLazy<T> : ILazy<T>
{
    private Func<T> supplier;
    private T value;
    private bool isValueCreated;
    private readonly object lockObject = new object();
    
    public ThreadSafeLazy(Func<T> supplier)
    {
        this.supplier = supplier ?? throw new ArgumentNullException(nameof(supplier));
        this.isValueCreated = false;
    }

    public T Get()
    {
        if (!isValueCreated)
        {
            lock(lockObject)
            {
                if (!isValueCreated)
                {
                    value = supplier();
                    supplier = null;
                    isValueCreated = true;
                }
            }
        }
        return value;
    }
}
