// <copyright file="IMyTask.cs" author="bogdanovaarina">
// under MIT License
// </copyright>

namespace HW2.ThreadPool;

public interface IMyTask<TResult>
{
    bool IsCompleted { get; }
    TResult Result { get; }
    IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> func);
}