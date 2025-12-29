// <copyright file="TestRunRecord.cs" author="bogdanovaarina">
// under MIT License
// </copyright>

namespace DefaultNamespace;

public class TestRunRecord
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public List<AssemblyRun> Assemblies { get; set; } = new();
}