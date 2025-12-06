// <copyright file="MyNUnit.cs" author="bogdanovaarina">
// under MIT License
// </copyright>

using System.Diagnostics;
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
                        lock (ConsoleLock)
                        {
                            Console.WriteLine($"[ERROR] Метод {method.Name} в классе {type.Name} помечен [AfterClass], но не является статическим");
                        }
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
                        lock (ConsoleLock)
                        {
                            Console.WriteLine($"[ERROR] Метод {method.Name} в классе {type.Name} помечен [BeforeClass], но не является статическим");
                        }
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
            foreach (var method in classInfo.BeforeClassMethods)
            {
                method.Invoke(null, null);
            }

            RunTestForClass(classInfo);

            foreach (var method in classInfo.AfterClassMethods)
            {
                method.Invoke(null, null);
            }
        }
    }

    private static readonly object ConsoleLock = new object();

    private static void RunTestForClass(TestClassInfo classInfo)
    {
        Parallel.ForEach(classInfo.TestMethods, test =>
        {
            var classInstance = Activator.CreateInstance(classInfo.ClassType);
            if (test.Ignore != null)
            {
                lock (ConsoleLock)
                {
                    Console.WriteLine(
                        $"[IGNORED] {test.Method.DeclaringType?.Name}.{test.Method.Name}. Причина игнорирования теста - {test.Ignore}");
                }

                return;
            }

            foreach (var methodB in classInfo.BeforeMethods)
            {
                methodB.Invoke(classInstance, null);
            }

            var timer = new Stopwatch();
            timer.Start();
            try
            {
                test.Method.Invoke(classInstance, null);
                timer.Stop();
                var time = timer.Elapsed;

                if (test.Exeption != null)
                {
                    lock (ConsoleLock)
                    {
                        Console.WriteLine(
                            $"[FAIL] {test.Method.DeclaringType?.Name}.{test.Method.Name} - исключения не было, но ожидалось {test.Exeption.Name}. Время выполнения - {time.ToString()} мс");
                    }
                }
                else
                {
                    lock (ConsoleLock)
                    {
                        Console.WriteLine(
                            $"[SUCCESS] {test.Method.DeclaringType?.Name}.{test.Method.Name} - тест пройден. Время выполнения - {time.ToString()} мс");
                    }
                }
            }
            catch (Exception ex) when (ex is TargetInvocationException || ex.InnerException != null)
            {
                timer.Stop();
                var time = timer.Elapsed;
                var realException = ex.InnerException ?? ex;

                if (test.Exeption == null)
                {
                    lock (ConsoleLock)
                    {
                        Console.WriteLine($"[FAIL] {test.Method.DeclaringType?.Name}.{test.Method.Name} - неожиданное исключение: {realException.GetType().Name}: {realException.Message}. Время: {time.TotalMilliseconds} мс");
                    }
                }
                else if (realException.GetType() != test.Exeption && !realException.GetType().IsSubclassOf(test.Exeption))
                {
                    lock (ConsoleLock)
                    {
                        Console.WriteLine($"[FAIL] {test.Method.DeclaringType?.Name}.{test.Method.Name} - ожидалось {test.Exeption.Name}, но было {realException.GetType().Name}. Время: {time.TotalMilliseconds} мс");
                    }
                }
                else
                {
                    lock (ConsoleLock)
                    {
                        Console.WriteLine($"[SUCCESS] {test.Method.DeclaringType?.Name}.{test.Method.Name} - ожидаемое исключение {realException.GetType().Name} получено. Время: {time.TotalMilliseconds} мс");
                    }
                }
            }
            finally
            {
                foreach (var methodA in classInfo.AfterMethods)
                {
                    methodA.Invoke(classInstance, null);
                }
            }
        });
    }
}