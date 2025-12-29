// <copyright file="TestRunService.cs" author="bogdanovaarina">
// under MIT License
// </copyright>

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MyNUnit;

namespace DefaultNamespace;

public class TestRunService
{
    private readonly ConcurrentDictionary<Guid, TestRunRecord> _runs = new();

    public Guid StartRun(List<string> assemblyPaths)
    {
        var runId = Guid.NewGuid();
        var record = new TestRunRecord
        {
            Id = runId,
            Timestamp = DateTime.UtcNow,
            Assemblies = new List<AssemblyRun>()
        };

        foreach (var path in assemblyPaths)
        {
            var result = MyNUnit.MyNUnit.Testing(path);
            record.Assemblies.Add(new AssemblyRun
            {
                Path = path,
                Result = result
            });
        }

        _runs.TryAdd(runId, record);
        return runId;
    }
    
    public IEnumerable<TestRunRecord> GetAllRuns()
    {
        return _runs.Values.OrderByDescending(r => r.Timestamp);
    }

    public TestRunRecord? GetRun(Guid id)
    {
        _runs.TryGetValue(id, out var run);
        return run;
    }
}