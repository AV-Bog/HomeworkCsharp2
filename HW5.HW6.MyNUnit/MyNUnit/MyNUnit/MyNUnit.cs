// <copyright file="MyNUnit.cs" author="bogdanovaarina">
// under MIT License
// </copyright>

using System.Diagnostics;
using System.Reflection;
using DefaultNamespace;
using static System.Reflection.Assembly;

public class MyNUnit
{
    public static TestRunResult Testing(string filePath)
    {
        var result = new TestRunResult { AssemblyPath = filePath };
        var assembly = LoadFrom(filePath);
        var allTypes = assembly.GetTypes();
        List<TestClassInfo> allTestClasses = new();

        foreach (var type in allTypes)
        {
            var classInfo = new TestClassInfo();
            classInfo.ClassType = type;
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
                    if (method.IsStatic)
                    {
                        classInfo.AfterClassMethods.Add(method);
                    }
                    else
                    {
                    }
                }
                else if (method.GetCustomAttributes(typeof(BeforeClass), false).Length > 0)
                {
                    if (method.IsStatic)
                    {
                        classInfo.BeforeClassMethods.Add(method);
                    }
                    else
                    {
                    }
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

            if (classInfo.TestMethods.Count > 0)
            {
                allTestClasses.Add(classInfo);
            }
        }

        foreach (var classInfo in allTestClasses)
        {
            var classResult = new TestClassResult { ClassName = classInfo.ClassType.Name };
            
            foreach (var method in classInfo.BeforeClassMethods)
            {
                method.Invoke(null, null);
            }

            foreach (var test in classInfo.TestMethods)
            {
                var testResult = RunSingleTest(classInfo, test);
                classResult.Tests.Add(testResult);
            }

            foreach (var method in classInfo.AfterClassMethods)
            {
                method.Invoke(null, null);
            }
        }
        
        return result;
    }

    private static TestMethodResult RunSingleTest(TestClassInfo classInfo, TestMethodInfo test)
{
    var result = new TestMethodResult
    {
        MethodName = test.Method.Name,
        Status = TestStatus.Failed,
        DurationMs = 0
    };

    var classInstance = Activator.CreateInstance(classInfo.ClassType);

    if (!string.IsNullOrEmpty(test.Ignore))
    {
        result.Status = TestStatus.Ignored;
        result.Message = test.Ignore;
        return result;
    }

    foreach (var beforeMethod in classInfo.BeforeMethods)
    {
        try
        {
            beforeMethod.Invoke(classInstance, null);
        }
        catch (Exception ex)
        {
        }
    }

    var timer = new Stopwatch();
    timer.Start();

    try
    {
        test.Method.Invoke(classInstance, null);
        timer.Stop();
        result.DurationMs = timer.ElapsedMilliseconds;

        if (test.Exeption != null)
        {
            result.Status = TestStatus.Failed;
            result.Message = $"Ожидалось исключение {test.Exeption.Name}, но его не было.";
        }
        else
        {
            result.Status = TestStatus.Passed;
        }
    }
    catch (Exception ex) when (ex is TargetInvocationException || ex.InnerException != null)
    {
        timer.Stop();
        result.DurationMs = timer.ElapsedMilliseconds;
        var realException = ex.InnerException ?? ex;

        if (test.Exeption == null)
        {
            result.Status = TestStatus.Failed;
            result.Message = $"Неожиданное исключение: {realException.GetType().Name}: {realException.Message}";
        }
        else if (realException.GetType() != test.Exeption && !realException.GetType().IsSubclassOf(test.Exeption))
        {
            result.Status = TestStatus.Failed;
            result.Message = $"Ожидалось {test.Exeption.Name}, но было {realException.GetType().Name}.";
        }
        else
        {
            result.Status = TestStatus.Passed;
            result.Message = $"Получено ожидаемое исключение {realException.GetType().Name}.";
        }
    }
    finally
    {
        foreach (var afterMethod in classInfo.AfterMethods)
        {
            try
            {
                afterMethod.Invoke(classInstance, null);
            }
            catch (Exception ex)
            {
            }
        }
    }

    return result;
}
}