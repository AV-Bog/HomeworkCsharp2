using System.Reflection;
using System.Reflection.Emit;
using NUnit.Framework.Legacy;

namespace TestsMyNUnit;

using System;
using System.IO;
using System.Reflection;
using System.Text;
using NUnit.Framework;

[TestFixture]
public class MyNUnitFunctionalTest
{
    [Test]
    public void MyNUnit_HandlesBasicTestCasesCorrectly()
    {
        var writer = new StringWriter();
        var originalOut = Console.Out;
        Console.SetOut(writer);

        try
        {
            MyNUnit.Testing(Assembly.GetExecutingAssembly().Location);
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        string actual = writer.ToString();

        StringAssert.Contains("[SUCCESS] BasicTestSuite.PassingTest", actual);
        StringAssert.Contains("[IGNORED] BasicTestSuite.IgnoredTest", actual);
        StringAssert.Contains("по замыслу", actual);
        StringAssert.Contains("ожидаемое исключение ArgumentException получено", actual);
        StringAssert.Contains("ожидалось InvalidOperationException, но было ArgumentException", actual);
    }
}