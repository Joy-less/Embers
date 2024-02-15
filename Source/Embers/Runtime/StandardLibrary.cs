using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

#pragma warning disable IDE1006

namespace Embers {
    public sealed class StandardLibrary {
        internal static void Setup(Axis Axis) {
            void SetGlobalMethod(string Name, Delegate Delegate) {
                Axis.Object.SetClassMethod(Name, Delegate, AccessModifier.Private);
                Axis.Object.SetInstanceMethod(Name, Delegate, AccessModifier.Private);
            }

            // Global
            SetGlobalMethod("puts", _Global.puts);
            SetGlobalMethod("print", _Global.print);
            SetGlobalMethod("p", _Global.p);
            SetGlobalMethod("warn", _Global.warn);
            SetGlobalMethod("gets", _Global.gets);
            SetGlobalMethod("getc", _Global.getc);
            SetGlobalMethod("sleep", _Global.sleep);
            SetGlobalMethod("loop", _Global.loop);
            SetGlobalMethod("eval", _Global.eval);
            SetGlobalMethod("raise", _Global.raise);
            SetGlobalMethod("throw", _Global.@throw);
            SetGlobalMethod("catch", _Global.@catch);
            SetGlobalMethod("lambda", _Global.lambda);
            SetGlobalMethod("exit", _Global.exit);
            SetGlobalMethod("quit", _Global.exit);
            SetGlobalMethod("local_variables", _Global.local_variables);
            SetGlobalMethod("global_variables", _Global.global_variables);
            SetGlobalMethod("rand", _Global.rand);
            SetGlobalMethod("srand", _Global.srand);
            SetGlobalMethod("public", _Global.@public);
            SetGlobalMethod("private", _Global.@private);
            SetGlobalMethod("protected", _Global.@protected);
            SetGlobalMethod("attr_reader", _Global.attr_reader);
            SetGlobalMethod("attr_writer", _Global.attr_writer);
            SetGlobalMethod("attr_accessor", _Global.attr_accessor);

            // Object
            // (Class)
            Axis.Object.SetClassMethod("==", _Object._Equals);
            Axis.Object.SetClassMethod("!=", _Object._NotEquals);
            Axis.Object.SetClassMethod("===", _Object._CaseEquals);
            Axis.Object.SetClassMethod("<=>", _Object._Spaceship);
            Axis.Object.SetClassMethod("to_s", _Object.to_s);
            Axis.Object.SetClassMethod("inspect", _Object.inspect);
            Axis.Object.SetClassMethod("hash", _Object.hash);
            Axis.Object.SetClassMethod("class", _Object.@class);
            Axis.Object.SetClassMethod("object_id", _Object.object_id);
            Axis.Object.SetClassMethod("method", _Object.method);
            Axis.Object.SetClassMethod("send", _Object.send);
            Axis.Object.SetClassMethod("is_a?", _Object.is_a7);
            Axis.Object.SetClassMethod("instance_of?", _Object.instance_of7);
            Axis.Object.SetClassMethod("in?", _Object.in7);
            Axis.Object.SetClassMethod("eql?", _Object.eql7);
            Axis.Object.SetClassMethod("clone", _Object.clone);
            Axis.Object.SetClassMethod("nil?", _Object.nil7);
            // (Instance)
            Axis.Object.SetInstanceMethod("==", _Object._Equals);
            Axis.Object.SetInstanceMethod("!=", _Object._NotEquals);
            Axis.Object.SetInstanceMethod("===", _Object._CaseEquals);
            Axis.Object.SetInstanceMethod("<=>", _Object._Spaceship);
            Axis.Object.SetInstanceMethod("to_s", _Object.to_s);
            Axis.Object.SetInstanceMethod("inspect", _Object.inspect);
            Axis.Object.SetInstanceMethod("hash", _Object.hash);
            Axis.Object.SetInstanceMethod("class", _Object.@class);
            Axis.Object.SetInstanceMethod("object_id", _Object.object_id);
            Axis.Object.SetInstanceMethod("method", _Object.method);
            Axis.Object.SetInstanceMethod("send", _Object.send);
            Axis.Object.SetInstanceMethod("is_a?", _Object.is_a7);
            Axis.Object.SetInstanceMethod("instance_of?", _Object.instance_of7);
            Axis.Object.SetInstanceMethod("in?", _Object.in7);
            Axis.Object.SetInstanceMethod("eql?", _Object.eql7);
            Axis.Object.SetInstanceMethod("clone", _Object.clone);
            Axis.Object.SetInstanceMethod("nil?", _Object.nil7);
            Axis.Object.SetInstanceMethod("methods", _Object.methods);
            Axis.Object.SetInstanceMethod("instance_variables", _Object.instance_variables);
            Axis.Object.SetInstanceMethod("instance_methods", _Object.instance_methods);
            // (Constants)
            Axis.Object.SetConstant("RUBY_VERSION", $"Embers {Info.Version}");
            Axis.Object.SetConstant("RUBY_RELEASE_DATE", Info.ReleaseDate);
            Axis.Object.SetConstant("RUBY_COPYRIGHT", $"{Info.Copyright}, {Info.RubyCopyright}");
            Axis.Object.SetConstant("RUBY_PLATFORM", $"{RuntimeInformation.OSArchitecture}-{RuntimeInformation.OSDescription}");
#if DEBUG
            Axis.Object.SetConstant("DEBUG", true);
#else
            Axis.Object.SetConstant("DEBUG", false);
#endif
            Axis.Object.SetConstant("Object", Axis.Object);
            Axis.Object.SetConstant("Module", Axis.Module);
            Axis.Object.SetConstant("Class", Axis.Class);
            Axis.Object.SetConstant("NilClass", Axis.NilClass);
            Axis.Object.SetConstant("TrueClass", Axis.TrueClass);
            Axis.Object.SetConstant("FalseClass", Axis.FalseClass);
            Axis.Object.SetConstant("String", Axis.String);
            Axis.Object.SetConstant("Symbol", Axis.Symbol);
            Axis.Object.SetConstant("Integer", Axis.Integer);
            Axis.Object.SetConstant("Float", Axis.Float);
            Axis.Object.SetConstant("Proc", Axis.Proc);
            Axis.Object.SetConstant("Array", Axis.Array);
            Axis.Object.SetConstant("Hash", Axis.Hash);
            Axis.Object.SetConstant("Time", Axis.Time);
            Axis.Object.SetConstant("Range", Axis.Range);
            Axis.Object.SetConstant("Enumerator", Axis.Enumerator);
            Axis.Object.SetConstant("Exception", Axis.Exception);
            Axis.Object.SetConstant("WeakRef", Axis.WeakRef);
            Axis.Object.SetConstant("Thread", Axis.Thread);
            Axis.Object.SetConstant("Math", Axis.Math);
            Axis.Object.SetConstant("GC", Axis.GC);
            Axis.Object.SetConstant("File", Axis.File);

            // Module
            // (Class)
            Axis.Module.SetClassMethod("name", _Module.name);
            Axis.Module.SetClassMethod("methods", _Module.methods);
            Axis.Module.SetClassMethod("constants", _Module.constants);
            Axis.Module.SetClassMethod("class_variables", _Module.class_variables);
            Axis.Module.SetClassMethod("class_methods", _Module.class_methods);

            // Class
            // (Class)
            Axis.Class.SetClassMethod("===", _Class._CaseEquals);
            Axis.Class.SetClassMethod("superclass", _Class.superclass);
            Axis.Class.SetClassMethod("instance_variables", _Class.instance_variables);
            Axis.Class.SetClassMethod("instance_methods", _Class.instance_methods);

            // NilClass
            // (Instance)
            Axis.NilClass.SetInstanceMethod("inspect", _NilClass.inspect);

            // TrueClass
            // (Instance)
            Axis.TrueClass.SetInstanceMethod("to_s", _TrueClass.to_s);
            Axis.TrueClass.SetInstanceMethod("inspect", _TrueClass.inspect);

            // FalseClass
            // (Instance)
            Axis.FalseClass.SetInstanceMethod("to_s", _FalseClass.to_s);
            Axis.FalseClass.SetInstanceMethod("inspect", _FalseClass.inspect);

            // String
            // (Instance)
            Axis.String.SetInstanceMethod("[]", _String._Index);
            Axis.String.SetInstanceMethod("[]=", _String._SetIndex);
            Axis.String.SetInstanceMethod("+", _String._Add);
            Axis.String.SetInstanceMethod("*", _String._Multiply);
            Axis.String.SetInstanceMethod("==", _String._Equals);
            Axis.String.SetInstanceMethod("<", _String._LessThan);
            Axis.String.SetInstanceMethod("<=", _String._LessThanOrEqualTo);
            Axis.String.SetInstanceMethod(">=", _String._GreaterThanOrEqualTo);
            Axis.String.SetInstanceMethod(">", _String._GreaterThan);
            Axis.String.SetInstanceMethod("<=>", _String._Spaceship);
            Axis.String.SetInstanceMethod("to_sym", _String.to_sym);
            Axis.String.SetInstanceMethod("to_i", _String.to_i);
            Axis.String.SetInstanceMethod("to_f", _String.to_f);
            Axis.String.SetInstanceMethod("to_a", _String.to_a);
            Axis.String.SetInstanceMethod("inspect", _String.inspect);
            Axis.String.SetInstanceMethod("length", _String.length);
            Axis.String.SetInstanceMethod("count", _String.count);
            Axis.String.SetInstanceMethod("chomp", _String.chomp);
            Axis.String.SetInstanceMethod("chomp!", _String.chomp1);
            Axis.String.SetInstanceMethod("chop", _String.chop);
            Axis.String.SetInstanceMethod("chop!", _String.chop1);
            Axis.String.SetInstanceMethod("strip", _String.strip);
            Axis.String.SetInstanceMethod("strip!", _String.strip1);
            Axis.String.SetInstanceMethod("lstrip", _String.lstrip);
            Axis.String.SetInstanceMethod("lstrip!", _String.lstrip1);
            Axis.String.SetInstanceMethod("rstrip", _String.rstrip);
            Axis.String.SetInstanceMethod("rstrip!", _String.rstrip1);
            Axis.String.SetInstanceMethod("squeeze", _String.squeeze);
            Axis.String.SetInstanceMethod("squeeze!", _String.squeeze1);
            Axis.String.SetInstanceMethod("capitalize", _String.capitalize);
            Axis.String.SetInstanceMethod("capitalize!", _String.capitalize1);
            Axis.String.SetInstanceMethod("upcase", _String.upcase);
            Axis.String.SetInstanceMethod("upcase!", _String.upcase1);
            Axis.String.SetInstanceMethod("downcase", _String.downcase);
            Axis.String.SetInstanceMethod("downcase!", _String.downcase1);
            Axis.String.SetInstanceMethod("sub", _String.sub);
            Axis.String.SetInstanceMethod("sub!", _String.sub1);
            Axis.String.SetInstanceMethod("gsub", _String.gsub);
            Axis.String.SetInstanceMethod("gsub!", _String.gsub1);
            Axis.String.SetInstanceMethod("split", _String.split);
            Axis.String.SetInstanceMethod("chr", _String.chr);
            Axis.String.SetInstanceMethod("include?", _String.include7);
            Axis.String.SetInstanceMethod("contain?", _String.include7);
            Axis.String.SetInstanceMethod("eql?", _String.eql7);

            // Symbol
            // (Instance)
            Axis.Symbol.SetInstanceMethod("inspect", _Symbol.inspect);
            Axis.Symbol.SetInstanceMethod("length", _Symbol.length);

            // Integer
            // (Instance)
            Axis.Integer.SetInstanceMethod("+", _Integer._Add);
            Axis.Integer.SetInstanceMethod("-", _Integer._Sub);
            Axis.Integer.SetInstanceMethod("*", _Integer._Mul);
            Axis.Integer.SetInstanceMethod("/", _Integer._Div);
            Axis.Integer.SetInstanceMethod("%", _Integer._Mod);
            Axis.Integer.SetInstanceMethod("**", _Integer._Pow);
            Axis.Integer.SetInstanceMethod("+@", _Integer._UnaryPlus);
            Axis.Integer.SetInstanceMethod("-@", _Integer._UnaryMinus);
            Axis.Integer.SetInstanceMethod("==", _Float._Equals);
            Axis.Integer.SetInstanceMethod("<", _Float._LessThan);
            Axis.Integer.SetInstanceMethod("<=", _Float._LessThanOrEqualTo);
            Axis.Integer.SetInstanceMethod(">=", _Float._GreaterThanOrEqualTo);
            Axis.Integer.SetInstanceMethod(">", _Float._GreaterThan);
            Axis.Integer.SetInstanceMethod("<=>", _Float._Spaceship);
            Axis.Integer.SetInstanceMethod("to_i", _Integer.to_i);
            Axis.Integer.SetInstanceMethod("to_f", _Integer.to_f);
            Axis.Integer.SetInstanceMethod("clamp", _Integer.clamp);
            Axis.Integer.SetInstanceMethod("floor", _Integer.floor);
            Axis.Integer.SetInstanceMethod("ceil", _Integer.ceil);
            Axis.Integer.SetInstanceMethod("round", _Integer.round);
            Axis.Integer.SetInstanceMethod("truncate", _Integer.truncate);
            Axis.Integer.SetInstanceMethod("abs", _Integer.abs);
            Axis.Integer.SetInstanceMethod("times", _Integer.times);
            Axis.Integer.SetInstanceMethod("upto", _Integer.upto);
            Axis.Integer.SetInstanceMethod("downto", _Integer.downto);

            // Float
            // (Instance)
            Axis.Float.SetInstanceMethod("+", _Float._Add);
            Axis.Float.SetInstanceMethod("-", _Float._Sub);
            Axis.Float.SetInstanceMethod("*", _Float._Mul);
            Axis.Float.SetInstanceMethod("/", _Float._Div);
            Axis.Float.SetInstanceMethod("%", _Float._Mod);
            Axis.Float.SetInstanceMethod("**", _Float._Pow);
            Axis.Float.SetInstanceMethod("+@", _Float._UnaryPlus);
            Axis.Float.SetInstanceMethod("-@", _Float._UnaryMinus);
            Axis.Float.SetInstanceMethod("==", _Float._Equals);
            Axis.Float.SetInstanceMethod("<", _Float._LessThan);
            Axis.Float.SetInstanceMethod("<=", _Float._LessThanOrEqualTo);
            Axis.Float.SetInstanceMethod(">=", _Float._GreaterThanOrEqualTo);
            Axis.Float.SetInstanceMethod(">", _Float._GreaterThan);
            Axis.Float.SetInstanceMethod("<=>", _Float._Spaceship);
            Axis.Float.SetInstanceMethod("to_i", _Float.to_i);
            Axis.Float.SetInstanceMethod("to_f", _Float.to_f);
            Axis.Float.SetInstanceMethod("clamp", _Float.clamp);
            Axis.Float.SetInstanceMethod("floor", _Float.floor);
            Axis.Float.SetInstanceMethod("ceil", _Float.ceil);
            Axis.Float.SetInstanceMethod("round", _Float.round);
            Axis.Float.SetInstanceMethod("truncate", _Float.truncate);
            Axis.Float.SetInstanceMethod("abs", _Float.abs);
            // (Constants)
            Axis.Float.SetConstant("INFINITY", Float.Infinity);
            Axis.Float.SetConstant("NAN", Float.NaN);

            // Proc
            // (Instance)
            Axis.Proc.SetInstanceMethod("call", _Proc.call);

            // Array
            // (Instance)
            Axis.Array.SetInstanceMethod("[]", _Array._Index);
            Axis.Array.SetInstanceMethod("[]=", _Array._SetIndex);
            Axis.Array.SetInstanceMethod("*", _Array._Mul);
            Axis.Array.SetInstanceMethod("<<", _Array._Append);
            Axis.Array.SetInstanceMethod("==", _Array._Equals);
            Axis.Array.SetInstanceMethod("to_s", _Array.to_s);
            Axis.Array.SetInstanceMethod("inspect", _Array.inspect);
            Axis.Array.SetInstanceMethod("length", _Array.length);
            Axis.Array.SetInstanceMethod("count", _Array.count);
            Axis.Array.SetInstanceMethod("push", _Array._Append);
            Axis.Array.SetInstanceMethod("append", _Array._Append);
            Axis.Array.SetInstanceMethod("prepend", _Array.prepend);
            Axis.Array.SetInstanceMethod("pop", _Array.pop);
            Axis.Array.SetInstanceMethod("insert", _Array.insert);
            Axis.Array.SetInstanceMethod("delete", _Array.delete);
            Axis.Array.SetInstanceMethod("remove", _Array.delete);
            Axis.Array.SetInstanceMethod("delete_at", _Array.delete_at);
            Axis.Array.SetInstanceMethod("remove_at", _Array.delete_at);
            Axis.Array.SetInstanceMethod("uniq", _Array.uniq);
            Axis.Array.SetInstanceMethod("uniq!", _Array.uniq1);
            Axis.Array.SetInstanceMethod("first", _Array.first);
            Axis.Array.SetInstanceMethod("last", _Array.last);
            Axis.Array.SetInstanceMethod("forty_two", _Array.forty_two);
            Axis.Array.SetInstanceMethod("sample", _Array.sample);
            Axis.Array.SetInstanceMethod("min", _Array.min);
            Axis.Array.SetInstanceMethod("max", _Array.max);
            Axis.Array.SetInstanceMethod("sum", _Array.sum);
            Axis.Array.SetInstanceMethod("each", _Array.each);
            Axis.Array.SetInstanceMethod("reverse_each", _Array.reverse_each);
            Axis.Array.SetInstanceMethod("shuffle", _Array.shuffle);
            Axis.Array.SetInstanceMethod("shuffle!", _Array.shuffle1);
            Axis.Array.SetInstanceMethod("sort", _Array.sort);
            Axis.Array.SetInstanceMethod("sort!", _Array.sort1);
            Axis.Array.SetInstanceMethod("map", _Array.map);
            Axis.Array.SetInstanceMethod("map!", _Array.map1);
            Axis.Array.SetInstanceMethod("reverse", _Array.reverse);
            Axis.Array.SetInstanceMethod("reverse!", _Array.reverse1);
            Axis.Array.SetInstanceMethod("join", _Array.join);
            Axis.Array.SetInstanceMethod("clear", _Array.clear);
            Axis.Array.SetInstanceMethod("include?", _Array.include7);
            Axis.Array.SetInstanceMethod("contain?", _Array.include7);
            Axis.Array.SetInstanceMethod("empty?", _Array.empty7);

            // Hash
            // (Class)
            Axis.Hash.SetClassMethod("new", _Hash.@new);
            // (Instance)
            Axis.Hash.SetInstanceMethod("[]", _Hash._Index);
            Axis.Hash.SetInstanceMethod("[]=", _Hash._SetIndex);
            Axis.Hash.SetInstanceMethod("==", _Hash._Equals);
            Axis.Hash.SetInstanceMethod("to_s", _Hash.to_s);
            Axis.Hash.SetInstanceMethod("to_a", _Hash.to_a);
            Axis.Hash.SetInstanceMethod("inspect", _Hash.inspect);
            Axis.Hash.SetInstanceMethod("length", _Hash.length);
            Axis.Hash.SetInstanceMethod("count", _Hash.length);
            Axis.Hash.SetInstanceMethod("has_key?", _Hash.has_key7);
            Axis.Hash.SetInstanceMethod("has_value?", _Hash.has_value7);
            Axis.Hash.SetInstanceMethod("keys", _Hash.keys);
            Axis.Hash.SetInstanceMethod("values", _Hash.values);
            Axis.Hash.SetInstanceMethod("delete", _Hash.delete);
            Axis.Hash.SetInstanceMethod("remove", _Hash.delete);
            Axis.Hash.SetInstanceMethod("clear", _Hash.clear);
            Axis.Hash.SetInstanceMethod("each", _Hash.each);
            Axis.Hash.SetInstanceMethod("reverse_each", _Hash.reverse_each);
            Axis.Hash.SetInstanceMethod("invert", _Hash.invert);
            Axis.Hash.SetInstanceMethod("empty?", _Hash.empty7);

            // Time
            // (Class)
            Axis.Time.SetClassMethod("new", _Time.@new);
            Axis.Time.SetClassMethod("now", _Time.now);
            Axis.Time.SetClassMethod("at", _Time.at);
            // (Instance)
            Axis.Time.SetInstanceMethod("to_s", _Time.to_s);
            Axis.Time.SetInstanceMethod("to_i", _Time.to_i);
            Axis.Time.SetInstanceMethod("to_f", _Time.to_f);

            // Range
            // (Instance)
            Axis.Range.SetInstanceMethod("min", _Range.min);
            Axis.Range.SetInstanceMethod("max", _Range.max);
            Axis.Range.SetInstanceMethod("exclude_end?", _Range.exclude_end7);
            Axis.Range.SetInstanceMethod("each", _Range.each);
            Axis.Range.SetInstanceMethod("reverse_each", _Range.reverse_each);
            Axis.Range.SetInstanceMethod("to_a", _Range.to_a);
            Axis.Range.SetInstanceMethod("length", _Range.length);
            Axis.Range.SetInstanceMethod("count", _Range.length);

            // Enumerator
            // (Instance)
            Axis.Enumerator.SetInstanceMethod("each", _Enumerator.each);
            Axis.Enumerator.SetInstanceMethod("step", _Enumerator.step);
            Axis.Enumerator.SetInstanceMethod("next", _Enumerator.next);
            Axis.Enumerator.SetInstanceMethod("peek", _Enumerator.peek);

            // Exception
            // (Class)
            Axis.Exception.SetClassMethod("new", _Exception.@new);
            // (Instance)
            Axis.Exception.SetInstanceMethod("to_s", _Exception.to_s);
            Axis.Exception.SetInstanceMethod("inspect", _Exception.inspect);
            Axis.Exception.SetInstanceMethod("message", _Exception.message);
            Axis.Exception.SetInstanceMethod("backtrace", _Exception.backtrace);

            // WeakRef
            // (Class)
            Axis.WeakRef.SetClassMethod("new", _WeakRef.@new);
            // (Instance)
            Axis.WeakRef.SetInstanceMethod("to_s", _WeakRef.to_s);
            Axis.WeakRef.SetInstanceMethod("inspect", _WeakRef.inspect);
            Axis.WeakRef.SetInstanceMethod("weakref_alive?", _WeakRef.weakref_alive7);
            Axis.WeakRef.SetInstanceMethod("method_missing", _WeakRef.method_missing);

            // Thread
            // (Class)
            Axis.Thread.SetClassMethod("new", _Thread.@new);
            // (Instance)
            Axis.Thread.SetInstanceMethod("stop", _Thread.stop);
            Axis.Thread.SetInstanceMethod("join", _Thread.join);

            // Math
            // (Class)
            Axis.Math.SetConstant("PI", Math.PI);
            Axis.Math.SetConstant("E", Math.E);
            Axis.Math.SetConstant("TAU", Math.Tau);
            Axis.Math.SetClassMethod("sin", Math.Sin);
            Axis.Math.SetClassMethod("cos", Math.Cos);
            Axis.Math.SetClassMethod("tan", Math.Tan);
            Axis.Math.SetClassMethod("asin", Math.Asin);
            Axis.Math.SetClassMethod("acos", Math.Acos);
            Axis.Math.SetClassMethod("atan", Math.Atan);
            Axis.Math.SetClassMethod("atan2", Math.Atan2);
            Axis.Math.SetClassMethod("sinh", Math.Sinh);
            Axis.Math.SetClassMethod("cosh", Math.Cosh);
            Axis.Math.SetClassMethod("tanh", Math.Tanh);
            Axis.Math.SetClassMethod("asinh", Math.Asinh);
            Axis.Math.SetClassMethod("acosh", Math.Acosh);
            Axis.Math.SetClassMethod("atanh", Math.Atanh);
            Axis.Math.SetClassMethod("exp", Math.Exp);
            Axis.Math.SetClassMethod("log", _Math.log);
            Axis.Math.SetClassMethod("log10", Math.Log10);
            Axis.Math.SetClassMethod("log2", _Math.log2);
            Axis.Math.SetClassMethod("sqrt", Math.Sqrt);
            Axis.Math.SetClassMethod("cbrt", Math.Cbrt);
            Axis.Math.SetClassMethod("hypot", _Math.hypot);
            Axis.Math.SetClassMethod("to_rad", _Math.to_rad);
            Axis.Math.SetClassMethod("to_deg", _Math.to_deg);
            Axis.Math.SetClassMethod("lerp", _Math.lerp);
            Axis.Math.SetClassMethod("abs", _Math.abs);

            // GC
            // (Class)
            Axis.GC.SetClassMethod("start", _GC.start);
            Axis.GC.SetClassMethod("count", _GC.count);

            // File
            // (Class)
            if (!Axis.Options.Sandbox) {
                Axis.File.SetClassMethod("read", _File.read);
                Axis.File.SetClassMethod("write", _File.write);
                Axis.File.SetClassMethod("append", _File.append);
                Axis.File.SetClassMethod("delete", _File.delete);
                Axis.File.SetClassMethod("exist?", _File.exist7);
                Axis.File.SetClassMethod("absolute_path", _File.absolute_path);
                Axis.File.SetClassMethod("absolute_path?", _File.absolute_path7);
                Axis.File.SetClassMethod("basename", _File.basename);
                Axis.File.SetClassMethod("dirname", _File.dirname);
            }
        }

