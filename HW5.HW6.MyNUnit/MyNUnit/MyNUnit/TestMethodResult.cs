// <copyright file="TestMethodResult.cs" author="bogdanovaarina">
// under MIT License
// </copyright>

namespace DefaultNamespace;

public class TestMethodResult
{
    public string MethodName { get; set; } = string.Empty;
    public TestStatus Status { get; set; }
    public string? Message { get; set; }
    public long DurationMs { get; set; }
}