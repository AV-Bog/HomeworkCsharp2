// <copyright file="TestClassInfo.cs" author="bogdanovaarina">
// under MIT License
// </copyright>

using System.Reflection;

public class TestClassInfo
{
    public Type ClassType = null!;

    public List<MethodInfo> BeforeClassMethods { get; set; } = new();

    public List<MethodInfo> AfterClassMethods { get; set; } = new();

    public List<MethodInfo> BeforeMethods { get; set; } = new();

    public List<MethodInfo> AfterMethods { get; set; } = new();

    public List<TestMethodInfo> TestMethods { get; set; } = new();
}
