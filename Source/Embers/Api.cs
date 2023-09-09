using System.Text;
using static Embers.Script;

#pragma warning disable CS1998
#pragma warning disable IDE1006

namespace Embers
{
    public static class Api
    {
        public static void Setup(Script Script) {
            Interpreter Interpreter = Script.Interpreter;

            // Global methods
            Interpreter.RootInstance.InstanceMethods["puts"] = new Method(puts, null);
            Interpreter.RootInstance.InstanceMethods["print"] = new Method(print, null);
            Interpreter.RootInstance.InstanceMethods["p"] = new Method(p, null);
            Interpreter.RootInstance.InstanceMethods["gets"] = new Method(gets, 0);
            Interpreter.RootInstance.InstanceMethods["getc"] = new Method(getc, 0);
            Interpreter.RootInstance.InstanceMethods["warn"] = new Method(warn, null);
            Interpreter.RootInstance.InstanceMethods["sleep"] = new Method(sleep, 0..1);
            Interpreter.RootInstance.InstanceMethods["raise"] = new Method(@raise, 1);
            Interpreter.RootInstance.InstanceMethods["throw"] = new Method(@throw, 1);
            Interpreter.RootInstance.InstanceMethods["catch"] = new Method(@catch, 1);
            Interpreter.RootInstance.InstanceMethods["lambda"] = new Method(lambda, 0);
            Interpreter.RootInstance.InstanceMethods["loop"] = new Method(loop, 0);
            Interpreter.RootInstance.InstanceMethods["rand"] = new Method(Random.rand, 0..1);
            Interpreter.RootInstance.InstanceMethods["srand"] = new Method(Random.srand, 0..1);
            Interpreter.RootInstance.InstanceMethods["exit"] = new Method(exit, 0);
            Interpreter.RootInstance.InstanceMethods["quit"] = new Method(quit, 0);

            // String
            Interpreter.String.InstanceMethods["[]"] = new Method(String._Indexer, 1);
            Interpreter.String.InstanceMethods["+"] = new Method(String._Add, 1);
            Interpreter.String.InstanceMethods["*"] = new Method(String._Multiply, 1);
            Interpreter.String.InstanceMethods["=="] = new Method(String._Equals, 1);
            Interpreter.String.InstanceMethods["initialize"] = new Method(String.initialize, 0..1);
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
            Interpreter.Integer.InstanceMethods["<"] = new Method(Integer._LessThan, 1);
            Interpreter.Integer.InstanceMethods[">"] = new Method(Integer._GreaterThan, 1);
            Interpreter.Integer.InstanceMethods["+@"] = new Method(Integer._UnaryPlus, 0);
            Interpreter.Integer.InstanceMethods["-@"] = new Method(Integer._UnaryMinus, 0);
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
            Interpreter.Float.InstanceMethods["+@"] = new Method(Float._UnaryPlus, 0);
            Interpreter.Float.InstanceMethods["-@"] = new Method(Float._UnaryMinus, 0);
            Interpreter.Float.InstanceMethods["to_i"] = new Method(Float.to_i, 0);
            Interpreter.Float.InstanceMethods["to_f"] = new Method(Float.to_f, 0);

            // Proc
            Interpreter.Proc.InstanceMethods["call"] = new Method(Proc.call, null);

            // Array
            Interpreter.Array.InstanceMethods["[]"] = new Method(Array._Indexer, 1);
            Interpreter.Array.InstanceMethods["length"] = new Method(Array.length, 0);
            Interpreter.Array.InstanceMethods["count"] = new Method(Array.count, 0..1);
            Interpreter.Array.InstanceMethods["first"] = new Method(Array.first, 0);
            Interpreter.Array.InstanceMethods["last"] = new Method(Array.last, 0);
            Interpreter.Array.InstanceMethods["forty_two"] = new Method(Array.forty_two, 0);
            Interpreter.Array.InstanceMethods["sample"] = new Method(Array.sample, 0);
            Interpreter.Array.InstanceMethods["insert"] = new Method(Array.insert, 1..);
            Interpreter.Array.InstanceMethods["each"] = new Method(Array.each, 0);
            Interpreter.Array.InstanceMethods["reverse_each"] = new Method(Array.reverse_each, 0);
            Interpreter.Array.InstanceMethods["map"] = new Method(Array.map, 0);

            // Hash
            Interpreter.Hash.InstanceMethods["[]"] = new Method(Hash._Indexer, 1);
            Interpreter.Hash.InstanceMethods["initialize"] = new Method(Hash.initialize, 0..1);

            // Random
            Class RandomClass = Script.CreateClass("Random");
            RandomClass.Methods["rand"] = new Method(Random.rand, 0..1);
            RandomClass.Methods["srand"] = new Method(Random.srand, 0..1);

            //
            // UNSAFE APIS
            //

            // Global methods
            Interpreter.RootInstance.InstanceMethods["system"] = new Method(system, 1, isUnsafe: true);

            // File
            Module FileModule = Script.CreateModule("File");
            FileModule.Methods["read"] = new Method(File.read, 1, isUnsafe: true);
            FileModule.Methods["write"] = new Method(File.write, 2, isUnsafe: true);
        }

