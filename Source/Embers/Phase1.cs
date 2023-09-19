using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace Embers
{
    public static class Phase1
    {
        public enum Phase1TokenType {
            Identifier,
            Integer,
            String,
            OpenBracket,
            CloseBracket,
            EndOfStatement,
            AssignmentOperator,
            Operator,
            Dot,
            DoubleColon,
            Comma,
            SplatOperator,
            Colon,
            StartCurly,
            EndCurly,
            StartSquare,
            EndSquare,
            Pipe,
            RightArrow,
            InclusiveRange,
            ExclusiveRange,
            TernaryQuestion,
            TernaryElse,
        }
        public static readonly Phase1TokenType[] OmitEndOfStatementAfterList = new Phase1TokenType[] {
            Phase1TokenType.OpenBracket, Phase1TokenType.AssignmentOperator, Phase1TokenType.Operator, Phase1TokenType.Dot, Phase1TokenType.DoubleColon, Phase1TokenType.Comma,
            Phase1TokenType.StartCurly, Phase1TokenType.StartSquare, Phase1TokenType.Pipe, Phase1TokenType.RightArrow, Phase1TokenType.TernaryQuestion, Phase1TokenType.TernaryElse
        };
        public class Phase1Token {
            public readonly DebugLocation Location;
            public Phase1TokenType Type;
            public string? Value;
            public readonly bool FollowsWhitespace;
            public readonly bool FollowedByWhitespace;
            public readonly bool ProcessFormatting;
            public Phase1Token(DebugLocation location, Phase1TokenType type, string? value, bool followsWhitespace, bool followedByWhitespace, bool processFormatting = false) {
                Location = location;
                Type = type;
                Value = value;
                FollowsWhitespace = followsWhitespace;
                FollowedByWhitespace = followedByWhitespace;
                ProcessFormatting = processFormatting;
            }
            public string NonNullValue {
                get { return Value ?? throw new InternalErrorException("Value was null"); }
            }
            public string Inspect() {
                return Type + (Value != null ? ":" : "") + Value?.Replace("\n", "\\n").Replace("\r", "\\r") + (FollowsWhitespace ? " (follows whitespace)" : "") + (FollowedByWhitespace ? " (followed by whitespace)" : "");
            }
            public string Serialise() {
                return $"new {typeof(Phase1Token).PathTo()}({Location.Serialise()}, {typeof(Phase1TokenType).PathTo()}.{Type}, \"{Value}\", {(FollowsWhitespace ? "true" : "false")}, {(FollowedByWhitespace ? "true" : "false")}, {(ProcessFormatting ? "true" : "false")})";
            }
        }

        static readonly IReadOnlyList<char> InvalidIdentifierCharacters = new List<char>() {'.', ',', '(', ')', '"', '\'', ';', ':',
            '=', '+', '-', '*', '/', '%', '#', '?', '!', '{', '}', '[', ']', '|', '^', '&', '~', '<', '>', '\\'};
        public static List<Phase1Token> GetPhase1Tokens(string Code) {
            Code += "\n";

            List<Phase1Token> Tokens = new();
            Stack<char> Brackets = new();

            int CurrentLine = 1;
            int IndexOfLastNewline = 0;
            
            for (int i = 0; i < Code.Length; i++) {
                // Functions
                string BuildWhile(Func<char, bool> While) {
                    return BuildUntil((c) => !While(c));
                }
                string BuildUntil(Func<char, bool> Until) {
                    string Build = "";
                    while (i < Code.Length) {
                        if (Until(Code[i]))
                            break;
                        Build += Code[i];
                        i++;
                    }
                    return Build;
                }
                static bool IsWhitespace(char Chara) {
                    // return Chara is ' ' or '\t';
                    return char.IsWhiteSpace(Chara);
                }
                bool LastTokenWas(Phase1TokenType Type) {
                    return Tokens.Count != 0 && Tokens[^1].Type == Type;
                }
                bool LastTokenWasAny(params Phase1TokenType[] Types) {
                    if (Tokens.Count != 0) {
                        Phase1TokenType LastTokenType = Tokens[^1].Type;
                        for (int i = 0; i < Types.Length; i++) {
                            if (LastTokenType == Types[i]) {
                                return true;
                            }
                        }
                    }
                    return false;
                }
                string EscapeString(string String) {
                    for (int i = 0; i < String.Length; i++) {
                        bool NextCharacterIs(char Character) {
                            return i + 1 < String.Length && String[i + 1] == Character;
                        }
                        static string ConvertOctalToChar(string? OctalDigits) {
                            return ((char)Convert.ToInt32(OctalDigits, 8)).ToString();
                        }
                        bool TryGetOctalCharacterEscape(out string? Characters) {
                            int OctalDigitCounter = 0;
                            for (int i2 = i + 1; i2 < String.Length; i2++) {
                                char CurrentChara = String[i2];
                                if ("01234567".Contains(CurrentChara)) {
                                    OctalDigitCounter++;
                                }
                                else {
                                    break;
                                }
                                if (OctalDigitCounter == 3) {
                                    Characters = String.Substring(i + 1, 3);
                                    return true;
                                }
                            }
                            Characters = null;
                            return false;
                        }
                        static string ConvertHexadecimalToChar(string? HexadecimalDigits) {
                            return ((char)Convert.ToInt32(HexadecimalDigits, 16)).ToString();
                        }
                        bool TryGetHexadecimalCharacterEscape(out string? Characters) {
                            int StartIndex = i + 1;
                            if (StartIndex < String.Length && (String[StartIndex] == 'u' || String[StartIndex] == 'x')) {
                                int ExpectDigitCount = String[StartIndex] == 'u' ? 4 : 2;
                                StartIndex++;

                                if (StartIndex + ExpectDigitCount - 1 < String.Length) {
                                    string HexDigits = String[StartIndex..(StartIndex + ExpectDigitCount)];
                                    if (HexDigits.All(c => "0123456789ABCDEF".Contains(char.ToUpper(c)))) {
                                        Characters = HexDigits;
                                        return true;
                                    }
                                    else {
                                        throw new SyntaxErrorException($"{CurrentLine}: Invalid escape (expected {ExpectDigitCount}-digit integer, got '{HexDigits}')");
                                    }
                                }
                                else {
                                    throw new SyntaxErrorException($"{CurrentLine}: Invalid escape (expected {ExpectDigitCount} digits, got {String.Length - StartIndex})");
                                }
                            }
                            Characters = null;
                            return false;
                        }
                        void RemoveAndInsert(int Count, string With) {
                            String = String.Remove(i, Count)
                                .Insert(i, With);
                            i += With.Length - 1;
                        }
                        void Remove(int Count) {
                            String = String.Remove(i, Count);
                        }

                        if (String[i] == '\\') {
                            if (NextCharacterIs('0')) RemoveAndInsert(2, "\0");
                            else if (NextCharacterIs('a')) RemoveAndInsert(2, "\a");
                            else if (NextCharacterIs('b')) RemoveAndInsert(2, "\b");
                            else if (NextCharacterIs('t')) RemoveAndInsert(2, "\t");
                            else if (NextCharacterIs('n')) RemoveAndInsert(2, "\n");
                            else if (NextCharacterIs('v')) RemoveAndInsert(2, "\v");
                            else if (NextCharacterIs('f')) RemoveAndInsert(2, "\f");
                            else if (NextCharacterIs('r')) RemoveAndInsert(2, "\r");
                            else if (NextCharacterIs('e')) RemoveAndInsert(2, "\x1B");
                            else if (NextCharacterIs('s')) RemoveAndInsert(2, " ");
                            else if (NextCharacterIs('\\')) RemoveAndInsert(2, "\\");
                            else if (NextCharacterIs('"')) RemoveAndInsert(2, "\"");
                            else if (NextCharacterIs('\'')) RemoveAndInsert(2, "\'");
                            else if (TryGetOctalCharacterEscape(out string? OctDigits)) {
                                // "\000"
                                RemoveAndInsert(1 + 3, ConvertOctalToChar(OctDigits));
                            }
                            else if (TryGetHexadecimalCharacterEscape(out string? HexDigits)) {
                                // "\x00", "\u0000"
                                RemoveAndInsert(1 + HexDigits!.Length + 1, ConvertHexadecimalToChar(HexDigits));
                            }
                            else Remove(1);
                        }
                    }
                    return String;
                }
                void RemoveEndOfStatement() {
                    if (LastTokenWas(Phase1TokenType.EndOfStatement)) {
                        Tokens.RemoveAt(Tokens.Count - 1);
                    }
                }
                bool IsDefMethodName() {
                    for (int i2 = Tokens.Count - 1; i2 >= 0; i2--) {
                        if (Tokens[i2].Type == Phase1TokenType.Identifier && Tokens[i2].Value == "def") {
                            return true;
                        }
                        else if (Tokens[i2].Type != Phase1TokenType.Identifier && Tokens[i2].Type != Phase1TokenType.Dot) {
                            break;
                        }
                    }
                    return false;
                }
                bool IsClassName() {
                    for (int i2 = Tokens.Count - 1; i2 >= 0; i2--) {
                        if (Tokens[i2].Type != Phase1TokenType.Identifier && Tokens[i2].Type != Phase1TokenType.DoubleColon) {
                            break;
                        }
                        else if (Tokens[i2].Type == Phase1TokenType.Identifier && Tokens[i2].Value == "class") {
                            return true;
                        }
                    }
                    return false;
                }
                bool IsDefStatement() {
                    for (int i2 = Tokens.Count - 1; i2 >= 0; i2--) {
                        if (Tokens[i2].Type == Phase1TokenType.EndOfStatement) {
                            break;
                        }
                        else if (Tokens[i2].Type == Phase1TokenType.Identifier && Tokens[i2].Value == "def") {
                            return true;
                        }
                    }
                    return false;
                }
                bool IsPipeStatement() {
                    // Start pipe
                    if ((LastTokenWas(Phase1TokenType.Identifier) && Tokens[^1].Value == "do") || LastTokenWas(Phase1TokenType.StartCurly)) {
                        return true;
                    }
                    // End pipe
                    else {
                        for (int i2 = Tokens.Count - 1; i2 >= 0; i2--) {
                            if (Tokens[i2].Type == Phase1TokenType.EndOfStatement) {
                                break;
                            }
                            else if (Tokens[i2].Type == Phase1TokenType.Pipe) {
                                return true;
                            }
                        }
                    }
                    return false;
                }
                bool IsTernaryStatement() {
                    for (int i2 = Tokens.Count - 1; i2 >= 0; i2--) {
                        if (Tokens[i2].Type == Phase1TokenType.EndOfStatement) {
                            break;
                        }
                        else if (Tokens[i2].Type == Phase1TokenType.TernaryQuestion) {
                            return true;
                        }
                    }
                    return false;
                }
                bool NextCharactersAre(string Characters) {
                    for (int i2 = 0; i2 < Characters.Length; i2++) {
                        if (i2 + i + 1 >= Code.Length || Code[i2 + i + 1] != Characters[i2]) {
                            return false;
                        }
                    }
                    return true;
                }
                static bool IsValidIdentifierCharacter(char Chara) {
                    if (InvalidIdentifierCharacters.Contains(Chara))
                        return false;
                    if (char.IsWhiteSpace(Chara)) return false;
                    return true;
                }

                // Process current character
                char Chara = Code[i];
                char? NextChara = i + 1 < Code.Length ? Code[i + 1] : null;
                char? NextNextChara = i + 2 < Code.Length ? Code[i + 2] : null;
                char? LastChara = i - 1 >= 0 ? Code[i - 1] : null;
                bool FollowsWhitespace = i - 1 >= 0 && IsWhitespace(Code[i - 1]);
                bool FollowedByWhitespace = i + 1 < Code.Length && IsWhitespace(Code[i + 1]);

                // Get debug location
                int CurrentColumn = i - IndexOfLastNewline;
                DebugLocation Location = new(CurrentLine, CurrentColumn);

                void AddToken(Phase1TokenType Type, string? Value) {
                    Tokens.Add(new(Location, Type, Value, FollowsWhitespace, FollowedByWhitespace));
                }

                // Integer
                if (Chara.IsAsciiDigit()) {
                    // Build integer
                    string Number = BuildWhile(c => c.IsAsciiDigit() || c is '_' or 'e' or 'E' or 'x' or 'X');
                    if (Number.EndsWith('e') || Number.EndsWith('E')) {
                        if (Code[i] is '+' or '-') {
                            Number += Code[i];
                            i++;
                            string Extra = BuildWhile(c => c.IsAsciiDigit());
                            if (Extra.Length == 0) {
                                throw new SyntaxErrorException($"{Location}: Trailing '{Number.Last()}' in number");
                            }
                            Number += Extra;
                        }
                    }
                    // Remove '_'
                    if (Number.EndsWith('_')) throw new SyntaxErrorException($"{Location}: Trailing '_' in number");
                    Number = Number.Replace("_", "");
                    // Hexadecimal notation
                    if (Number.StartsWith("0x")) {
                        if (Number.Length == 2) throw new SyntaxErrorException($"{Location}: '0x' is not a valid number");
                        Number = Number[2..].ParseHexLong().ToString();
                    }
                    // Add integer to tokens
                    Tokens.Add(new(Location, Phase1TokenType.Integer, Number, FollowsWhitespace, FollowedByWhitespace));
                    i--;
                }
                // Special character
                else {
                    switch (Chara) {
                        case '.':
                            RemoveEndOfStatement();
                            if (NextChara == '.') {
                                if (NextNextChara == '.') {
                                    Tokens.Add(new(Location, Phase1TokenType.ExclusiveRange, "...", FollowsWhitespace, FollowedByWhitespace));
                                    i += 2;
                                }
                                else {
                                    AddToken(Phase1TokenType.InclusiveRange, "..");
                                    i++;
                                }
                            }
                            else {
                                AddToken(Phase1TokenType.Dot, ".");
                            }
                            break;
                        case ',':
                            RemoveEndOfStatement();
                            AddToken(Phase1TokenType.Comma, ",");
                            break;
                        case '(':
                            AddToken(Phase1TokenType.OpenBracket, "(");
                            Brackets.Push('(');
                            break;
                        case ')':
                            RemoveEndOfStatement();
                            AddToken(Phase1TokenType.CloseBracket, ")");
                            // Handle unexpected close bracket
                            if (Brackets.TryPop(out char Opener) == false || Opener != '(')
                                throw new SyntaxErrorException($"{Location}: Unexpected close bracket: )");
                            // Add EndOfStatement after def method name
                            if (IsDefMethodName())
                                AddToken(Phase1TokenType.EndOfStatement, null);
                            break;
                        case '"':
                        case '\'':
                            i++;
                            char? LastC = null;
                            string String;
                            // String that can be formatted/escaped
                            if (Chara == '"') {
                                bool ProcessFormatting = false;
                                int FormatDepth = 0;
                                String = BuildUntil(C => {
                                    if (LastC == '\\') {
                                        LastC = C;
                                        return false;
                                    }
                                    else if (LastC == '#' && C == '{') {
                                        FormatDepth++;
                                        ProcessFormatting = true;
                                    }
                                    else if (C == '}') {
                                        if (FormatDepth != 0)
                                            FormatDepth--;
                                    }
                                    LastC = C;
                                    return FormatDepth == 0 && C == Chara;
                                });
                                String = EscapeString(String);
                                Tokens.Add(new(Location, Phase1TokenType.String, String, FollowsWhitespace, FollowedByWhitespace, processFormatting: ProcessFormatting));
                            }
                            // String that cannot be formatted/escaped
                            else {
                                String = BuildUntil(C => {
                                    if (LastC == '\\') {
                                        LastC = C;
                                        return false;
                                    }
                                    LastC = C;
                                    return C == Chara;
                                }).Replace("\\'", "'");
                                Tokens.Add(new(Location, Phase1TokenType.String, String, FollowsWhitespace, false));
                            }
                            // Symbol string
                            if (Tokens.Count >= 2 && Tokens[^2].Type == Phase1TokenType.Colon) {
                                Phase1Token SymbolToken = Tokens[^2];
                                Phase1Token StringToken = Tokens[^1];
                                if (!StringToken.FollowsWhitespace) {
                                    Tokens.RemoveRange(Tokens.Count - 2, 2);
                                    Tokens.Add(new Phase1Token(Location, Phase1TokenType.Identifier, ":" + StringToken.Value, SymbolToken.FollowsWhitespace, StringToken.ProcessFormatting));
                                }
                            }
                            break;
                        case '\n':
                        case '\r':
                            if (LastTokenWasAny(Phase1TokenType.Operator, Phase1TokenType.AssignmentOperator, Phase1TokenType.Comma, Phase1TokenType.Dot)
                                || Brackets.Count != 0)
                            {
                                break;
                            }
                            goto case ';';
                        case ';':
                            // Add EndOfStatement if there isn't already one
                            if (!LastTokenWas(Phase1TokenType.EndOfStatement) && !LastTokenWasAny(OmitEndOfStatementAfterList))
                                AddToken(Phase1TokenType.EndOfStatement, Chara.ToString());
                            // \r + \n --> \r\n
                            else if (Chara == '\n' && Tokens[^1].Value == "\r")
                                Tokens[^1].Value += "\n";
                            
                            // Increment line if \n
                            if (Chara == '\n') {
                                CurrentLine++;
                                IndexOfLastNewline = i;
                            }
                            break;
                        case ':':
                            if (NextChara == ':') {
                                AddToken(Phase1TokenType.DoubleColon, "::");
                                i++;
                            }
                            else if (Tokens.Count >= 2 && IsTernaryStatement()) {
                                RemoveEndOfStatement();
                                AddToken(Phase1TokenType.TernaryElse, ":");
                            }
                            else {
                                AddToken(Phase1TokenType.Colon, ":");
                            }
                            break;
                        case '=':
                            RemoveEndOfStatement();
                            if (NextChara == '=') {
                                if (NextNextChara == '=') {
                                    AddToken(Phase1TokenType.Operator, "===");
                                    i += 2;
                                }
                                else {
                                    AddToken(Phase1TokenType.Operator, "==");
                                    i++;
                                }
                            }
                            else if (NextChara == '>') {
                                AddToken(Phase1TokenType.RightArrow, "=>");
                                i++;
                            }
                            else if (NextCharactersAre("begin") && LastChara is null or '\n' or '\r') {
                                i += 5;
                                while (i < Code.Length) {
                                    i++;
                                    if (NextCharactersAre("\n=end") || NextCharactersAre("\r=end")) {
                                        i += 4 + 1;
                                        break;
                                    }
                                }
                            }
                            else {
                                AddToken(Phase1TokenType.AssignmentOperator, "=");
                            }
                            break;
                        case '+':
                            RemoveEndOfStatement();
                            if (NextChara == '=') {
                                AddToken(Phase1TokenType.AssignmentOperator, "+=");
                                i++;
                            }
                            else {
                                AddToken(Phase1TokenType.Operator, "+");
                            }
                            break;
                        case '-':
                            RemoveEndOfStatement();
                            if (NextChara == '=') {
                                AddToken(Phase1TokenType.AssignmentOperator, "-=");
                                i++;
                            }
                            else {
                                AddToken(Phase1TokenType.Operator, "-");
                            }
                            break;
                        case '*':
                            if (IsDefStatement()) {
                                if (NextChara == '*') {
                                    AddToken(Phase1TokenType.SplatOperator, "**");
                                    i++;
                                }
                                else
                                    AddToken(Phase1TokenType.SplatOperator, "*");
                            }
                            else {
                                RemoveEndOfStatement();
                                if (NextChara == '*') {
                                    if (NextNextChara == '=') {
                                        AddToken(Phase1TokenType.AssignmentOperator, "**=");
                                        i += 2;
                                    }
                                    else {
                                        AddToken(Phase1TokenType.Operator, "**");
                                        i++;
                                    }
                                }
                                else if (NextChara == '=') {
                                    AddToken(Phase1TokenType.AssignmentOperator, "*=");
                                    i++;
                                }
                                else {
                                    AddToken(Phase1TokenType.Operator, "*");
                                }
                            }
                            break;
                        case '/':
                            RemoveEndOfStatement();
                            if (NextChara == '=') {
                                AddToken(Phase1TokenType.AssignmentOperator, "/=");
                                i++;
                            }
                            else {
                                AddToken(Phase1TokenType.Operator, "/");
                            }
                            break;
                        case '%':
                            RemoveEndOfStatement();
                            if (NextChara == '=') {
                                AddToken(Phase1TokenType.AssignmentOperator, "%=");
                                i++;
                            }
                            else {
                                AddToken(Phase1TokenType.Operator, "%");
                            }
                            break;
                        case '>':
                            RemoveEndOfStatement();
                            if (NextChara == '=') {
                                AddToken(Phase1TokenType.Operator, ">=");
                                i++;
                            }
                            else {
                                AddToken(Phase1TokenType.Operator, ">");
                            }
                            break;
                        case '<':
                            RemoveEndOfStatement();
                            if (NextChara == '=') {
                                if (NextNextChara == '>') {
                                    AddToken(Phase1TokenType.Operator, "<=>");
                                    i += 2;
                                }
                                else {
                                    AddToken(Phase1TokenType.Operator, "<=");
                                    i++;
                                }
                            }
                            else if (NextChara == '<') {
                                AddToken(Phase1TokenType.Operator, "<<");
                                i++;
                            }
                            else {
                                AddToken(Phase1TokenType.Operator, "<");
                            }
                            break;
                        case '&':
                            RemoveEndOfStatement();
                            if (NextChara == '&') {
                                AddToken(Phase1TokenType.Operator, "&&");
                                i++;
                            }
                            else {
                                AddToken(Phase1TokenType.Operator, "&");
                            }
                            break;
                        case '|':
                            RemoveEndOfStatement();
                            if (NextChara == '|') {
                                AddToken(Phase1TokenType.Operator, "||");
                                i++;
                            }
                            else if (IsPipeStatement()) {
                                AddToken(Phase1TokenType.Pipe, "|");
                            }
                            else {
                                AddToken(Phase1TokenType.Operator, "|");
                            }
                            break;
                        case '^':
                            RemoveEndOfStatement();
                            AddToken(Phase1TokenType.Operator, "^");
                            break;
                        case '#':
                            do {
                                i++;
                            } while (Code[i] != '\n' && Code[i] != '\r');
                            break;
                        case '!':
                            if (NextChara == '=') {
                                AddToken(Phase1TokenType.Operator, "!=");
                                i++;
                            }
                            else {
                                AddToken(Phase1TokenType.Operator, "!");
                            }
                            break;
                        case '{':
                            AddToken(Phase1TokenType.StartCurly, "{");
                            break;
                        case '}':
                            // Add EndOfStatement before }
                            if (!LastTokenWas(Phase1TokenType.EndOfStatement))
                                AddToken(Phase1TokenType.EndOfStatement, null);
                            // Add end curly bracket
                            AddToken(Phase1TokenType.EndCurly, "}");
                            break;
                        case '[':
                            AddToken(Phase1TokenType.StartSquare, "[");
                            break;
                        case ']':
                            RemoveEndOfStatement();
                            AddToken(Phase1TokenType.EndSquare, "]");
                            break;
                        case '\\':
                            throw new SyntaxErrorException($"{Location}: Unexpected '\\'");
                        case '?':
                            RemoveEndOfStatement();
                            AddToken(Phase1TokenType.TernaryQuestion, "?");
                            break;
                        default:
                            // Skip whitespace
                            if (char.IsWhiteSpace(Chara)) {
                                // Add EndOfStatement after class statement
                                if (!(LastTokenWas(Phase1TokenType.Identifier) && Tokens[^1].Value == "class") && IsClassName())
                                    AddToken(Phase1TokenType.EndOfStatement, null);
                                break;
                            }
                            // Build identifier
                            string Identifier = BuildWhile(IsValidIdentifierCharacter);
                            // Add exclamation mark
                            if (Code[i] is '?' or '!') {
                                Identifier += Code[i];
                                i++;
                            }
                            // Double check identifier
                            if (Identifier.Length == 0)
                                throw new InternalErrorException($"{Location}: Character not handled correctly: '{Chara}'");
                            // Handle symbol
                            if (LastTokenWas(Phase1TokenType.Colon) && !FollowsWhitespace) {
                                Tokens.RemoveAt(Tokens.Count - 1);
                                Identifier = ":" + Identifier;
                            }
                            // Add EndOfStatement before end keyword
                            if (Identifier == "end" && !LastTokenWas(Phase1TokenType.EndOfStatement))
                                AddToken(Phase1TokenType.EndOfStatement, null);
                            // Add identifier
                            AddToken(Phase1TokenType.Identifier, Identifier);
                            // Add EndOfStatement after else keyword
                            if (Identifier == "else")
                                AddToken(Phase1TokenType.EndOfStatement, null);
                            //
                            i--;
                            break;
                    }
                }
            }
            // Parse "or", "and" and "not" as operators
            for (int i = 0; i < Tokens.Count; i++) {
                Phase1Token Token = Tokens[i];
                if (Token.Type == Phase1TokenType.Identifier) {
                    if (Token.Value is "or" or "and" or "not") {
                        Token.Type = Phase1TokenType.Operator;
                    }
                }
            }
            return Tokens;
        }
    }
}
