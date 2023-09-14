using System;
using static Embers.Script;

namespace Embers
{
    public static class Info {
        public const string Version = "1.0.0";
        public const string ReleaseDate = "2023-09-14";
        public const string Copyright = "Embers - Copyright © 2023 Joyless";
        public const string RubyCopyright = "Ruby - Copyright © Yukihiro Matsumoto";
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
    public class ThrowException : EmbersException {
        public readonly string Identifier;
        public static ThrowException New(Instance Identifier) {
            string Message = $"uncaught throw {Identifier.Inspect()}";
            return new ThrowException(Message, Identifier.String);
        }
        private ThrowException(string Message, string identifier) : base(Message) {
            Identifier = identifier;
        }
    }
    public abstract class NonErrorException : EmbersException { }
    public abstract class LoopControlException : NonErrorException { }
    public class BreakException : LoopControlException { }
    public class RetryException : LoopControlException { }
    public class RedoException : LoopControlException { }
    public class NextException : LoopControlException { }
    public class ReturnException : NonErrorException {
        public readonly Instance Instance;
        public ReturnException(Instance instance) {
            Instance = instance;
        }
    }
    public class ExitException : NonErrorException { }
    public class StopException : NonErrorException { }

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
        public static readonly DebugLocation Unknown = new(-1, 0);
    }
}