        public static class _Global {
            public static void puts(Context Context, params Instance[] Messages) {
                var logger = Context.Axis.Globals.Logger;
                if (Messages.Length == 0) {
                    logger.WriteLine();
                    return;
                }
                foreach (Instance Message in Messages) {
                    logger.WriteLine(Message.ToS());
                }
            }
            public static void print(Context Context, params Instance[] Messages) {
                var logger = Context.Axis.Globals.Logger;
                foreach (Instance Message in Messages) {
                    logger.Write(Message.ToS());
                }
            }
            public static void p(Context Context, params Instance[] Messages) {
                var logger = Context.Axis.Globals.Logger;
                if (Messages.Length == 0) {
                    logger.WriteLine();
                    return;
                }
                foreach (Instance Message in Messages) {
                    logger.WriteLine(Message.Inspect());
                }
            }
            public static void warn(Context Context, params Instance[] Messages) {
                var logger = Context.Axis.Globals.Logger;
                if (Messages.Length == 0) {
                    logger.WriteLine();
                    return;
                }
                foreach (Instance Message in Messages) {
                    logger.WriteLine(Message.ToS());
                }
            }
            public static string gets(Context Context) {
                string? Input = Context.Axis.Globals.Logger.ReadLine();
                return Input is not null ? Input + "\n" : "";
            }
            public static char getc(Context Context, bool Print = true) {
                return Context.Axis.Globals.Logger.ReadKey(Print);
            }
            public static async Task sleep(double? Duration) {
                await Task.Delay(Duration is not null
                    ? TimeSpan.FromSeconds(Duration.Value)
                    : Timeout.InfiniteTimeSpan);
            }
            public static Instance loop(Context Context, [Block] Proc? Block) {
                if (Block is null) {
                    throw new RuntimeError($"{Context.Location}: no block given for loop");
                }
                while (true) {
                    // Call block
                    Instance Instance = Block.Call();

                    // Control code
                    if (Instance is ControlCode ControlCode) {
                        switch (ControlCode.Type) {
                            // Break
                            case ControlType.Break:
                                return Context.Axis.Nil;
                            // Next, Redo, Retry
                            case ControlType.Next or ControlType.Redo or ControlType.Retry:
                                continue;
                            // Return
                            case ControlType.Return:
                                return ControlCode;
                            // Invalid
                            default:
                                throw new SyntaxError($"{Context.Location}: invalid {ControlCode}");
                        }
                    }
                }
            }
            public static Instance eval(Context Context, string Code) {
                return Context.Scope.Evaluate(Code);
            }
            public static Instance raise(Context Context, Instance? Argument = null) {
                if (Argument is null) {
                    throw new Exception();
                }
                else if (Argument.Value is string Message) {
                    throw new Exception(Message);
                }
                else if (Argument.Value is Exception Exception) {
                    throw Exception;
                }
                else {
                    throw new RuntimeError($"{Context.Location}: expected string or exception for raise");
                }
            }
            public static Instance @throw(Context Context, Instance Identifier, Instance? Argument = null) {
                throw new ThrowError(Identifier, Argument);
            }
            public static Instance @catch(Context Context, Instance Identifier, [Block] Proc? Block) {
                if (Block is null) {
                    throw new RuntimeError($"{Context.Location}: no block given for catch");
                }
                try {
                    return Block.Call();
                }
                catch (ThrowError ThrowError) when (ThrowError.Identifier.CallMethod("==", Identifier).Truthy) {
                    return ThrowError.Argument ?? Context.Axis.Nil;
                }
            }
            public static Proc lambda(Context Context, [Block] Proc? Block) {
                if (Block is null) {
                    throw new RuntimeError($"{Context.Location}: no block given for lambda");
                }
                return Block;
            }
            public static void exit(int ExitCode = 0) {
                Environment.Exit(ExitCode);
            }
            public static Array local_variables(Context Context) {
                return Adapter.GetArray(Context, Context.Scope.GetVariableNames());
            }
            public static Array global_variables(Context Context) {
                return Adapter.GetArray(Context, (ICollection)Context.Axis.Globals.GlobalVariables.Keys);
            }
            public static object rand(Context Context, Instance? Argument = null) {
                Random Random = Context.Axis.Globals.Random;
                // Integer random
                if (Argument?.Value is Integer Integer) {
                    return Random.NextInt64(0, (long)Integer);
                }
                // Range random
                else if (Argument?.Value is InstanceRange Range) {
                    // Float range random
                    if (Range.Min.Value is Float || Range.Max.Value is Float) {
                        return Random.NextDouble() * (Range.Max.CastFloat - (Range.ExcludeEnd ? 1 : 0) - Range.Min.CastFloat) + Range.Min.CastFloat;
                    }
                    // Integer range random
                    else {
                        return Random.NextInt64((long)Range.Min.CastInteger, (long)Range.Max.CastInteger + (Range.ExcludeEnd ? 0 : 1));
                    }
                }
                // Float random
                else {
                    Float Max = Argument?.Value is not null ? Argument.CastFloat : 1;
                    return Random.NextDouble() * Max;
                }
            }
            public static Integer srand(Context Context, Integer? Seed = null) {
                // Get previous seed and new seed
                Integer PreviousSeed = Context.Axis.Globals.RandomSeed;
                Integer NewSeed = Seed?.Value ?? Context.Axis.Globals.Random.NextInt64();
                // Set new seed
                Context.Axis.Globals.RandomSeed = NewSeed;
                Context.Axis.Globals.Random = new Random(NewSeed.GetHashCode());
                // Return previous seed
                return PreviousSeed;
            }
            public static void @public(Context Context) {
                Context.Locals.AccessModifier = AccessModifier.Public;
            }
            public static void @private(Context Context) {
                Context.Locals.AccessModifier = AccessModifier.Private;
            }
            public static void @protected(Context Context) {
                Context.Locals.AccessModifier = AccessModifier.Protected;
            }
            public static void attr_reader(Context Context, string InstanceVariableName) {
                Class Class = Context.Module.CastClass;
                Class.SetInstanceMethod(InstanceVariableName, (Context Context) =>
                    Class.GetInstanceVariable("@" + InstanceVariableName)
                );
            }
            public static void attr_writer(Context Context, string InstanceVariableName) {
                Class Class = Context.Module.CastClass;
                Class.SetInstanceMethod(InstanceVariableName + "=", (Context Context, Instance Value) =>
                    Class.SetInstanceVariable("@" + InstanceVariableName, Value)
                );
            }
            public static void attr_accessor(Context Context, string InstanceVariableName) {
                attr_reader(Context, InstanceVariableName);
                attr_writer(Context, InstanceVariableName);
            }
        }
        public static class _Object {
            public static bool _Equals(Context Context, Instance Other) {
                return Equals(Context.Instance.Value, Other.Value);
            }
            public static bool _NotEquals(Context Context, Instance Other) {
                return Context.Instance.CallMethod("==", Other).Falsey;
            }
            public static bool _CaseEquals(Context Context, Instance Other) {
                return Context.Instance.CallMethod("==", Other).Truthy;
            }
            public static int? _Spaceship(Context Context, Instance Other) {
                return Equals(Context.Instance.Value, Other.Value) ? 0 : null;
            }
            public static string to_s(Context Context) {
                return Context.Instance.ToString();
            }
            public static string inspect(Context Context) {
                return Context.Instance.ToString();
            }
            public static int hash(Context Context) {
                // (class.hash * 37) + (value.hash)
                return unchecked ((Context.Instance.Class.GetHashCode() * 37) + (Context.Instance.Value?.GetHashCode() ?? 0));
            }
            public static Class @class(Context Context) {
                return Context.Instance.Class;
            }
            public static Proc method(Context Context, string MethodName) {
                Method Method = Context.Instance.GetMethod(MethodName)
                    ?? throw new RuntimeError($"{Context.Location}: undefined method '{MethodName}' for {Context.Instance.Describe()}");
                return new Proc(Context.Scope, Context.Instance, Method);
            }
            public static Instance send(Context Context, string MethodName, [Splat] Instance[] Arguments, [Block] Proc? Block) {
                return Context.Instance.CallMethod(
                    new Context(Context.Location, Context.Scope, Context.Module, Context.Instance, Block, Context.Method),
                    MethodName, Arguments
                );
            }
            public static long object_id(Context Context) {
                return Context.Instance.ObjectId;
            }
            public static bool is_a7(Context Context, Module Module) {
                Module CurrentModule = Module;
                while (true) {
                    if (Context.Instance.Class == CurrentModule) {
                        return true;
                    }
                    else if (CurrentModule.SuperClass is not null) {
                        CurrentModule = CurrentModule.SuperClass;
                    }
                    else {
                        return false;
                    }
                }
            }
            public static bool instance_of7(Context Context, Module Module) {
                return Context.Instance.Class == Module;
            }
            public static bool in7(Context Context, Array Array) {
                return Array.Inner.FirstOrDefault(Item => Item.CallMethod(Context, "==", Context.Instance).Truthy) is not null;
            }
            public static bool eql7(Context Context, Instance Other) {
                return Context.Instance.Hash() == Other.Hash();
            }
            public static Instance clone(Context Context) {
                return new Instance(Context.Instance.Class, Context.Instance.Value);
            }
            public static bool nil7(Context Context) {
                return Context.Instance.Value is null;
            }
            public static Array methods(Context Context) {
                return Adapter.GetArray(Context, Context.Instance.Class.GetInstanceMethodNames());
            }
            public static Array instance_variables(Context Context) {
                return Adapter.GetArray(Context, Context.Instance.Class.GetInstanceVariableNames());
            }
            public static Array instance_methods(Context Context) {
                return Adapter.GetArray(Context, Context.Instance.Class.GetInstanceMethodNames());
            }
        }
        public static class _Module {
            public static string name(Context Context) {
                return Context.Instance.CastModule.Name;
            }
            public static Array methods(Context Context) {
                return Adapter.GetArray(Context, Context.Instance.CastModule.GetClassMethodNames());
            }
            public static Array constants(Context Context) {
                return Adapter.GetArray(Context, Context.Instance.CastModule.GetConstantNames());
            }
            public static Array class_variables(Context Context) {
                return Adapter.GetArray(Context, Context.Instance.CastModule.GetClassVariableNames());
            }
            public static Array class_methods(Context Context) {
                return Adapter.GetArray(Context, Context.Instance.CastModule.GetClassMethodNames());
            }
        }
        public static class _Class {
            public static bool _CaseEquals(Context Context, Instance Other) {
                return Context.Instance.CastModule.DerivesFrom(Other.Class);
            }
            public static Class? superclass(Context Context) {
                return Context.Instance.CastClass.SuperClass;
            }
            public static Array instance_variables(Context Context) {
                return Adapter.GetArray(Context, Context.Instance.CastClass.GetInstanceVariableNames());
            }
            public static Array instance_methods(Context Context) {
                return Adapter.GetArray(Context, Context.Instance.CastClass.GetInstanceMethodNames());
            }
        }
        public static class _NilClass {
            public static string inspect() {
                return "nil";
            }
        }
        public static class _TrueClass {
            public static string to_s() {
                return "true";
            }
            public static string inspect() {
                return "true";
            }
        }
        public static class _FalseClass {
            public static string to_s() {
                return "false";
            }
            public static string inspect() {
                return "false";
            }
        }
        public static class _String {
            public static string? _Index(Context Context, Instance Index) {
                string String = Context.Instance.CastString;
                // Get index range
                Range RangeIndex = Index.Value is InstanceRange InstanceRange
                    ? ((Range)InstanceRange).Clamp(String.Length)
                    : ((int)Index.CastInteger).ClampAsRange(String.Length);
                // Ensure index in range
                if (RangeIndex.Count(String.Length) < 0) {
                    return null;
                }
                // Get range
                return String[RangeIndex];
            }
            public static string _SetIndex(Context Context, Instance Index, string Substring) {
                string String = Context.Instance.CastString;
                // Get index range
                Range RangeIndex = Index.Value is InstanceRange InstanceRange
                    ? ((Range)InstanceRange).Clamp(String.Length)
                    : ((int)Index.CastInteger).ClampAsRange(String.Length);
                // Ensure index in range
                if (RangeIndex.Count(String.Length) < 0) {
                    return String;
                }
                // Set range
                string NewString = String.ReplaceRange(RangeIndex, Substring);
                Context.Instance.Value = NewString;
                return NewString;
            }
            public static string _Add(Context Context, string Right) {
                return Context.Instance.CastString + Right;
            }
            public static string _Multiply(Context Context, int Times) {
                string OriginalString = Context.Instance.CastString;
                return new StringBuilder(OriginalString.Length * Times).Insert(0, OriginalString, Times).ToString();
            }
            public static bool _Equals(Context Context, Instance Other) {
                return Context.Instance.Class == Other.Class && Context.Instance.CastString == Other.CastString;
            }
            public static bool _LessThan(Context Context, Instance Other) {
                return string.Compare(Context.Instance.CastString, Other.CastString) < 0;
            }
            public static bool _LessThanOrEqualTo(Context Context, Instance Other) {
                return string.Compare(Context.Instance.CastString, Other.CastString) <= 0;
            }
            public static bool _GreaterThanOrEqualTo(Context Context, Instance Other) {
                return string.Compare(Context.Instance.CastString, Other.CastString) >= 0;
            }
            public static bool _GreaterThan(Context Context, Instance Other) {
                return string.Compare(Context.Instance.CastString, Other.CastString) > 0;
            }
            public static int? _Spaceship(Context Context, Instance Other) {
                if (Context.Instance.CallMethod("==", Other).Truthy) {
                    return 0;
                }
                else if (Context.Instance.Class == Other.Class) {
                    return string.Compare(Context.Instance.CastString, Other.CastString);
                }
                else {
                    return null;
                }
            }
            public static Instance to_sym(Context Context) {
                return Context.Axis.Globals.GetMortalSymbol(Context.Instance.CastString);
            }
            public static Integer? to_i(Context Context) {
                try {
                    return Integer.Parse(Context.Instance.CastString);
                }
                catch (Exception) {
                    return null;
                }
            }
            public static Float? to_f(Context Context) {
                try {
                    return Float.Parse(Context.Instance.CastString);
                }
                catch (Exception) {
                    return null;
                }
            }
            public static Array to_a(Context Context) {
                IEnumerable<Instance> Charas = Context.Instance.CastString.Select(Chara => new Instance(Context.Axis.String, Chara.ToString()));
                return new Array(Context.Location, Charas.ToList());
            }
            public static string inspect(Context Context) {
                return '"' + Context.Instance.CastString.Replace("\n", "\\n").Replace("\r", "\\r") + '"';
            }
            public static int length(Context Context) {
                return Context.Instance.CastString.Length;
            }
            public static int count(Context Context, string? Substring = null) {
                string String = Context.Instance.CastString;
                if (Substring is null) {
                    return String.Length;
                }
                else {
                    int Count = 0;
                    int CurrentIndex = 0;
                    while ((CurrentIndex = String.IndexOf(Substring, CurrentIndex)) != -1) {
                        CurrentIndex += Substring.Length;
                        Count++;
                    }
                    return Count;
                }
            }
            public static string chomp(Context Context, string? RemoveFromEnd = null) {
                string String = Context.Instance.CastString;
                if (RemoveFromEnd is null) {
                    if (String.EndsWith("\r\n")) {
                        return String[..^2];
                    }
                    else if (String.EndsWith('\n') || String.EndsWith('\r')) {
                        return String[..^1];
                    }
                }
                else {
                    if (String.EndsWith(RemoveFromEnd)) {
                        return String[..^RemoveFromEnd.Length];
                    }
                }
                return String;
            }
            public static Instance chomp1(Context Context, string? RemoveFromEnd = null) {
                Context.Instance.Value = chomp(Context, RemoveFromEnd);
                return Context.Instance;
            }
            public static string chop(Context Context) {
                string String = Context.Instance.CastString;
                return String.Length != 0 ? String[..^1] : String;
            }
            public static Instance chop1(Context Context) {
                Context.Instance.Value = chop(Context);
                return Context.Instance;
            }
            public static string strip(Context Context) {
                return Context.Instance.CastString.Trim();
            }
            public static Instance strip1(Context Context) {
                Context.Instance.Value = strip(Context);
                return Context.Instance;
            }
            public static string lstrip(Context Context) {
                return Context.Instance.CastString.TrimStart();
            }
            public static Instance lstrip1(Context Context) {
                Context.Instance.Value = lstrip(Context);
                return Context.Instance;
            }
            public static string rstrip(Context Context) {
                return Context.Instance.CastString.TrimEnd();
            }
            public static Instance rstrip1(Context Context) {
                Context.Instance.Value = rstrip(Context);
                return Context.Instance;
            }
            public static string squeeze(Context Context) {
                string OriginalString = Context.Instance.CastString;
                StringBuilder NewString = new(OriginalString.Length);
                foreach (char Chara in OriginalString) {
                    if (NewString.Length == 0 || Chara != NewString[^1]) {
                        NewString.Append(Chara);
                    }
                }
                return NewString.ToString();
            }
            public static Instance squeeze1(Context Context) {
                Context.Instance.Value = squeeze(Context);
                return Context.Instance;
            }
            public static string capitalize(Context Context) {
                string String = Context.Instance.CastString;
                return String.Length switch {
                    0 => String,
                    1 => String.ToUpperInvariant(),
                    _ => char.ToUpperInvariant(String[0]) + String[1..].ToLowerInvariant()
                };
            }
            public static Instance capitalize1(Context Context) {
                Context.Instance.Value = capitalize(Context);
                return Context.Instance;
            }
            public static string upcase(Context Context) {
                return Context.Instance.CastString.ToUpperInvariant();
            }
            public static Instance upcase1(Context Context) {
                Context.Instance.Value = upcase(Context);
                return Context.Instance;
            }
            public static string downcase(Context Context) {
                return Context.Instance.CastString.ToUpperInvariant();
            }
            public static Instance downcase1(Context Context) {
                Context.Instance.Value = downcase(Context);
                return Context.Instance;
            }
            public static string sub(Context Context, string Replace, string With) {
                return Context.Instance.CastString.ReplaceFirst(Replace, With);
            }
            public static Instance sub1(Context Context, string Replace, string With) {
                Context.Instance.Value = sub(Context, Replace, With);
                return Context.Instance;
            }
            public static string gsub(Context Context, string Replace, string With) {
                return Context.Instance.CastString.Replace(Replace, With);
            }
            public static Instance gsub1(Context Context, string Replace, string With) {
                Context.Instance.Value = gsub(Context, Replace, With);
                return Context.Instance;
            }
            public static Array split(Context Context, string Delimiter = " ", int Limit = int.MaxValue, bool RemoveEmptyEntries = true) {
                return Adapter.GetArray(Context, Context.Instance.CastString.Split(Delimiter, Limit, RemoveEmptyEntries ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None));
            }
            public static string chr(Context Context) {
                string String = Context.Instance.CastString;
                return String.Length != 0 ? String[0].ToString() : "";
            }
            public static bool include7(Context Context, string Substring) {
                return Context.Instance.CastString.Contains(Substring);
            }
            public static bool eql7(Context Context, Instance Other) {
                return Context.Instance.Class == Other.Class && Context.Instance.CastString == Other.CastString;
            }
        }
        public static class _Symbol {
            public static string inspect(Context Context) {
                return ':' + Context.Instance.CastString;
            }
            public static int length(Context Context) {
                return Context.Instance.CastString.Length;
            }
        }
        public static class _Integer {
            public static object _Add(Context Context, Instance Right) {
                Integer Left = Context.Instance.CastInteger;
                if (Right.Value is Integer RightInteger) {
                    return Left + RightInteger;
                }
                else {
                    return Left + Right.CastFloat;
                }
            }
            public static object _Sub(Context Context, Instance Right) {
                Integer Left = Context.Instance.CastInteger;
                if (Right.Value is Integer RightInteger) {
                    return Left - RightInteger;
                }
                else {
                    return Left - Right.CastFloat;
                }
            }
            public static object _Mul(Context Context, Instance Right) {
                Integer Left = Context.Instance.CastInteger;
                if (Right.Value is Integer RightInteger) {
                    return Left * RightInteger;
                }
                else {
                    return Left * Right.CastFloat;
                }
            }
            public static object _Div(Context Context, Instance Right) {
                Integer Left = Context.Instance.CastInteger;
                if (Right.Value is Integer RightInteger) {
                    return Left / RightInteger;
                }
                else {
                    return Left / Right.CastFloat;
                }
            }
            public static object _Mod(Context Context, Instance Right) {
                Integer Left = Context.Instance.CastInteger;
                if (Right.Value is Integer RightInteger) {
                    return Left % RightInteger;
                }
                else {
                    return Left % Right.CastFloat;
                }
            }
            public static object _Pow(Context Context, Instance Right) {
                Integer Left = Context.Instance.CastInteger;
                // Integer ** Integer
                if (Right.Value is Integer RightInteger) {
                    return Left.Pow(RightInteger);
                }
                // Float ** Float
                else {
                    Float RightFloat = Right.CastFloat;
                    return Left.Pow(RightFloat);
                }
            }
            public static Instance _UnaryPlus(Context Context) {
                return Context.Instance;
            }
            public static Integer _UnaryMinus(Context Context) {
                return -Context.Instance.CastInteger;
            }
            public static Integer to_i(Context Context) {
                return Context.Instance.CastInteger;
            }
            public static Float to_f(Context Context) {
                return Context.Instance.CastInteger;
            }
            public static Integer clamp(Context Context, Integer Min, Integer Max) {
                Integer Integer = Context.Instance.CastInteger;
                return Integer < Min ? Min : (Integer > Max ? Max : Integer);
            }
            public static Integer floor(Context Context) {
                return Context.Instance.CastInteger;
            }
            public static Integer ceil(Context Context) {
                return Context.Instance.CastInteger;
            }
            public static Integer round(Context Context) {
                return Context.Instance.CastInteger;
            }
            public static Integer truncate(Context Context) {
                return Context.Instance.CastInteger;
            }
            public static Integer abs(Context Context) {
                return Context.Instance.CastInteger.Abs();
            }
            public static object? times(Context Context, [Block] Proc? Block) {
                return Block is not null
                    ? Loop.For(Context, 0, Context.Instance.CastInteger, 1, Block)
                    : new Enumerator(Context.Location, 0, Context.Instance.CastInteger, 1);
            }
            public static object? upto(Context Context, Integer Limit, [Block] Proc? Block) {
                return Block is not null
                    ? Loop.For(Context, Context.Instance.CastInteger, Limit, 1, Block)
                    : new Enumerator(Context.Location, Context.Instance.CastInteger, Limit, 1);
            }
            public static object? downto(Context Context, Integer Limit, [Block] Proc? Block) {
                return Block is not null
                    ? Loop.For(Context, Limit, Context.Instance.CastInteger, -1, Block)
                    : new Enumerator(Context.Location, Limit, Context.Instance.CastInteger, -1);
            }
        }
        public static class _Float {
            public static Float _Add(Context Context, Instance Right) {
                return Context.Instance.CastFloat + Right.CastFloat;
            }
            public static Float _Sub(Context Context, Instance Right) {
                return Context.Instance.CastFloat - Right.CastFloat;
            }
            public static Float _Mul(Context Context, Instance Right) {
                return Context.Instance.CastFloat * Right.CastFloat;
            }
            public static Float _Div(Context Context, Instance Right) {
                return Context.Instance.CastFloat / Right.CastFloat;
            }
            public static Float _Mod(Context Context, Instance Right) {
                return Context.Instance.CastFloat % Right.CastFloat;
            }
            public static object _Pow(Context Context, Instance Right) {
                Float LeftFloat = Context.Instance.CastFloat;
                Float RightFloat = Right.CastFloat;
                return LeftFloat.Pow(RightFloat);
            }
            public static Instance _UnaryPlus(Context Context) {
                return Context.Instance;
            }
            public static Float _UnaryMinus(Context Context) {
                return -Context.Instance.CastFloat;
            }
            public static bool _Equals(Context Context, Instance Other) {
                return Other.Value is Integer or Float && Context.Instance.CastFloat == Other.CastFloat;
            }
            public static bool _LessThan(Context Context, Float Other) {
                return Context.Instance.CastFloat < Other;
            }
            public static bool _LessThanOrEqualTo(Context Context, Float Other) {
                return Context.Instance.CastFloat <= Other;
            }
            public static bool _GreaterThanOrEqualTo(Context Context, Float Other) {
                return Context.Instance.CastFloat >= Other;
            }
            public static bool _GreaterThan(Context Context, Float Other) {
                return Context.Instance.CastFloat > Other;
            }
            public static int? _Spaceship(Context Context, Instance Other) {
                if (Context.Instance.CallMethod("==", Other).Truthy) {
                    return 0;
                }
                else if (Context.Instance.Class == Other.Class) {
                    return Context.Instance.CastInteger.CompareTo(Other.CastInteger);
                }
                else {
                    return null;
                }
            }
            public static Integer to_i(Context Context) {
                return (Integer)Context.Instance.CastFloat;
            }
            public static Float to_f(Context Context) {
                return Context.Instance.CastFloat;
            }
            public static Float clamp(Context Context, Float Min, Float Max) {
                Float Float = Context.Instance.CastFloat;
                return Float < Min ? Min : (Float > Max ? Max : Float);
            }
            public static Integer floor(Context Context) {
                return Context.Instance.CastFloat.Floor();
            }
            public static Integer ceil(Context Context) {
                return Context.Instance.CastFloat.Ceil();
            }
            public static Integer round(Context Context) {
                return Context.Instance.CastFloat.Round();
            }
            public static Integer truncate(Context Context) {
                return Context.Instance.CastFloat.Truncate();
            }
            public static Float abs(Context Context) {
                return Context.Instance.CastFloat.Abs();
            }
        }
        public static class _Proc {
            public static Instance call(Context Context, [Splat] Instance[] Arguments, [Block] Proc? Block) {
                return Context.Instance.CastProc.Call(Arguments, Block);
            }
        }
        public static class _Array {
            public static object _Index(Context Context, Instance Index) {
                Array Array = Context.Instance.CastArray;
                // Get range index
                if (Index.Value is InstanceRange InstanceRange) {
                    Array Slice = new(Context.Location, (int)(InstanceRange.Count ?? 0));
                    foreach (Integer RangeIndex in InstanceRange) {
                        Slice.Add(Array[(int)RangeIndex]);
                    }
                    return Slice;
                }
                // Get integer index
                else {
                    return Array[(int)Index.CastInteger];
                }
            }
            public static Instance _SetIndex(Context Context, int Index, Instance Value) {
                Array Array = Context.Instance.CastArray;
                return Array[Index] = Value;
            }
            public static Array _Mul(Context Context, int Count) {
                // Get array and create new array
                Array Array = Context.Instance.CastArray;
                Array NewArray = new(Context.Location, Array.Count * Count);
                // Repeatedly add each item from the original list
                for (int i = 0; i < Count; i++) {
                    foreach (Instance Item in Array) {
                        NewArray.Add(Item);
                    }
                }
                return NewArray;
            }
            public static Instance _Append(Context Context, Instance Item) {
                Context.Instance.CastArray.Add(Item);
                return Context.Instance;
            }
            public static bool _Equals(Context Context, Instance Other) {
                Array Array = Context.Instance.CastArray;
                if (Other.Value is Array OtherArray && Array.Count == OtherArray.Count) {
                    for (int i = 0; i < Array.Count; i++) {
                        if (Array[i].CallMethod("==", OtherArray[i]).Falsey) {
                            return false;
                        }
                    }
                    return true;
                }
                else {
                    return false;
                }
            }
            public static string to_s(Context Context) {
                return Context.Instance.CastArray.Inner.ObjectsToString(
                    Separator: "\n",
                    Converter: Item => Item.ToS()
                );
            }
            public static string inspect(Context Context) {
                return "[" + Context.Instance.CastArray.Inner.ObjectsToString(
                    Converter: Item => Item.Inspect()
                ) + "]";
            }
            public static int length(Context Context) {
                return Context.Instance.CastArray.Count;
            }
            public static int count(Context Context, Instance? ItemToCount = null) {
                if (ItemToCount is null) {
                    return Context.Instance.CastArray.Count;
                }
                else {
                    int Count = 0;
                    foreach (Instance Item in Context.Instance.CastArray) {
                        if (Item.CallMethod("==", ItemToCount).Truthy) {
                            Count++;
                        }
                    }
                    return Count;
                }
            }
            public static Instance prepend(Context Context, Instance Item) {
                Context.Instance.CastArray.Insert(0, Item);
                return Context.Instance;
            }
            public static Instance pop(Context Context) {
                Array Array = Context.Instance.CastArray;
                Array.RemoveAt(Array.Count - 1);
                return Context.Instance;
            }
            public static Instance insert(Context Context, Instance FirstArgument, params Instance[] Items) {
                // array.insert(item)
                if (Items.Length == 0) {
                    Context.Instance.CastArray.Insert(0, FirstArgument);
                }
                // array.insert(index, item)
                else {
                    Context.Instance.CastArray.InsertRange((int)FirstArgument.CastInteger, Items);
                }
                return Context.Instance;
            }
            public static Instance? delete(Context Context, Instance ItemToDelete) {
                Array Array = Context.Instance.CastArray;
                Instance? LastDeletedItem = null;
                for (int i = 0; i < Array.Count; i++) {
                    Instance Item = Array[i];
                    if (Item.CallMethod("==", ItemToDelete).Truthy) {
                        Array.RemoveAt(i);
                        LastDeletedItem = Item;
                    }
                }
                return LastDeletedItem;
            }
            public static Instance? delete_at(Context Context, int Index) {
                Array Array = Context.Instance.CastArray;
                if (Index < Array.Count) {
                    Instance ItemDeleted = Array[Index];
                    Array.RemoveAt(Index);
                    return ItemDeleted;
                }
                return null;
            }
            public static Array uniq(Context Context) {
                Array Array = new(Context.Location, Context.Instance.CastArray.Inner);
                Array.Inner.RemoveDuplicates();
                return Array;
            }
            public static Array uniq1(Context Context) {
                Array Array = Context.Instance.CastArray;
                Array.Inner.RemoveDuplicates();
                return Array;
            }
            public static Instance first(Context Context) {
                return Context.Instance.CastArray[0];
            }
            public static Instance last(Context Context) {
                return Context.Instance.CastArray[^1];
            }
            public static Instance forty_two(Context Context) {
                return Context.Instance.CastArray[41];
            }
            public static Instance sample(Context Context) {
                Array Array = Context.Instance.CastArray;
                return Array[Context.Axis.Globals.Random.Next(Array.Count)];
            }
            public static Instance? min(Context Context) {
                Instance? Min = null;
                foreach (Instance Item in Context.Instance.CastArray) {
                    if (Min is null || Item.CallMethod("<", Min).Truthy) {
                        Min = Item;
                    }
                }
                return Min;
            }
            public static Instance? max(Context Context) {
                Instance? Max = null;
                foreach (Instance Item in Context.Instance.CastArray) {
                    if (Max is null || Item.CallMethod(">", Max).Truthy) {
                        Max = Item;
                    }
                }
                return Max;
            }
            public static object sum(Context Context) {
                Float Sum = 0;
                bool ReturnInteger = false;
                foreach (Instance Item in Context.Instance.CastArray) {
                    Sum += Item.CastFloat;
                    if (Item.Value is Integer) {
                        ReturnInteger = true;
                    }
                }
                return ReturnInteger ? (Integer)Sum : Sum;
            }
            public static object? each(Context Context, [Block] Proc? Block) {
                return Block is not null
                    ? Loop.Each(Context, Context.Instance.CastArray, Block)
                    : new Enumerator(Context, Context.Instance.CastArray);
            }
            public static object? reverse_each(Context Context, [Block] Proc? Block) {
                return Block is not null
                    ? Loop.Each(Context, Context.Instance.CastArray.Reverse(), Block)
                    : new Enumerator(Context, Context.Instance.CastArray.Reverse());
            }
            public static Array shuffle(Context Context) {
                Array Array = Context.Instance.CastArray.Clone();
                Array.Inner.Shuffle(Context.Axis.Globals.Random);
                return Array;
            }
            public static Instance shuffle1(Context Context) {
                Context.Instance.CastArray.Inner.Shuffle(Context.Axis.Globals.Random);
                return Context.Instance;
            }
            public static Array sort(Context Context, [Block] Proc? Block) {
                Array Array = Context.Instance.CastArray.Clone();
                Array.Sort(Block);
                return Array;
            }
            public static Instance sort1(Context Context, [Block] Proc? Block) {
                Context.Instance.CastArray.Sort(Block);
                return Context.Instance;
            }
            public static Array map(Context Context, [Block] Proc? Block) {
                if (Block is null) {
                    throw new RuntimeError($"{Context.Location}: no block given for map");
                }
                Array Array = Context.Instance.CastArray;
                Array NewArray = new(Context.Location, Array.Count);
                foreach (Instance Item in Array) {
                    NewArray.Add(Block.Call(Item));
                }
                return NewArray;
            }
            public static Instance map1(Context Context, [Block] Proc? Block) {
                if (Block is null) {
                    throw new RuntimeError($"{Context.Location}: no block given for map!");
                }
                Context.Instance.Value = map(Context, Block);
                return Context.Instance;
            }
            public static Array reverse(Context Context) {
                Array Array = Context.Instance.CastArray;
                return new Array(Context.Location, Array.Inner.Reverse().ToArray());
            }
            public static Instance reverse1(Context Context) {
                Context.Instance.Value = reverse(Context);
                return Context.Instance;
            }
            public static string join(Context Context, string Separator = "") {
                return string.Join(Separator, Context.Instance.CastArray.Inner.Select(Item => Item.Inspect()));
            }
            public static void clear(Context Context) {
                Context.Instance.CastArray.Clear();
            }
            public static bool include7(Context Context, Instance ItemToFind) {
                foreach (Instance Item in Context.Instance.CastArray) {
                    if (Item.CallMethod("==", ItemToFind).Truthy) {
                        return true;
                    }
                }
                return false;
            }
            public static bool empty7(Context Context) {
                return Context.Instance.CastArray.Count == 0;
            }
        }
        public static class _Hash {
            public static Hash @new(Context Context, Instance? DefaultValue = null) {
                return new Hash(Context.Location, default_value: DefaultValue);
            }
            public static Instance _Index(Context Context, Instance Key) {
                return Context.Instance.CastHash[Key];
            }
            public static Instance _SetIndex(Context Context, Instance Key, Instance Value) {
                return Context.Instance.CastHash[Key] = Value;
            }
            public static bool _Equals(Context Context, Instance Other) {
                Hash Hash = Context.Instance.CastHash;
                if (Other.Value is Hash OtherHash && Hash.Count == OtherHash.Count) {
                    foreach (KeyValuePair<Instance, Instance> Entry in Hash) {
                        if (!OtherHash.HasKey(Entry.Key)) {
                            return false;
                        }
                        if (Entry.Value.CallMethod("==", OtherHash[Entry.Key]).Falsey) {
                            return false;
                        }
                    }
                    return true;
                }
                else {
                    return false;
                }
            }
            public static string to_s(Context Context) {
                return inspect(Context);
            }
            public static Array to_a(Context Context) {
                Array Entries = new(Context.Location, Context.Instance.CastArray.Count);
                foreach (KeyValuePair<Instance, Instance> Entry in Context.Instance.CastHash) {
                    Array EntryArray = new(Context.Location, new[] { Entry.Key, Entry.Value });
                    Entries.Add(new Instance(Context.Axis.Array, EntryArray));
                }
                return Entries;
            }
            public static string inspect(Context Context) {
                Hash Hash = Context.Instance.CastHash;
                StringBuilder String = new();
                String.Append('{');
                bool Comma = false;
                foreach (KeyValuePair<Instance, Instance> Entry in Hash) {
                    // Append comma
                    if (Comma) String.Append(", ");
                    Comma = true;
                    // Append key-value pair
                    String.Append(Entry.Key.Inspect());
                    String.Append(" => ");
                    String.Append(Entry.Value.Inspect());
                }
                String.Append('}');
                return String.ToString();
            }
            public static int length(Context Context) {
                return Context.Instance.CastHash.Count;
            }
            public static bool has_key7(Context Context, Instance Key) {
                return Context.Instance.CastHash.HasKey(Key);
            }
            public static bool has_value7(Context Context, Instance Value) {
                foreach (Instance Value2 in Context.Instance.CastHash.Inner.Values) {
                    if (Value.CallMethod("==", Value2).Truthy) {
                        return true;
                    }
                }
                return false;
            }
            public static Array keys(Context Context) {
                return new Array(Context.Location, Context.Instance.CastHash.Inner.Keys.ToArray());
            }
            public static Array values(Context Context) {
                return new Array(Context.Location, Context.Instance.CastHash.Inner.Values.ToArray());
            }
            public static Instance delete(Context Context, Instance Key) {
                Instance RemovingValue = Context.Instance.CastHash[Key];
                Context.Instance.CastHash.Remove(Key);
                return RemovingValue;
            }
            public static void clear(Context Context) {
                Context.Instance.CastHash.Clear();
            }
            public static object? each(Context Context, [Block] Proc? Block) {
                return Block is not null
                    ? Loop.Each(Context.Instance.CastHash, (Entry, Index) => Block.Call(Entry.Key, Entry.Value))
                    : new Enumerator(Context, Context.Instance.CastHash);
            }
            public static object? reverse_each(Context Context, [Block] Proc? Block) {
                return Block is not null
                    ? Loop.Each(Context.Instance.CastHash.Reverse(), (Entry, Index) => Block.Call(Entry.Key, Entry.Value))
                    : new Enumerator(Context, Context.Instance.CastHash.Reverse());
            }
            public static Hash invert(Context Context) {
                return Context.Instance.CastHash.Invert();
            }
            public static bool empty7(Context Context) {
                return Context.Instance.CastHash.Count == 0;
            }
        }
        public static class _Time {
            public static DateTimeOffset @new(int Year = 0, int Month = 0, double Day = 0, double Hour = 0, double Minute = 0, double Second = 0, double UtcOffset = 0) {
                // Get time arguments
                int TimeYear = Year;
                int TimeMonth = Month;
                double TimeDay = Day;
                double TimeHour = Hour;
                double TimeMinute = Minute;
                double TimeSecond = Second;
                TimeSpan TimeUtcOffset = TimeSpan.FromHours(UtcOffset);

                // Create base time
                DateTimeOffset Time = new(TimeYear, TimeMonth, (int)TimeDay, (int)TimeHour, (int)TimeMinute, (int)TimeSecond, TimeUtcOffset);

                // Improve precision of time
                Time = Time
                    .AddDays(TimeDay - (int)TimeDay)
                    .AddHours(TimeHour - (int)TimeHour)
                    .AddMinutes(TimeMinute - (int)TimeMinute)
                    .AddSeconds(TimeSecond - (int)TimeSecond);

                // Return time
                return Time;
            }
            public static DateTimeOffset now() {
                return DateTimeOffset.Now;
            }
            public static DateTimeOffset at(double Seconds) {
                // Get time at given local timestamp
                DateTimeOffset DateTimeOffset = DateTimeOffset.FromUnixTimeSeconds((long)Seconds);
                DateTime Time = DateTimeOffset.ToLocalTime().DateTime
                    .AddSeconds(Seconds - (long)Seconds);
                return Time;
            }
            public static string to_s(Context Context) {
                return Context.Instance.CastTime.ToString(CultureInfo.GetCultureInfo("ja-jp"));
            }
            public static long to_i(Context Context) {
                return Context.Instance.CastTime.ToUnixTimeSeconds();
            }
            public static double to_f(Context Context) {
                return Context.Instance.CastTime.ToUnixTimeSecondsDouble();
            }
        }
        public static class _Range {
            public static Instance min(Context Context) {
                return Context.Instance.CastRange.Min;
            }
            public static Instance max(Context Context) {
                return Context.Instance.CastRange.Max;
            }
            public static bool exclude_end7(Context Context) {
                return Context.Instance.CastRange.ExcludeEnd;
            }
            public static object? each(Context Context, [Block] Proc? Block) {
                return Block is not null
                    ? Loop.Each(Context, Context.Instance.CastRange, Block)
                    : new Enumerator(Context, Context.Instance.CastRange);
            }
            public static object? reverse_each(Context Context, [Block] Proc? Block) {
                return Block is not null
                    ? Loop.Each(Context, Context.Instance.CastRange.Reverse(), Block)
                    : new Enumerator(Context, Context.Instance.CastRange.Reverse());
            }
            public static Array to_a(Context Context) {
                Array Indices = new(Context.Location, (int)(Context.Instance.CastRange.Count ?? 0));
                foreach (Integer Index in Context.Instance.CastRange) {
                    Indices.Add(new Instance(Context.Axis.Integer, Index));
                }
                return Indices;
            }
            public static Integer? length(Context Context) {
                return Context.Instance.CastRange.Count;
            }
        }
        public static class _Enumerator {
            public static Instance? each(Context Context, [Block] Proc? Block) {
                // If block given, enumerate block & return nil
                if (Block is not null) {
                    foreach (Instance Item in Context.Instance.CastEnumerator) {
                        Block.Call(Item);
                    }
                    return null;
                }
                // If no block given, return enumerator
                else {
                    return Context.Instance;
                }
            }
            public static Enumerator? step(Context Context, Integer Interval, [Block] Proc? Block) {
                // Get enumerable
                IEnumerable<Instance> Enumerable = Context.Instance.CastEnumerator.Where((Item, Index) => Index % Interval == 0);
                // If block given, enumerate block & return nil
                if (Block is not null) {
                    foreach (Instance Item in Enumerable) {
                        Block.Call(Item);
                    }
                    return null;
                }
                // If no block given, return enumerator
                else {
                    return new Enumerator(Enumerable);
                }
            }
            public static Instance? next(Context Context) {
                Enumerator Enumerator = Context.Instance.CastEnumerator;
                if (!Enumerator.MoveNext()) {
                    throw new RuntimeError($"{Context.Location}: no more items in the enumerator.");
                }
                return Enumerator.Current;
            }
            public static Instance? peek(Context Context) {
                return Context.Instance.CastEnumerator.Peek();
            }
        }
        public static class _Exception {
            public static Exception @new(string? Message = null) {
                return new Exception(Message);
            }
            public static string to_s() {
                return "exception";
            }
            public static string inspect(Context Context) {
                Exception Exception = Context.Instance.CastException;
                return $"#<{Exception.GetType().Name}: {Exception.Message}>";
            }
            public static string message(Context Context) {
                return Context.Instance.CastException.Message;
            }
            public static string backtrace(Context Context) {
                return Context.Instance.CastException.StackTrace ?? "";
            }
        }
        public static class _WeakRef {
            public static WeakReference<Instance> @new(Instance Target) {
                return new WeakReference<Instance>(Target);
            }
            public static string to_s() {
                return "weakref";
            }
            public static string inspect(Context Context) {
                Context.Instance.CastWeakRef.TryGetTarget(out Instance? Target);
                Target ??= Context.Axis.Nil;
                return $"#<WeakRef: {Target.CallMethod("inspect")}>";
            }
            public static bool weakref_alive7(Context Context) {
                return Context.Instance.CastWeakRef.TryGetTarget(out _);
            }
            public static Instance method_missing(Context Context, string MethodName, [Splat] Instance[] Arguments, [Block] Proc? Block) {
                if (Context.Instance.CastWeakRef.TryGetTarget(out Instance? Target)) {
                    return Target.CallMethod(new Context(Context.Location, Context.Scope, Context.Module, Context.Instance, Block, Context.Method), MethodName, Arguments);
                }
                else {
                    throw new RuntimeError($"{Context.Location}: weakref is dead");
                }
            }
        }
        public static class _Thread {
            public static Thread @new(Context Context, [Splat] Instance[] Arguments, [Block] Proc? Block) {
                if (Block is null) {
                    throw new RuntimeError($"{Context.Location}: no block given for Thread.new");
                }
                // Create new thread
                return new Thread(Context.Location, Thread => {
                    // Create call context for block
                    Context CallContext = new(Context.Location, Context.Scope, Context.Module, Context.Instance);
                    // Set current thread
                    CallContext.Locals.Thread = Thread;
                    // Call method in thread
                    Block.Method.Call(CallContext, Arguments);
                });
            }
            public static void stop(Context Context) {
                Context.Instance.CastThread.Stop();
            }
            public static void join(Context Context) {
                Context.Instance.CastThread.Wait();
            }
        }
        public static class _Math {
            public static double log(double X, double Y) {
                return Math.Log(X, Y);
            }
            public static double log2(double X) {
#if NET5_0_OR_GREATER
                return Math.Log2(X);
#else
                return Math.Log(X, 2);
#endif
            }
            public static double hypot(double X, double Y) {
                return Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2));
            }
            public static Float to_rad(Float Degrees) {
                return Degrees * (Math.PI / 180);
            }
            public static Float to_deg(Float Radians) {
                return Radians / (Math.PI / 180);
            }
            public static Float lerp(Float Start, Float End, Float Alpha) {
                return Start * (1 - Alpha) + (End * Alpha);
            }
            public static Float abs(Float X) {
                return X.Abs();
            }
        }
        public static class _GC {
            public static void start(int? MaxGeneration = null) {
                if (MaxGeneration is not null) {
                    GC.Collect(MaxGeneration.Value);
                }
                else {
                    GC.Collect();
                }
            }
            public static Integer count(int? Generation = null) {
                if (Generation is not null) {
                    return GC.CollectionCount(Generation.Value);
                }
                else {
                    long Count = 0;
                    for (int i = 0; i <= GC.MaxGeneration; i++) {
                        Count += GC.CollectionCount(i);
                    }
                    return Count;
                }
            }
        }
        public static class _File {
            public static string read(string FilePath) {
                return File.ReadAllText(FilePath);
            }
            public static void write(string FilePath, string Text) {
                File.WriteAllText(FilePath, Text);
            }
            public static void append(string FilePath, string Text) {
                File.AppendAllText(FilePath, Text);
            }
            public static void delete(string FilePath) {
                File.Delete(FilePath);
            }
            public static bool exist7(string FilePath) {
                return File.Exists(FilePath);
            }
            public static string absolute_path(string FilePath) {
                return Path.GetFullPath(FilePath);
            }
            public static string absolute_path7(string FilePath) {
                return Path.GetFullPath(FilePath);
            }
            public static string basename(string FilePath) {
                return Path.GetFileName(FilePath);
            }
            public static string dirname(string FilePath) {
                return Path.GetDirectoryName(FilePath) ?? "";
            }
        }
    }
}
