using System;

namespace Embers {
    /// <summary>An error raised by an Embers program.</summary>
    public abstract class EmbersError : Exception {
        public EmbersError(string message) : base(message) { }
        public EmbersError() : base() { }
    }
    /// <summary>A code structure error that inhibits parsing.</summary>
    public class SyntaxError : EmbersError {
        public SyntaxError(string message) : base(message) { }
        public SyntaxError() { }
    }
    /// <summary>A logic error that prevents program execution.</summary>
    public class RuntimeError : EmbersError {
        public RuntimeError(string message) : base(message) { }
        public RuntimeError() { }
    }
    /// <summary>A logic error within Embers itself.</summary>
    public class InternalError : EmbersError {
        public InternalError(string message) : base(message) { }
        public InternalError() { }
    }
    /// <summary>An error from interacting with Embers incorrectly.</summary>
    public class InteropError : EmbersError {
        public InteropError(string message) : base(message) { }
        public InteropError() { }
    }
    /// <summary>An exception intended for control flow that can be caught with catch but not rescue.</summary>
    public class ThrowError : EmbersError {
        public readonly Instance Identifier;
        public readonly Instance? Argument;
        public ThrowError(Instance identifier, Instance? argument = null) : base($"uncaught throw {identifier.Inspect()}") {
            Identifier = identifier;
            Argument = argument;
        }
    }
}
