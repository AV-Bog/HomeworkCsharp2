// <copyright file="MyNUnit.cs" author="bogdanovaarina">
// under MIT License
// </copyright>

using static System.Reflection.Assembly;

public class MyNUnit
{
    public static void Testing(string filePath)
    {
        var assembly = LoadFrom(filePath);
        var allTypes = assembly.GetTypes();
        foreach (var type in allTypes)
        {
            var classInfo = new TestClassInfo();
            var methodInfo = type.GetMethods();
            foreach (var method in methodInfo)
            {
                if (method.GetCustomAttributes(typeof(After), false).Length > 0)
                {
                    classInfo.AfterMethods.Add(method);
                }
                else if (method.GetCustomAttributes(typeof(Before), false).Length > 0)
                {
                    classInfo.BeforeMethods.Add(method);
                }
                else if (method.GetCustomAttributes(typeof(AfterClass), false).Length > 0)
                {
                    classInfo.AfterClassMethods.Add(method);
                }
                else if (method.GetCustomAttributes(typeof(BeforeClass), false).Length > 0)
                {
                    classInfo.BeforeClassMethods.Add(method);
                }
                
                TestAttribute testAttribute = method
                    .GetCustomAttributes(typeof(TestAttribute), false)
                    .FirstOrDefault() as TestAttribute;
                if (testAttribute != null)
                {
                    Type expectedException = testAttribute.Expected;
                    string ignoreReason = testAttribute.Ignore;

                    TestMethodInfo testMethodInfo = new TestMethodInfo
                    {
                        Method = method,
                        Exeption = expectedException,
                        Ignore = ignoreReason
                    };

                    TestClassInfo.TestMethods.Add(testMethodInfo);
                }
                
            }
            
        }
    }
    
}