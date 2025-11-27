// <copyright file="SimpleLazy.cs" author="bogdanovaarina">
// under MIT License
// </copyright>

namespace HW2.Lazy;

public class SimpleLazy<T> : ILazy<T>
{
    private Func<T>? supplier;
    private T? value;
    private bool isValueCreated;

    public SimpleLazy(Func<T> supplier)
    {
        this.supplier = supplier ?? throw new ArgumentNullException(nameof(supplier));
    }

    public T Get()
    {
        if (!isValueCreated)
        {
            value = supplier!();
            supplier = null;
            isValueCreated = true;
        }
        return value!;
    }
}