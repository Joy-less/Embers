using System;
using System.Collections.Generic;
using System.Linq;

namespace Embers {
    internal static class Parser {
        public static Expression[] ParseNullSeparatedExpressions(CodeLocation Location, List<RubyObject?> Objects) {
            return ParseExpressions(Location, Objects, Objects => Objects.Split(Object => Object is null, RemoveEmptyEntries: true));
        }
        public static Expression[] ParseCommaSeparatedExpressions(CodeLocation Location, List<RubyObject?> Objects) {
            return ParseExpressions(Location, Objects, Objects => Objects.Split(Object => Object is Token Token && Token.Type is TokenType.Comma));
        }
        static Expression[] ParseExpressions(CodeLocation Location, List<RubyObject?> Objects, Func<List<RubyObject?>, List<List<RubyObject?>>> Split) {
            // Parse general code structure
            ParseGeneralStructure(Objects);
            // Split objects
            List<List<RubyObject?>> SeparatedObjectsList = Split(Objects);
            Expression[] Expressions = new Expression[SeparatedObjectsList.Count];
            // Parse each expression
            for (int i = 0; i < Expressions.Length; i++) {
                Expressions[i] = ParseExpression(Location, SeparatedObjectsList[i]);
            }
            return Expressions;
        }
        static void ParseGeneralStructure(List<RubyObject?> Objects) {
            // Brackets
            ParseBrackets(Objects, TokenType.OpenBracket, TokenType.CloseBracket, (Location, Objects, StartToken)
                => new TemporaryBracketsExpression(Location, Objects, StartToken.WhitespaceBefore));

            // Square brackets
            ParseBrackets(Objects, TokenType.OpenSquareBracket, TokenType.CloseSquareBracket, (Location, Objects, StartToken)
                => new TemporarySquareBracketsExpression(Location, Objects, StartToken.WhitespaceBefore));

            // Curly brackets
            ParseBrackets(Objects, TokenType.OpenCurlyBracket, TokenType.CloseCurlyBracket, (Location, Objects, StartToken)
                => new TemporaryCurlyBracketsExpression(Location, Objects, StartToken.WhitespaceBefore));

            // Blocks
            ParseBlocks(Objects);
        }
        static void ParseBrackets(List<RubyObject?> Objects, TokenType OpenType, TokenType CloseType, Func<CodeLocation, List<RubyObject?>, Token, Expression> Creator) {
            Stack<(int Index, Token Token)> OpenBrackets = new();
            // Find and condense brackets
            for (int i = 0; i < Objects.Count; i++) {
                RubyObject? Object = Objects[i];

                // Token
                if (Object is Token Token) {
                    // Open bracket
                    if (Token.Type == OpenType) {
                        OpenBrackets.Push((i, Token));
                    }
                    // Close bracket
                    else if (Token.Type == CloseType) {
                        if (OpenBrackets.TryPop(out (int Index, Token Token) OpenBracket)) {
                            // Get bracket objects
                            List<RubyObject?> BracketObjects = Objects.GetIndexRange(OpenBracket.Index + 1, i - 1);
                            Objects.RemoveIndexRange(OpenBracket.Index, i);

                            // Insert expression at open bracket index
                            i = OpenBracket.Index;
                            Objects.Insert(i, Creator(OpenBracket.Token.Location, BracketObjects, OpenBracket.Token));
                        }
                        else {
                            throw new SyntaxError($"{Token.Location}: unexpected '{Token.Value}'");
                        }
                    }
                }
            }
            // Unclosed open bracket
            if (OpenBrackets.TryPop(out (int Index, Token Token) UnclosedOpenBracket)) {
                throw new SyntaxError($"{UnclosedOpenBracket.Token.Location}: unclosed '{UnclosedOpenBracket.Token.Value}'");
            }
        }
        static void ParseBlocks(List<RubyObject?> Objects) {
            Stack<(int Index, Token Token)> StartBlocks = new();
            bool DoBlockValid = true;
            // Find and condense blocks
            for (int i = 0; i < Objects.Count; i++) {
                RubyObject? LastObject = i - 1 >= 0 ? Objects[i - 1] : null;
                RubyObject? Object = Objects[i];

                // Keyword
                if (Object is Token Token && Token.Type is TokenType.Identifier) {
                    // Don't match keywords in path (e.g. 5.class)
                    if (LastObject is Token LastToken && LastToken.Type is TokenType.Dot or TokenType.DoubleColon or TokenType.SafeDot) {
                        continue;
                    }

                    // Start block
                    if ((Token.Value is "begin" or "def" or "for" or "class" or "module" or "case") || (Token.Value is "if" or "unless" or "while" or "until" && LastObject is null) || (Token.Value is "do" && DoBlockValid)) {
                        // Push block to stack
                        StartBlocks.Push((i, Token));
                        // Set do block validity
                        if (Token.Value is "while" or "until") {
                            DoBlockValid = false;
                        }
                    }
                    // End block
                    else if (Token.Value is "end") {
                        if (StartBlocks.TryPop(out (int Index, Token Token) StartBlock)) {
                            // Create locals for block
                            CodeLocation BlockLocation = StartBlock.Token.Location;

                            // Get block objects
                            List<RubyObject?> BlockObjects = Objects.GetIndexRange(StartBlock.Index + 1, i - 1);
                            Objects.RemoveIndexRange(StartBlock.Index, i);

                            // Get block creator
                            Func<CodeLocation, List<RubyObject?>, Expression> Creator = StartBlock.Token.Value switch {
                                "begin" => (Location, Objects) => ParseBeginBlock(Location, BlockObjects),
                                "def" => (Location, Objects) => ParseDefBlock(Location, BlockObjects),
                                "for" => (Location, Objects) => ParseForBlock(Location, BlockObjects),
                                "class" => (Location, Objects) => ParseModuleBlock(Location, BlockObjects, IsClass: true),
                                "module" => (Location, Objects) => ParseModuleBlock(Location, BlockObjects, IsClass: false),
                                "case" => (Location, Objects) => ParseCaseBlock(Location, BlockObjects),
                                "if" => (Location, Objects) => ParseIfBlock(Location, BlockObjects),
                                "unless" => (Location, Objects) => ParseIfBlock(Location, BlockObjects, Negate: true),
                                "while" => (Location, Objects) => ParseWhileBlock(Location, BlockObjects),
                                "until" => (Location, Objects) => ParseWhileBlock(Location, BlockObjects, Negate: true),
                                "do" => (Location, Objects) => ParseBlock(Location, BlockObjects, HighPrecedence: false),
                                _ => throw new InternalError($"{BlockLocation}: block not handled: '{StartBlock.Token.Value}'")
                            };

                            // Insert block expression at start block index
                            i = StartBlock.Index;
                            Objects.Insert(i, new TemporaryScopeExpression(BlockLocation, BlockObjects, Creator));
                        }
                        else {
                            throw new SyntaxError($"{Token.Location}: unexpected '{Token.Value}'");
                        }
                    }
                }
                // End of statement
                else if (Object is null) {
                    DoBlockValid = true;
                }
            }
            // Unclosed start block
            if (StartBlocks.TryPop(out (int Index, Token Token) UnclosedStartBlock)) {
                throw new SyntaxError($"{UnclosedStartBlock.Token.Location}: unclosed '{UnclosedStartBlock.Token.Value}'");
            }
        }
        static TemporaryBlockExpression ParseBlock(CodeLocation Location, List<RubyObject?> Objects, bool HighPrecedence) {
            // Get arguments
            Argument[] Arguments = System.Array.Empty<Argument>();
            // Find start arguments
            if (Objects.FirstOrDefault() is Token Token) {
                if (Token.Type is TokenType.Operator && Token.Value is "|") {
                    // Find end of arguments
                    int EndArgumentsIndex = Objects.FindIndex(1, Object => Object is Token NextToken && NextToken.Type is TokenType.Operator && NextToken.Value is "|");
                    if (EndArgumentsIndex != -1) {
                        // Parse arguments
                        Arguments = ParseDefArguments(Objects.GetIndexRange(1, EndArgumentsIndex - 1));
                        // Remove arguments
                        Objects.RemoveIndexRange(0, EndArgumentsIndex);
                    }
                    else {
                        throw new SyntaxError($"{Token.Location}: unclosed '|'");
                    }
                }
                else if (Token.Type is TokenType.LogicOperator && Token.Value is "||") {
                    // No arguments
                }
            }
            // Create block expression
            return new TemporaryBlockExpression(Location, Objects, Arguments, HighPrecedence);
        }
        static List<Branch> ParseBranches(List<RubyObject?> Objects, params string[] BranchOrder) {
            // Find start of branches
            int StartIndex = Objects.FindIndex(Object => Object is Token Token && Token.Type is TokenType.Identifier && BranchOrder.Contains(Token.Value));

            // No branches
            if (StartIndex < 0) {
                return new List<Branch>();
            }

            // Ensure branches are in correct order
            int CurrentOrderIndex = 0;
            foreach (RubyObject? Object in Objects) {
                // Identifier
                if (Object is Token Token && Token.Type is TokenType.Identifier) {
                    // Check against each branch type
                    for (int i2 = 0; i2 < BranchOrder.Length; i2++) {
                        // Compare branch types
                        if (BranchOrder[i2] == Token.Value) {
                            // Throw error if branch matches passed branch type
                            if (i2 < CurrentOrderIndex) {
                                throw new SyntaxError($"{Token.Location}: invalid {Token.Value} (incorrect order)");
                            }
                            // Move forward
                            CurrentOrderIndex = i2;
                        }
                    }
                }
            }

            // Parse branches
            List<Branch> Branches = new();
            for (int i = 0; i < BranchOrder.Length; i++) {
                string Branch = BranchOrder[i];
                while (true) {
                    // Find branch index
                    int BranchIndex = Objects.FindIndex(StartIndex, Object => Object is Token Token && Token.AsIdentifier == Branch);
                    if (BranchIndex == -1) {
                        break;
                    }
                    Token BranchToken = (Token)Objects[BranchIndex]!;

                    // Find branch end index
                    int EndBranchIndex = Objects.FindIndex(BranchIndex + 1,
                        Object => Object is Token Token && Token.Type is TokenType.Identifier && BranchOrder.Contains(Token.Value)
                    );

                    // Take branch objects
                    List<RubyObject?> BranchObjects;
                    if (EndBranchIndex != -1) {
                        BranchObjects = Objects.GetIndexRange(BranchIndex + 1, EndBranchIndex - 1);
                        Objects.RemoveIndexRange(BranchIndex, EndBranchIndex);
                    }
                    else {
                        BranchObjects = Objects.GetIndexRange(BranchIndex + 1);
                        Objects.RemoveIndexRange(BranchIndex);
                    }
                    
                    // Add branch
                    Branches.Add(new Branch(BranchToken.Location, Branch, BranchObjects));
                }
            }
            return Branches;
        }
        sealed class Branch {
            public readonly CodeLocation Location;
            public readonly string Name;
            public readonly List<RubyObject?> Objects;
            public Branch(CodeLocation location, string name, List<RubyObject?> objects) {
                Location = location;
                Name = name;
                Objects = objects;
            }
        }
        static BeginExpression ParseBeginBlock(CodeLocation Location, List<RubyObject?> Objects) {
            // Branches
            List<Branch> Branches = ParseBranches(Objects, "rescue", "else", "ensure");

            // Parse begin expressions
            Expression[] BeginExpressions = ParseNullSeparatedExpressions(Location, Objects);

            // Parse branches
            List<RescueExpression> RescueBranches = new();
            Expression[]? ElseExpressions = null;
            Expression[]? EnsureExpressions = null;
            foreach (Branch Branch in Branches) {
                // Rescue
                if (Branch.Name is "rescue") {
                    // Find end of rescue information
                    int NullIndex = Branch.Objects.FindIndex(Object => Object is null);
                    if (NullIndex == -1) {
                        NullIndex = Branch.Objects.Count;
                    }

                    // Find hash rocket index
                    int HashRocketIndex = Branch.Objects.FindIndex(0, NullIndex, Object => Object is Token Token && Token.Type is TokenType.HashRocket);

                    // Get exception type
                    Expression? ExceptionType = null;
                    int EndExceptionType = HashRocketIndex != -1 ? HashRocketIndex - 1 : NullIndex - 1;
                    if (EndExceptionType >= 0) {
                        ExceptionType = ParseExpression(Branch.Location, Branch.Objects.GetIndexRange(0, EndExceptionType));
                    }

                    // Get exception variable
                    string? ExceptionVariable = null;
                    if (HashRocketIndex != -1) {
                        // Exception variable
                        if (HashRocketIndex + 1 < Branch.Objects.Count && Branch.Objects[HashRocketIndex + 1] is Token Variable && Variable.AsIdentifier is string Identifier) {
                            ExceptionVariable = Identifier;
                        }
                        // Error
                        else {
                            throw new SyntaxError($"{Branch.Objects[HashRocketIndex]!.Location}: expected identifier after '=>'");
                        }
                    }

                    // Parse rescue expressions
                    Expression[] RescueExpressions = ParseNullSeparatedExpressions(Branch.Location, Branch.Objects.GetIndexRange(NullIndex + 1));

                    // Create rescue branch
                    RescueBranches.Add(new RescueExpression(Branch.Location, RescueExpressions, ExceptionType, ExceptionVariable));
                }
                // Else
                else if (Branch.Name is "else") {
                    if (ElseExpressions is not null) {
                        throw new SyntaxError($"{Branch.Location}: multiple else branches not valid");
                    }
                    ElseExpressions = ParseNullSeparatedExpressions(Branch.Location, Branch.Objects);
                }
                // Ensure
                else if (Branch.Name is "ensure") {
                    if (EnsureExpressions is not null) {
                        throw new SyntaxError($"{Branch.Location}: multiple ensure branches not valid");
                    }
                    EnsureExpressions = ParseNullSeparatedExpressions(Branch.Location, Branch.Objects);
                }
            }

            // Create begin expression
            return new BeginExpression(Location, BeginExpressions, RescueBranches.ToArray(), ElseExpressions, EnsureExpressions);
        }
        static DefMethodExpression ParseDefBlock(CodeLocation Location, List<RubyObject?> Objects) {
            // Find end of path
            int EndPathIndex;
            bool ExpectIdentifier = true;
            for (EndPathIndex = 0; EndPathIndex < Objects.Count; EndPathIndex++) {
                RubyObject? Object = Objects[EndPathIndex];

                // End of path
                if (!(ExpectIdentifier || Object is Token DotToken && DotToken.Type is TokenType.Dot)) {
                    break;
                }
                // Token
                if (Object is Token Token) {
                    if (Token.Type is TokenType.Dot) {
                        ExpectIdentifier = true;
                    }
                    else if (Token.Type is TokenType.Identifier or TokenType.Operator) {
                        ExpectIdentifier = false;
                    }
                }
            }
            // Get path objects
            List<RubyObject?> PathObjects = Objects.GetIndexRange(0, EndPathIndex - 1);
            // Parse method path
            (ReferenceExpression? PathParent, string PathName) = ParseSeparatedPath(Location, PathObjects, ConstantPath: false);

            // Parse method= as name
            if (EndPathIndex < Objects.Count && Objects[EndPathIndex] is Token EndPathToken) {
                // '=' follows method name
                if (EndPathToken.Type is TokenType.AssignmentOperator && EndPathToken.Value is "=" && !EndPathToken.WhitespaceBefore) {
                    PathName += "=";
                    EndPathIndex++;
                }
            }

            // Get argument objects
            List<RubyObject?> ArgumentObjects;
            int EndArgumentsIndex;
            // Get argument objects (in brackets)
            if (EndPathIndex < Objects.Count && Objects[EndPathIndex] is TemporaryBracketsExpression ArgumentBrackets) {
                EndArgumentsIndex = EndPathIndex;
                ArgumentObjects = ArgumentBrackets.Objects;
            }
            // Get argument objects (no brackets)
            else {
                EndArgumentsIndex = Objects.FindIndex(EndPathIndex, Object => Object is null);
                if (EndArgumentsIndex == -1) {
                    EndArgumentsIndex = Objects.Count - 1;
                }
                ArgumentObjects = Objects.GetIndexRange(EndPathIndex, EndArgumentsIndex);
            }
            // Parse arguments
            Argument[] Arguments = ParseDefArguments(ArgumentObjects);

            // Parse expressions
            Expression[] Expressions = ParseNullSeparatedExpressions(Location, Objects.GetIndexRange(EndArgumentsIndex + 1));

            // Create define method expression
            return new DefMethodExpression(Location, Expressions, PathParent, PathName, Arguments);
        }
        static ForExpression ParseForBlock(CodeLocation Location, List<RubyObject?> Objects) {
            // Get arguments
            int EndArgumentsIndex = Objects.FindIndex(Object => Object is Token Token && Token.AsIdentifier is "in");
            if (EndArgumentsIndex == -1) {
                throw new SyntaxError($"{Location}: expected 'in' after 'for'");
            }
            // Parse arguments
            Argument[] Arguments = ParseDefArguments(Objects.GetIndexRange(0, EndArgumentsIndex - 1));

            // Get target
            int EndTargetIndex = Objects.FindIndex(EndArgumentsIndex, Object => Object is null || Object is Token Token && Token.AsIdentifier is "do");
            if (EndTargetIndex == -1) {
                EndTargetIndex = Objects.Count;
            }
            // Parse target
            Expression Target = ParseExpression(Location, Objects.GetIndexRange(EndArgumentsIndex + 1, EndTargetIndex - 1));

            // Parse expressions
            Expression[] Expressions = ParseNullSeparatedExpressions(Location, Objects.GetIndexRange(EndTargetIndex + 1));
            // Create block from expressions
            Method BlockMethod = new(Location, Arguments, Expressions);

            // Create define method expression
            return new ForExpression(Location, Expressions, Target, Arguments, BlockMethod);
        }
        static DefModuleExpression ParseModuleBlock(CodeLocation Location, List<RubyObject?> Objects, bool IsClass) {
            // Find end of path
            int EndPathIndex = Objects.FindIndex(Object => Object is null);
            // Get path objects
            List<RubyObject?> PathObjects = Objects.GetIndexRange(0, EndPathIndex - 1);

            // Parse module path and super path
            (ReferenceExpression? Parent, string Name) Path;
            ReferenceExpression? Super;
            int SuperIndex = PathObjects.FindIndex(Object => Object is Token Token && Token.Type is TokenType.Operator && Token.Value is "<");
            // Module < superclass
            if (SuperIndex != -1) {
                // Class
                if (IsClass) {
                    Path = ParseSeparatedPath(Location, PathObjects.GetIndexRange(0, SuperIndex - 1), ConstantPath: true);
                    Super = ParsePath(Location, PathObjects.GetIndexRange(SuperIndex + 1), ConstantPath: true);
                }
                // Module
                else {
                    throw new SyntaxError($"{Location}: modules do not support inheritance");
                }
            }
            // Module
            else {
                Path = ParseSeparatedPath(Location, PathObjects, ConstantPath: true);
                Super = null;
            }

            // Get module expressions
            Expression[] Expressions = ParseNullSeparatedExpressions(Location, Objects.GetIndexRange(EndPathIndex + 1));

            // Create module expression
            return new DefModuleExpression(Location, Expressions, Path.Name, Super, IsClass);
        }
        static IfExpression ParseIfBlock(CodeLocation Location, List<RubyObject?> Objects, bool Negate = false) {
            // Branches
            List<Branch> Branches = ParseBranches(Objects, "elsif", "else");

            // Parse condition
            Expression ParseCondition(CodeLocation BranchLocation, List<RubyObject?> BranchObjects, out int EndConditionIndex) {
                // Get end of condition
                EndConditionIndex = BranchObjects.FindIndex(Object => Object is null || Object is Token Token && Token.AsIdentifier is "then");
                if (EndConditionIndex == -1) {
                    EndConditionIndex = BranchObjects.Count;
                }
                // Parse condition
                Expression Condition = ParseExpression(BranchLocation, BranchObjects.GetIndexRange(0, EndConditionIndex - 1));
                // Warn if condition is (a = b) not (a == b)
                if (Condition is AssignmentExpression) {
                    Location.Axis.Warn(Location, "assignment found in condition (did you mean to compare?)");
                }
                // Negate condition
                if (Negate) {
                    Condition = new NotExpression(Condition);
                }
                return Condition;
            }

            // Parse condition
            Expression MainCondition = ParseCondition(Location, Objects, out int EndMainConditionIndex);

            // Parse if expressions
            Expression[] MainExpressions = ParseNullSeparatedExpressions(Location, Objects.GetIndexRange(EndMainConditionIndex + 1));

            // Parse branches
            IfExpression? LastBranch = null;
            for (int i = Branches.Count - 1; i >= 0; i--) {
                Branch Branch = Branches[i];

                // Elsif
                if (Branch.Name is "elsif") {
                    // Get end of condition
                    Expression Condition = ParseCondition(Branch.Location, Branch.Objects, out int EndConditionIndex);
                    // Parse elsif expressions
                    Expression[] ElsifExpressions = ParseNullSeparatedExpressions(Branch.Location, Branch.Objects.GetIndexRange(EndConditionIndex + 1));
                    // Add elsif expression
                    LastBranch = LastBranch is not null
                        ? new IfElseExpression(Branch.Location, ElsifExpressions, Condition, LastBranch)
                        : new IfExpression(Branch.Location, ElsifExpressions, Condition);
                }
                // Else
                else if (Branch.Name is "else") {
                    if (LastBranch is not null) {
                        throw new SyntaxError($"{Branch.Location}: else must be the last branch");
                    }
                    // Parse else expressions
                    Expression[] ElseExpressions = ParseNullSeparatedExpressions(Branch.Location, Branch.Objects);
                    // Add else expression
                    LastBranch = new IfExpression(Branch.Location, ElseExpressions, null);
                }
            }

            // Create if expression
            return LastBranch is not null
                ? new IfElseExpression(Location, MainExpressions, MainCondition, LastBranch)
                : new IfExpression(Location, MainExpressions, MainCondition);
        }
        static WhileExpression ParseWhileBlock(CodeLocation Location, List<RubyObject?> Objects, bool Negate = false) {
            // Get condition
            int EndConditionIndex = Objects.FindIndex(Object => Object is null || Object is Token Token && Token.AsIdentifier is "do");
            Expression Condition = ParseExpression(Location, Objects.GetIndexRange(0, EndConditionIndex - 1));
            // Negate condition if block is 'until'
            if (Negate) {
                Condition = new NotExpression(Condition);
            }
            // Get expressions
            Expression[] Expressions = ParseNullSeparatedExpressions(Location, Objects.GetIndexRange(EndConditionIndex + 1));
            // Create while expression
            return new WhileExpression(Location, Expressions, Condition);
        }
        static CaseExpression ParseCaseBlock(CodeLocation Location, List<RubyObject?> Objects) {
            // Get branches
            List<List<RubyObject?>> BranchObjects = Objects.Split(Object => Object is Token Token && Token.AsIdentifier is "when");

            // Parse subject
            Expression Subject = ParseExpression(Location, BranchObjects.FirstOrDefault()
                ?? throw new SyntaxError($"{Location}: expected case subject, got nothing"));

            // Find else branch
            List<RubyObject?>? ElseBranchObjects = null;
            if (BranchObjects.Count != 0) {
                List<RubyObject?> LastBranch = BranchObjects[^1];
                // Find else index
                int ElseIndex = LastBranch.FindIndex(Object => Object is Token Token && Token.AsIdentifier is "else");
                // Take else branch objects
                if (ElseIndex != -1) {
                    ElseBranchObjects = LastBranch.GetIndexRange(ElseIndex + 1);
                    LastBranch.RemoveIndexRange(ElseIndex);
                }
            }

            // Parse when branches
            List<WhenExpression> WhenBranches = new(BranchObjects.Count - 1);
            for (int i = 1; i < BranchObjects.Count; i++) {
                List<RubyObject?> Branch = BranchObjects[i];
                // Get end of match
                int EndMatchIndex = Branch.FindIndex(Object => Object is null);
                // Parse match
                Expression Match = ParseExpression(Location, Branch.GetIndexRange(0, EndMatchIndex - 1));
                // Parse expressions
                Expression[] Expressions = ParseNullSeparatedExpressions(Location, Branch.GetIndexRange(EndMatchIndex + 1));
                // Add when branch
                WhenBranches.Add(new WhenExpression(Location, Match, Expressions));
            }

            // Parse else branch
            Expression[]? ElseBranch = ElseBranchObjects is not null ? ParseNullSeparatedExpressions(Location, ElseBranchObjects) : null;

            // Warn if case expression is empty
            if (WhenBranches.Count == 0) {
                Location.Axis.Warn(Location, "empty case expression");
            }

            // Create case expression
            return new CaseExpression(Location, Subject, WhenBranches, ElseBranch);
        }

