// <copyright file="ClickCounterModel.cs" author="bogdanovaarina">
// under MIT License
// </copyright>

namespace MauiCalc.Models;

public class ClickCounterModel
{
    public int Count { get; set; } = 0;

    public void Increment()
    {
        Count++;
    }
}