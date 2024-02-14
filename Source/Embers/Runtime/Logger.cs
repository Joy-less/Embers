using System;

namespace Embers;

public interface ILogger
{
    void    Write(string     message);
    void    WriteLine(string message);
    void    WriteLine();
    string? ReadLine();
    char    ReadKey(bool intercept);
}

public class ConsoleLogger : ILogger
{

    public void Write(string message)
    {
        Console.Write(message);
    }

    public void WriteLine(string message)
    {
        Console.WriteLine(message);
    }

    public void WriteLine()
    {
        Console.WriteLine();
    }
    public string? ReadLine()              => Console.ReadLine();
    public char    ReadKey(bool intercept) => Console.ReadKey(intercept).KeyChar;
}