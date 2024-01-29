namespace Embers {
    public enum TokenType {
        Identifier,
        GlobalVariable,
        ClassVariable,
        InstanceVariable,
        NilTrueFalse,
        String,
        Symbol,
        Integer,
        Float,
        OpenBracket,
        CloseBracket,
        OpenSquareBracket,
        CloseSquareBracket,
        OpenCurlyBracket,
        CloseCurlyBracket,
        Comma,
        Dot,
        DoubleColon,
        Colon,
        TernaryTruthy,
        TernaryFalsey,
        HashRocket,
        SafeDot,
        InclusiveRange,
        ExclusiveRange,
        Not, // e.g. (not, !)
        Operator, // e.g. (+, -, >, ==)
        AssignmentOperator, // e.g. (=, +=)
        LogicOperator, // e.g. (and, &&)
    }
    public sealed class Token : RubyObject {
        public readonly TokenType Type;
        public readonly string? Value;
        public readonly bool WhitespaceBefore;
        public readonly bool WhitespaceAfter;
        public readonly bool Formatted;
        public Token(CodeLocation location, TokenType type, string? value = null, bool whitespace_before = false, bool whitespace_after = false, bool formatted = false) : base(location) {
            Type = type;
            Value = value;
            WhitespaceBefore = whitespace_before;
            WhitespaceAfter = whitespace_after;
            Formatted = formatted;
        }
        public override string ToString()
            => Value is not null ? $"{Type}:{Value}" : $"{Type}";
        public bool IsTokenLiteral
            => Type is TokenType.NilTrueFalse or TokenType.String or TokenType.Symbol or TokenType.Integer or TokenType.Float;
        public string? AsIdentifier
            => Type is TokenType.Identifier ? Value : null;
    }
}
