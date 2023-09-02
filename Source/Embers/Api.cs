using System.Text;
using static Embers.Interpreter;

#pragma warning disable CS1998
#pragma warning disable IDE1006

namespace Embers
{
    public static class Api
    {
        public static void Setup(Interpreter Interpreter) {
            // Global methods
            Interpreter.RootInstance.InstanceMethods["puts"] = new Method(puts, null);
            Interpreter.RootInstance.InstanceMethods["print"] = new Method(print, null);
            Interpreter.RootInstance.InstanceMethods["p"] = new Method(p, null);
            Interpreter.RootInstance.InstanceMethods["gets"] = new Method(gets, 0);
            Interpreter.RootInstance.InstanceMethods["getc"] = new Method(getc, 0);
            Interpreter.RootInstance.InstanceMethods["warn"] = new Method(warn, null);
            Interpreter.RootInstance.InstanceMethods["sleep"] = new Method(sleep, 0..1);

            // String
            Interpreter.String.InstanceMethods["+"] = new Method(String._Add, 1);
            Interpreter.String.InstanceMethods["*"] = new Method(String._Multiply, 1);
            Interpreter.String.InstanceMethods["=="] = new Method(String._Equals, 1);
            Interpreter.String.InstanceMethods["to_str"] = new Method(String.to_str, 0);
            Interpreter.String.InstanceMethods["to_i"] = new Method(String.to_i, 0);
            Interpreter.String.InstanceMethods["to_f"] = new Method(String.to_f, 0);
            Interpreter.String.InstanceMethods["to_sym"] = new Method(String.to_sym, 0);
            Interpreter.String.InstanceMethods["chomp"] = new Method(String.chomp, 0..1);
            Interpreter.String.InstanceMethods["strip"] = new Method(String.strip, 0);

            // Integer
            Interpreter.Integer.InstanceMethods["+"] = new Method(Integer._Add, 1);
            Interpreter.Integer.InstanceMethods["-"] = new Method(Integer._Subtract, 1);
            Interpreter.Integer.InstanceMethods["*"] = new Method(Integer._Multiply, 1);
            Interpreter.Integer.InstanceMethods["/"] = new Method(Integer._Divide, 1);
            Interpreter.Integer.InstanceMethods["%"] = new Method(Integer._Modulo, 1);
            Interpreter.Integer.InstanceMethods["**"] = new Method(Integer._Exponentiate, 1);
            Interpreter.Integer.InstanceMethods["=="] = new Method(Integer._Equals, 1);
            Interpreter.Integer.InstanceMethods["to_i"] = new Method(Integer.to_i, 0);
            Interpreter.Integer.InstanceMethods["to_f"] = new Method(Integer.to_f, 0);
            Interpreter.Integer.InstanceMethods["times"] = new Method(Integer.times, 0);

            // Float
            Interpreter.Float.InstanceMethods["+"] = new Method(Float._Add, 1);
            Interpreter.Float.InstanceMethods["-"] = new Method(Float._Subtract, 1);
            Interpreter.Float.InstanceMethods["*"] = new Method(Float._Multiply, 1);
            Interpreter.Float.InstanceMethods["/"] = new Method(Float._Divide, 1);
            Interpreter.Float.InstanceMethods["%"] = new Method(Float._Modulo, 1);
            Interpreter.Float.InstanceMethods["**"] = new Method(Float._Exponentiate, 1);
            Interpreter.Float.InstanceMethods["=="] = new Method(Float._Equals, 1);
            Interpreter.Float.InstanceMethods["to_i"] = new Method(Float.to_i, 0);
            Interpreter.Float.InstanceMethods["to_f"] = new Method(Float.to_f, 0);

            // Unsafe Api
            if (Interpreter.AllowUnsafeApi) {
                // File
                Module FileModule = Interpreter.CreateModule("File");
                FileModule.Methods.Add("read", new Method(File.read, 1));
                FileModule.Methods.Add("write", new Method(File.write, 2));
            }
        }

        public static readonly IReadOnlyDictionary<string, Method> DefaultClassAndInstanceMethods = new Dictionary<string, Method>() {
            {"==", new Method(ClassInstance._Equals, 1)},
            {"!=", new Method(ClassInstance._NotEquals, 1)},
            {"inspect", new Method(ClassInstance.inspect, 0)},
            {"to_s", new Method(ClassInstance.to_s, 0)},
        };

