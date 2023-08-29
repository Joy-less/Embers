namespace Embers
{
    public static class Phase1
    {
        static readonly string EndKeyword = Phase2.Phase2TokenType.End.ToString().ToLower();
        static readonly string DefKeyword = Phase2.Phase2TokenType.Def.ToString().ToLower();
        static readonly string ClassKeyword = Phase2.Phase2TokenType.Class.ToString().ToLower();

        public enum Phase1TokenType {
            Identifier,
            Integer,
            String,
            OpenBracket,
            CloseBracket,
            EndOfStatement,
            AssignmentOperator,
            ArithmeticOperator,
            Dot,
            DoubleColon,
            Comma,
            SplatOperator,
        }
        public class Phase1Token {
            public DebugLocation Location;
            public readonly Phase1TokenType Type;
            public string? Value;
            public readonly bool FollowsWhitespace;
            public Phase1Token(DebugLocation location, Phase1TokenType type, string? value, bool followsWhitespace) {
                Location = location;
                Type = type;
                Value = value;
                FollowsWhitespace = followsWhitespace;
            }
            public string NonNullValue {
                get { return Value ?? throw new InternalErrorException("Value was null"); }
            }
            public string Inspect() {
                return $"{Type}{(Value != null ? ":" : "")}{Value?.Replace("\n", "\\n")} {(FollowsWhitespace ? "(true)" : "")}";
            }
        }

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
                    return Chara == ' ' || Chara == '\t';
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
                static string EscapeString(string String) {
                    for (int i = 0; i < String.Length; i++) {
                        /*bool NextCharactersAre(string Characters) {
                            int Offset = 0;
                            for (int i2 = i + 1; i2 < String.Length; i2++) {
                                if (Offset == Characters.Length)
                                    return true;
                                if (String[i2] != Characters[Offset])
                                    return false;
                                Offset++;
                            }
                            return Offset == Characters.Length;
                        }*/
                        bool NextCharacterIs(char Character) {
                            return i + 1 < String.Length && String[i + 1] == Character;
                        }
                        bool NextThreeCharactersAreOctal(out string? Characters) {
                            int OctalDigitCounter = 0;
                            for (int i2 = i + 1; i2 < String.Length; i2++) {
                                char CurrentChara = String[i2];
                                if (CurrentChara >= '0' || CurrentChara <= '7') {
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
                        string ConvertOctalToChar(string? OctalDigits) {
                            return ((char)Convert.ToInt32(OctalDigits, 8)).ToString();
                        }
                        string ConvertHexadecimalToChar(string? HexadecimalDigits) {
                            return ((char)Convert.ToInt32(HexadecimalDigits, 16)).ToString();
                        }
                        bool NextThreeCharactersAreHexadecimal(out string? Characters) {
                            int HexadecimalDigitCounter = 0;
                            for (int i2 = i + 1; i2 < String.Length; i2++) {
                                char CurrentChara = String[i2];
                                if (HexadecimalDigitCounter == 0) {
                                    if (CurrentChara == 'x') {
                                        HexadecimalDigitCounter++;
                                    }
                                    else {
                                        break;
                                    }
                                }
                                else {
                                    if ((CurrentChara >= '0' && CurrentChara <= '9') || (CurrentChara >= 'A' && CurrentChara <= 'F') || (CurrentChara >= 'a' && CurrentChara <= 'f')) {
                                        HexadecimalDigitCounter++;
                                    }
                                    else {
                                        break;
                                    }
                                }
                                if (HexadecimalDigitCounter == 3) {
                                    Characters = String.Substring(i + 2, 2);
                                    return true;
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
                            else if (NextThreeCharactersAreOctal(out string? OctDigits)) {
                                RemoveAndInsert(4, ConvertOctalToChar(OctDigits));
                            }
                            else if (NextThreeCharactersAreHexadecimal(out string? HexDigits)) {
                                RemoveAndInsert(4, ConvertHexadecimalToChar(HexDigits));
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
                        if (Tokens[i2].Type == Phase1TokenType.Identifier && Tokens[i2].Value == DefKeyword) {
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
                        else if (Tokens[i2].Type == Phase1TokenType.Identifier && Tokens[i2].Value == ClassKeyword) {
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
                        else if (Tokens[i2].Type == Phase1TokenType.Identifier && Tokens[i2].Value == DefKeyword) {
                            return true;
                        }
                    }
                    return false;
                }
                static bool IsValidIdentifierCharacter(char Chara) {
                    if (Chara == '.' || Chara == ',' || Chara == '(' || Chara == ')' || Chara == '"' || Chara == '\'' || Chara == '\n' || Chara == ';'
                        || Chara == '=' || Chara == '+' || Chara == '-' || Chara == '*' || Chara == '/' || Chara == '%' || Chara == '#' || Chara == '?')
                        return false;
                    if (char.IsWhiteSpace(Chara)) return false;
                    return true;
                }

                // Process current character
                char Chara = Code[i];
                char? NextChara = i + 1 < Code.Length ? Code[i + 1] : null;
                bool FollowsWhitespace = i - 1 >= 0 && IsWhitespace(Code[i - 1]);

                // Get debug location
                int CurrentColumn = i - IndexOfLastNewline;
                DebugLocation Location = new(CurrentLine, CurrentColumn);

                // Integer
                if (char.IsAsciiDigit(Chara)) {
                    string Number = BuildWhile(char.IsAsciiDigit);
                    Tokens.Add(new(Location, Phase1TokenType.Integer, Number, FollowsWhitespace));
                    i--;
                }
                // Special character
                else {
                    switch (Chara) {
                        case '.':
                            RemoveEndOfStatement();
                            Tokens.Add(new(Location, Phase1TokenType.Dot, null, FollowsWhitespace));
                            break;
                        case ',':
                            RemoveEndOfStatement();
                            Tokens.Add(new(Location, Phase1TokenType.Comma, null, FollowsWhitespace));
                            break;
                        case '(':
                            Tokens.Add(new(Location, Phase1TokenType.OpenBracket, null, FollowsWhitespace));
                            Brackets.Push('(');
                            break;
                        case ')':
                            RemoveEndOfStatement();
                            Tokens.Add(new(Location, Phase1TokenType.CloseBracket, null, FollowsWhitespace));
                            // Handle unexpected close bracket
                            if (Brackets.TryPop(out char Opener) == false || Opener != '(')
                                throw new SyntaxErrorException($"{Location}: Unexpected close bracket: )");
                            // Add EndOfStatement after def method name
                            if (IsDefMethodName())
                                Tokens.Add(new(Location, Phase1TokenType.EndOfStatement, null, FollowsWhitespace));
                            break;
                        case '"':
                        case '\'':
                            i++;
                            char? lastc = null;
                            string String = BuildUntil(c => {
                                if (lastc == '\\')
                                    return false;
                                lastc = c;
                                return c == Chara;
                            });
                            String = EscapeString(String);
                            Tokens.Add(new(Location, Phase1TokenType.String, String, FollowsWhitespace));
                            break;
                        case '\n':
                        case '\r':
                            if (LastTokenWasAny(Phase1TokenType.ArithmeticOperator, Phase1TokenType.AssignmentOperator, Phase1TokenType.Comma, Phase1TokenType.Dot)
                                || Brackets.Count != 0)
                            {
                                break;
                            }
                            goto case ';';
                        case ':':
                            if (NextChara == ':') {
                                i++;
                                Tokens.Add(new(Location, Phase1TokenType.DoubleColon, null, FollowsWhitespace));
                            }
                            else {
                                throw new NotImplementedException();
                            }
                            break;
                        case ';':
                            if (!LastTokenWas(Phase1TokenType.EndOfStatement))
                                Tokens.Add(new(Location, Phase1TokenType.EndOfStatement, Chara.ToString(), FollowsWhitespace));
                            if (Chara == '\n') {
                                CurrentLine++;
                                IndexOfLastNewline = i;
                            }
                            break;
                        case '=':
                            RemoveEndOfStatement();
                            Tokens.Add(new(Location, Phase1TokenType.AssignmentOperator, "=", FollowsWhitespace));
                            break;
                        case '+':
                            RemoveEndOfStatement();
                            Tokens.Add(new(Location, Phase1TokenType.ArithmeticOperator, "+", FollowsWhitespace));
                            break;
                        case '-':
                            RemoveEndOfStatement();
                            Tokens.Add(new(Location, Phase1TokenType.ArithmeticOperator, "-", FollowsWhitespace));
                            break;
                        case '*':
                            if (IsDefStatement()) {
                                if (NextChara == '*') {
                                    Tokens.Add(new(Location, Phase1TokenType.SplatOperator, "**", FollowsWhitespace));
                                    i++;
                                }
                                else
                                    Tokens.Add(new(Location, Phase1TokenType.SplatOperator, "*", FollowsWhitespace));
                            }
                            else {
                                RemoveEndOfStatement();
                                if (NextChara == '*') {
                                    Tokens.Add(new(Location, Phase1TokenType.ArithmeticOperator, "**", FollowsWhitespace));
                                    i++;
                                }
                                else
                                    Tokens.Add(new(Location, Phase1TokenType.ArithmeticOperator, "*", FollowsWhitespace));
                            }
                            break;
                        case '/':
                            RemoveEndOfStatement();
                            Tokens.Add(new(Location, Phase1TokenType.ArithmeticOperator, "/", FollowsWhitespace));
                            break;
                        case '%':
                            RemoveEndOfStatement();
                            Tokens.Add(new(Location, Phase1TokenType.ArithmeticOperator, "%", FollowsWhitespace));
                            break;
                        case '#':
                            do {
                                i++;
                            } while (Code[i] != '\n');
                            break;
                        case '?':
                            if (Tokens.Count != 0 && Tokens[^1].Type == Phase1TokenType.Identifier && !Tokens[^1].Value!.EndsWith('?') && !FollowsWhitespace) {
                                Tokens[^1].Value += '?';
                                continue;
                            }
                            else {
                                throw new SyntaxErrorException($"{Location}: '?' is only valid at the end of a method name identifier");
                            }
                        default:
                            // Skip whitespace
                            if (char.IsWhiteSpace(Chara)) {
                                // Add EndOfStatement after class statement
                                if (!(LastTokenWas(Phase1TokenType.Identifier) && Tokens[^1].Value == ClassKeyword) && IsClassName())
                                    Tokens.Add(new(Location, Phase1TokenType.EndOfStatement, null, FollowsWhitespace));
                                break;
                            }
                            // Build identifier
                            string Identifier = BuildWhile(IsValidIdentifierCharacter);
                            // Double check identifier
                            if (Identifier.Length == 0)
                                throw new InternalErrorException($"{Location}: Character not handled correctly: '{Chara}'");
                            // Add EndOfStatement before end keyword
                            if (Identifier == EndKeyword && !LastTokenWas(Phase1TokenType.EndOfStatement))
                                Tokens.Add(new(Location, Phase1TokenType.EndOfStatement, null, FollowsWhitespace));
                            // Add identifier
                            Tokens.Add(new(Location, Phase1TokenType.Identifier, Identifier, FollowsWhitespace));
                            i--;
                            break;
                    }
                }
            }

            return Tokens;
        }
    }
}
