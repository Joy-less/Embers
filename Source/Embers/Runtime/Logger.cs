using System;

namespace Embers {
    public class Logger {
        public virtual void Write(string Message)
            => Console.Write(Message);
        public virtual void WriteLine(string Message)
            => Console.WriteLine(Message);
        public virtual void WriteLine()
            => Console.WriteLine();
        public virtual string? ReadLine()
            => Console.ReadLine();
        public virtual char ReadKey(bool Display)
            => Console.ReadKey(!Display).KeyChar;
    }
}