        // API
        static async Task<Instances> puts(MethodInput Input) {
            if (Input.Arguments.Count != 0) {
                foreach (Instance Message in Input.Arguments) {
                    Console.WriteLine(Message.LightInspect());
                }
            }
            else {
                Console.WriteLine();
            }
            return Input.Interpreter.Nil;
        }
        static async Task<Instances> print(MethodInput Input) {
            foreach (Instance Message in Input.Arguments) {
                Console.Write(Message.LightInspect());
            }
            return Input.Interpreter.Nil;
        }
        static async Task<Instances> p(MethodInput Input) {
            foreach (Instance Message in Input.Arguments) {
                Console.WriteLine(Message.Inspect());
            }
            return Input.Interpreter.Nil;
        }
        static async Task<Instances> gets(MethodInput Input) {
            string? UserInput = Console.ReadLine();
            UserInput = UserInput != null ? UserInput + "\n" : "";
            return new StringInstance(Input.Interpreter.String, UserInput);
        }
        static async Task<Instances> getc(MethodInput Input) {
            string UserInput = Console.ReadKey().KeyChar.ToString();
            return new StringInstance(Input.Interpreter.String, UserInput);
        }
        static async Task<Instances> warn(MethodInput Input) {
            Console.ForegroundColor = ConsoleColor.Yellow;
            foreach (Instance Message in Input.Arguments) {
                Console.WriteLine(Message.Object);
            }
            Console.ResetColor();
            return Input.Interpreter.Nil;
        }
        static async Task<Instances> sleep(MethodInput Input) {
            if (Input.Arguments.Count == 1) {
                double SecondsToSleep = Input.Arguments[0].Float;
                await Task.Delay((int)(SecondsToSleep * 1000));
            }
            else {
                await Task.Delay(Timeout.Infinite);
            }
            return Input.Interpreter.Nil;
        }
        static class ClassInstance {
            public static async Task<Instances> _Equals(MethodInput Input) {
                Instance Left = Input.Instance;
                Instance Right = Input.Arguments[0];
                if (Left is ModuleReference LeftModule && Right is ModuleReference RightModule) {
                    return LeftModule.Module == RightModule.Module ? Input.Interpreter.True : Input.Interpreter.False;
                }
                else {
                    return Left == Right ? Input.Interpreter.True : Input.Interpreter.False;
                }
            }
            public static async Task<Instances> _NotEquals(MethodInput Input) {
                Instance Left = Input.Instance;
                Instance Right = Input.Arguments[0];
                return (await Left.TryCallInstanceMethod(Input.Interpreter, "==", Right)).SingleInstance().IsTruthy ? Input.Interpreter.False : Input.Interpreter.True;
            }
            public static async Task<Instances> inspect(MethodInput Input) {
                return new StringInstance(Input.Interpreter.String, Input.Instance.Inspect());
            }
            public static async Task<Instances> to_s(MethodInput Input) {
                return new StringInstance(Input.Interpreter.String, Input.Instance.LightInspect());
            }
        }
        static class String {
            public static async Task<Instances> _Add(MethodInput Input) {
                Instance Right = Input.Arguments[0];
                return new StringInstance(Input.Interpreter.String, Input.Instance.String + Right.String);
            }
            public static async Task<Instances> _Multiply(MethodInput Input) {
                Instance Right = Input.Arguments[0];
                StringBuilder JoinedString = new();
                long RepeatCount = Right.Integer;
                for (long i = 0; i < RepeatCount; i++) {
                    JoinedString.Append(Input.Instance.String);
                }
                return new StringInstance(Input.Interpreter.String, JoinedString.ToString());
            }
            public static async Task<Instances> _Equals(MethodInput Input) {
                Instance Left = Input.Instance;
                Instance Right = Input.Arguments[0];
                if (Right is StringInstance RightString && Left.String == RightString.String) {
                    return Input.Interpreter.True;
                }
                else {
                    return Input.Interpreter.False;
                }
            }
            public static async Task<Instances> to_str(MethodInput Input) {
                return await ClassInstance.to_s(Input);
            }
            public static async Task<Instances> to_i(MethodInput Input) {
                string IntegerAsString = Input.Instance.LightInspect();
                StringBuilder IntegerString = new();
                for (int i = 0; i < IntegerAsString.Length; i++) {
                    char Chara = IntegerAsString[i];

                    if (char.IsAsciiDigit(Chara)) {
                        IntegerString.Append(Chara);
                    }
                    else {
                        break;
                    }
                }
                if (IntegerString.Length == 0) return new IntegerInstance(Input.Interpreter.Integer, 0);
                return new IntegerInstance(Input.Interpreter.Integer, long.Parse(IntegerString.ToString()));
            }
            public static async Task<Instances> to_f(MethodInput Input) {
                string FloatAsString = Input.Instance.LightInspect();
                StringBuilder FloatString = new();
                bool SeenDot = false;
                for (int i = 0; i < FloatAsString.Length; i++) {
                    char Chara = FloatAsString[i];

                    if (char.IsAsciiDigit(Chara)) {
                        FloatString.Append(Chara);
                    }
                    else if (Chara == '.') {
                        if (SeenDot) break;
                        SeenDot = true;
                        FloatString.Append(Chara);
                    }
                    else {
                        break;
                    }
                }
                if (FloatString.Length == 0) return new FloatInstance(Input.Interpreter.Float, 0);
                if (!SeenDot) FloatString.Append(".0");
                return new FloatInstance(Input.Interpreter.Float, double.Parse(FloatString.ToString()));
            }
            public static async Task<Instances> to_sym(MethodInput Input) {
                return new SymbolInstance(Input.Interpreter.Symbol, Input.Instance.LightInspect());
            }
            public static async Task<Instances> chomp(MethodInput Input) {
                string String = Input.Instance.String;
                if (Input.Arguments.Count == 0) {
                    if (String.EndsWith('\n') || String.EndsWith('\r')) {
                        return new StringInstance(Input.Interpreter.String, String[0..^1]);
                    }
                    else if (String.EndsWith("\r\n")) {
                        return new StringInstance(Input.Interpreter.String, String[0..^2]);
                    }
                }
                else {
                    string RemoveFromEnd = Input.Arguments[0].String;
                    if (String.EndsWith(RemoveFromEnd)) {
                        return new StringInstance(Input.Interpreter.String, String[0..^RemoveFromEnd.Length]);
                    }
                }
                return Input.Instance;
            }
            public static async Task<Instances> strip(MethodInput Input) {
                string String = Input.Instance.String;
                string Stripped = String.Trim();
                if (Stripped.Length != String.Length) {
                    return new StringInstance(Input.Interpreter.String, Stripped);
                }
                return Input.Instance;
            }
        }
        static class Integer {
            private static Instance _GetResult(Interpreter Interpreter, double Result, bool RightIsInteger) {
                if (RightIsInteger) {
                    return new IntegerInstance(Interpreter.Integer, (long)Result);
                }
                else {
                    return new FloatInstance(Interpreter.Integer, Result);
                }
            }
            public static async Task<Instances> _Add(MethodInput Input) {
                Instance Right = Input.Arguments[0];
                return _GetResult(Input.Interpreter, Input.Instance.Integer + Right.Float, Right is IntegerInstance);
            }
            public static async Task<Instances> _Subtract(MethodInput Input) {
                Instance Right = Input.Arguments[0];
                return _GetResult(Input.Interpreter, Input.Instance.Integer - Right.Float, Right is IntegerInstance);
            }
            public static async Task<Instances> _Multiply(MethodInput Input) {
                Instance Right = Input.Arguments[0];
                return _GetResult(Input.Interpreter, Input.Instance.Integer * Right.Float, Right is IntegerInstance);
            }
            public static async Task<Instances> _Divide(MethodInput Input) {
                Instance Right = Input.Arguments[0];
                return _GetResult(Input.Interpreter, Input.Instance.Integer / Right.Float, Right is IntegerInstance);
            }
            public static async Task<Instances> _Modulo(MethodInput Input) {
                Instance Right = Input.Arguments[0];
                return _GetResult(Input.Interpreter, Input.Instance.Integer % Right.Float, Right is IntegerInstance);
            }
            public static async Task<Instances> _Exponentiate(MethodInput Input) {
                Instance Right = Input.Arguments[0];
                return _GetResult(Input.Interpreter, Math.Pow(Input.Instance.Integer, Right.Float), Right is IntegerInstance);
            }
            public static async Task<Instances> _Equals(MethodInput Input) {
                Instance Left = Input.Instance;
                Instance Right = Input.Arguments[0];
                if (Right is IntegerInstance RightInteger && Left.Integer == RightInteger.Integer) {
                    return Input.Interpreter.True;
                }
                else if (Right is FloatInstance RightFloat && Left.Float == RightFloat.Float) {
                    return Input.Interpreter.True;
                }
                else {
                    return Input.Interpreter.False;
                }
            }
            public static async Task<Instances> to_i(MethodInput Input) {
                return Input.Instance;
            }
            public static async Task<Instances> to_f(MethodInput Input) {
                return new FloatInstance(Input.Interpreter.Float, Input.Instance.Float);
            }
            public static async Task<Instances> times(MethodInput Input) {
                if (Input.OnYield != null) {
                    long Times = Input.Instance.Integer;
                    for (long i = 0; i < Times; i++) {
                        await Input.OnYield.Call(Input.Interpreter, Input.Instance);
                    }
                }
                return Input.Interpreter.Nil;
            }
        }
        static class Float {
            public static async Task<Instances> _Add(MethodInput Input) {
                Instance Right = Input.Arguments[0];
                return new FloatInstance(Input.Interpreter.Float, Input.Instance.Float + Right.Float);
            }
            public static async Task<Instances> _Subtract(MethodInput Input) {
                Instance Right = Input.Arguments[0];
                return new FloatInstance(Input.Interpreter.Float, Input.Instance.Float - Right.Float);
            }
            public static async Task<Instances> _Multiply(MethodInput Input) {
                Instance Right = Input.Arguments[0];
                return new FloatInstance(Input.Interpreter.Float, Input.Instance.Float * Right.Float);
            }
            public static async Task<Instances> _Divide(MethodInput Input) {
                Instance Right = Input.Arguments[0];
                return new FloatInstance(Input.Interpreter.Float, Input.Instance.Float / Right.Float);
            }
            public static async Task<Instances> _Modulo(MethodInput Input) {
                Instance Right = Input.Arguments[0];
                return new FloatInstance(Input.Interpreter.Float, Input.Instance.Float % Right.Float);
            }
            public static async Task<Instances> _Exponentiate(MethodInput Input) {
                Instance Right = Input.Arguments[0];
                return new FloatInstance(Input.Interpreter.Float, Math.Pow(Input.Instance.Float, Right.Float));
            }
            public static async Task<Instances> _Equals(MethodInput Input) {
                Instance Left = Input.Instance;
                Instance Right = Input.Arguments[0];
                if (Right is IntegerInstance RightInteger && Left.Float == RightInteger.Float) {
                    return Input.Interpreter.True;
                }
                else if (Right is FloatInstance RightFloat && Left.Float == RightFloat.Float) {
                    return Input.Interpreter.True;
                }
                else {
                    return Input.Interpreter.False;
                }
            }
            public static async Task<Instances> to_i(MethodInput Input) {
                return new IntegerInstance(Input.Interpreter.Float, Input.Instance.Integer);
            }
            public static async Task<Instances> to_f(MethodInput Input) {
                return Input.Instance;
            }
        }
        static class File {
            public static async Task<Instances> read(MethodInput Input) {
                string FilePath = Input.Arguments[0].String;
                try {
                    string FileContents = System.IO.File.ReadAllText(FilePath);
                    return new StringInstance(Input.Interpreter.String, FileContents);
                }
                catch (FileNotFoundException) {
                    throw new RuntimeException($"No such file or directory: '{FilePath}'");
                }
                catch (Exception Ex) {
                    throw new RuntimeException($"Error reading file: '{Ex.Message}'");
                }
            }
            public static async Task<Instances> write(MethodInput Input) {
                string FilePath = Input.Arguments[0].String;
                string Text = Input.Arguments[1].String;
                try {
                    System.IO.File.WriteAllText(FilePath, Text);
                    return Input.Interpreter.Nil;
                }
                catch (Exception Ex) {
                    throw new RuntimeException($"Error writing file: '{Ex.Message}'");
                }
            }
        }
    }
}
