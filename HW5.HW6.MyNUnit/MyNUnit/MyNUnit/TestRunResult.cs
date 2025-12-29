// <copyright file="TestRunResult.cs" author="bogdanovaarina">
// under MIT License
// </copyright>

namespace DefaultNamespace;

public class TestRunResult
{
    public var AssemblyPath { get; set; } = string.Empty;
    public List<TestClassResult> Classes { get; set; } = new();
    public var TotalTests => Classes.Sum(c => c.Tests.Count);
    public var PassedTests => Classes.Sum(c => c.Tests.Count(t => t.Status == TestStatus.Passed));
    public var FailedTests => Classes.Sum(c => c.Tests.Count(t => t.Status == TestStatus.Failed));
    public var IgnoredTests => Classes.Sum(c => c.Tests.Count(t => t.Status == TestStatus.Ignored));

    public override string ToString()
    {
        return $"Assembly: {AssemblyPath}\n" +
               $"Total: {TotalTests}, Passed: {PassedTests}, Failed: {FailedTests}, Ignored: {IgnoredTests}";
    }
}