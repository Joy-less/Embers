using System;

namespace Embers
{
    public static class Info {
        public const string Version = "2.0.0";
        public const string ReleaseDate = "2023-11-15";
        public const string Copyright = "Embers - Copyright © 2023 Joyless";
        public const string RubyCopyright = "Ruby - Copyright © 1995 Yukihiro Matsumoto";
    }

    public abstract class EmbersException : Exception {
        public EmbersException(string Message) : base(Message) { }
        public EmbersException() : base() {}
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
        public readonly bool IsUnknown = true;
        public DebugLocation(int line, int column) {
            Line = line;
            Column = column;
            IsUnknown = false;
        }
        public override string ToString() {
            return !IsUnknown ? $"{Line}:{Column}" : "?";
        }
        public string Serialise() {
            if (!IsUnknown)
                return $"new DebugLocation({Line}, {Column})";
            else
                return "new DebugLocation()";
        }
    }
}
