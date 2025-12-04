// <copyright file="TestClassInfo.cs" author="bogdanovaarina">
// under MIT License
// </copyright>

using System.Reflection;

public class TestClassInfo
{
    public Type ClassType;
    public List<MethodInfo> BeforeClassMethods;
    public List<MethodInfo> AfterClassMethods;
    public List<MethodInfo> BeforeMethods;
    public List<MethodInfo> AfterMethods;
    public static List<TestMethodInfo> TestMethods;
}
