using System.Net.Sockets;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace TestsTest3;

public class Tests
{
    [Test]
    public void Test1()
    {
        var originalOut =  Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);
        
        var arg = Array.Empty<string>();
        Program.Main(arg);
        
        var output = writer.ToString();
            
        Assert.That(output, Does.Contain("Usage:"));
        Assert.That(output, Does.Contain("Server mode:"));
        Assert.That(output, Does.Contain("Client mode:"));
        Assert.That(output, Does.Contain("Example:"));
        
        Console.SetOut(originalOut);
    }
    
    [Test]
    public void Test2()
    {
        var args = new[] { "8888" };
        
        Assert.That(() => Program.Main(args), Throws.Nothing);
    }
    
    [Test]
    public void Test3()
    {
        var args = new[] { "127.0.0.1", "8888" };

        Assert.Throws<SocketException>(() => Program.Main(args));
    }
}