        static Expression ParseExpression(CodeLocation Location, List<RubyObject?> Objects) {
            // Alias
            MatchAlias(Objects);

            // Tokens -> Expressions
            MatchTokensToExpressions(Objects);

            // Temporary expressions
            MatchTemporaryExpressions(Objects);

            // String formatting
            MatchStringFormatting(Objects);

            // Conditional modifiers
            MatchConditionalModifiers(Location, Objects);

            // Curly brackets
            MatchCurlyBrackets(Objects);

            // Method calls (brackets)
            MatchMethodCallsBrackets(Objects);

            // Block expressions (high precedence)
            MatchBlockExpressions(Objects, HighPrecedence: true);

            // Paths
            MatchPaths(Objects);

            // Indexers
            MatchIndexers(Location, Objects);

            // Assignment
            MatchAssignment(Location, Objects);

            // Unary
            MatchUnary(Objects);

            // Ranges
            MatchRanges(Objects);

            // Defined?
            MatchDefined(Objects);

            // Not (high precedence)
            MatchNot(Objects, HighPrecedence: true);

            // Operators
            MatchOperators(Objects);

            // Logic (high precedence)
            MatchLogic(Objects, HighPrecedence: true);

            // Ternary
            MatchTernary(Objects);

            // Key-value pairs
            MatchKeyValuePairs(Objects);

            // Method calls (no brackets)
            MatchMethodCallsNoBrackets(Location, Objects);

            // Block expressions (low precedence)
            MatchBlockExpressions(Objects, HighPrecedence: false);

            // Control statements
            MatchControlStatements(Location, Objects);

            // Not (low precedence)
            MatchNot(Objects, HighPrecedence: false);

            // Logic (low precedence)
            MatchLogic(Objects, HighPrecedence: false);

            // Extract expression from objects
            Expression? Result = null;
            foreach (RubyObject? Object in Objects) {
                // Expression
                if (Object is Expression Expression) {
                    if (Result is not null) {
                        throw new SyntaxError($"{Location}: unexpected expression: '{Expression}'");
                    }
                    Result = Expression;
                }
                // Invalid object
                else if (Object is not null) {
                    throw new SyntaxError($"{Object.Location}: invalid {Object}");
                }
            }
            if (Result is null) {
                throw new SyntaxError($"{Location}: expected expression");
            }
            return Result;
        }

