// <copyright file="BasicTestSuite.cs" author="bogdanovaarina">
// under MIT License
// </copyright>

namespace TestsMyNUnit;

public class BasicTestSuite
{
    [Test]
    public void PassingTest() { }

    [Test(Ignore = "по замыслу")]
    public void IgnoredTest() { }

    [Test(Expected = typeof(ArgumentException))]
    public void ThrowsExpectedException()
    {
        throw new ArgumentException("намеренно");
    }

    [Test(Expected = typeof(InvalidOperationException))]
    public void ThrowsUnexpectedException()
    {
        throw new ArgumentException("не тот тип");
    }
}