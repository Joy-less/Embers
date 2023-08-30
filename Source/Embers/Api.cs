using System.Text;
using static Embers.Interpreter;

#pragma warning disable CS1998
#pragma warning disable IDE1006

namespace Embers
{
    public static class Api
    {
        public static void Setup(Interpreter Interpreter) {
            // Interpreter.RootInstance.Constants["Integer"].Class.Methods["+"] = new Method(Integer._Add, 1);

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

            // Integer
            Interpreter.Integer.InstanceMethods["+"] = new Method(Integer._Add, 1);
            Interpreter.Integer.InstanceMethods["-"] = new Method(Integer._Subtract, 1);
            Interpreter.Integer.InstanceMethods["*"] = new Method(Integer._Multiply, 1);
            Interpreter.Integer.InstanceMethods["/"] = new Method(Integer._Exponentiate, 1);
            Interpreter.Integer.InstanceMethods["%"] = new Method(Integer._Modulo, 1);
            Interpreter.Integer.InstanceMethods["**"] = new Method(Integer._Exponentiate, 1);
            Interpreter.Integer.InstanceMethods["times"] = new Method(Integer.times, 0);

            // Float
            Interpreter.Float.InstanceMethods["+"] = new Method(Float._Add, 1);
            Interpreter.Float.InstanceMethods["-"] = new Method(Float._Subtract, 1);
            Interpreter.Float.InstanceMethods["*"] = new Method(Float._Multiply, 1);
            Interpreter.Float.InstanceMethods["/"] = new Method(Float._Exponentiate, 1);
            Interpreter.Float.InstanceMethods["%"] = new Method(Float._Modulo, 1);
            Interpreter.Float.InstanceMethods["**"] = new Method(Float._Exponentiate, 1);
        }

        static async Task<Instances> puts(MethodInput Input) {
            if (Input.Arguments.Count != 0) {
                foreach (Instance Message in Input.Arguments) {
                    Console.WriteLine(Message.Object);
                }
            }
            else {
                Console.WriteLine();
            }
            return Input.Interpreter.Nil;
        }
        static async Task<Instances> print(MethodInput Input) {
            foreach (Instance Message in Input.Arguments) {
                Console.Write(Message.Object);
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
            string UserInput = Console.ReadLine() ?? "";
            return new StringInstance(Input.Interpreter.String, UserInput);
        }
        static async Task<Instances> getc(MethodInput Input) {
            string UserInput = Console.ReadKey().KeyChar.ToString();
            return new StringInstance(Input.Interpreter.String, UserInput);
        }
        static async Task<Instances> warn(MethodInput Input) {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            foreach (Instance Message in Input.Arguments) {
                Console.WriteLine(Message.Object);
            }
            Console.ResetColor();
            return Input.Interpreter.Nil;
        }
        static async Task<Instances> sleep(MethodInput Input) {
            if (Input.Arguments.Count == 1) {
                await Task.Delay(1);
            }
            else {
                await Task.Delay(Timeout.Infinite);
            }
            return Input.Interpreter.Nil;
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
        }
        static class Integer {
            static Instance _GetResult(Interpreter Interpreter, double Result, bool RightIsInteger) {
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
        }
    }
}
