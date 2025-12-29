// <copyright file="TestClassResult.cs" author="bogdanovaarina">
// under MIT License
// </copyright>

namespace DefaultNamespace;

public class TestClassResult
{
    public string ClassName { get; set; } = string.Empty;
    public List<TestMethodResult> Tests { get; set; } = new();
}