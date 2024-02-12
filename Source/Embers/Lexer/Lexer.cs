using System;
using System.Collections.Generic;
using System.Linq;

namespace Embers {
    internal static class Lexer {
        public static List<Token?> Analyse(CodeLocation Location, string Code) {
            List<Token?> Tokens = new();

            for (int i = 0; i < Code.Length; i++) {
                char Chara = Code[i];
                char? NextChara = i + 1 < Code.Length ? Code[i + 1] : null;
                char? NextNextChara = i + 2 < Code.Length ? Code[i + 2] : null;

                Location = new CodeLocation(Location.Axis, Location.Line, Location.Column + 1);

                bool WhitespaceBefore = i - 1 >= 0 && char.IsWhiteSpace(Code[i - 1]);
                bool WhitespaceAfter = i + 1 < Code.Length && char.IsWhiteSpace(Code[i + 1]);
                
                void AddToken(TokenType Type, string? Value = null, bool Formatted = false) {
                    Tokens.Add(new Token(Location, Type, Value, WhitespaceBefore, WhitespaceAfter, Formatted));
                }
                string BuildWhile(Func<int, bool> Condition) {
                    int Start = i;
                    while (i + 1 < Code.Length && Condition(i + 1)) {
                        i++;
                    }
                    return Code[Start..(i + 1)];
                }
                string BuildString() {
                    // Skip opening speech mark
                    char OpeningChara = Code[i];
                    i++;
                    // Build string
                    string String = BuildWhile(i => Code[i] != OpeningChara);
                    // Ensure closing speech mark is present
                    if (i + 1 >= Code.Length) {
                        throw new SyntaxError($"{Location}: unclosed string");
                    }
                    // Skip closing speech mark
                    i++;
                    // Process escape sequences
                    if (OpeningChara == '"') {
                        String = String.ProcessEscapeSequences();
                    }
                    // Return string
                    return String;
                }
                string BuildIdentifier() {
                    // Build identifier
                    string Identifier = BuildWhile(i => IsValidIdentifierChara(Code[i], (i - 1 >= 0) ? Code[i - 1] : null));
                    // Return identifier
                    return Identifier;
                }

                // Dot
                if (Chara is '.') {
                    if (NextChara is '.') {
                        // Exclusive Range
                        if (NextNextChara is '.') {
                            AddToken(TokenType.ExclusiveRange);
                            i += 2;
                        }
                        // Inclusive Range
                        else {
                            AddToken(TokenType.InclusiveRange);
                            i++;
                        }
                    }
                    // Dot
                    else {
                        AddToken(TokenType.Dot);
                    }
                }
                // Comma
                else if (Chara is ',') {
                    AddToken(TokenType.Comma);
                }
                // String
                else if (Chara is '"' or '\'') {
                    // Build string
                    string String = BuildString();
                    // Create string token
                    AddToken(TokenType.String, String, Formatted: Chara is '"');
                }
                // Open bracket
                else if (Chara is '(') {
                    AddToken(TokenType.OpenBracket);
                }
                // Close bracket
                else if (Chara is ')') {
                    AddToken(TokenType.CloseBracket);
                }
                // Open square bracket
                else if (Chara is '[') {
                    AddToken(TokenType.OpenSquareBracket);
                }
                // Close square bracket
                else if (Chara is ']') {
                    AddToken(TokenType.CloseSquareBracket);
                }
                // Open curly bracket
                else if (Chara is '{') {
                    AddToken(TokenType.OpenCurlyBracket);
                }
                // Close curly bracket
                else if (Chara is '}') {
                    AddToken(TokenType.CloseCurlyBracket);
                }
                // Comment
                else if (Chara is '#') {
                    // Build comment
                    BuildWhile(i => Code[i] is not ('\r' or '\n'));
                }
                // Multiline comment
                else if (i + 6 < Code.Length && Code[i..(i + 6)] is "=begin") {
                    // Pass =begin
                    i += "=begin".Length;
                    // Initialise variables
                    (int Line, int Column) = (Location.Line, Location.Column);
                    bool Found = false;
                    // Loop until =end reached
                    while (i + "=end".Length < Code.Length) {
                        // Found =end
                        if (Code[i..(i + "=end".Length)] is "=end") {
                            i += "=end".Length - 1;
                            Found = true;
                            break;
                        }
                        // Newline
                        else if (Code[i] is '\r' or '\n') {
                            // Treat \r\n as single newline
                            if (Code[i] is '\r' && Code[i + 1] is '\n') {
                                i++;
                            }
                            // Add newline
                            Line++;
                            Column = 0;
                        }
                        // Comment chara
                        else {
                            Column++;
                        }
                        // Next chara
                        i++;
                    }
                    // Error if =end not found
                    if (!Found) {
                        throw new SyntaxError($"{Location}: expected '=end' to close '=begin'");
                    }
                    // Set location
                    Location = new CodeLocation(Location.Axis, Line, Column);
                }
                // Semi-colon
                else if (Chara is ';') {
                    // Create end of statement token
                    if (Tokens.Count == 0 || Tokens[^1] is not null) {
                        Tokens.Add(null);
                    }
                }
                // Exclamation
                else if (Chara is '!') {
                    // Not equals
                    if (NextChara is '=') {
                        AddToken(TokenType.Operator, "!=");
                        i++;
                    }
                    // Not
                    else {
                        AddToken(TokenType.Not, "!");
                    }
                }
                // Equals
                else if (Chara is '=') {
                    if (NextChara is '=') {
                        // Case equals
                        if (NextNextChara is '=') {
                            AddToken(TokenType.Operator, "===");
                            i += 2;
                        }
                        // Comparison equals
                        else {
                            AddToken(TokenType.Operator, "==");
                            i++;
                        }
                    }
                    // Hash rocket
                    else if (NextChara is '>') {
                        AddToken(TokenType.HashRocket, "=>");
                        i++;
                    }
                    // Assignment equals
                    else {
                        AddToken(TokenType.AssignmentOperator, "=");
                    }
                }
                // Arithmetic
                else if (Chara is '+' or '/' or '%') {
                    // Compound assignment
                    if (NextChara is '=') {
                        AddToken(TokenType.AssignmentOperator, $"{Chara}=");
                        i++;
                    }
                    // Arithmetic
                    else {
                        AddToken(TokenType.Operator, $"{Chara}");
                    }
                }
                else if (Chara is '-') {
                    // Lambda
                    if (NextChara is '>') {
                        AddToken(TokenType.Lambda);
                        i++;
                    }
                    // Compound assignment
                    else if (NextChara is '=') {
                        AddToken(TokenType.AssignmentOperator, "-=");
                        i++;
                    }
                    // Subtract
                    else {
                        AddToken(TokenType.Operator, "-");
                    }
                }
                else if (Chara is '*') {
                    // Exponentiate
                    if (NextChara is '*') {
                        // Compound assignment
                        if (NextNextChara is '=') {
                            AddToken(TokenType.AssignmentOperator, "**=");
                            i += 2;
                        }
                        // Exponentiate
                        else {
                            AddToken(TokenType.Operator, "**");
                            i++;
                        }
                    }
                    // Compound assignment
                    else if (NextChara is '=') {
                        AddToken(TokenType.AssignmentOperator, "*=");
                        i++;
                    }
                    // Multiply
                    else {
                        AddToken(TokenType.Operator, "*");
                    }
                }
                // Less than
                else if (Chara is '<') {
                    if (NextChara is '=') {
                        // Spaceship
                        if (NextNextChara is '>') {
                            AddToken(TokenType.Operator, "<=>");
                            i += 2;
                        }
                        // Less than or equal to
                        else {
                            AddToken(TokenType.Operator, "<=");
                            i++;
                        }
                    }
                    // Append / Left shift
                    else if (NextChara is '<') {
                        AddToken(TokenType.Operator, "<<");
                        i++;
                    }
                    // Less than
                    else {
                        AddToken(TokenType.Operator, "<");
                    }
                }
                // Greater than
                else if (Chara is '>') {
                    // Greater than or equal to
                    if (NextChara is '=') {
                        AddToken(TokenType.Operator, ">=");
                        i++;
                    }
                    // Right shift
                    else if (NextChara is '>') {
                        AddToken(TokenType.Operator, ">>");
                        i++;
                    }
                    // Greater than
                    else {
                        AddToken(TokenType.Operator, ">");
                    }
                }
                // Ampersand
                else if (Chara is '&') {
                    // Safe dot
                    if (NextChara is '.') {
                        AddToken(TokenType.SafeDot, "&.");
                        i++;
                    }
                    // And
                    else if (NextChara is '&') {
                        AddToken(TokenType.LogicOperator, "&&");
                        i++;
                    }
                    // Bitwise and
                    else {
                        AddToken(TokenType.Operator, "&");
                    }
                }
                // Pipe
                else if (Chara is '|') {
                    // Or
                    if (NextChara is '|') {
                        AddToken(TokenType.LogicOperator, "||");
                        i++;
                    }
                    // Bitwise or
                    else {
                        AddToken(TokenType.Operator, "|");
                    }
                }
                // Ternary Truthy
                else if (Chara is '?') {
                    AddToken(TokenType.TernaryTruthy);
                }
                // Colon
                else if (Chara is ':') {
                    // Constant path
                    if (NextChara is ':') {
                        AddToken(TokenType.DoubleColon);
                        i++;
                    }
                    // Symbol
                    else if (NextChara is '"' or '\'') {
                        // Skip colon
                        i++;
                        // Build symbol
                        string Symbol = BuildString();
                        // Create string token
                        AddToken(TokenType.Symbol, Symbol);
                    }
                    else if (IsValidIdentifierChara(NextChara)) {
                        // Skip colon
                        i++;
                        // Build symbol
                        string Symbol = BuildIdentifier();
                        // Create string token
                        AddToken(TokenType.Symbol, Symbol);
                    }
                    // Ternary Falsey
                    else {
                        AddToken(TokenType.TernaryFalsey);
                    }
                }
                // Whitespace
                else if (char.IsWhiteSpace(Chara)) {
                    // New line
                    if (Chara is '\r' or '\n') {
                        Location = new CodeLocation(Location.Axis, Location.Line + 1, 0);
                        // Treat \r\n as single newline
                        if (Chara is '\r' && NextChara is '\n') {
                            i++;
                        }
                        // Create end of statement token
                        if (Tokens.LastOrDefault() is Token LastToken && !IsGreedy(LastToken.Type)) {
                            Tokens.Add(null);
                        }
                    }
                }
                // Integer
                else if (Chara.IsAsciiDigit()) {
                    // Build integer
                    string Integer = BuildWhile(i => Code[i].IsAsciiDigit() || Code[i] is '_');
                    // Remove underscores
                    if (Integer.EndsWith('_')) {
                        throw new SyntaxError($"{Location}: trailing '_' in number");
                    }
                    Integer = Integer.Replace("_", "");

                    // Float
                    if (Tokens.Count >= 2 && Tokens[^1]?.Type is TokenType.Dot && Tokens[^2] is Token FirstInteger && FirstInteger.Type is TokenType.Integer) {
                        // Build float from integers and dot
                        string Float = $"{FirstInteger.Value}.{Integer}";
                        // Remove integer token and dot token
                        Tokens.RemoveRange(Tokens.Count - 2, 2);
                        // Create float token
                        AddToken(TokenType.Float, Float);
                    }
                    // Integer
                    else {
                        // Create integer token
                        AddToken(TokenType.Integer, Integer);
                    }
                }
                // Number exponent (e.g. 5e3)
                else if (Chara is 'e' or 'E' && Tokens.LastOrDefault() is Token LastNumber && LastNumber.Type is TokenType.Integer or TokenType.Float && !LastNumber.WhitespaceAfter) {
                    // Get number and exponent
                    string Number = LastNumber.Value!;
                    string Exponent = "";
                    if (NextChara is '+' or '-') {
                        Exponent += NextChara;
                        i++;
                    }
                    i++; // Skip e
                    Exponent += BuildWhile(i => Code[i].IsAsciiDigit() || Code[i] is '_');

                    // Calculate number (number * 10^exponent)
                    Float Result = Float.Parse(Number) * Math.Pow(10, double.Parse(Exponent));

                    // Replace number with exponentiated number
                    Tokens[^1] = LastNumber.Type is TokenType.Integer
                        ? new Token(LastNumber.Location, TokenType.Integer, ((Integer)Result).ToString())
                        : new Token(LastNumber.Location, TokenType.Float, Result.ToPlainString());
                }
                // Identifier
                else {
                    // Invalid character
                    if (!IsValidIdentifierChara(Chara)) {
                        throw new SyntaxError($"{Location}: invalid '{Chara}'");
                    }

                    // Build identifier
                    string Identifier = BuildIdentifier();

                    // Create nil/true/false token
                    if (Identifier is "nil" or "true" or "false") {
                        AddToken(TokenType.NilTrueFalse, Identifier);
                    }
                    // Create not token
                    else if (Identifier is "not") {
                        AddToken(TokenType.Not, Identifier);
                    }
                    // Create and/or token
                    else if (Identifier is "and" or "or") {
                        AddToken(TokenType.LogicOperator, Identifier);
                    }
                    // Create global variable token
                    else if (Identifier.StartsWith('$')) {
                        AddToken(TokenType.GlobalVariable, Identifier);
                    }
                    // Create class variable token
                    else if (Identifier.StartsWith("@@")) {
                        AddToken(TokenType.ClassVariable, Identifier);
                    }
                    // Create instance variable token
                    else if (Identifier.StartsWith('@')) {
                        AddToken(TokenType.InstanceVariable, Identifier);
                    }
                    // Create identifier token
                    else {
                        AddToken(TokenType.Identifier, Identifier);
                    }
                }
            }

            return Tokens;
        }

        /// <summary>Returns <see langword="true"/> if a line break after the token should not break up an expression.</summary>
        private static bool IsGreedy(TokenType TokenType) {
            return TokenType is TokenType.OpenBracket or TokenType.OpenSquareBracket or TokenType.OpenCurlyBracket or TokenType.Comma or TokenType.Dot
                or TokenType.DoubleColon or TokenType.HashRocket or TokenType.Not or TokenType.AssignmentOperator or TokenType.Operator or TokenType.LogicOperator
                or TokenType.TernaryTruthy or TokenType.TernaryFalsey;
        }
        /// <summary>Returns <see langword="true"/> if the character is valid in an identifier.</summary>
        private static bool IsValidIdentifierChara(char? Chara, char? LastChara = null) {
            if (Chara is null || char.IsWhiteSpace(Chara.Value)) return false;
            if (Chara is '.' or ',' or '"' or '\'' or '(' or ')' or '[' or ']' or '{' or '}' or '#' or ';' or ':' or '=' or
                '+' or '-' or '/' or '%' or '*' or '<' or '>' or '&' or '|' or '\\') return false;
            if (Chara is '?' or '!') return IsValidIdentifierChara(LastChara);
            if (Chara is '$') return LastChara is null;
            return true;
        }
    }
}