        static (ReferenceExpression? Parent, string Name) ParseSeparatedPath(CodeLocation Location, List<RubyObject?> PathObjects, bool ConstantPath) {
            // Parse path parts
            List<Token> Parts = ParsePathTokens(Location, PathObjects, ConstantPath ? TokenType.DoubleColon : TokenType.Dot);
            Token Name = Parts[^1];

            // Name
            if (Parts.Count == 1) {
                return (null, Name.Value!);
            }
            // Parent + name
            else if (Parts.Count == 2) {
                Token Parent = Parts[^2];
                return (new IdentifierExpression(Parent.Location, Parent.Value!), Name.Value!);
            }
            // Parent path + name
            else {
                Token Parent = Parts[^2];
                ReferenceExpression ParentPath = new IdentifierExpression(Parent.Location, Parent.Value!);
                for (int i = Parts.Count - 2; i >= 0; i++) {
                    ParentPath = ConstantPath
                        ? new ConstantPathExpression(ParentPath, Parts[i].Value!)
                        : new MethodCallExpression(ParentPath.Location, ParentPath, Parts[i].Value!);
                }
                return (ParentPath, Name.Value!);
            }
        }
        static ReferenceExpression ParsePath(CodeLocation Location, List<RubyObject?> PathObjects, bool ConstantPath) {
            // Parse path parts
            (ReferenceExpression? Parent, string Name) = ParseSeparatedPath(Location, PathObjects, ConstantPath);
            // Combine path parts
            if (Parent is null) {
                return new IdentifierExpression(Location, Name);
            }
            else {
                return ConstantPath
                    ? new ConstantPathExpression(Parent, Name)
                    : new MethodCallExpression(Parent.Location, Parent, Name);
            }
        }
        static List<Token> ParsePathTokens(CodeLocation Location, List<RubyObject?> PathObjects, TokenType Separator) {
            List<Token> Parts = new();

            bool ExpectAnotherIdentifier = true;
            CodeLocation LastLocation = Location;
            for (int i = 0; i < PathObjects.Count; i++) {
                RubyObject? Object = PathObjects[i];
                if (Object is null) continue;
                LastLocation = Object.Location;

                // Token
                if (Object is Token Token) {
                    // Separator
                    if (Token.Type == Separator) {
                        if (!ExpectAnotherIdentifier) {
                            ExpectAnotherIdentifier = true;
                        }
                        else {
                            throw new SyntaxError($"{Token.Location}: unexpected {Token}");
                        }
                    }
                    // Identifier or Operator as Identifier
                    else if (Token.Type is TokenType.Identifier or TokenType.Operator) {
                        if (ExpectAnotherIdentifier) {
                            ExpectAnotherIdentifier = false;
                            Parts.Add(Token);
                        }
                        else {
                            throw new SyntaxError($"{Token.Location}: unexpected '{Token}'");
                        }
                    }
                    // Unexpected token
                    else {
                        throw new SyntaxError($"{Token.Location}: unexpected '{Token}'");
                    }
                }
                else {
                    throw new SyntaxError($"{Object.Location}: unexpected '{Object}'");
                }
            }
            if (ExpectAnotherIdentifier) {
                throw new SyntaxError($"{LastLocation}: expected identifier");
            }

            return Parts;
        }
        static Argument[] ParseDefArguments(List<RubyObject?> Objects) {
            List<Argument> Arguments = new();

            ArgumentType CurrentArgumentType = ArgumentType.Normal;
            bool ExpectArgument = true;
            for (int i = 0; i < Objects.Count; i++) {
                RubyObject? Object = Objects[i];
                RubyObject? NextObject = i + 1 < Objects.Count ? Objects[i + 1] : null;

                // Token
                if (Object is Token Token) {
                    // Identifier
                    if (Token.Type is TokenType.Identifier) {
                        // Unexpected argument
                        if (!ExpectArgument) {
                            throw new SyntaxError($"{Token.Location}: unexpected argument: '{Token}'");
                        }
                        // Default value
                        Expression? DefaultValue = null;
                        if (NextObject is Token NextToken && NextToken.Type is TokenType.AssignmentOperator && NextToken.Value is "=") {
                            // Find end of default value objects
                            int StartDefaultValueIndex = i + 2;
                            for (i = StartDefaultValueIndex; i < Objects.Count; i++) {
                                if (Objects[i] is Token Token2 && Token2.Type is TokenType.Comma) {
                                    break;
                                }
                            }
                            // Get default value objects
                            List<RubyObject?> DefaultValueObjects = Objects.GetIndexRange(StartDefaultValueIndex, i - 1);
                            // Parse default value
                            DefaultValue = ParseExpression(Token.Location, DefaultValueObjects);
                        }
                        // Add argument
                        Arguments.Add(new Argument(Token.Location, Token.Value!, DefaultValue, CurrentArgumentType));
                        // Reset flags
                        ExpectArgument = false;
                        CurrentArgumentType = ArgumentType.Normal;
                    }
                    // Comma
                    else if (Token.Type is TokenType.Comma) {
                        // Unexpected comma
                        if (ExpectArgument) {
                            throw new SyntaxError($"{Token.Location}: expected argument before comma");
                        }
                        // Expect an argument after comma
                        ExpectArgument = true;
                    }
                    // Splat / Double Splat / Block
                    else if (Token.Type is TokenType.Operator && Token.Value is "*" or "**" or "&") {
                        // Unexpected splat or block
                        if (CurrentArgumentType is not ArgumentType.Normal) {
                            throw new SyntaxError($"{Token.Location}: unexpected '{Token}'");
                        }
                        // Modify next argument
                        CurrentArgumentType = Token.Value switch {
                            "*" => ArgumentType.Splat,
                            "**" => ArgumentType.DoubleSplat,
                            "&" or _ => ArgumentType.Block,
                        };
                    }
                    // Invalid
                    else {
                        throw new SyntaxError($"{Token.Location}: unexpected '{Token}'");
                    }
                }
                // Pass
                else if (Object is null) { }
                // Invalid
                else {
                    throw new SyntaxError($"{Object.Location}: unexpected '{Object}'");
                }
            }
            // Unexpected comma
            if (ExpectArgument && Arguments.Count != 0) {
                throw new SyntaxError($"{Arguments[^1].Location}: expected argument before comma");
            }

            return Arguments.ToArray();
        }
        static Expression[] ParseCallArgumentsNoBrackets(CodeLocation Location, List<RubyObject?> Objects, int StartIndex) {
            List<Expression> Arguments = new();
            List<RubyObject?> CurrentArgument = new();

            void SubmitArgument() {
                if (CurrentArgument.Count != 0) {
                    Arguments.Add(ParseExpression(Location, CurrentArgument));
                    CurrentArgument.Clear();
                }
            }

            bool ExpectArgument = true;
            int Index = StartIndex;
            for (; Index < Objects.Count; Index++) {
                RubyObject? Object = Objects[Index];

                // Token
                if (Object is Token Token) {
                    // Comma
                    if (Token.Type is TokenType.Comma) {
                        // Unexpected comma
                        if (ExpectArgument) {
                            throw new SyntaxError($"{Token.Location}: expected argument before comma");
                        }
                        // Submit argument
                        SubmitArgument();
                        // Expect an argument after comma
                        ExpectArgument = true;
                    }
                    // End of arguments
                    else {
                        break;
                    }
                }
                // Argument
                else if (Object is Expression Expression) {
                    // Block - end of arguments
                    if (Expression is TemporaryBlockExpression) {
                        break;
                    }
                    // Reset flag
                    if (ExpectArgument) {
                        ExpectArgument = false;
                    }
                    // Add argument object
                    CurrentArgument.Add(Expression);
                }
                // End of statement
                else if (Object is null) {
                    // Ignore end of statement
                    if (ExpectArgument) {
                        // Pass
                    }
                    // End of arguments
                    else {
                        break;
                    }
                }
                // Invalid
                else {
                    throw new SyntaxError($"{Object.Location}: unexpected '{Object}'");
                }
            }
            // Unexpected comma
            if (ExpectArgument && Arguments.Count != 0) {
                throw new SyntaxError($"{Arguments[^1].Location}: expected argument before comma");
            }
            // Submit argument
            SubmitArgument();

            // Remove arguments
            Objects.RemoveIndexRange(StartIndex, Index - 1);

            return Arguments.ToArray();
        }
        