        public static readonly IReadOnlyDictionary<string, Method> DefaultClassAndInstanceMethods = new Dictionary<string, Method>() {
            {"==", new Method(ClassInstance._Equals, 1)},
            {"!=", new Method(ClassInstance._NotEquals, 1)},
            {"inspect", new Method(ClassInstance.inspect, 0)},
            {"class", new Method(ClassInstance.@class, 0)},
            {"to_s", new Method(ClassInstance.to_s, 0)},
            {"method", new Method(ClassInstance.method, 1)},
            {"object_id", new Method(ClassInstance.object_id, 0)},
        };
        public static readonly IReadOnlyDictionary<string, Method> DefaultInstanceMethods = new Dictionary<string, Method>() {
            {"attr_reader", new Method(ClassInstance.attr_reader, 1)},
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
        static async Task<Instances> raise(MethodInput Input) {
            throw new RuntimeException(Input.Arguments[0].String);
        }
        static async Task<Instances> @throw(MethodInput Input) {
            throw ThrowException.New(Input.Arguments[0]);
        }
        static async Task<Instances> @catch(MethodInput Input) {
            Method? OnYield = Input.OnYield ?? throw new RuntimeException($"{Input.Location}: No block given for catch");

            string CatchIdentifier = Input.Arguments[0].String;
            try {
                await OnYield.Call(Input.Script, Input.Instance);
            }
            catch (ThrowException Ex) {
                if (Ex.Identifier != CatchIdentifier)
                    throw Ex;
            }
            return Input.Interpreter.Nil;
        }
        static async Task<Instances> lambda(MethodInput Input) {
            Method? OnYield = Input.OnYield ?? throw new RuntimeException($"{Input.Location}: No block given for lambda");

            Instance NewProc = new ProcInstance(Input.Interpreter.Proc, new Method(
                async Input => await OnYield.Call(Input.Script, Input.Instance, Input.Arguments, Input.OnYield),
                null
            ));
            return NewProc;
        }
        static async Task<Instances> loop(MethodInput Input) {
            Method? OnYield = Input.OnYield ?? throw new RuntimeException($"{Input.Location}: No block given for loop");

            while (true) {
                try {
                    await OnYield.Call(Input.Script, Input.Instance, BreakHandleType: BreakHandleType.Rethrow);
                }
                catch (BreakException) {
                    break;
                }
            }
            return Input.Interpreter.Nil;
        }
        static async Task<Instances> system(MethodInput Input) {
            string Command = Input.Arguments[0].String;

            // Start command line process
            System.Diagnostics.ProcessStartInfo Info = new("cmd.exe") {
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = "/c " + Command
            };
            System.Diagnostics.Process Process = new() {
                StartInfo = Info
            };
            Process.Start();

            // Close in case it asks for input
            StreamWriter ProcessInput = Process.StandardInput;
            StreamReader ProcessOutput = Process.StandardOutput;
            ProcessInput.Close();

            // Get output
            await Process.WaitForExitAsync();
            string Output = await ProcessOutput.ReadToEndAsync();

            // Return output
            return new StringInstance(Input.Interpreter.String, Output);
        }
        static async Task<Instances> exit(MethodInput Input) {
            throw new ExitException();
        }
        static async Task<Instances> quit(MethodInput Input) {
            throw new ExitException();
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
                return (await Left.TryCallInstanceMethod(Input.Script, "==", Right)).SingleInstance.IsTruthy ? Input.Interpreter.False : Input.Interpreter.True;
            }
            public static async Task<Instances> inspect(MethodInput Input) {
                return new StringInstance(Input.Interpreter.String, Input.Instance.Inspect());
            }
            public static async Task<Instances> @class(MethodInput Input) {
                return new ModuleReference(Input.Instance.Module!);
            }
            public static async Task<Instances> to_s(MethodInput Input) {
                return new StringInstance(Input.Interpreter.String, Input.Instance.LightInspect());
            }
            public static async Task<Instances> method(MethodInput Input) {
                // Find method
                string MethodName = Input.Arguments[0].String;
                Method? FindMethod;
                bool Found;
                if (Input.Instance is PseudoInstance) {
                    Found = Input.Instance.Module!.Methods.TryGetValue(MethodName, out FindMethod);
                }
                else {
                    Found = Input.Instance.InstanceMethods.TryGetValue(MethodName, out FindMethod);
                }
                // Return method if found
                if (Found) {
                    if (!Input.Script.AllowUnsafeApi && FindMethod!.Unsafe) {
                        throw new RuntimeException($"{Input.Location}: The method '{MethodName}' is unavailable since 'AllowUnsafeApi' is disabled for this script.");
                    }
                    return new ProcInstance(Input.Interpreter.Proc, FindMethod!);
                }
                else {
                    throw new RuntimeException($"{Input.Location}: Undefined method '{MethodName}' for {Input.Instance.LightInspect()}");
                }
            }
            public static async Task<Instances> object_id(MethodInput Input) {
                return new IntegerInstance(Input.Interpreter.Integer, Input.Instance.ObjectId);
            }
            public static async Task<Instances> attr_reader(MethodInput Input) {
                string VariableName = Input.Arguments[0].String;
                // Prevent redefining unsafe API methods
                if (!Input.Script.AllowUnsafeApi && Input.Instance.InstanceMethods.TryGetValue(VariableName, out Method? ExistingMethod) && ExistingMethod.Unsafe) {
                    throw new RuntimeException($"{Input.Location}: The instance method '{VariableName}' cannot be redefined since 'AllowUnsafeApi' is disabled for this script.");
                }
                // Create or overwrite instance method
                Input.Instance.AddOrUpdateInstanceMethod(VariableName, new Method(async Input2 => {
                    return Input2.Instance.InstanceVariables[VariableName];
                }, 0));

                return Input.Interpreter.Nil;
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
            private static int _RealisticIndex(MethodInput Input, long RawIndex) {
                if (RawIndex < int.MinValue || RawIndex > int.MaxValue) {
                    throw new RuntimeException($"{Input.Script.ApproximateLocation}: Index ({RawIndex}) is too large for string.");
                }
                int Index = (int)RawIndex;
                return Index;
            }
            public static async Task<Instances> _Indexer(MethodInput Input) {
                // Get string and index
                string String = Input.Instance.String;
                int Index = _RealisticIndex(Input, Input.Arguments[0].Integer);

                // Return value at string index or nil
                if (Index >= 0 && Index < String.Length) {
                    return new StringInstance(Input.Interpreter.String, String[Index].ToString());
                }
                else if (Index < 0 && Index > -String.Length) {
                    return new StringInstance(Input.Interpreter.String, String[^(-Index)].ToString());
                }
                else {
                    return Input.Interpreter.Nil;
                }
            }
            public static async Task<Instances> initialize(MethodInput Input) {
                if (Input.Arguments.Count == 1) {
                    ((StringInstance)Input.Instance).SetValue(Input.Arguments[0].String);
                }
                return Input.Interpreter.Nil;
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
                    if (String.EndsWith("\r\n")) {
                        return new StringInstance(Input.Interpreter.String, String[0..^2]);
                    }
                    else if (String.EndsWith('\n') || String.EndsWith('\r')) {
                        return new StringInstance(Input.Interpreter.String, String[0..^1]);
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
                    return new FloatInstance(Interpreter.Float, Result);
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
            public static async Task<Instances> _LessThan(MethodInput Input) {
                Instance Left = Input.Instance;
                Instance Right = Input.Arguments[0];
                if (Right is IntegerInstance RightInteger && Left.Integer < RightInteger.Integer) {
                    return Input.Interpreter.True;
                }
                else if (Right is FloatInstance RightFloat && Left.Float < RightFloat.Float) {
                    return Input.Interpreter.True;
                }
                else {
                    return Input.Interpreter.False;
                }
            }
            public static async Task<Instances> _GreaterThan(MethodInput Input) {
                Instance Left = Input.Instance;
                Instance Right = Input.Arguments[0];
                if (Right is IntegerInstance RightInteger && Left.Integer > RightInteger.Integer) {
                    return Input.Interpreter.True;
                }
                else if (Right is FloatInstance RightFloat && Left.Float > RightFloat.Float) {
                    return Input.Interpreter.True;
                }
                else {
                    return Input.Interpreter.False;
                }
            }
            public static async Task<Instances> _UnaryPlus(MethodInput Input) {
                return Input.Instance;
            }
            public static async Task<Instances> _UnaryMinus(MethodInput Input) {
                return new IntegerInstance(Input.Interpreter.Integer, -Input.Instance.Integer);
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
                    // x.times do |n|
                    if (Input.OnYield.ArgumentNames.Count == 1) {
                        for (long i = 0; i < Times; i++) {
                            await Input.OnYield.Call(Input.Script, Input.Instance, new IntegerInstance(Input.Interpreter.Integer, i));
                        }
                    }
                    // x.times do
                    else {
                        for (long i = 0; i < Times; i++) {
                            await Input.OnYield.Call(Input.Script, Input.Instance);
                        }
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
            public static async Task<Instances> _UnaryPlus(MethodInput Input) {
                return Input.Instance;
            }
            public static async Task<Instances> _UnaryMinus(MethodInput Input) {
                return new FloatInstance(Input.Interpreter.Float, -Input.Instance.Float);
            }
            public static async Task<Instances> to_i(MethodInput Input) {
                return new IntegerInstance(Input.Interpreter.Integer, Input.Instance.Integer);
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
                    throw new RuntimeException($"{Input.Location}: No such file or directory: '{FilePath}'");
                }
                catch (Exception Ex) {
                    throw new RuntimeException($"{Input.Location}: Error reading file: '{Ex.Message}'");
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
                    throw new RuntimeException($"{Input.Location}: Error writing file: '{Ex.Message}'");
                }
            }
        }
        static class Proc {
            public static async Task<Instances> call(MethodInput Input) {
                return await Input.Instance.Proc.Call(Input.Script, Input.Instance, Input.Arguments, Input.OnYield);
            }
        }
        static class Array {
            private static async Task<Instances> _GetIndex(MethodInput Input, int ArrayIndex) {
                Instance Index = new IntegerInstance(Input.Interpreter.Integer, ArrayIndex);
                return await Input.Instance.InstanceMethods["[]"].Call(Input.Script, Input.Instance, new Instances(Index));
            }
            private static int _RealisticIndex(MethodInput Input, long RawIndex) {
                if (RawIndex < int.MinValue || RawIndex > int.MaxValue) {
                    throw new RuntimeException($"{Input.Script.ApproximateLocation}: Index ({RawIndex}) is too large for array.");
                }
                int Index = (int)RawIndex;
                return Index;
            }
            public static async Task<Instances> _Indexer(MethodInput Input) {
                // Get array and index
                List<Instance> Array = Input.Instance.Array;
                int Index = _RealisticIndex(Input, Input.Arguments[0].Integer);

                // Return value at array index or nil
                if (Index >= 0 && Index < Array.Count) {
                    return Array[Index];
                }
                else if (Index < 0 && Index > -Array.Count) {
                    return Array[^(-Index)];
                }
                else {
                    return Input.Interpreter.Nil;
                }
            }
            public static async Task<Instances> length(MethodInput Input) {
                List<Instance> Items = Input.Instance.Array;
                return new IntegerInstance(Input.Interpreter.Integer, Items.Count);
            }
            public static async Task<Instances> count(MethodInput Input) {
                if (Input.Arguments.Count == 0) {
                    return await length(Input);
                }
                else {
                    // Get the items and the item to count
                    List<Instance> Items = Input.Instance.Array;
                    Instance ItemToCount = Input.Arguments[0];

                    // Count how many times the item appears in the array
                    int Count = 0;
                    foreach (Instance Item in Items) {
                        Instances IsEqual = await Item.InstanceMethods["=="].Call(Input.Script, Item, ItemToCount);
                        if (IsEqual[0].IsTruthy) {
                            Count++;
                        }
                    }

                    // Return the count
                    return new IntegerInstance(Input.Interpreter.Integer, Count);
                }
            }
            public static async Task<Instances> first(MethodInput Input) {
                return await _GetIndex(Input, 0);
            }
            public static async Task<Instances> last(MethodInput Input) {
                return await _GetIndex(Input, -1);
            }
            public static async Task<Instances> forty_two(MethodInput Input) {
                return await _GetIndex(Input, 41);
            }
            public static async Task<Instances> sample(MethodInput Input) {
                List<Instance> Items = Input.Instance.Array;
                if (Items.Count != 0) {
                    return Items[Input.Interpreter.InternalRandom.Next(0, Items.Count)];
                }
                else {
                    return Input.Interpreter.Nil;
                }
            }
            public static async Task<Instances> insert(MethodInput Input) {
                List<Instance> Items = Input.Instance.Array;
                int Index = _RealisticIndex(Input, Input.Arguments[0].Integer);

                if (Input.Arguments.Count == 1) {
                    Items.Insert(Index, Input.Arguments[1]);
                }
                else {
                    Items.InsertRange(Index, Input.Arguments.MultiInstance.GetIndexRange(1));
                }
                return Input.Instance;
            }
            public static async Task<Instances> each(MethodInput Input) {
                if (Input.OnYield != null) {
                    List<Instance> Array = Input.Instance.Array;
                    // x.each do |n, i|
                    if (Input.OnYield.ArgumentNames.Count == 2) {
                        for (int i = 0; i < Array.Count; i++) {
                            await Input.OnYield.Call(Input.Script, Input.Instance, new List<Instance>() {Array[i], new IntegerInstance(Input.Interpreter.Integer, i)});
                        }
                    }
                    // x.each do |n|
                    else if (Input.OnYield.ArgumentNames.Count == 1) {
                        for (int i = 0; i < Array.Count; i++) {
                            await Input.OnYield.Call(Input.Script, Input.Instance, Array[i]);
                        }
                    }
                    // x.each do
                    else {
                        for (int i = 0; i < Array.Count; i++) {
                            await Input.OnYield.Call(Input.Script, Input.Instance);
                        }
                    }
                }
                return Input.Interpreter.Nil;
            }
            public static async Task<Instances> reverse_each(MethodInput Input) {
                if (Input.OnYield != null) {
                    List<Instance> Array = Input.Instance.Array;
                    // x.reverse_each do |n, i|
                    if (Input.OnYield.ArgumentNames.Count == 2) {
                        for (int i = Array.Count - 1; i >= 0; i--) {
                            await Input.OnYield.Call(Input.Script, Input.Instance, new List<Instance>() {Array[i], new IntegerInstance(Input.Interpreter.Integer, i)});
                        }
                    }
                    // x.reverse_each do |n|
                    else if (Input.OnYield.ArgumentNames.Count == 1) {
                        for (int i = Array.Count - 1; i >= 0; i--) {
                            await Input.OnYield.Call(Input.Script, Input.Instance, Array[i]);
                        }
                    }
                    // x.reverse_each do
                    else {
                        for (int i = Array.Count - 1; i >= 0; i--) {
                            await Input.OnYield.Call(Input.Script, Input.Instance);
                        }
                    }
                }
                return Input.Interpreter.Nil;
            }
            public static async Task<Instances> map(MethodInput Input) {
                if (Input.OnYield != null) {
                    List<Instance> Array = Input.Instance.Array;
                    List<Instance> MappedArray = new();
                    for (int i = 0; i < Array.Count; i++) {
                        Instance Item = Array[i];
                        Instance MappedItem = await Input.OnYield.Call(Input.Script, Input.Instance, Item);
                        MappedArray.Add(MappedItem);
                    }
                    return new ArrayInstance(Input.Interpreter.Array, MappedArray);
                }
                return Input.Instance;
            }
        }
        static class Hash {
            public static async Task<Instances> _Indexer(MethodInput Input) {
                // Get hash and key
                Dictionary<Instance, Instance> Hash = Input.Instance.Hash;
                Instance Key = Input.Arguments[0];

                // Return value at hash index or default value
                if (Hash.TryGetValue(Key, out Instance? Value)) {
                    return Value;
                }
                else {
                    foreach (KeyValuePair<Instance, Instance> Item in Hash) {
                        if ((await Item.Key.InstanceMethods["=="].Call(Input.Script, Item.Key, Key))[0].IsTruthy) {
                            return Item.Value;
                        }
                    }
                    return ((HashInstance)Input.Instance).DefaultValue;
                }
            }
            public static async Task<Instances> initialize(MethodInput Input) {
                if (Input.Arguments.Count == 1) {
                    ((HashInstance)Input.Instance).SetValue(Input.Instance.Hash, Input.Arguments[0]);
                }
                return Input.Interpreter.Nil;
            }
        }
        static class Random {
            public static async Task<Instances> rand(MethodInput Input) {
                // Integer random
                if (Input.Arguments.Count == 1 && Input.Arguments[0] is IntegerInstance) {
                    long IncludingMin = 0;
                    long ExcludingMax = Input.Arguments[0].Integer;
                    long RandomNumber = Input.Interpreter.Random.NextInt64(IncludingMin, ExcludingMax);
                    return new IntegerInstance(Input.Interpreter.Integer, RandomNumber);
                }
                // Float random
                else {
                    double IncludingMin = 0;
                    double ExcludingMax;
                    if (Input.Arguments.Count == 0) {
                        ExcludingMax = 1;
                    }
                    else {
                        ExcludingMax = Input.Arguments[0].Float;
                    }
                    double RandomNumber = Input.Interpreter.Random.NextDouble() * (ExcludingMax - IncludingMin) + IncludingMin;
                    return new FloatInstance(Input.Interpreter.Float, RandomNumber);
                }
            }
            public static async Task<Instances> srand(MethodInput Input) {
                long PreviousSeed = Input.Interpreter.RandomSeed;
                long NewSeed;
                if (Input.Arguments.Count == 1) {
                    NewSeed = Input.Arguments[0].Integer;
                }
                else {
                    NewSeed = Input.Interpreter.InternalRandom.NextInt64();
                }

                Input.Interpreter.RandomSeed = NewSeed;
                Input.Interpreter.Random = new System.Random(NewSeed.GetHashCode());

                return new IntegerInstance(Input.Interpreter.Integer, PreviousSeed);
            }
        }
    }
}
