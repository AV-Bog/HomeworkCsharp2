// <copyright file="AssemblyRun.cs" author="bogdanovaarina">
// under MIT License
// </copyright>

namespace DefaultNamespace;

public class AssemblyRun
{
    public string Path { get; set; } = string.Empty;
    public TestRunResult Result { get; set; } = new();
}