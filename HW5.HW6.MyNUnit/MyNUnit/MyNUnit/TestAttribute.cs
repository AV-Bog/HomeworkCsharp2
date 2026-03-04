// <copyright file="TestAttribute.cs" author="bogdanovaarina">
// under MIT License
// </copyright>

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class TestAttribute : Attribute
{
    public Type Expected { get; set; }

    public string Ignore { get; set; }
}