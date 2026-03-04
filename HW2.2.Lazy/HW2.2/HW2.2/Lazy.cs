// <copyright file="ILazy.cs" author="bogdanovaarina">
// under MIT License
// </copyright>

namespace HW2.Lazy;

public interface ILazy<T>
{
    T Get();
}