        static void MatchAlias(List<RubyObject?> Objects) {
            for (int i = 0; i < Objects.Count; i++) {
                RubyObject? Object = Objects[i];
                RubyObject? NextObject = i + 1 < Objects.Count ? Objects[i + 1] : null;
                RubyObject? NextNextObject = i + 2 < Objects.Count ? Objects[i + 2] : null;

                // Alias keyword
                if (Object is Token Token && Token.Value is "alias") {
                    // Alias
                    if (NextObject is Token Alias && Alias.Type is TokenType.Identifier) {
                        // Original
                        if (NextNextObject is Token Original && Original.Type is TokenType.Identifier) {
                            // Remove alias objects
                            Objects.RemoveRange(i, 3);
                            // Insert alias expression
                            Objects.Insert(i, new AliasExpression(Token.Location, Alias.Value!, Original.Value!));
                        }
                        else {
                            throw new SyntaxError($"{Token.Location}: expected method identifier for original after 'alias'");
                        }
                    }
                    else {
                        throw new SyntaxError($"{Token.Location}: expected method identifier for alias after 'alias'");
                    }
                }
            }
        }
        static void MatchTokensToExpressions(List<RubyObject?> Objects) {
            for (int i = 0; i < Objects.Count; i++) {
                RubyObject? LastObject = i - 1 >= 0 ? Objects[i - 1] : null;
                RubyObject? Object = Objects[i];

                // Token
                if (Object is Token Token) {
                    // Literal
                    if (Token.IsTokenLiteral) {
                        Objects[i] = new TokenLiteralExpression(Token);
                    }
                    // Identifier
                    else if (Token.Type is TokenType.Identifier) {
                        // Identifier if follows dot
                        if (LastObject is Token LastToken && LastToken.Type is TokenType.Dot or TokenType.SafeDot or TokenType.DoubleColon) {
                            Objects[i] = new IdentifierExpression(Token.Location, Token.Value!);
                        }
                        // self
                        else if (Token.Value is "self") {
                            Objects[i] = new SelfExpression(Token.Location);
                        }
                        // __LINE__
                        else if (Token.Value is "__LINE__") {
                            Objects[i] = new LineExpression(Token.Location);
                        }
                        // __FILE__
                        else if (Token.Value is "__FILE__") {
                            Objects[i] = new FileExpression(Token.Location);
                        }
                        // block_given?
                        else if (Token.Value is "block_given?") {
                            Objects[i] = new BlockGivenExpression(Token.Location);
                        }
                        // Keywords
                        else if (Token.Value is "if" or "unless" or "while" or "until" or "rescue" or "break" or "next" or "redo" or "retry" or "return" or "yield" or "super" or "defined?" or "alias") {
                            // Pass
                        }
                        // Identifier
                        else {
                            Objects[i] = new IdentifierExpression(Token.Location, Token.Value!);
                        }
                    }
                    // Global variable
                    else if (Token.Type is TokenType.GlobalVariable) {
                        Objects[i] = new GlobalExpression(Token.Location, Token.Value!);
                    }
                    // Class variable
                    else if (Token.Type is TokenType.ClassVariable) {
                        Objects[i] = new ClassVariableExpression(Token.Location, Token.Value!);
                    }
                    // Instance variable
                    else if (Token.Type is TokenType.InstanceVariable) {
                        Objects[i] = new InstanceVariableExpression(Token.Location, Token.Value!);
                    }
                }
            }
        }
        static void MatchTemporaryExpressions(List<RubyObject?> Objects) {
            for (int i = 0; i < Objects.Count; i++) {
                RubyObject? LastObject = i - 1 >= 0 ? Objects[i - 1] : null;
                RubyObject? Object = Objects[i];

                // Scope expression
                if (Object is TemporaryScopeExpression ScopeExpression) {
                    // Create scope expression
                    Objects[i] = ScopeExpression.Create(Object.Location);
                }
                // Brackets expression
                else if (Object is TemporaryBracketsExpression BracketsExpression) {
                    // Method call brackets
                    if (LastObject is ReferenceExpression) {
                        // Parse comma-separated expressions
                        Expression[] Expressions = ParseCommaSeparatedExpressions(Object.Location, BracketsExpression.Objects);
                        BracketsExpression.Expressions = Expressions;
                    }
                    // Single brackets
                    else {
                        // Expand brackets expressions
                        Objects[i] = ParseExpression(Object.Location, BracketsExpression.Objects);
                    }
                }
                // Square brackets expression
                else if (Object is TemporarySquareBracketsExpression SquareBracketsExpression) {
                    // Parse comma-separated expressions
                    Expression[] Expressions = ParseCommaSeparatedExpressions(Object.Location, SquareBracketsExpression.Objects);
                    // Create array expression
                    Objects[i] = new ArrayExpression(Object.Location, Expressions, SquareBracketsExpression.WhitespaceBefore);
                }
            }
        }
        static void MatchStringFormatting(List<RubyObject?> Objects) {
            for (int i = 0; i < Objects.Count; i++) {
                RubyObject? Object = Objects[i];

                // Literal
                if (Object is TokenLiteralExpression TokenLiteralExpression) {
                    Token Token = TokenLiteralExpression.Token;

                    // Formatted string
                    if (Token.Type is TokenType.String && Token.Formatted) {
                        string String = Token.Value!;

                        // Get format components
                        List<object> Components = new();
                        int Index = 0;
                        while (Index < String.Length) {
                            // Find '#{'
                            int StartFormattingIndex = String.IndexOf("#{", Index);
                            // Otherwise add rest of characters as literal
                            if (StartFormattingIndex == -1) {
                                Components.Add(String[Index..]);
                                break;
                            }
                            // Find '}'
                            int EndFormattingIndex = String.IndexOf("}", StartFormattingIndex + "#{".Length);
                            // Otherwise error
                            if (EndFormattingIndex == -1) {
                                throw new SyntaxError($"{Token.Location}: expected '}}' to conclude '#{{'");
                            }
                            // Get literal up to '#{'
                            string Literal = String[Index..StartFormattingIndex];
                            // Add literal if it's not empty
                            if (Literal.Length != 0) {
                                Components.Add(Literal);
                            }
                            // Get expression in '#{}'
                            string Expression = String[(StartFormattingIndex + "#{".Length)..EndFormattingIndex];
                            // Add parsed expression
                            Components.Add(ParseExpression(Token.Location, Lexer.Analyse(Token.Location, Expression).CastTo<RubyObject?>()));
                            // Move on
                            Index = EndFormattingIndex + 1;
                        }
                        // Create formatted string expression if string contains any formatting
                        if (Components.Find(Component => Component is not string) is not null) {
                            Objects[i] = new FormattedStringExpression(Token.Location, String, Components.ToArray());
                        }
                    }
                }
            }
        }
        static void MatchConditionalModifiers(CodeLocation Location, List<RubyObject?> Objects) {
            for (int i = 0; i < Objects.Count; i++) {
                RubyObject? Object = Objects[i];

                // Conditional modifier
                if (Object is Token Token && Token.AsIdentifier is "if" or "unless" or "while" or "until" or "rescue") {
                    // Get statement and condition
                    Expression Statement = ParseExpression(Location, Objects.GetIndexRange(0, i - 1));
                    Expression Condition = ParseExpression(Location, Objects.GetIndexRange(i + 1));
                    // Remove objects
                    Objects.Clear();
                    // Create conditional expression
                    Expression ConditionalExpression = Token.Value switch {
                        "if" => new IfModifierExpression(Condition, Statement),
                        "unless" => new IfModifierExpression(new NotExpression(Condition), Statement),
                        "while" => new WhileModifierExpression(Condition, Statement),
                        "until" => new WhileModifierExpression(new NotExpression(Condition), Statement),
                        "rescue" => new RescueModifierExpression(Statement, Condition),
                        _ => throw new InternalError($"{Token.Location}: '{Token.Value}' modifier not handled")
                    };
                    // Insert conditional expression
                    Objects.Insert(0, ConditionalExpression);
                }
            }
        }
        static void MatchCurlyBrackets(List<RubyObject?> Objects) {
            for (int i = 0; i < Objects.Count; i++) {
                RubyObject? LastObject = i - 1 >= 0 ? Objects[i - 1] : null;
                RubyObject? Object = Objects[i];

                // Curly brackets expression
                if (Object is TemporaryCurlyBracketsExpression CurlyBracketsExpression) {
                    // Block expression
                    if (LastObject is ReferenceExpression or TemporaryBracketsExpression) {
                        // Create block expression
                        Objects[i] = ParseBlock(Object.Location, CurlyBracketsExpression.Objects, HighPrecedence: true);
                    }
                    // Hash expression
                    else {
                        // Get expressions in hash
                        Expression[] Expressions = ParseCommaSeparatedExpressions(CurlyBracketsExpression.Location, CurlyBracketsExpression.Objects);
                        // Build key-value dictionary
                        Dictionary<Expression, Expression> HashExpressions = new(Expressions.Length);
                        foreach (Expression Item in Expressions) {
                            // Key-value pair
                            if (Item is KeyValuePairExpression KeyValuePair) {
                                HashExpressions.Add(KeyValuePair.Key, KeyValuePair.Value);
                            }
                            // Invalid
                            else {
                                throw new SyntaxError($"{Item.Location}: expected key-value pair, got '{Item}'");
                            }
                        }
                        // Create hash expression
                        Objects[i] = new HashExpression(CurlyBracketsExpression.Location, HashExpressions);
                    }
                }
            }
        }
        static void MatchMethodCallsBrackets(List<RubyObject?> Objects) {
            for (int i = 0; i < Objects.Count; i++) {
                RubyObject? Object = Objects[i];
                RubyObject? NextObject = i + 1 < Objects.Count ? Objects[i + 1] : null;

                // Method call
                if (Object is ReferenceExpression MethodPath) {
                    // Get arguments in brackets
                    if (NextObject is TemporaryBracketsExpression BracketsExpression && !BracketsExpression.WhitespaceBefore) {
                        // Remove method path and brackets arguments
                        Objects.RemoveRange(i, 2);
                        // Take arguments
                        Expression[] Arguments = BracketsExpression.Expressions!;

                        // Identifier + (arguments)
                        if (MethodPath is IdentifierExpression Identifier) {
                            // Insert method call with arguments
                            Objects.Insert(i, new MethodCallExpression(Identifier.Location, null, Identifier.Name, Arguments));
                        }
                        // Method call + (arguments)
                        else {
                            MethodCallExpression MethodCall = (MethodCallExpression)MethodPath;
                            if (MethodCall.Arguments is not null) {
                                throw new SyntaxError($"{BracketsExpression.Location}: unexpected arguments");
                            }
                            // Insert method call with arguments
                            Objects.Insert(i, new MethodCallExpression(MethodCall.Location, MethodCall.Parent, MethodCall.Name, MethodCall.Arguments, MethodCall.Block));
                        }
                    }
                }
            }
        }
        static void MatchBlockExpressions(List<RubyObject?> Objects, bool HighPrecedence) {
            for (int i = 0; i < Objects.Count; i++) {
                RubyObject? LastObject = i - 1 >= 0 ? Objects[i - 1] : null;
                RubyObject? Object = Objects[i];

                // Block expression
                if (Object is TemporaryBlockExpression Block && Block.HighPrecedence == HighPrecedence) {
                    // Method reference
                    if (LastObject is IdentifierExpression or MethodCallExpression) {
                        // Move back to method reference
                        i--;
                        // Remove reference and block
                        Objects.RemoveRange(i, 2);

                        // Parse block
                        Method BlockMethod = new(Block.Location, Block.Arguments, ParseNullSeparatedExpressions(Block.Location, Block.Objects));

                        // Identifier + block
                        if (LastObject is IdentifierExpression LastIdentifier) {
                            // Insert method call with block
                            Objects.Insert(i, new MethodCallExpression(LastIdentifier.Location, null, LastIdentifier.Name, block: BlockMethod));
                        }
                        // Method call + block
                        else {
                            MethodCallExpression LastMethodCall = (MethodCallExpression)LastObject;
                            if (LastMethodCall.Block is not null) {
                                throw new SyntaxError($"{Block.Location}: unexpected block");
                            }
                            // Insert method call with block
                            Objects.Insert(i, new MethodCallExpression(LastMethodCall.Location, LastMethodCall.Parent, LastMethodCall.Name, LastMethodCall.Arguments, BlockMethod));
                        }
                    }
                    // Unexpected block
                    else {
                        throw new SyntaxError($"{Block.Location}: unexpected block");
                    }
                }
            }
        }
        static void MatchPaths(List<RubyObject?> Objects) {
            for (int i = 0; i < Objects.Count; i++) {
                RubyObject? Object = Objects[i];
                RubyObject? NextObject = i + 1 < Objects.Count ? Objects[i + 1] : null;
                RubyObject? NextNextObject = i + 2 < Objects.Count ? Objects[i + 2] : null;

                // Parent
                if (Object is Expression Parent) {
                    // Token
                    if (NextObject is Token NextToken) {
                        // Dot ('.' or '&.')
                        if (NextToken.Type is TokenType.Dot or TokenType.SafeDot) {
                            // Remove path objects
                            Objects.RemoveRange(i, 3);
                            // Child identifier (a.b)
                            if (NextNextObject is IdentifierExpression ChildIdentifier) {
                                // Insert method call expression
                                Objects.Insert(i, new MethodCallExpression(
                                    Parent.Location, Parent, ChildIdentifier.Name, safe_navigation: NextToken.Type is TokenType.SafeDot
                                ));
                            }
                            // Child method call (a.b())
                            else if (NextNextObject is MethodCallExpression ChildMethodCall) {
                                // Insert method call expression
                                Objects.Insert(i, new MethodCallExpression(
                                    Parent.Location, Parent, ChildMethodCall.Name, ChildMethodCall.Arguments, ChildMethodCall.Block, safe_navigation: NextToken.Type is TokenType.SafeDot
                                ));
                            }
                            // Invalid path
                            else {
                                throw new SyntaxError($"{NextObject.Location}: expected identifier after '.', got '{NextNextObject}'");
                            }
                            // Reprocess path (a.b.c)
                            i--;
                        }
                        // Double colon ('::')
                        else if (NextToken.Type is TokenType.DoubleColon) {
                            // Child
                            if (NextNextObject is IdentifierExpression Child) {
                                // Remove path objects
                                Objects.RemoveRange(i, 3);
                                // Get constant parent
                                ReferenceExpression ConstantParent = Parent as ReferenceExpression
                                    ?? throw new SyntaxError($"{Parent.Location}: constant path must have constant parent");
                                // Insert constant path expression
                                Objects.Insert(i, new ConstantPathExpression(ConstantParent, Child.Name));
                                // Reprocess path (A::B::C)
                                i--;
                            }
                            // Invalid path
                            else {
                                throw new SyntaxError($"{NextObject.Location}: expected identifier after '::', got '{NextNextObject}'");
                            }
                        }
                    }
                }
            }
        }
        static void MatchIndexers(CodeLocation Location, List<RubyObject?> Objects) {
            for (int i = 0; i < Objects.Count; i++) {
                RubyObject? Object = Objects[i];
                RubyObject? NextObject = i + 1 < Objects.Count ? Objects[i + 1] : null;
                RubyObject? NextNextObject = i + 2 < Objects.Count ? Objects[i + 2] : null;

                // Indexer path
                if (Object is Expression Path) {
                    // Indexer
                    if (NextObject is ArrayExpression Indexer && !Indexer.WhitespaceBefore) {
                        // Set index
                        if (NextNextObject is Token NextNextToken && NextNextToken.Type is TokenType.AssignmentOperator) {
                            // Get value objects
                            List<RubyObject?> ValueObjects = Objects.GetIndexRange(i + 3);
                            if (ValueObjects.Count == 0) throw new SyntaxError($"{NextNextToken.Location}: expected value after '{NextNextToken}'");
                            // Get value
                            Expression Value = ParseExpression(Location, ValueObjects);
                            // Remove set index objects
                            Objects.RemoveIndexRange(i);
                            // Get index assignment arguments
                            Expression[] SetIndexArguments = Indexer.Items.Append(Value).ToArray();
                            // Insert index assignment call
                            Objects.Insert(i, new MethodCallExpression(Path.Location, Path, "[]=", SetIndexArguments));
                        }
                        // Index
                        else {
                            // Remove indexer objects
                            Objects.RemoveRange(i, 2);
                            // Insert indexer call
                            Objects.Insert(i, new MethodCallExpression(Path.Location, Path, "[]", Indexer.Items));
                        }
                        // Reprocess indexer (a[b][c])
                        i--;
                    }
                }
            }
        }
        static void MatchAssignment(CodeLocation Location, List<RubyObject?> Objects) {
            for (int i = 0; i < Objects.Count; i++) {
                RubyObject? Object = Objects[i];

                if (Object is Token Token && Token.Type is TokenType.AssignmentOperator) {
                    // Assignment targets
                    ReferenceExpression[] Targets = ParseCommaSeparatedExpressions(Location, Objects.GetIndexRange(0, i - 1)).TryCast<ReferenceExpression>()
                        ?? throw new SyntaxError($"{Location}: expected reference for assignment");
                    // Assignment values
                    Expression[] Values = ParseCommaSeparatedExpressions(Location, Objects.GetIndexRange(i + 1));
                    // Remove assignment objects
                    Objects.Clear();

                    // Get compound assignment operator
                    string? CompoundOperator = null;
                    if (Token.Value is not "=") {
                        CompoundOperator = Token.Value![..^1];
                    }
                    // Create assignment value
                    Expression CreateValue(Expression Target, Expression Value) {
                        return CompoundOperator is not null
                            // Convert (a += b) to (a = a.+(b))
                            ? new MethodCallExpression(Target.Location, Target, CompoundOperator, new Expression[] { Value })
                            // Direct value
                            : Value;
                    }

                    // Replace identifier targets with constants or locals
                    for (int i2 = 0; i2 < Targets.Length; i2++) {
                        if (Targets[i2] is IdentifierExpression Identifier) {
                            Targets[i2] = Identifier.PossibleConstant
                                ? new ConstantExpression(Identifier.Location, Identifier.Name)
                                : new LocalExpression(Identifier.Location, Identifier.Name);
                        }
                    }

                    // = b
                    if (Targets.Length == 0) {
                        throw new SyntaxError($"{Token.Location}: expected variable before '{Token}'");
                    }
                    // a =
                    else if (Values.Length == 0) {
                        throw new SyntaxError($"{Token.Location}: expected value after '{Token}'");
                    }
                    // a = b
                    else if (Targets.Length == 1 && Values.Length == 1) {
                        Objects.Add(new AssignmentExpression(Targets[0], CreateValue(Targets[0], Values[0])));
                    }
                    // a, b = c, d
                    else if (Targets.Length == Values.Length) {
                        AssignmentExpression[] Assignments = new AssignmentExpression[Targets.Length];
                        for (int i2 = 0; i2 < Targets.Length; i2++) {
                            Assignments[i2] = new AssignmentExpression(Targets[i2], CreateValue(Targets[i2], Values[i2]));
                        }
                        Objects.Add(new MultiAssignmentExpression(Location, Assignments));
                    }
                    // a, b = c
                    else if (Values.Length == 1) {
                        if (CompoundOperator is not null) {
                            throw new SyntaxError($"{Location}: compound operator not valid for array expanding assignment");
                        }
                        Objects.Add(new ExpandAssignmentExpression(Location, Targets, Values[0]));
                    }
                    // a = b, c
                    else {
                        throw new SyntaxError($"{Token.Location}: assignment count mismatch");
                    }
                }
            }
        }
        static void MatchUnary(List<RubyObject?> Objects) {
            for (int i = Objects.Count - 1; i >= 0; i--) {
                RubyObject? LastObject = i - 1 >= 0 ? Objects[i - 1] : null;
                RubyObject? Object = Objects[i];
                RubyObject? NextObject = i + 1 < Objects.Count ? Objects[i + 1] : null;

                // Plus or Minus
                if (Object is Token Token && Token.Type is TokenType.Operator && Token.Value is "+" or "-") {
                    // Expression
                    if (NextObject is Expression NextExpression) {
                        // Resolve arithmetic / unary ambiguity
                        if (LastObject is not Expression || (Token.WhitespaceBefore && !Token.WhitespaceAfter)) {
                            // Remove unary operator and expression
                            Objects.RemoveRange(i, 2);
                            // Insert unary method call expression
                            Objects.Insert(i, new MethodCallExpression(Token.Location, NextExpression, $"{Token.Value}@"));
                        }
                    }
                }
            }
        }
        static void MatchRanges(List<RubyObject?> Objects) {
            for (int i = Objects.Count - 1; i >= 0; i--) {
                RubyObject? LastObject = i - 1 >= 0 ? Objects[i - 1] : null;
                RubyObject? Object = Objects[i];
                RubyObject? NextObject = i + 1 < Objects.Count ? Objects[i + 1] : null;

                // Range
                if (Object is Token Token && Token.Type is TokenType.InclusiveRange or TokenType.ExclusiveRange) {
                    // Get min and max
                    Expression? Min = LastObject is Expression LastExp ? LastExp : null;
                    Expression? Max = NextObject is Expression NextExp ? NextExp : null;

                    // Remove objects
                    if (Max is not null) {
                        Objects.RemoveAt(i + 1);
                    }
                    Objects.RemoveAt(i);
                    if (Min is not null) {
                        Objects.RemoveAt(i - 1);
                        i--;
                    }

                    // Insert range expression
                    Objects.Insert(i, new RangeExpression(Token.Location, Min, Max, Token.Type is TokenType.ExclusiveRange));
                }
            }
        }
        static void MatchDefined(List<RubyObject?> Objects) {
            for (int i = 0; i < Objects.Count; i++) {
                RubyObject? Object = Objects[i];
                RubyObject? NextObject = i + 1 < Objects.Count ? Objects[i + 1] : null;

                // Defined?
                if (Object is Token Token && Token.AsIdentifier is "defined?") {
                    // Expression
                    if (NextObject is Expression NextExpression) {
                        // Remove defined? and expression
                        Objects.RemoveRange(i, 2);
                        // Insert defined? expression
                        Objects.Insert(i, new DefinedExpression(Token.Location, NextExpression));
                    }
                    // Error
                    else {
                        throw new SyntaxError($"{Token.Location}: expected expression after defined?");
                    }
                }
            }
        }
        static void MatchNot(List<RubyObject?> Objects, bool HighPrecedence) {
            for (int i = Objects.Count - 1; i >= 0; i--) {
                RubyObject? Object = Objects[i];
                RubyObject? NextObject = i + 1 < Objects.Count ? Objects[i + 1] : null;

                // Not
                if (Object is Token Token && Token.Type is TokenType.Not) {
                    // Ignore until later if low precedence
                    if (HighPrecedence && Token.Value is not "!") {
                        continue;
                    }
                    // Expression to negate
                    if (NextObject is Expression NextExpression) {
                        // Remove objects
                        Objects.RemoveRange(i, 2);
                        // Insert not expression
                        Objects.Insert(i, new NotExpression(NextExpression));
                    }
                }
            }
        }
        static void MatchOperators(List<RubyObject?> Objects) {
            void Match(Func<string, bool> Filter) {
                for (int i = 0; i < Objects.Count; i++) {
                    RubyObject? Object = Objects[i];
                    RubyObject? NextObject = i + 1 < Objects.Count ? Objects[i + 1] : null;
                    RubyObject? NextNextObject = i + 2 < Objects.Count ? Objects[i + 2] : null;

                    // Left expression
                    if (Object is Expression LeftExpression) {
                        // Operator
                        if (NextObject is Token NextToken && NextToken.Type is TokenType.Operator && Filter(NextToken.Value!)) {
                            // Right expression
                            if (NextNextObject is Expression RightExpression) {
                                // Remove left expression, operator, and right expression
                                Objects.RemoveRange(i, 3);
                                // Insert method call expression
                                Objects.Insert(i, new MethodCallExpression(LeftExpression.Location, LeftExpression, NextToken.Value!, new Expression[] { RightExpression }));
                                // Reprocess operator (a + b + c)
                                i--;
                            }
                            // Right expression missing
                            else {
                                throw new SyntaxError($"{NextToken.Location}: expected expression after '{NextToken}'");
                            }
                        }
                    }
                }
            }
            // Operator precedence
            Match(Op => Op is "**");
            Match(Op => Op is "*" or "/" or "%");
            Match(Op => Op is "+" or "-");
            Match(Op => Op is "<<" or ">>");
            Match(Op => Op is "&");
            Match(Op => Op is "|");
            Match(Op => Op is "<" or "<=" or ">=" or ">");
            Match(Op => Op is "==" or "===" or "!=" or "<=>");
        }
        static void MatchLogic(List<RubyObject?> Objects, bool HighPrecedence) {
            void Match(Func<string, bool> Filter, bool IsAnd) {
                for (int i = 0; i < Objects.Count; i++) {
                    RubyObject? Object = Objects[i];
                    RubyObject? NextObject = i + 1 < Objects.Count ? Objects[i + 1] : null;
                    RubyObject? NextNextObject = i + 2 < Objects.Count ? Objects[i + 2] : null;

                    // Left expression
                    if (Object is Expression LeftExpression) {
                        // Logic operator
                        if (NextObject is Token NextToken && NextToken.Type is TokenType.LogicOperator && Filter(NextToken.Value!)) {
                            // Right expression
                            if (NextNextObject is Expression RightExpression) {
                                // Remove left expression, logic operator, and right expression
                                Objects.RemoveRange(i, 3);
                                // Insert logic expression
                                Objects.Insert(i, new LogicExpression(LeftExpression, RightExpression, IsAnd));
                                // Reprocess logic (a and b and c)
                                i--;
                            }
                            // Right expression missing
                            else {
                                throw new SyntaxError($"{NextToken.Location}: expected expression after '{NextToken}'");
                            }
                        }
                    }
                }
            }
            // Operator precedence
            if (HighPrecedence) {
                Match(Op => Op is "&&", IsAnd: true);
                Match(Op => Op is "||", IsAnd: false);
            }
            else {
                Match(Op => Op is "and", IsAnd: true);
                Match(Op => Op is "or", IsAnd: false);
            }
        }
        static void MatchTernary(List<RubyObject?> Objects) {
            for (int i = 0; i < Objects.Count; i++) {
                RubyObject? Object = Objects[i];
                RubyObject? NextObject = i + 1 < Objects.Count ? Objects[i + 1] : null;

                // Condition
                if (Object is Expression Condition) {
                    // '?'
                    if (NextObject is Token TernaryTruthyToken && TernaryTruthyToken.Type is TokenType.TernaryTruthy) {
                        // Find '?' and ':'
                        int TernaryTruthyIndex = i + 1;
                        int TernaryFalseyIndex = Objects.FindIndex(Object => Object is Token Token && Token.Type is TokenType.TernaryFalsey);
                        Token TernaryFalseyToken = TernaryFalseyIndex != -1
                            ? (Token)Objects[TernaryFalseyIndex]!
                            : throw new SyntaxError($"{TernaryTruthyToken.Location}: incomplete ternary (expected ':' after '?')");

                        // Get expression after '?'
                        Expression ExpressionIfTruthy = ParseExpression(TernaryTruthyToken.Location, Objects.GetIndexRange(TernaryTruthyIndex + 1, TernaryFalseyIndex - 1));

                        // Get expression after ':'
                        Expression ExpressionIfFalsey = (TernaryFalseyIndex + 1 < Objects.Count ? Objects[TernaryFalseyIndex + 1] as Expression : null)
                            ?? throw new SyntaxError($"{TernaryFalseyToken.Location}: expected expression after ':'");

                        // Remove ternary objects
                        Objects.RemoveIndexRange(i, TernaryFalseyIndex + 1);
                        // Insert ternary expression
                        Objects.Insert(i, new TernaryExpression(Condition, ExpressionIfTruthy, ExpressionIfFalsey));
                    }
                }
            }
        }
        static void MatchKeyValuePairs(List<RubyObject?> Objects) {
            for (int i = 0; i < Objects.Count; i++) {
                RubyObject? Object = Objects[i];
                RubyObject? NextObject = i + 1 < Objects.Count ? Objects[i + 1] : null;
                RubyObject? NextNextObject = i + 2 < Objects.Count ? Objects[i + 2] : null;

                // Key expression
                if (Object is Expression KeyExpression) {
                    // Hash rocket
                    if (NextObject is Token NextToken && NextToken.Type is TokenType.HashRocket) {
                        // Value expression
                        if (NextNextObject is Expression ValueExpression) {
                            // Remove key-value pair objects
                            Objects.RemoveRange(i, 3);
                            // Insert key-value pair expression
                            Objects.Insert(i, new KeyValuePairExpression(KeyExpression, ValueExpression));
                        }
                        // Invalid
                        else {
                            throw new SyntaxError($"{(NextNextObject ?? NextToken).Location}: expected value after '=>', got '{NextNextObject}'");
                        }
                    }
                }
            }
        }
        static void MatchMethodCallsNoBrackets(CodeLocation Location, List<RubyObject?> Objects) {
            for (int i = 0; i < Objects.Count; i++) {
                RubyObject? Object = Objects[i];

                // Method call
                if (Object is ReferenceExpression Reference && Reference is IdentifierExpression or MethodCallExpression) {
                    // Take call arguments
                    Expression[] Arguments = ParseCallArgumentsNoBrackets(Location, Objects, i + 1);
                    // Ensure arguments are present
                    if (Arguments.Length == 0) {
                        continue;
                    }
                    // Get parent
                    Expression? Parent = null;
                    if (Object is MethodCallExpression MethodCall) {
                        Parent = MethodCall.Parent;
                    }
                    // Create method call expression
                    Objects[i] = new MethodCallExpression(Object.Location, Parent, Reference.Name, Arguments);
                }
            }
        }
        static void MatchControlStatements(CodeLocation Location, List<RubyObject?> Objects) {
            bool GetControlStatement(string Keyword, CodeLocation Location, out bool TakeArguments, out Func<Expression[], Expression>? Creator) {
                // Return whether the keyword is a control statement, whether it takes arguments, and a control expression creator
                switch (Keyword) {
                    case "break":
                        TakeArguments = false;
                        Creator = Argument => new ControlExpression(Location, ControlType.Break);
                        return true;
                    case "next":
                        TakeArguments = false;
                        Creator = Argument => new ControlExpression(Location, ControlType.Next);
                        return true;
                    case "redo":
                        TakeArguments = false;
                        Creator = Argument => new ControlExpression(Location, ControlType.Redo);
                        return true;
                    case "retry":
                        TakeArguments = false;
                        Creator = Argument => new ControlExpression(Location, ControlType.Retry);
                        return true;
                    case "return":
                        TakeArguments = true;
                        Creator = Arguments => {
                            Expression? Argument = Arguments.Length switch {
                                0 => null,
                                1 => Arguments[0],
                                _ => new ArrayExpression(Location, Arguments)
                            };
                            return new ControlExpression(Location, ControlType.Return, Argument);
                        };
                        return true;
                    case "yield":
                        TakeArguments = true;
                        Creator = Arguments => new YieldExpression(Location, Arguments);
                        return true;
                    case "super":
                        TakeArguments = true;
                        Creator = Arguments => new SuperExpression(Location, Arguments);
                        return true;
                    default:
                        TakeArguments = false;
                        Creator = null;
                        return false;
                };
            }
            for (int i = 0; i < Objects.Count; i++) {
                RubyObject? Object = Objects[i];

                if (Object is Token Token) {
                    // Control statement
                    if (GetControlStatement(Token.Value!, Token.Location, out bool TakeArgument, out Func<Expression[], Expression>? Creator)) {
                        // Remove control statement object
                        Objects.RemoveAt(i);

                        // Get argument if valid
                        Expression[] Arguments = System.Array.Empty<Expression>();
                        if (TakeArgument) {
                            // Take argument objects
                            List<RubyObject?> ArgumentObjects = Objects.GetIndexRange(i);
                            Objects.RemoveIndexRange(i);
                            // Parse arguments
                            Arguments = ParseCommaSeparatedExpressions(Location, ArgumentObjects);
                        }

                        // Insert control expression
                        Objects.Insert(i, Creator!(Arguments));
                    }
                }
            }
        }
    }
}
