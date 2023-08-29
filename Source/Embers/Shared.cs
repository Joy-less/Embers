namespace Embers
{
    public abstract class EmbersException : Exception {
        public EmbersException(string Message) : base(Message) { }
    }
    public class SyntaxErrorException : EmbersException {
        public SyntaxErrorException(string Message) : base(Message) { }
    }
    public class InternalErrorException : EmbersException {
        public InternalErrorException(string Message) : base(Message) { }
    }
    public class RuntimeException : EmbersException {
        public RuntimeException(string Message) : base(Message) { }
    }
    public class ApiException : EmbersException {
        public ApiException(string Message) : base(Message) { }
    }

    public readonly struct DebugLocation {
        public readonly int Line;
        public readonly int Column;
        public DebugLocation(int line, int column) {
            Line = line;
            Column = column;
        }
        public override string ToString() {
            return $"{Line}:{Column}";
        }
    }
}
