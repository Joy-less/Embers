﻿using System.Text;
using System.Runtime.InteropServices;
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
            Interpreter.RootInstance.InstanceMethods["raise"] = new Method(@raise, 0..1);
            Interpreter.RootInstance.InstanceMethods["throw"] = new Method(@throw, 1);
            Interpreter.RootInstance.InstanceMethods["catch"] = new Method(@catch, 1);
            Interpreter.RootInstance.InstanceMethods["lambda"] = new Method(lambda, 0);
            Interpreter.RootInstance.InstanceMethods["loop"] = new Method(loop, 0);
            Interpreter.RootInstance.InstanceMethods["rand"] = new Method(_Random.rand, 0..1);
            Interpreter.RootInstance.InstanceMethods["srand"] = new Method(_Random.srand, 0..1);
            Interpreter.RootInstance.InstanceMethods["exit"] = new Method(exit, 0);
            Interpreter.RootInstance.InstanceMethods["quit"] = new Method(exit, 0);
            Interpreter.RootInstance.InstanceMethods["eval"] = new Method(eval, 1);

            // Global constants
            Interpreter.RootScope.Constants["EMBERS_VERSION"] = new StringInstance(Interpreter.String, Info.Version);
            Interpreter.RootScope.Constants["EMBERS_RELEASE_DATE"] = new StringInstance(Interpreter.String, Info.ReleaseDate);
            Interpreter.RootScope.Constants["EMBERS_PLATFORM"] = new StringInstance(Interpreter.String, $"{RuntimeInformation.OSArchitecture}-{RuntimeInformation.OSDescription}");
            Interpreter.RootScope.Constants["EMBERS_COPYRIGHT"] = new StringInstance(Interpreter.String, Info.Copyright);
            Interpreter.RootScope.Constants["RUBY_COPYRIGHT"] = new StringInstance(Interpreter.String, Info.RubyCopyright);

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
            Interpreter.String.InstanceMethods["to_a"] = new Method(String.to_a, 0);
            Interpreter.String.InstanceMethods["chomp"] = new Method(String.chomp, 0..1);
            Interpreter.String.InstanceMethods["strip"] = new Method(String.strip, 0);
            Interpreter.String.InstanceMethods["lstrip"] = new Method(String.lstrip, 0);
            Interpreter.String.InstanceMethods["rstrip"] = new Method(String.rstrip, 0);
            Interpreter.String.InstanceMethods["squeeze"] = new Method(String.squeeze, 0);
            Interpreter.String.InstanceMethods["chop"] = new Method(String.chop, 0);
            Interpreter.String.InstanceMethods["chr"] = new Method(String.chr, 0);
            Interpreter.String.InstanceMethods["capitalize"] = new Method(String.capitalize, 0);
            Interpreter.String.InstanceMethods["upcase"] = new Method(String.upcase, 0);
            Interpreter.String.InstanceMethods["downcase"] = new Method(String.downcase, 0);
            Interpreter.String.InstanceMethods["sub"] = new Method(String.sub, 2);
            Interpreter.String.InstanceMethods["gsub"] = new Method(String.gsub, 2);

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

            // Range
            Interpreter.Range.InstanceMethods["min"] = new Method(Range.min, 0);
            Interpreter.Range.InstanceMethods["max"] = new Method(Range.max, 0);
            Interpreter.Range.InstanceMethods["each"] = new Method(Range.each, 0);
            Interpreter.Range.InstanceMethods["reverse_each"] = new Method(Range.reverse_each, 0);
            Interpreter.Range.InstanceMethods["length"] = new Method(Range.length, 0);
            Interpreter.Range.InstanceMethods["to_a"] = new Method(Range.to_a, 0);

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
            Interpreter.Array.InstanceMethods["contains?"] = new Method(Array.contains, 1);
            Interpreter.Array.InstanceMethods["include?"] = new Method(Array.contains, 1);

            // Hash
            Interpreter.Hash.InstanceMethods["[]"] = new Method(Hash._Indexer, 1);
            Interpreter.Hash.InstanceMethods["initialize"] = new Method(Hash.initialize, 0..1);
            Interpreter.Hash.InstanceMethods["has_key?"] = new Method(Hash.has_key, 1);
            Interpreter.Hash.InstanceMethods["has_value?"] = new Method(Hash.has_value, 1);
            Interpreter.Hash.InstanceMethods["keys"] = new Method(Hash.keys, 0);
            Interpreter.Hash.InstanceMethods["values"] = new Method(Hash.values, 0);
            Interpreter.Hash.InstanceMethods["invert"] = new Method(Hash.invert, 0);
            Interpreter.Hash.InstanceMethods["to_a"] = new Method(Hash.to_a, 0);
            Interpreter.Hash.InstanceMethods["to_hash"] = new Method(Hash.to_hash, 0);

            // Random
            Class RandomClass = Script.CreateClass("Random");
            RandomClass.Methods["rand"] = new Method(_Random.rand, 0..1);
            RandomClass.Methods["srand"] = new Method(_Random.srand, 0..1);

            // Math
            Module MathModule = Script.CreateModule("Math");
            MathModule.Constants["PI"] = new FloatInstance(Interpreter.Float, Math.PI);
            MathModule.Constants["E"] = new FloatInstance(Interpreter.Float, Math.E);
            MathModule.Methods["sin"] = new Method(_Math.sin, 1);
            MathModule.Methods["cos"] = new Method(_Math.cos, 1);
            MathModule.Methods["tan"] = new Method(_Math.tan, 1);
            MathModule.Methods["asin"] = new Method(_Math.asin, 1);
            MathModule.Methods["acos"] = new Method(_Math.acos, 1);
            MathModule.Methods["atan"] = new Method(_Math.atan, 1);
            MathModule.Methods["atan2"] = new Method(_Math.atan2, 2);
            MathModule.Methods["sinh"] = new Method(_Math.sinh, 1);
            MathModule.Methods["cosh"] = new Method(_Math.cosh, 1);
            MathModule.Methods["tanh"] = new Method(_Math.tanh, 1);
            MathModule.Methods["asinh"] = new Method(_Math.asinh, 1);
            MathModule.Methods["acosh"] = new Method(_Math.acosh, 1);
            MathModule.Methods["atanh"] = new Method(_Math.atanh, 1);
            MathModule.Methods["exp"] = new Method(_Math.exp, 1);
            MathModule.Methods["log"] = new Method(_Math.log, 2);
            MathModule.Methods["log10"] = new Method(_Math.log10, 1);
            MathModule.Methods["log2"] = new Method(_Math.log2, 1);
            MathModule.Methods["frexp"] = new Method(_Math.frexp, 1);
            MathModule.Methods["ldexp"] = new Method(_Math.ldexp, 2);
            MathModule.Methods["sqrt"] = new Method(_Math.sqrt, 1);
            MathModule.Methods["cbrt"] = new Method(_Math.cbrt, 1);
            MathModule.Methods["hypot"] = new Method(_Math.hypot, 2);
            MathModule.Methods["erf"] = new Method(_Math.erf, 1);
            MathModule.Methods["erfc"] = new Method(_Math.erfc, 1);
            MathModule.Methods["gamma"] = new Method(_Math.gamma, 1);
            MathModule.Methods["lgamma"] = new Method(_Math.lgamma, 1);

            // Exception
            Interpreter.Exception.InstanceMethods["initialize"] = new Method(_Exception.initialize, 0..1);
            Interpreter.Exception.InstanceMethods["message"] = new Method(_Exception.message, 0);

            //
            // UNSAFE APIS
            //

            // Global methods
            Interpreter.RootInstance.InstanceMethods["system"] = new Method(system, 1, IsUnsafe: true);

            // File
            Module FileModule = Script.CreateModule("File");
            FileModule.Methods["read"] = new Method(File.read, 1, IsUnsafe: true);
            FileModule.Methods["write"] = new Method(File.write, 2, IsUnsafe: true);
        }

        public static readonly IReadOnlyDictionary<string, Method> DefaultClassAndInstanceMethods = new Dictionary<string, Method>() {
            {"==", new Method(ClassInstance._Equals, 1)},
            {"!=", new Method(ClassInstance._NotEquals, 1)},
            {"inspect", new Method(ClassInstance.inspect, 0)},
            {"class", new Method(ClassInstance.@class, 0)},
            {"to_s", new Method(ClassInstance.to_s, 0)},
            {"method", new Method(ClassInstance.method, 1)},
            {"object_id", new Method(ClassInstance.object_id, 0)},
            {"methods", new Method(ClassInstance.methods, 0)},
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
            ConsoleColor PreviousForegroundColour = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            foreach (Instance Message in Input.Arguments) {
                Console.WriteLine(Message.Object);
            }
            Console.ForegroundColor = PreviousForegroundColour;
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
            if (Input.Arguments.Count == 1) {
                Instance Argument = Input.Arguments[0];
                if (Argument is ExceptionInstance ExceptionInstance) {
                    Exception ExceptionToRaise = Argument.Exception;
                    Input.Script.ExceptionsTable.TryAdd(ExceptionToRaise, ExceptionInstance);
                    throw ExceptionToRaise;
                }
                else {
                    throw new RuntimeException(Argument.String);
                }
            }
            else {
                throw new RuntimeException("");
            }
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
                catch (LoopControlException Ex) when (Ex is RetryException || Ex is RedoException || Ex is NextException) {
                    continue;
                }
                catch (LoopControlException Ex) {
                    throw new SyntaxErrorException($"{Input.Script.ApproximateLocation}: {Ex.GetType().Name} not valid in loop do end");
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
        static async Task<Instances> eval(MethodInput Input) {
            return await Input.Script.InternalEvaluateAsync(Input.Arguments[0].String);
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
            public static async Task<Instances> methods(MethodInput Input) {
                List<Instance> MethodsDictToSymbolsArray(Dictionary<string, Method> MethodDict) {
                    List<Instance> Symbols = new();
                    foreach (string MethodName in MethodDict.Keys) {
                        Symbols.Add(Input.Script.GetSymbol(MethodName));
                    }
                    return Symbols;
                }
                // Get class methods
                if (Input.Instance is ModuleReference ModuleReference) {
                    return new ArrayInstance(Input.Interpreter.Array, MethodsDictToSymbolsArray(ModuleReference.Module!.Methods));
                }
                // Get instance methods
                else {
                    return new ArrayInstance(Input.Interpreter.Array, MethodsDictToSymbolsArray(Input.Instance.InstanceMethods));
                }
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
            public static async Task<Instances> attr_writer(MethodInput Input) {
                string VariableName = Input.Arguments[0].String;
                // Prevent redefining unsafe API methods
                if (!Input.Script.AllowUnsafeApi && Input.Instance.InstanceMethods.TryGetValue(VariableName, out Method? ExistingMethod) && ExistingMethod.Unsafe) {
                    throw new RuntimeException($"{Input.Location}: The instance method '{VariableName}' cannot be redefined since 'AllowUnsafeApi' is disabled for this script.");
                }
                // Create or overwrite instance method
                Input.Instance.AddOrUpdateInstanceMethod($"{VariableName}=", new Method(async Input2 => {
                    return Input2.Instance.InstanceVariables[VariableName] = Input2.Arguments[0];
                }, 1));

                return Input.Interpreter.Nil;
            }
            public static async Task<Instances> attr_accessor(MethodInput Input) {
                await attr_writer(Input);
                return await attr_reader(Input);
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
                Instance Indexer = Input.Arguments[0];

                if (Indexer is RangeInstance RangeIndexer) {
                    // Return substring in range
                    int StartIndex = RangeIndexer.Min != null ? _RealisticIndex(Input, RangeIndexer.Min.Integer) : 0;
                    int EndIndex = RangeIndexer.Max != null ? _RealisticIndex(Input, RangeIndexer.Max.Integer) : String.Length - 1;
                    if (StartIndex < 0) StartIndex = 0;
                    if (EndIndex >= String.Length) EndIndex = String.Length - 1;

                    return new StringInstance(Input.Interpreter.String, String[StartIndex..(EndIndex + 1)]);
                }
                else {
                    int Index = _RealisticIndex(Input, Input.Arguments[0].Integer);

                    // Return character at string index or nil
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
            public static async Task<Instances> to_a(MethodInput Input) {
                List<Instance> Array = new();
                foreach (char Chara in Input.Instance.String) {
                    Array.Add(new StringInstance(Input.Interpreter.Array, Chara.ToString()));
                }
                return new ArrayInstance(Input.Interpreter.Array, Array);
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
            static async Task<Instances> ModifyString(MethodInput Input, Func<string, string> Modifier) {
                string OriginalString = Input.Instance.String;
                string ModifiedString = Modifier(OriginalString);
                if (ModifiedString != OriginalString) {
                    return new StringInstance(Input.Interpreter.String, ModifiedString);
                }
                return Input.Instance;
            }
            public static async Task<Instances> strip(MethodInput Input) {
                return await ModifyString(Input, Str => Str.Trim());
            }
            public static async Task<Instances> lstrip(MethodInput Input) {
                return await ModifyString(Input, Str => Str.TrimStart());
            }
            public static async Task<Instances> rstrip(MethodInput Input) {
                return await ModifyString(Input, Str => Str.TrimEnd());
            }
            public static async Task<Instances> squeeze(MethodInput Input) {
                return await ModifyString(Input, Str => {
                    StringBuilder SqueezedString = new();
                    char? LastChara = null;
                    for (int i = 0; i < Str.Length; i++) {
                        char Chara = Str[i];
                        if (Chara != LastChara) {
                            LastChara = Chara;
                            SqueezedString.Append(Chara);
                        }
                    }
                    return SqueezedString.ToString();
                });
            }
            public static async Task<Instances> chop(MethodInput Input) {
                return await ModifyString(Input, Str => Str.Length != 0 ? Str[..^1] : Str);
            }
            public static async Task<Instances> chr(MethodInput Input) {
                return await ModifyString(Input, Str => Str.Length != 0 ? Str[0].ToString() : Str);
            }
            public static async Task<Instances> capitalize(MethodInput Input) {
                return await ModifyString(Input, Str => {
                    if (Str.Length == 0) {
                        return Str;
                    }
                    else if (Str.Length == 1) {
                        return char.ToUpperInvariant(Str[0]).ToString();
                    }
                    else {
                        return char.ToUpperInvariant(Str[0]) + Str[1..].ToLowerInvariant();
                    }
                });
            }
            public static async Task<Instances> upcase(MethodInput Input) {
                return await ModifyString(Input, Str => Str.ToUpperInvariant());
            }
            public static async Task<Instances> downcase(MethodInput Input) {
                return await ModifyString(Input, Str => Str.ToLowerInvariant());
            }
            public static async Task<Instances> sub(MethodInput Input) {
                string Replace = Input.Arguments[0].String;
                string With = Input.Arguments[1].String;
                return await ModifyString(Input, Str => Str.ReplaceFirst(Replace, With));
            }
            public static async Task<Instances> gsub(MethodInput Input) {
                string Replace = Input.Arguments[0].String;
                string With = Input.Arguments[1].String;
                return await ModifyString(Input, Str => Str.Replace(Replace, With));
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
                    bool TakesArgument = Input.OnYield.ArgumentNames.Count == 1;

                    for (long i = 0; i < Times; i++) {
                        try {
                            // x.times do |n|
                            if (TakesArgument) {
                                await Input.OnYield.Call(Input.Script, Input.Instance, new IntegerInstance(Input.Interpreter.Integer, i), BreakHandleType: BreakHandleType.Rethrow);
                            }
                            // x.times do
                            else {
                                await Input.OnYield.Call(Input.Script, Input.Instance, BreakHandleType: BreakHandleType.Rethrow);
                            }
                        }
                        catch (BreakException) {
                            break;
                        }
                        catch (RedoException) {
                            i--;
                            continue;
                        }
                        catch (NextException) {
                            continue;
                        }
                        catch (LoopControlException Ex) {
                            throw new SyntaxErrorException($"{Input.Script.ApproximateLocation}: {Ex.GetType().Name} not valid in {Times}.times do end");
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
        static class Range {
            public static async Task<Instances> min(MethodInput Input) {
                return ((RangeInstance)Input.Instance).AppliedMin;
            }
            public static async Task<Instances> max(MethodInput Input) {
                return ((RangeInstance)Input.Instance).AppliedMax;
            }
            public static async Task<Instances> each(MethodInput Input) {
                if (Input.OnYield != null) {
                    LongRange Range = Input.Instance.Range;
                    long Min = (long)(Range.Min != null ? Range.Min : 0);
                    long Max = (long)(Range.Max != null ? Range.Max : throw new RuntimeException($"{Input.Script.ApproximateLocation}: Cannot call 'each' on range if max is endless"));
                    
                    bool TakesArgument = Input.OnYield.ArgumentNames.Count == 1;
                    for (long i = Min; i <= Max; i++) {
                        try {
                            // x.each do |n|
                            if (TakesArgument) {
                                await Input.OnYield.Call(Input.Script, Input.Instance, new IntegerInstance(Input.Interpreter.Integer, i), BreakHandleType: BreakHandleType.Rethrow);
                            }
                            // x.each do
                            else {
                                await Input.OnYield.Call(Input.Script, Input.Instance, BreakHandleType: BreakHandleType.Rethrow);
                            }
                        }
                        catch (BreakException) {
                            break;
                        }
                        catch (RedoException) {
                            i--;
                            continue;
                        }
                        catch (NextException) {
                            continue;
                        }
                        catch (LoopControlException Ex) {
                            throw new SyntaxErrorException($"{Input.Script.ApproximateLocation}: {Ex.GetType().Name} not valid in range.each do end");
                        }
                    }
                }
                return Input.Interpreter.Nil;
            }
            public static async Task<Instances> reverse_each(MethodInput Input) {
                if (Input.OnYield != null) {
                    LongRange Range = Input.Instance.Range;
                    long Min = (long)(Range.Min != null ? Range.Min : 0);
                    long Max = (long)(Range.Max != null ? Range.Max : throw new RuntimeException($"{Input.Script.ApproximateLocation}: Cannot call 'reverse_each' on range if max is endless"));
                    
                    bool TakesArgument = Input.OnYield.ArgumentNames.Count == 1;
                    for (long i = Max; i >= Min; i--) {
                        try {
                            // x.reverse_each do |n|
                            if (TakesArgument) {
                                await Input.OnYield.Call(Input.Script, Input.Instance, new IntegerInstance(Input.Interpreter.Integer, i), BreakHandleType: BreakHandleType.Rethrow);
                            }
                            // x.reverse_each do
                            else {
                                await Input.OnYield.Call(Input.Script, Input.Instance, BreakHandleType: BreakHandleType.Rethrow);
                            }
                        }
                        catch (BreakException) {
                            break;
                        }
                        catch (RedoException) {
                            i--;
                            continue;
                        }
                        catch (NextException) {
                            continue;
                        }
                        catch (LoopControlException Ex) {
                            throw new SyntaxErrorException($"{Input.Script.ApproximateLocation}: {Ex.GetType().Name} not valid in range.reverse_each do end");
                        }
                    }
                }
                return Input.Interpreter.Nil;
            }
            public static async Task<Instances> to_a(MethodInput Input) {
                List<Instance> Array = new();
                LongRange Range = Input.Instance.Range;
                long Min = (long)(Range.Min != null ? Range.Min : 0);
                long Max = (long)(Range.Max != null ? Range.Max : throw new RuntimeException($"{Input.Script.ApproximateLocation}: Cannot call 'to_a' on range if max is endless"));
                for (long i = Min; i <= Max; i++) {
                    Array.Add(new IntegerInstance(Input.Interpreter.Integer, i));
                }
                return new ArrayInstance(Input.Interpreter.Array, Array);
            }
            public static async Task<Instances> length(MethodInput Input) {
                LongRange Range = Input.Instance.Range;
                if (Range.Min != null && Range.Max != null) {
                    return new IntegerInstance(Input.Interpreter.Integer, (long)Range.Max - (long)Range.Min + 1);
                }
                else {
                    return Input.Interpreter.Nil;
                }
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
                Instance Indexer = Input.Arguments[0];

                if (Indexer is RangeInstance RangeIndexer) {
                    // Return values in range
                    int StartIndex = RangeIndexer.Min != null ? _RealisticIndex(Input, RangeIndexer.Min.Integer) : 0;
                    int EndIndex = RangeIndexer.Max != null ? _RealisticIndex(Input, RangeIndexer.Max.Integer) : Array.Count - 1;
                    if (StartIndex < 0) StartIndex = 0;
                    if (EndIndex >= Array.Count) EndIndex = Array.Count - 1;

                    return new ArrayInstance(Input.Interpreter.Array, Array.GetIndexRange(StartIndex, EndIndex));
                }
                else {
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
                    Items.Add(Input.Arguments[0]);
                }
                else if (Input.Arguments.Count == 2) {
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
                    
                    int TakesArguments = Input.OnYield.ArgumentNames.Count;
                    for (int i = 0; i < Array.Count; i++) {
                        try {
                            // x.each do |n, i|
                            if (TakesArguments == 2) {
                                await Input.OnYield.Call(Input.Script, Input.Instance, new List<Instance>() { Array[i], new IntegerInstance(Input.Interpreter.Integer, i) }, BreakHandleType: BreakHandleType.Rethrow);
                            }
                            // x.each do |n|
                            else if (TakesArguments == 1) {
                                await Input.OnYield.Call(Input.Script, Input.Instance, Array[i], BreakHandleType: BreakHandleType.Rethrow);
                            }
                            // x.each do
                            else {
                                await Input.OnYield.Call(Input.Script, Input.Instance, BreakHandleType: BreakHandleType.Rethrow);
                            }
                        }
                        catch (BreakException) {
                            break;
                        }
                        catch (RedoException) {
                            i--;
                            continue;
                        }
                        catch (NextException) {
                            continue;
                        }
                        catch (LoopControlException Ex) {
                            throw new SyntaxErrorException($"{Input.Script.ApproximateLocation}: {Ex.GetType().Name} not valid in array.each do end");
                        }
                    }
                }
                return Input.Interpreter.Nil;
            }
            public static async Task<Instances> reverse_each(MethodInput Input) {
                if (Input.OnYield != null) {
                    List<Instance> Array = Input.Instance.Array;
                    
                    int TakesArguments = Input.OnYield.ArgumentNames.Count;
                    for (int i = Array.Count - 1; i >= 0; i--) {
                        try {
                            // x.reverse_each do |n, i|
                            if (TakesArguments == 2) {
                                await Input.OnYield.Call(Input.Script, Input.Instance, new List<Instance>() { Array[i], new IntegerInstance(Input.Interpreter.Integer, i) }, BreakHandleType: BreakHandleType.Rethrow);
                            }
                            // x.reverse_each do |n|
                            else if (TakesArguments == 1) {
                                await Input.OnYield.Call(Input.Script, Input.Instance, Array[i], BreakHandleType: BreakHandleType.Rethrow);
                            }
                            // x.reverse_each do
                            else {
                                await Input.OnYield.Call(Input.Script, Input.Instance, BreakHandleType: BreakHandleType.Rethrow);
                            }
                        }
                        catch (BreakException) {
                            break;
                        }
                        catch (RedoException) {
                            i--;
                            continue;
                        }
                        catch (NextException) {
                            continue;
                        }
                        catch (LoopControlException Ex) {
                            throw new SyntaxErrorException($"{Input.Script.ApproximateLocation}: {Ex.GetType().Name} not valid in array.reverse_each do end");
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
            public static async Task<Instances> contains(MethodInput Input) {
                Instance ItemToFind = Input.Arguments[0];
                foreach (Instance Item in Input.Instance.Array) {
                    if ((await Item.InstanceMethods["=="].Call(Input.Script, Item, ItemToFind))[0].IsTruthy) {
                        return Input.Interpreter.True;
                    }
                }
                return Input.Interpreter.False;
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
            public static async Task<Instances> has_key(MethodInput Input) {
                Instance ItemToFind = Input.Arguments[0];
                foreach (Instance Item in Input.Instance.Hash.Keys) {
                    if ((await Item.InstanceMethods["=="].Call(Input.Script, Item, ItemToFind))[0].IsTruthy) {
                        return Input.Interpreter.True;
                    }
                }
                return Input.Interpreter.False;
            }
            public static async Task<Instances> has_value(MethodInput Input) {
                Instance ItemToFind = Input.Arguments[0];
                foreach (Instance Item in Input.Instance.Hash.Values) {
                    if ((await Item.InstanceMethods["=="].Call(Input.Script, Item, ItemToFind))[0].IsTruthy) {
                        return Input.Interpreter.True;
                    }
                }
                return Input.Interpreter.False;
            }
            public static async Task<Instances> keys(MethodInput Input) {
                return new ArrayInstance(Input.Interpreter.Array, Input.Instance.Hash.Keys.ToList());
            }
            public static async Task<Instances> values(MethodInput Input) {
                return new ArrayInstance(Input.Interpreter.Array, Input.Instance.Hash.Values.ToList());
            }
            public static async Task<Instances> invert(MethodInput Input) {
                HashInstance Hash = (HashInstance)Input.Instance;
                Dictionary<Instance, Instance> Inverted = Hash.Hash.ToDictionary(kv => kv.Value, kv => kv.Key);
                return new HashInstance(Input.Interpreter.Hash, Inverted, Hash.DefaultValue);
            }
            public static async Task<Instances> to_a(MethodInput Input) {
                List<Instance> Array = new();
                foreach (KeyValuePair<Instance, Instance> Item in Input.Instance.Hash) {
                    Array.Add(new ArrayInstance(Input.Interpreter.Array, new List<Instance>() { Item.Key, Item.Value }));
                }
                return new ArrayInstance(Input.Interpreter.Array, Array);
            }
            public static async Task<Instances> to_hash(MethodInput Input) {
                return Input.Instance;
            }
        }
        static class _Random {
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
                Input.Interpreter.Random = new Random(NewSeed.GetHashCode());

                return new IntegerInstance(Input.Interpreter.Integer, PreviousSeed);
            }
        }
        static class _Math {
            public static async Task<Instances> sin(MethodInput Input) {
                return new FloatInstance(Input.Interpreter.Float, Math.Sin(Input.Arguments[0].Float));
            }
            public static async Task<Instances> cos(MethodInput Input) {
                return new FloatInstance(Input.Interpreter.Float, Math.Cos(Input.Arguments[0].Float));
            }
            public static async Task<Instances> tan(MethodInput Input) {
                return new FloatInstance(Input.Interpreter.Float, Math.Tan(Input.Arguments[0].Float));
            }
            public static async Task<Instances> asin(MethodInput Input) {
                return new FloatInstance(Input.Interpreter.Float, Math.Asin(Input.Arguments[0].Float));
            }
            public static async Task<Instances> acos(MethodInput Input) {
                return new FloatInstance(Input.Interpreter.Float, Math.Acos(Input.Arguments[0].Float));
            }
            public static async Task<Instances> atan(MethodInput Input) {
                return new FloatInstance(Input.Interpreter.Float, Math.Atan(Input.Arguments[0].Float));
            }
            public static async Task<Instances> atan2(MethodInput Input) {
                return new FloatInstance(Input.Interpreter.Float, Math.Atan2(Input.Arguments[0].Float, Input.Arguments[1].Float));
            }
            public static async Task<Instances> sinh(MethodInput Input) {
                return new FloatInstance(Input.Interpreter.Float, Math.Sinh(Input.Arguments[0].Float));
            }
            public static async Task<Instances> cosh(MethodInput Input) {
                return new FloatInstance(Input.Interpreter.Float, Math.Cosh(Input.Arguments[0].Float));
            }
            public static async Task<Instances> tanh(MethodInput Input) {
                return new FloatInstance(Input.Interpreter.Float, Math.Tanh(Input.Arguments[0].Float));
            }
            public static async Task<Instances> asinh(MethodInput Input) {
                return new FloatInstance(Input.Interpreter.Float, Math.Asinh(Input.Arguments[0].Float));
            }
            public static async Task<Instances> acosh(MethodInput Input) {
                return new FloatInstance(Input.Interpreter.Float, Math.Acosh(Input.Arguments[0].Float));
            }
            public static async Task<Instances> atanh(MethodInput Input) {
                return new FloatInstance(Input.Interpreter.Float, Math.Atanh(Input.Arguments[0].Float));
            }
            public static async Task<Instances> exp(MethodInput Input) {
                return new FloatInstance(Input.Interpreter.Float, Math.Exp(Input.Arguments[0].Float));
            }
            public static async Task<Instances> log(MethodInput Input) {
                return new FloatInstance(Input.Interpreter.Float, Math.Log(Input.Arguments[0].Float, Input.Arguments[1].Float));
            }
            public static async Task<Instances> log10(MethodInput Input) {
                return new FloatInstance(Input.Interpreter.Float, Math.Log10(Input.Arguments[0].Float));
            }
            public static async Task<Instances> log2(MethodInput Input) {
                return new FloatInstance(Input.Interpreter.Float, Math.Log2(Input.Arguments[0].Float));
            }
            public static async Task<Instances> frexp(MethodInput Input) {
                double Value = Input.Arguments[0].Float;

                // Calculate fractional exponent
                // From https://stackoverflow.com/a/390072
                long Bits = BitConverter.DoubleToInt64Bits(Value);
                bool Negative = Bits < 0;
                int Exponent = (int)((Bits >> 52) & 0x7ffL);
                long Mantissa = Bits & 0xfffffffffffffL;
                if (Exponent == 0) Exponent++;
                else Mantissa |= 1L << 52;
                if (Mantissa == 0)
                    return new ArrayInstance(Input.Interpreter.Array, new List<Instance>() {
                        new FloatInstance(Input.Interpreter.Float, 0),
                        new IntegerInstance(Input.Interpreter.Integer, 0)
                    });
                Exponent -= 1075;
                while ((Mantissa & 1) == 0) {
                    Mantissa >>= 1;
                    Exponent++;
                }
                double M = Mantissa;
                long E = Exponent;
                while (M >= 1) {
                    M /= 2.0;
                    E += 1;
                }
                if (Negative) M = -M;

                // Return [mantissa, exponent]
                return new ArrayInstance(Input.Interpreter.Array, new List<Instance>() {
                    new FloatInstance(Input.Interpreter.Float, M),
                    new IntegerInstance(Input.Interpreter.Integer, E)
                });
            }
            public static async Task<Instances> ldexp(MethodInput Input) {
                double Fraction = Input.Arguments[0].Float;
                long Exponent = Input.Arguments[1].Integer;
                return new FloatInstance(Input.Interpreter.Float, Fraction * Math.Pow(2, Exponent));
            }
            public static async Task<Instances> sqrt(MethodInput Input) {
                return new FloatInstance(Input.Interpreter.Float, Math.Sqrt(Input.Arguments[0].Float));
            }
            public static async Task<Instances> cbrt(MethodInput Input) {
                return new FloatInstance(Input.Interpreter.Float, Math.Cbrt(Input.Arguments[0].Float));
            }
            public static async Task<Instances> hypot(MethodInput Input) {
                double A = Input.Arguments[0].Float;
                double B = Input.Arguments[1].Float;
                return new FloatInstance(Input.Interpreter.Float, Math.Sqrt(Math.Pow(A, 2) + Math.Pow(B, 2)));
            }
            private static double _Erf(double x) {
                // Approximate error function
                // From https://www.johndcook.com/blog/csharp_erf

                // constants
                double a1 = 0.254829592;
                double a2 = -0.284496736;
                double a3 = 1.421413741;
                double a4 = -1.453152027;
                double a5 = 1.061405429;
                double p = 0.3275911;

                // Save the sign of x
                int sign = 1;
                if (x < 0)
                    sign = -1;
                x = Math.Abs(x);

                // A&S formula 7.1.26
                double t = 1.0 / (1.0 + p * x);
                double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x);

                return sign * y;
            }
            public static async Task<Instances> erf(MethodInput Input) {
                return new FloatInstance(Input.Interpreter.Float, _Erf(Input.Arguments[0].Float));
            }
            public static async Task<Instances> erfc(MethodInput Input) {
                return new FloatInstance(Input.Interpreter.Float, 1.0 - _Erf(Input.Arguments[0].Float));
            }
            private static double _Gamma(double z) {
                // Approximate gamma
                // From https://stackoverflow.com/a/66193379
                const int g = 7;
                double[] p = { 0.99999999999980993, 676.5203681218851, -1259.1392167224028, 771.32342877765313, -176.61502916214059, 12.507343278686905, -0.13857109526572012, 9.9843695780195716e-6, 1.5056327351493116e-7 };
                if (z < 0.5)
                    return Math.PI / (Math.Sin(Math.PI * z) * _Gamma(1 - z));
                z -= 1;
                double x = p[0];
                for (var i = 1; i < g + 2; i++)
                    x += p[i] / (z + i);
                double t = z + g + 0.5;
                return Math.Sqrt(2 * Math.PI) * (Math.Pow(t, z + 0.5)) * Math.Exp(-t) * x;
            }
            public static async Task<Instances> gamma(MethodInput Input) {
                return new FloatInstance(Input.Interpreter.Float, _Gamma(Input.Arguments[0].Float));
            }
            public static async Task<Instances> lgamma(MethodInput Input) {
                double Value = Input.Arguments[0].Float;
                double GammaValue = _Gamma(Value);
                double A = Math.Log(Math.Abs(GammaValue));
                long B = GammaValue < 0 ? -1 : 1;
                return new ArrayInstance(Input.Interpreter.Float, new List<Instance>() {
                    new FloatInstance(Input.Interpreter.Float, A),
                    new IntegerInstance(Input.Interpreter.Float, B)
                });
            }
        }
        static class _Exception {
            public static async Task<Instances> initialize(MethodInput Input) {
                if (Input.Arguments.Count == 1) {
                    ((ExceptionInstance)Input.Instance).SetValue(Input.Arguments[0].String);
                }
                return Input.Interpreter.Nil;
            }
            public static async Task<Instances> message(MethodInput Input) {
                return new StringInstance(Input.Interpreter.String, Input.Instance.Exception.Message);
            }
        }
    }
}
