// <copyright file="MyNUnit.cs" author="bogdanovaarina">
// under MIT License
// </copyright>

using System.Reflection;
using static System.Reflection.Assembly;

public class MyNUnit
{
    public static void Testing(string filePath)
    {
        var assembly = LoadFrom(filePath);
        var allTypes = assembly.GetTypes();
        List<TestClassInfo> allTestClasses = new List<TestClassInfo>();

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

                    var testMethodInfo = new TestMethodInfo
                    {
                        Method = method,
                        Exeption = expectedException,
                        Ignore = ignoreReason,
                    };

                    classInfo.TestMethods.Add(testMethodInfo);
                }
            }

            allTestClasses.Add(classInfo);
        }

        foreach (var classInfo in allTestClasses)
        {
            RunTestForClass(classInfo);
        }
    }

    private static void RunTestForClass(TestClassInfo classInfo)
    {
        foreach (var method in classInfo.BeforeClassMethods)
        {
            method.Invoke(null, null);
        }

        var classInstance = Activator.CreateInstance(classInfo.ClassType);
        foreach (var test in classInfo.TestMethods)
        {
            if (test.Ignore != null)
            {
                Console.WriteLine($"[IGNORED] {test}");
                continue;
            }

            foreach (var methodB in classInfo.BeforeMethods)
            {
                methodB.Invoke(classInstance, null);
            }

            try
            {
                test.Method.Invoke(classInstance, null);
                if (test.Exeption != null)
                {
                    Console.WriteLine($"[FAIL] {test} - исключения не было, но ожидалось {test.Exeption}");
                }
                else
                {
                    Console.WriteLine($"[SUCCESS] {test} - исключения не было и не ожидалось");
                }
            }
            catch (Exception ex)
            {
                if (!Equals(ex.GetType(), test.Exeption))
                {
                    Console.WriteLine($"[FAIL] {test} - ожидаемое исключение {test.Exeption} не совпало с произошедшим {ex.GetType()}");
                }
                else
                {
                    Console.WriteLine($"[SUCCESS] {test} - ожидаемое исключение совпало с произошедшим");
                }
            }
            finally
            {
                foreach (var methodA in classInfo.AfterMethods)
                {
                    methodA.Invoke(classInstance, null);
                }
            }
        }

        foreach (var method in classInfo.AfterClassMethods)
        {
            method.Invoke(null, null);
        }
    }
}