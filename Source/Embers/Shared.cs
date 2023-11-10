using System;
using static Embers.Script;

namespace Embers
{
    public static class Info {
        public const string Version = "1.7.8";
        public const string ReleaseDate = "2023-11-10";
        public const string Copyright = "Embers - Copyright © 2023 Joyless";
        public const string RubyCopyright = "Ruby - Copyright © 1995 Yukihiro Matsumoto";
    }

    public abstract class EmbersException : Exception {
        public EmbersException(string Message) : base(Message) { }
        public EmbersException() { }
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
    public class ThrowException : Exception {
        public readonly string Identifier;
        public static ThrowException New(Instance Identifier) {
            string Message = $"uncaught throw {Identifier.Inspect()}";
            return new ThrowException(Message, Identifier.String);
        }
        private ThrowException(string Message, string identifier) : base(Message) {
            Identifier = identifier;
        }
    }

    public readonly struct DebugLocation {
        public readonly int Line;
        public readonly int Column;
        public readonly bool IsUnknown;
        public DebugLocation(int line, int column, bool isUnknown = false) {
            Line = line;
            Column = column;
            IsUnknown = isUnknown;
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
        public static readonly DebugLocation Unknown = new(-1, 0);
    }
}
