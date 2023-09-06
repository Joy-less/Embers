namespace Embers
{
    public abstract class EmbersException : Exception {
        public EmbersException(string Message) : base(Message) { }
        public EmbersException() : base() { }
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
    public class ThrowException : EmbersException {
        public readonly string Identifier;
        public static ThrowException New(Script.Instance Identifier) {
            string Message = $"uncaught throw {Identifier.Inspect()}";
            return new ThrowException(Message, Identifier.String);
        }
        private ThrowException(string Message, string identifier) : base(Message) {
            Identifier = identifier;
        }
    }
    public class BreakException : EmbersException {
        public BreakException() : base() { }
    }
    public class ReturnException : EmbersException {
        public readonly Script.Instances Instances;
        public ReturnException(Script.Instances instances) : base() {
            Instances = instances;
        }
    }

    public readonly struct DebugLocation {
        public readonly int Line;
        public readonly int Column;
        public readonly bool IsUnknown;
        public DebugLocation(int line, int column) {
            Line = line;
            Column = column;
            IsUnknown = false;
        }
        public DebugLocation() {
            Line = -1;
            Column = 0;
            IsUnknown = true;
        }
        public override string ToString() {
            if (!IsUnknown) {
                return $"{Line}:{Column}";
            }
            else {
                return "?";
            }
        }
        public string Serialise() {
            if (!IsUnknown)
                return $"new DebugLocation({Line}, {Column})";
            else
                return "new DebugLocation()";
        }
        public static readonly DebugLocation Unknown = new();
    }
}
