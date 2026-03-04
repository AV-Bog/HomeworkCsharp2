// <copyright file="IMyTask.cs" author="bogdanovaarina">
// under MIT License
// </copyright>

namespace HW3.ThreadPool;

/// <summary>
/// Represents a task running in <see cref="MyThreadPool"/>.
/// </summary>
/// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
public interface IMyTask<TResult>
{
    /// <summary>
    /// Gets a value indicating whether the task has completed.
    /// </summary>
    bool IsCompleted { get; }

    /// <summary>
    /// Gets the result of the task. Blocks until the task completes.
    /// Throws <see cref="AggregateException"/> if the task faulted.
    /// </summary>
    TResult Result { get; }

    /// <summary>
    /// Creates a continuation task that executes when this task completes.
    /// </summary>
    /// <typeparam name="TNewResult">The result type of the continuation.</typeparam>
    /// <param name="func">The function to apply to the result of this task.</param>
    /// <returns>A new task representing the continuation.</returns>
    IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> func);
}