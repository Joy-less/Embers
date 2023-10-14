using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Net.Http;
using static Embers.Script;
using static Embers.SpecialTypes;

#nullable enable
#pragma warning disable CS1998
#pragma warning disable IDE1006
#pragma warning disable SYSLIB1045

namespace Embers
{
    public class Api
    {
        public readonly Interpreter Interpreter;

        public readonly NilInstance Nil;
        public readonly TrueInstance True;
        public readonly FalseInstance False;

        public readonly Class NilClass;
        public readonly Class TrueClass;
        public readonly Class FalseClass;
        public readonly Class String;
        public readonly Class Symbol;
        public readonly Class Integer;
        public readonly Class Float;
        public readonly Class Proc;
        public readonly Class Range;
        public readonly Class Array;
        public readonly Class Hash;
        public readonly Module Random;
        public readonly Module Math;
        public readonly Class Exception;
            public readonly Class StandardError;
            public readonly Class RuntimeError;
        public readonly Class Thread;
        public readonly Module Parallel;
        public readonly Class Time;
        public readonly Class WeakRef;
        public readonly Module File;
        public readonly Module Net;
            public readonly Module HTTP;
                public readonly Class HTTPResponse;

        public SymbolInstance GetSymbol(string Value) {
            return Interpreter.Symbols[Value] ?? Interpreter.Symbols.Store(Value, new SymbolInstance(Symbol, Value));
        }
        public IntegerInstance GetInteger(DynInteger Value) {
            return Interpreter.Integers[Value] ?? Interpreter.Integers.Store(Value, new IntegerInstance(Integer, Value));
        }
        public FloatInstance GetFloat(DynFloat Value) {
            return Interpreter.Floats[Value] ?? Interpreter.Floats.Store(Value, new FloatInstance(Float, Value));
        }
        public Instance CreateInstanceFromClass(Script Script, Class Class) {
            if (Class.InheritsFrom(NilClass))
                return new NilInstance(Class);
            else if (Class.InheritsFrom(TrueClass))
                return new TrueInstance(Class);
            else if (Class.InheritsFrom(FalseClass))
                return new FalseInstance(Class);
            else if (Class.InheritsFrom(String))
                return new StringInstance(Class, "");
            else if (Class.InheritsFrom(Symbol))
                return GetSymbol("");
            else if (Class.InheritsFrom(Integer))
                return GetInteger(0);
            else if (Class.InheritsFrom(Float))
                return GetFloat(0);
            else if (Class.InheritsFrom(Proc))
                throw new RuntimeException($"{Script.ApproximateLocation}: Tried to create Proc instance without a block");
            else if (Class.InheritsFrom(Range))
                throw new RuntimeException($"{Script.ApproximateLocation}: Tried to create Range instance with new");
            else if (Class.InheritsFrom(Array))
                return new ArrayInstance(Class, new List<Instance>());
            else if (Class.InheritsFrom(Hash))
                return new HashInstance(Class, new HashDictionary(), Nil);
            else if (Class.InheritsFrom(Exception))
                return new ExceptionInstance(Class, "");
            else if (Class.InheritsFrom(Thread))
                return new ThreadInstance(Class, Script);
            else if (Class.InheritsFrom(Time))
                return new TimeInstance(Class, new DateTime());
            else if (Class.InheritsFrom(WeakRef))
                return new WeakRefInstance(Class, new WeakReference<Instance>(Nil));
            else if (Class.InheritsFrom(HTTPResponse))
                throw new RuntimeException($"{Script.ApproximateLocation}: Tried to create HTTPResponse instance with new");
            else
                return new Instance(Class);
        }

        public Api(Interpreter Interpreter) {
            this.Interpreter = Interpreter;
            Script Script = new(Interpreter);

            // Object
            Module Object = Interpreter.Object;
            Object.InstanceMethods["==", "==="] = Object.Methods["==", "==="] = Script.CreateMethod(_Object._Equals, 1);
            Object.InstanceMethods["!="] = Object.Methods["!="] = Script.CreateMethod(_Object._NotEquals, 1);
            Object.InstanceMethods["<=>"] = Object.Methods["<=>"] = Script.CreateMethod(_Object._Spaceship, 1);
            Object.InstanceMethods["inspect"] = Object.Methods["inspect"] = Script.CreateMethod(_Object.inspect, 0);
            Object.InstanceMethods["class"] = Object.Methods["class"] = Script.CreateMethod(_Object.@class, 0);
            Object.InstanceMethods["to_s"] = Object.Methods["to_s"] = Script.CreateMethod(_Object.to_s, 0);
            Object.InstanceMethods["method"] = Object.Methods["method"] = Script.CreateMethod(_Object.method, 1);
            Object.InstanceMethods["constants"] = Object.Methods["constants"] = Script.CreateMethod(_Object.constants, 0);
            Object.InstanceMethods["object_id"] = Object.Methods["object_id"] = Script.CreateMethod(_Object.object_id, 0);
            Object.InstanceMethods["hash"] = Object.Methods["hash"] = Script.CreateMethod(_Object.hash, 0);
            Object.InstanceMethods["eql?"] = Object.Methods["eql?"] = Script.CreateMethod(_Object.eql7, 1);
            Object.InstanceMethods["methods"] = Object.Methods["methods"] = Script.CreateMethod(_Object.methods, 0);
            Object.InstanceMethods["is_a?"] = Object.Methods["is_a?"] = Script.CreateMethod(_Object.is_a7, 1);
            Object.InstanceMethods["instance_of?"] = Object.Methods["instance_of?"] = Script.CreateMethod(_Object.instance_of7, 1);
            Object.InstanceMethods["in?"] = Object.Methods["in?"] = Script.CreateMethod(_Object.in7, 1);
            Object.InstanceMethods["clone"] = Object.Methods["clone"] = Script.CreateMethod(_Object.clone, 0);

            Script.CurrentAccessModifier = AccessModifier.Protected;
            Object.InstanceMethods["puts"] = Object.Methods["puts"] = Script.CreateMethod(puts, null);
            Object.InstanceMethods["print"] = Object.Methods["print"] = Script.CreateMethod(print, null);
            Object.InstanceMethods["p"] = Object.Methods["p"] = Script.CreateMethod(p, null);
            Object.InstanceMethods["gets"] = Object.Methods["gets"] = Script.CreateMethod(gets, 0);
            Object.InstanceMethods["getc"] = Object.Methods["getc"] = Script.CreateMethod(getc, 0);
            Object.InstanceMethods["warn"] = Object.Methods["warn"] = Script.CreateMethod(warn, null);
            Object.InstanceMethods["sleep"] = Object.Methods["sleep"] = Script.CreateMethod(sleep, 0..1);
            Object.InstanceMethods["raise"] = Object.Methods["raise"] = Script.CreateMethod(raise, 0..1);
            Object.InstanceMethods["throw"] = Object.Methods["throw"] = Script.CreateMethod(@throw, 1);
            Object.InstanceMethods["catch"] = Object.Methods["catch"] = Script.CreateMethod(@catch, 1);
            Object.InstanceMethods["lambda"] = Object.Methods["lambda"] = Script.CreateMethod(lambda, 0);
            Object.InstanceMethods["loop"] = Object.Methods["loop"] = Script.CreateMethod(loop, 0);
            Object.InstanceMethods["rand"] = Object.Methods["rand"] = Script.CreateMethod(_Random.rand, 0..1);
            Object.InstanceMethods["srand"] = Object.Methods["srand"] = Script.CreateMethod(_Random.srand, 0..1);
            Object.InstanceMethods["exit"] = Object.Methods["exit"] = Script.CreateMethod(exit, 0);
            Object.InstanceMethods["quit"] = Object.Methods["quit"] = Script.CreateMethod(exit, 0);
            Object.InstanceMethods["eval"] = Object.Methods["eval"] = Script.CreateMethod(eval, 1);
            Object.InstanceMethods["local_variables"] = Object.Methods["local_variables"] = Script.CreateMethod(local_variables, 0);
            Object.InstanceMethods["global_variables"] = Object.Methods["global_variables"] = Script.CreateMethod(global_variables, 0);

            Object.InstanceMethods["attr_reader"] = Script.CreateMethod(_Object.attr_reader, 1);
            Object.InstanceMethods["attr_writer"] = Script.CreateMethod(_Object.attr_writer, 1);
            Object.InstanceMethods["attr_accessor"] = Script.CreateMethod(_Object.attr_accessor, 1);
            Object.InstanceMethods["public"] = Script.CreateMethod(_Object.@public, 0);
            Object.InstanceMethods["private"] = Script.CreateMethod(_Object.@private, 0);
            Object.InstanceMethods["protected"] = Script.CreateMethod(_Object.@protected, 0);
            Script.CurrentAccessModifier = AccessModifier.Public;

            // Class
            Class Class = Interpreter.Class;
            Class.Methods["name"] = Script.CreateMethod(_Class.name, 0);
            Class.Methods["==="] = Script.CreateMethod(_Class._TripleEquals, 1);

            // Nil
            NilClass = Script.CreateClass("NilClass"); NilClass.InstanceMethods.Remove("initialize"); NilClass.Methods.Remove("new");
            Nil = new NilInstance(NilClass);

            // True
            TrueClass = Script.CreateClass("TrueClass"); TrueClass.InstanceMethods.Remove("initialize"); TrueClass.Methods.Remove("new");
            True = new TrueInstance(TrueClass);

            // False
            FalseClass = Script.CreateClass("FalseClass"); FalseClass.InstanceMethods.Remove("initialize"); FalseClass.Methods.Remove("new");
            False = new FalseInstance(FalseClass);

            // String
            String = Script.CreateClass("String");
            String.InstanceMethods["[]"] = Script.CreateMethod(_String._Indexer, 1);
            String.InstanceMethods["[]="] = Script.CreateMethod(_String._IndexEquals, 2);
            String.InstanceMethods["+"] = Script.CreateMethod(_String._Add, 1);
            String.InstanceMethods["*"] = Script.CreateMethod(_String._Multiply, 1);
            String.InstanceMethods["==", "==="] = Script.CreateMethod(_String._Equals, 1);
            String.InstanceMethods["<"] = Script.CreateMethod(_String._LessThan, 1);
            String.InstanceMethods[">"] = Script.CreateMethod(_String._GreaterThan, 1);
            String.InstanceMethods["<="] = Script.CreateMethod(_String._LessThanOrEqualTo, 1);
            String.InstanceMethods[">="] = Script.CreateMethod(_String._GreaterThanOrEqualTo, 1);
            String.InstanceMethods["<=>"] = Script.CreateMethod(_String._Spaceship, 1);
            String.InstanceMethods["initialize"] = Script.CreateMethod(_String.initialize, 0..1);
            String.InstanceMethods["to_str"] = Script.CreateMethod(_String.to_str, 0);
            String.InstanceMethods["to_i"] = Script.CreateMethod(_String.to_i, 0);
            String.InstanceMethods["to_f"] = Script.CreateMethod(_String.to_f, 0);
            String.InstanceMethods["to_sym"] = Script.CreateMethod(_String.to_sym, 0);
            String.InstanceMethods["to_a"] = Script.CreateMethod(_String.to_a, 0);
            String.InstanceMethods["length"] = Script.CreateMethod(_String.length, 0);
            String.InstanceMethods["chomp"] = Script.CreateMethod(_String.chomp, 0..1);
            String.InstanceMethods["chomp!"] = Script.CreateMethod(_String.chomp1, 0..1);
            String.InstanceMethods["strip"] = Script.CreateMethod(_String.strip, 0);
            String.InstanceMethods["strip!"] = Script.CreateMethod(_String.strip1, 0);
            String.InstanceMethods["lstrip"] = Script.CreateMethod(_String.lstrip, 0);
            String.InstanceMethods["lstrip!"] = Script.CreateMethod(_String.lstrip1, 0);
            String.InstanceMethods["rstrip"] = Script.CreateMethod(_String.rstrip, 0);
            String.InstanceMethods["rstrip!"] = Script.CreateMethod(_String.rstrip1, 0);
            String.InstanceMethods["squeeze"] = Script.CreateMethod(_String.squeeze, 0);
            String.InstanceMethods["squeeze!"] = Script.CreateMethod(_String.squeeze1, 0);
            String.InstanceMethods["chop"] = Script.CreateMethod(_String.chop, 0);
            String.InstanceMethods["chop!"] = Script.CreateMethod(_String.chop1, 0);
            String.InstanceMethods["chr"] = Script.CreateMethod(_String.chr, 0);
            String.InstanceMethods["capitalize"] = Script.CreateMethod(_String.capitalize, 0);
            String.InstanceMethods["capitalize!"] = Script.CreateMethod(_String.capitalize1, 0);
            String.InstanceMethods["upcase"] = Script.CreateMethod(_String.upcase, 0);
            String.InstanceMethods["upcase!"] = Script.CreateMethod(_String.upcase1, 0);
            String.InstanceMethods["downcase"] = Script.CreateMethod(_String.downcase, 0);
            String.InstanceMethods["downcase!"] = Script.CreateMethod(_String.downcase1, 0);
            String.InstanceMethods["sub"] = Script.CreateMethod(_String.sub, 2);
            String.InstanceMethods["sub!"] = Script.CreateMethod(_String.sub1, 2);
            String.InstanceMethods["gsub"] = Script.CreateMethod(_String.gsub, 2);
            String.InstanceMethods["gsub!"] = Script.CreateMethod(_String.gsub1, 2);
            String.InstanceMethods["eql?"] = Script.CreateMethod(_String.eql7, 1);

            // Symbol
            Symbol = Script.CreateClass("Symbol");

            // Integer
            Integer = Script.CreateClass("Integer");
            Integer.InstanceMethods["+"] = Script.CreateMethod(_Integer._Add, 1);
            Integer.InstanceMethods["-"] = Script.CreateMethod(_Integer._Subtract, 1);
            Integer.InstanceMethods["*"] = Script.CreateMethod(_Integer._Multiply, 1);
            Integer.InstanceMethods["/"] = Script.CreateMethod(_Integer._Divide, 1);
            Integer.InstanceMethods["%"] = Script.CreateMethod(_Integer._Modulo, 1);
            Integer.InstanceMethods["**"] = Script.CreateMethod(_Integer._Exponentiate, 1);
            Integer.InstanceMethods["==", "==="] = Script.CreateMethod(_Float._Equals, 1);
            Integer.InstanceMethods["<"] = Script.CreateMethod(_Float._LessThan, 1);
            Integer.InstanceMethods[">"] = Script.CreateMethod(_Float._GreaterThan, 1);
            Integer.InstanceMethods["<="] = Script.CreateMethod(_Float._LessThanOrEqualTo, 1);
            Integer.InstanceMethods[">="] = Script.CreateMethod(_Float._GreaterThanOrEqualTo, 1);
            Integer.InstanceMethods["<=>"] = Script.CreateMethod(_Float._Spaceship, 1);
            Integer.InstanceMethods["+@"] = Script.CreateMethod(_Integer._UnaryPlus, 0);
            Integer.InstanceMethods["-@"] = Script.CreateMethod(_Integer._UnaryMinus, 0);
            Integer.InstanceMethods["to_i"] = Script.CreateMethod(_Integer.to_i, 0);
            Integer.InstanceMethods["to_f"] = Script.CreateMethod(_Integer.to_f, 0);
            Integer.InstanceMethods["times"] = Script.CreateMethod(_Integer.times, 0);
            Integer.InstanceMethods["clamp"] = Script.CreateMethod(_Integer.clamp, 2);
            Integer.InstanceMethods["round"] = Script.CreateMethod(_Float.round, 0..1);
            Integer.InstanceMethods["floor"] = Script.CreateMethod(_Float.floor, 0);
            Integer.InstanceMethods["ceil"] = Script.CreateMethod(_Float.ceil, 0);
            Integer.InstanceMethods["truncate"] = Script.CreateMethod(_Float.truncate, 0);

            // Float
            Float = Script.CreateClass("Float");
            Float.Constants["INFINITY"] = GetFloat(double.PositiveInfinity);
            Float.InstanceMethods["+"] = Script.CreateMethod(_Float._Add, 1);
            Float.InstanceMethods["-"] = Script.CreateMethod(_Float._Subtract, 1);
            Float.InstanceMethods["*"] = Script.CreateMethod(_Float._Multiply, 1);
            Float.InstanceMethods["/"] = Script.CreateMethod(_Float._Divide, 1);
            Float.InstanceMethods["%"] = Script.CreateMethod(_Float._Modulo, 1);
            Float.InstanceMethods["**"] = Script.CreateMethod(_Float._Exponentiate, 1);
            Float.InstanceMethods["==", "==="] = Script.CreateMethod(_Float._Equals, 1);
            Float.InstanceMethods["<"] = Script.CreateMethod(_Float._LessThan, 1);
            Float.InstanceMethods[">"] = Script.CreateMethod(_Float._GreaterThan, 1);
            Float.InstanceMethods["<="] = Script.CreateMethod(_Float._LessThanOrEqualTo, 1);
            Float.InstanceMethods[">="] = Script.CreateMethod(_Float._GreaterThanOrEqualTo, 1);
            Float.InstanceMethods["<=>"] = Script.CreateMethod(_Float._Spaceship, 1);
            Float.InstanceMethods["+@"] = Script.CreateMethod(_Float._UnaryPlus, 0);
            Float.InstanceMethods["-@"] = Script.CreateMethod(_Float._UnaryMinus, 0);
            Float.InstanceMethods["to_i"] = Script.CreateMethod(_Float.to_i, 0);
            Float.InstanceMethods["to_f"] = Script.CreateMethod(_Float.to_f, 0);
            Float.InstanceMethods["clamp"] = Script.CreateMethod(_Float.clamp, 2);
            Float.InstanceMethods["round"] = Script.CreateMethod(_Float.round, 0..1);
            Float.InstanceMethods["floor"] = Script.CreateMethod(_Float.floor, 0);
            Float.InstanceMethods["ceil"] = Script.CreateMethod(_Float.ceil, 0);
            Float.InstanceMethods["truncate"] = Script.CreateMethod(_Float.truncate, 0);

            // Proc
            Proc = Script.CreateClass("Proc");
            Proc.InstanceMethods["call"] = Script.CreateMethod(_Proc.call, null);

            // Range
            Range = Script.CreateClass("Proc");
            Range.InstanceMethods["==="] = Script.CreateMethod(_Range._TripleEquals, 1);
            Range.InstanceMethods["min"] = Script.CreateMethod(_Range.min, 0);
            Range.InstanceMethods["max"] = Script.CreateMethod(_Range.max, 0);
            Range.InstanceMethods["each"] = Script.CreateMethod(_Range.each, 0);
            Range.InstanceMethods["reverse_each"] = Script.CreateMethod(_Range.reverse_each, 0);
            Range.InstanceMethods["length", "count"] = Script.CreateMethod(_Range.length, 0);
            Range.InstanceMethods["to_a"] = Script.CreateMethod(_Range.to_a, 0);

            // Array
            Array = Script.CreateClass("Array");
            Array.InstanceMethods["[]"] = Script.CreateMethod(_Array._Indexer, 1);
            Array.InstanceMethods["[]="] = Script.CreateMethod(_Array._IndexEquals, 2);
            Array.InstanceMethods["*"] = Script.CreateMethod(_Array._Multiply, 1);
            Array.InstanceMethods["==", "==="] = Script.CreateMethod(_Array._Equals, 1);
            Array.InstanceMethods["<<"] = Script.CreateMethod(_Array._Append, 1);
            Array.InstanceMethods["length"] = Script.CreateMethod(_Array.length, 0);
            Array.InstanceMethods["count"] = Script.CreateMethod(_Array.count, 0..1);
            Array.InstanceMethods["first"] = Script.CreateMethod(_Array.first, 0);
            Array.InstanceMethods["last"] = Script.CreateMethod(_Array.last, 0);
            Array.InstanceMethods["forty_two"] = Script.CreateMethod(_Array.forty_two, 0);
            Array.InstanceMethods["sample"] = Script.CreateMethod(_Array.sample, 0);
            Array.InstanceMethods["min"] = Script.CreateMethod(_Array.min, 0);
            Array.InstanceMethods["max"] = Script.CreateMethod(_Array.max, 0);
            Array.InstanceMethods["insert"] = Script.CreateMethod(_Array.insert, 1..);
            Array.InstanceMethods["each"] = Script.CreateMethod(_Array.each, 0);
            Array.InstanceMethods["reverse_each"] = Script.CreateMethod(_Array.reverse_each, 0);
            Array.InstanceMethods["map"] = Script.CreateMethod(_Array.map, 0);
            Array.InstanceMethods["map!"] = Script.CreateMethod(_Array.map1, 0);
            Array.InstanceMethods["sort"] = Script.CreateMethod(_Array.sort, 0);
            Array.InstanceMethods["sort!"] = Script.CreateMethod(_Array.sort1, 0);
            Array.InstanceMethods["include?", "includes?", "contain?", "contains?"] = Script.CreateMethod(_Array.include7, 1);
            Array.InstanceMethods["delete", "remove"] = Script.CreateMethod(_Array.delete, 1);
            Array.InstanceMethods["delete_at", "remove_at"] = Script.CreateMethod(_Array.delete_at, 1);
            Array.InstanceMethods["clear"] = Script.CreateMethod(_Array.clear, 0);
            Array.InstanceMethods["empty?"] = Script.CreateMethod(_Array.empty7, 0);
            Array.InstanceMethods["reverse"] = Script.CreateMethod(_Array.reverse, 0);
            Array.InstanceMethods["reverse!"] = Script.CreateMethod(_Array.reverse1, 0);

            // Hash
            Hash = Script.CreateClass("Hash");
            Hash.InstanceMethods["[]"] = Script.CreateMethod(_Hash._Indexer, 1);
            Hash.InstanceMethods["[]="] = Script.CreateMethod(_Hash._IndexEquals, 2);
            Hash.InstanceMethods["==", "==="] = Script.CreateMethod(_Hash._Equals, 1);
            Hash.InstanceMethods["initialize"] = Script.CreateMethod(_Hash.initialize, 0..1);
            Hash.InstanceMethods["length"] = Script.CreateMethod(_Hash.length, 0);
            Hash.InstanceMethods["has_key?"] = Script.CreateMethod(_Hash.has_key7, 1);
            Hash.InstanceMethods["has_value?"] = Script.CreateMethod(_Hash.has_value7, 1);
            Hash.InstanceMethods["keys"] = Script.CreateMethod(_Hash.keys, 0);
            Hash.InstanceMethods["values"] = Script.CreateMethod(_Hash.values, 0);
            Hash.InstanceMethods["delete", "remove"] = Script.CreateMethod(_Hash.delete, 1);
            Hash.InstanceMethods["clear"] = Script.CreateMethod(_Hash.clear, 0);
            Hash.InstanceMethods["each"] = Script.CreateMethod(_Hash.each, 0);
            Hash.InstanceMethods["invert"] = Script.CreateMethod(_Hash.invert, 0);
            Hash.InstanceMethods["to_a"] = Script.CreateMethod(_Hash.to_a, 0);
            Hash.InstanceMethods["to_hash"] = Script.CreateMethod(_Hash.to_hash, 0);
            Hash.InstanceMethods["empty?"] = Script.CreateMethod(_Hash.empty7, 0);

            // Random
            Random = Script.CreateModule("Random");
            Random.Methods["rand"] = Script.CreateMethod(_Random.rand, 0..1);
            Random.Methods["srand"] = Script.CreateMethod(_Random.srand, 0..1);

            // Math
            Math = Script.CreateModule("Math");
            Math.Constants["PI"] = GetFloat(System.Math.PI);
            Math.Constants["E"] = GetFloat(System.Math.E);
            Math.Methods["sin"] = Script.CreateMethod(_Math.sin, 1);
            Math.Methods["cos"] = Script.CreateMethod(_Math.cos, 1);
            Math.Methods["tan"] = Script.CreateMethod(_Math.tan, 1);
            Math.Methods["asin"] = Script.CreateMethod(_Math.asin, 1);
            Math.Methods["acos"] = Script.CreateMethod(_Math.acos, 1);
            Math.Methods["atan"] = Script.CreateMethod(_Math.atan, 1);
            Math.Methods["atan2"] = Script.CreateMethod(_Math.atan2, 2);
            Math.Methods["sinh"] = Script.CreateMethod(_Math.sinh, 1);
            Math.Methods["cosh"] = Script.CreateMethod(_Math.cosh, 1);
            Math.Methods["tanh"] = Script.CreateMethod(_Math.tanh, 1);
            Math.Methods["asinh"] = Script.CreateMethod(_Math.asinh, 1);
            Math.Methods["acosh"] = Script.CreateMethod(_Math.acosh, 1);
            Math.Methods["atanh"] = Script.CreateMethod(_Math.atanh, 1);
            Math.Methods["exp"] = Script.CreateMethod(_Math.exp, 1);
            Math.Methods["log"] = Script.CreateMethod(_Math.log, 2);
            Math.Methods["log10"] = Script.CreateMethod(_Math.log10, 1);
            Math.Methods["log2"] = Script.CreateMethod(_Math.log2, 1);
            Math.Methods["frexp"] = Script.CreateMethod(_Math.frexp, 1);
            Math.Methods["ldexp"] = Script.CreateMethod(_Math.ldexp, 2);
            Math.Methods["sqrt"] = Script.CreateMethod(_Math.sqrt, 1);
            Math.Methods["cbrt"] = Script.CreateMethod(_Math.cbrt, 1);
            Math.Methods["hypot"] = Script.CreateMethod(_Math.hypot, 2);
            Math.Methods["erf"] = Script.CreateMethod(_Math.erf, 1);
            Math.Methods["erfc"] = Script.CreateMethod(_Math.erfc, 1);
            Math.Methods["gamma"] = Script.CreateMethod(_Math.gamma, 1);
            Math.Methods["lgamma"] = Script.CreateMethod(_Math.lgamma, 1);
            Math.Methods["to_rad"] = Script.CreateMethod(_Math.to_rad, 1);
            Math.Methods["to_deg"] = Script.CreateMethod(_Math.to_deg, 1);
            Math.Methods["lerp"] = Script.CreateMethod(_Math.lerp, 3);

            // Exception
            Exception = Script.CreateClass("Exception");
            Exception.InstanceMethods["initialize"] = Script.CreateMethod(_Exception.initialize, 0..1);
            Exception.InstanceMethods["message"] = Script.CreateMethod(_Exception.message, 0);
            Exception.InstanceMethods["backtrace"] = Script.CreateMethod(_Exception.backtrace, 0);
            // StandardError
            StandardError = Script.CreateClass("StandardError", InheritsFrom: Exception);
            // RuntimeError
            RuntimeError = Script.CreateClass("RuntimeError", InheritsFrom: StandardError);

            // Thread
            Thread = Script.CreateClass("Thread");
            Thread.InstanceMethods["initialize"] = Script.CreateMethod(_Thread.initialize, null);
            Thread.InstanceMethods["join"] = Script.CreateMethod(_Thread.join, 0);
            Thread.InstanceMethods["stop"] = Script.CreateMethod(_Thread.stop, 0);

            // Parallel
            Parallel = Script.CreateModule("Parallel");
            Parallel.Methods["each"] = Script.CreateMethod(_Parallel.each, 1);
            Parallel.Methods["times"] = Script.CreateMethod(_Parallel.times, 1);

            // Time
            Time = Script.CreateClass("Time");
            Time.Methods["now"] = Script.CreateMethod(_Time.now, 0);
            Time.Methods["at"] = Script.CreateMethod(_Time.at, 1);
            Time.InstanceMethods["initialize"] = Script.CreateMethod(_Time.initialize, 0..7);
            Time.InstanceMethods["to_i"] = Script.CreateMethod(_Time.to_i, 0);
            Time.InstanceMethods["to_f"] = Script.CreateMethod(_Time.to_f, 0);

            // WeakRef
            WeakRef = Script.CreateClass("WeakRef");
            WeakRef.InstanceMethods["initialize"] = Script.CreateMethod(_WeakRef.initialize, 1);
            WeakRef.InstanceMethods["method_missing"] = Script.CreateMethod(_WeakRef.method_missing, null);
            WeakRef.InstanceMethods["weakref_alive?"] = Script.CreateMethod(_WeakRef.weakref_alive7, 0);

            // Global constants
            Interpreter.RootScope.Constants["EMBERS_VERSION"] = new StringInstance(String, Info.Version);
            Interpreter.RootScope.Constants["EMBERS_RELEASE_DATE"] = new StringInstance(String, Info.ReleaseDate);
            Interpreter.RootScope.Constants["EMBERS_PLATFORM"] = new StringInstance(String, $"{RuntimeInformation.OSArchitecture}-{RuntimeInformation.OSDescription}");
            Interpreter.RootScope.Constants["EMBERS_COPYRIGHT"] = new StringInstance(String, Info.Copyright);
            Interpreter.RootScope.Constants["RUBY_COPYRIGHT"] = new StringInstance(String, Info.RubyCopyright);

            //
            // UNSAFE APIS
            //

            // Global methods
            Script.CurrentAccessModifier = AccessModifier.Protected;
            Object.InstanceMethods["system"] = Object.Methods["system"] = Script.CreateMethod(system, 1, IsUnsafe: true);
            Script.CurrentAccessModifier = AccessModifier.Public;

            // File
            File = Script.CreateModule("File");
            File.Methods["read"] = Script.CreateMethod(_File.read, 1, IsUnsafe: true);
            File.Methods["write"] = Script.CreateMethod(_File.write, 2, IsUnsafe: true);
            File.Methods["append"] = Script.CreateMethod(_File.append, 2, IsUnsafe: true);
            File.Methods["delete"] = Script.CreateMethod(_File.delete, 1, IsUnsafe: true);
            File.Methods["exist?", "exists?"] = Script.CreateMethod(_File.exist7, 1, IsUnsafe: true);
            File.Methods["absolute_path"] = Script.CreateMethod(_File.absolute_path, 1, IsUnsafe: true);
            File.Methods["absolute_path?"] = Script.CreateMethod(_File.absolute_path7, 1, IsUnsafe: true);
            File.Methods["basename"] = Script.CreateMethod(_File.basename, 1, IsUnsafe: true);
            File.Methods["dirname"] = Script.CreateMethod(_File.dirname, 1, IsUnsafe: true);

            // Net
            Net = Script.CreateModule("Net");
            // Net::HTTP
            HTTP = Script.CreateModule("HTTP", Net);
            HTTP.Methods["get"] = Script.CreateMethod(_Net._HTTP.get, 1, IsUnsafe: true);
            // Net::HTTP::HTTPResponse
            HTTPResponse = Script.CreateClass("HTTPResponse", HTTP);
            HTTPResponse.InstanceMethods["body"] = Script.CreateMethod(_Net._HTTP._HTTPResponse.body, 0, IsUnsafe: true);
            HTTPResponse.InstanceMethods["code"] = Script.CreateMethod(_Net._HTTP._HTTPResponse.code, 0, IsUnsafe: true);
        }

        // API
        static async Task<Instance> puts(MethodInput Input) {
            if (Input.Arguments.Count != 0) {
                foreach (Instance Message in Input.Arguments) {
                    Console.WriteLine(Message.LightInspect());
                }
            }
            else {
                Console.WriteLine();
            }
            return Input.Api.Nil;
        }
        static async Task<Instance> print(MethodInput Input) {
            foreach (Instance Message in Input.Arguments) {
                Console.Write(Message.LightInspect());
            }
            return Input.Api.Nil;
        }
        static async Task<Instance> p(MethodInput Input) {
            foreach (Instance Message in Input.Arguments) {
                Console.WriteLine(Message.Inspect());
            }
            return Input.Api.Nil;
        }
        static async Task<Instance> gets(MethodInput Input) {
            string? UserInput = Console.ReadLine();
            UserInput = UserInput != null ? UserInput + "\n" : "";
            return new StringInstance(Input.Api.String, UserInput);
        }
        static async Task<Instance> getc(MethodInput Input) {
            string UserInput = Console.ReadKey().KeyChar.ToString();
            return new StringInstance(Input.Api.String, UserInput);
        }
        static async Task<Instance> warn(MethodInput Input) {
            ConsoleColor PreviousForegroundColour = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            foreach (Instance Message in Input.Arguments) {
                Console.WriteLine(Message.Object);
            }
            Console.ForegroundColor = PreviousForegroundColour;
            return Input.Api.Nil;
        }
        static async Task<Instance> sleep(MethodInput Input) {
            if (Input.Arguments.Count == 1) {
                DynFloat SecondsToSleep = Input.Arguments[0].Float;
                await Task.Delay((int)(SecondsToSleep * 1000));
            }
            else {
                await Task.Delay(Timeout.Infinite);
            }
            return Input.Api.Nil;
        }
        static async Task<Instance> raise(MethodInput Input) {
            if (Input.Arguments.Count == 1) {
                Instance Argument = Input.Arguments[0];
                if (Argument is ExceptionInstance ExceptionInstance) {
                    // raise Exception.new("message")
                    Exception ExceptionToRaise = Argument.Exception;
                    Input.Script.ExceptionsTable.TryAdd(ExceptionToRaise, ExceptionInstance);
                    throw ExceptionToRaise;
                }
                else {
                    // raise "message"
                    Exception NewExceptionToRaise = new RuntimeException(Argument.String);
                    Input.Script.ExceptionsTable.TryAdd(NewExceptionToRaise, new ExceptionInstance(Input.Api.RuntimeError, Argument.String));
                    throw NewExceptionToRaise;
                }
            }
            else {
                // raise
                Exception NewExceptionToRaise = new RuntimeException("");
                Input.Script.ExceptionsTable.TryAdd(NewExceptionToRaise, new ExceptionInstance(Input.Api.RuntimeError, ""));
                throw NewExceptionToRaise;
            }
        }
        static async Task<Instance> @throw(MethodInput Input) {
            throw ThrowException.New(Input.Arguments[0]);
        }
        static async Task<Instance> @catch(MethodInput Input) {
            Method? OnYield = Input.OnYield ?? throw new RuntimeException($"{Input.Location}: No block given for catch");

            string CatchIdentifier = Input.Arguments[0].String;
            try {
                await OnYield.Call(Input.Script, null, CatchReturn: false);
            }
            catch (ThrowException Ex) {
                if (Ex.Identifier != CatchIdentifier)
                    throw Ex;
            }
            return Input.Api.Nil;
        }
        static async Task<Instance> lambda(MethodInput Input) {
            Method? OnYield = Input.OnYield ?? throw new RuntimeException($"{Input.Location}: No block given for lambda");

            Instance NewProc = new ProcInstance(Input.Api.Proc, Input.Script.CreateMethod(
                async Input => await OnYield.Call(Input.Script, null, Input.Arguments, Input.OnYield, CatchReturn: false),
                null
            ));
            return NewProc;
        }
        static async Task<Instance> loop(MethodInput Input) {
            Method? OnYield = Input.OnYield ?? throw new RuntimeException($"{Input.Location}: No block given for loop");

            while (true) {
                try {
                    await OnYield.Call(Input.Script, null, BreakHandleType: BreakHandleType.Rethrow, CatchReturn: false);
                }
                catch (BreakException) {
                    break;
                }
                catch (LoopControlException Ex) when (Ex is RetryException or RedoException or NextException) {
                    continue;
                }
                catch (LoopControlException Ex) {
                    throw new SyntaxErrorException($"{Input.Location}: {Ex.GetType().Name} not valid in loop do end");
                }
            }
            return Input.Api.Nil;
        }
        static async Task<Instance> system(MethodInput Input) {
            string Command = Input.Arguments[0].String;

            // Start command line process
            ProcessStartInfo Info = new("cmd.exe") {
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = "/c " + Command
            };
            Process Process = new() {
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
            return new StringInstance(Input.Api.String, Output);
        }
        static async Task<Instance> exit(MethodInput Input) {
            throw new ExitException();
        }
        static async Task<Instance> eval(MethodInput Input) {
            try {
                return await Input.Script.InternalEvaluateAsync(Input.Arguments[0].String);
            }
            catch (LoopControlException Ex) {
                throw new SyntaxErrorException($"{Input.Location}: Can't escape from eval with {Ex.GetType().Name}");
            }
            catch (ReturnException Ex) {
                return Ex.Instance;
            }
        }
        static async Task<Instance> local_variables(MethodInput Input) {
            List<Instance> GlobalVariables = new();
            foreach (KeyValuePair<string, Instance> GlobalVariable in Input.Script.GetAllLocalVariables()) {
                GlobalVariables.Add(Input.Api.GetSymbol(GlobalVariable.Key));
            }
            return new ArrayInstance(Input.Api.Array, GlobalVariables);
        }
        static async Task<Instance> global_variables(MethodInput Input) {
            List<Instance> GlobalVariables = new();
            foreach (KeyValuePair<string, Instance> GlobalVariable in Input.Interpreter.GlobalVariables) {
                GlobalVariables.Add(Input.Api.GetSymbol(GlobalVariable.Key));
            }
            return new ArrayInstance(Input.Api.Array, GlobalVariables);
        }
        static class _Object {
            public static async Task<Instance> _Equals(MethodInput Input) {
                Instance Left = Input.Instance;
                Instance Right = Input.Arguments[0];
                if (Left is ModuleReference LeftModule && Right is ModuleReference RightModule) {
                    return LeftModule.Module == RightModule.Module ? Input.Api.True : Input.Api.False;
                }
                else {
                    return Left == Right ? Input.Api.True : Input.Api.False;
                }
            }
            public static async Task<Instance> _NotEquals(MethodInput Input) {
                Instance Left = Input.Instance;
                Instance Right = Input.Arguments[0];
                return (await Left.CallInstanceMethod(Input.Script, "==", Right)).IsTruthy ? Input.Api.False : Input.Api.True;
            }
            public static async Task<Instance> _Spaceship(MethodInput Input) {
                return Input.Api.Nil;
            }
            public static async Task<Instance> inspect(MethodInput Input) {
                return new StringInstance(Input.Api.String, Input.Instance.Inspect());
            }
            public static async Task<Instance> @class(MethodInput Input) {
                if (Input.Instance is ModuleReference) {
                    return new ModuleReference(Input.Interpreter.Class);
                }
                else {
                    return new ModuleReference(Input.Instance.Module!);
                }
            }
            public static async Task<Instance> to_s(MethodInput Input) {
                return new StringInstance(Input.Api.String, Input.Instance.LightInspect());
            }
            public static async Task<Instance> method(MethodInput Input) {
                // Find method
                string MethodName = Input.Arguments[0].String;
                Method? FindMethod;
                bool Found;
                if (Input.Instance is ModuleReference) {
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
                    return new ProcInstance(Input.Api.Proc, FindMethod!);
                }
                else {
                    throw new RuntimeException($"{Input.Location}: Undefined method '{MethodName}' for {Input.Instance.LightInspect()}");
                }
            }
            public static async Task<Instance> constants(MethodInput Input) {
                List<Instance> Constants = new();
                foreach (KeyValuePair<string, Instance> Constant in Input.Script.GetAllLocalConstants()) {
                    Constants.Add(Input.Api.GetSymbol(Constant.Key));
                }
                return new ArrayInstance(Input.Api.Array, Constants);
            }
            public static async Task<Instance> object_id(MethodInput Input) {
                return Input.Api.GetInteger(Input.Instance.ObjectId);
            }
            public static async Task<Instance> hash(MethodInput Input) {
                DynInteger Hash = (Input.Instance.GetHashCode().ToString() + ((DynInteger)31 * Input.Instance.Module!.GetHashCode()).ToString()).ParseInteger();
                return Input.Api.GetInteger(Hash);
            }
            public static async Task<Instance> eql7(MethodInput Input) {
                Instance Other = Input.Arguments[0];
                return (await Input.Instance.CallInstanceMethod(Input.Script, "hash")).Integer == (await Other.CallInstanceMethod(Input.Script, "hash")).Integer
                    ? Input.Api.True : Input.Api.False;
            }
            public static async Task<Instance> methods(MethodInput Input) {
                List<Instance> MethodsDictToSymbolsArray(LockingDictionary<string, Method> MethodDict) {
                    List<Instance> Symbols = new();
                    foreach (string MethodName in MethodDict.Keys) {
                        Symbols.Add(Input.Api.GetSymbol(MethodName));
                    }
                    return Symbols;
                }
                // Get class methods
                if (Input.Instance is ModuleReference ModuleReference) {
                    return new ArrayInstance(Input.Api.Array, MethodsDictToSymbolsArray(ModuleReference.Module!.Methods));
                }
                // Get instance methods
                else {
                    return new ArrayInstance(Input.Api.Array, MethodsDictToSymbolsArray(Input.Instance.InstanceMethods));
                }
            }
            public static async Task<Instance> is_a7(MethodInput Input) {
                Instance Argument = Input.Arguments[0];
                if (Argument is ModuleReference ModuleRef && Input.Instance is not (PseudoInstance or ModuleReference)) {
                    return Input.Instance.Module!.InheritsFrom(ModuleRef.Module!) ? Input.Api.True : Input.Api.False;
                }
                else {
                    throw new RuntimeException($"{Input.Location}: Expected class/module for is_a?");
                }
            }
            public static async Task<Instance> instance_of7(MethodInput Input) {
                Instance Argument = Input.Arguments[0];
                if (Argument is ModuleReference ModuleRef && Input.Instance is not (PseudoInstance or ModuleReference)) {
                    return Input.Instance.Module! == ModuleRef.Module! ? Input.Api.True : Input.Api.False;
                }
                else {
                    throw new RuntimeException($"{Input.Location}: Expected class/module for instance_of?");
                }
            }
            public static async Task<Instance> in7(MethodInput Input) {
                List<Instance> Array = Input.Arguments[0].Array;
                foreach (Instance Item in Array) {
                    if ((await Item.InstanceMethods["=="].Call(Input.Script, Item, Input.Instance)).IsTruthy) {
                        return Input.Api.True;
                    }
                }
                return Input.Api.False;
            }
            public static async Task<Instance> clone(MethodInput Input) {
                return Input.Instance.Clone(Input.Interpreter);
            }
            public static async Task<Instance> attr_reader(MethodInput Input) {
                string VariableName = Input.Arguments[0].String;
                // Prevent redefining unsafe API methods
                if (!Input.Script.AllowUnsafeApi && Input.Instance.InstanceMethods.TryGetValue(VariableName, out Method? ExistingMethod) && ExistingMethod.Unsafe) {
                    throw new RuntimeException($"{Input.Location}: The instance method '{VariableName}' cannot be redefined since 'AllowUnsafeApi' is disabled for this script.");
                }
                // Create or overwrite instance method
                Input.Instance.InstanceMethods[VariableName] = Input.Script.CreateMethod(async Input2 => {
                    Input2.Instance.InstanceVariables.TryGetValue(VariableName, out Instance? Value);
                    return Value ?? Input.Api.Nil;
                }, 0);

                return Input.Api.Nil;
            }
            public static async Task<Instance> attr_writer(MethodInput Input) {
                string VariableName = Input.Arguments[0].String;
                // Prevent redefining unsafe API methods
                if (!Input.Script.AllowUnsafeApi && Input.Instance.InstanceMethods.TryGetValue(VariableName, out Method? ExistingMethod) && ExistingMethod.Unsafe) {
                    throw new RuntimeException($"{Input.Location}: The instance method '{VariableName}' cannot be redefined since 'AllowUnsafeApi' is disabled for this script.");
                }
                // Create or overwrite instance method
                Input.Instance.InstanceMethods[$"{VariableName}="] = Input.Script.CreateMethod(async Input2 => {
                    return Input2.Instance.InstanceVariables[VariableName] = Input2.Arguments[0];
                }, 1);

                return Input.Api.Nil;
            }
            public static async Task<Instance> attr_accessor(MethodInput Input) {
                await attr_writer(Input);
                await attr_reader(Input);
                return Input.Api.Nil;
            }
            public static async Task<Instance> @public(MethodInput Input) {
                Input.Script.CurrentAccessModifier = AccessModifier.Public;
                return Input.Api.Nil;
            }
            public static async Task<Instance> @private(MethodInput Input) {
                Input.Script.CurrentAccessModifier = AccessModifier.Private;
                return Input.Api.Nil;
            }
            public static async Task<Instance> @protected(MethodInput Input) {
                Input.Script.CurrentAccessModifier = AccessModifier.Protected;
                return Input.Api.Nil;
            }
        }
        static class _String {
            public static async Task<Instance> _Add(MethodInput Input) {
                Instance Right = Input.Arguments[0];
                return new StringInstance(Input.Api.String, Input.Instance.String + Right.String);
            }
            public static async Task<Instance> _Multiply(MethodInput Input) {
                Instance Right = Input.Arguments[0];
                StringBuilder JoinedString = new();
                long RepeatCount = (long)Right.Integer;
                for (long i = 0; i < RepeatCount; i++) {
                    JoinedString.Append(Input.Instance.String);
                }
                return new StringInstance(Input.Api.String, JoinedString.ToString());
            }
            public static async Task<Instance> _Equals(MethodInput Input) {
                Instance Left = Input.Instance;
                Instance Right = Input.Arguments[0];
                if (Right is StringInstance RightString && Left.String == RightString.String) {
                    return Input.Api.True;
                }
                else {
                    return Input.Api.False;
                }
            }
            private static int _RealisticIndex(MethodInput Input, DynInteger RawIndex) {
                if (RawIndex < int.MinValue || RawIndex > int.MaxValue) {
                    throw new RuntimeException($"{Input.Location}: Index ({RawIndex}) is too large for string.");
                }
                int Index = (int)RawIndex;
                return Index;
            }
            public static async Task<Instance> _Indexer(MethodInput Input) {
                // Get string and index
                string String = Input.Instance.String;
                Instance Indexer = Input.Arguments[0];

                if (Indexer is RangeInstance RangeIndexer) {
                    // Return substring in range
                    int StartIndex = RangeIndexer.Min != null ? _RealisticIndex(Input, RangeIndexer.Min.Integer) : 0;
                    int EndIndex = RangeIndexer.Max != null ? _RealisticIndex(Input, RangeIndexer.Max.Integer) : String.Length - 1;
                    if (StartIndex < 0) StartIndex = 0;
                    if (EndIndex >= String.Length) EndIndex = String.Length - 1;

                    return new StringInstance(Input.Api.String, String[StartIndex..(EndIndex + 1)]);
                }
                else {
                    int Index = _RealisticIndex(Input, Input.Arguments[0].Integer);

                    // Return character at string index or nil
                    if (Index >= 0 && Index < String.Length) {
                        return new StringInstance(Input.Api.String, String[Index].ToString());
                    }
                    else if (Index < 0 && Index > -String.Length) {
                        return new StringInstance(Input.Api.String, String[^-Index].ToString());
                    }
                    else {
                        return Input.Api.Nil;
                    }
                }
            }
            public static async Task<Instance> _IndexEquals(MethodInput Input) {
                // Get string, index and value
                StringInstance StringInstance = ((StringInstance)Input.Instance);
                string String = StringInstance.String;
                int Index = _RealisticIndex(Input, Input.Arguments[0].Integer);
                string Value = Input.Arguments[1].String;

                // Set value
                if (Index >= 0 && Index < String.Length) {
                    List<string> Charas = String.ToList().ConvertAll(c => c.ToString());
                    Charas[Index] = Value;
                    StringInstance.SetValue(string.Concat(Charas));
                }
                else if (Index < 0 && Index > -String.Length) {
                    List<string> Charas = String.ToList().ConvertAll(c => c.ToString());
                    Charas[^-Index] = Value;
                    StringInstance.SetValue(string.Concat(Charas));
                }
                else {
                    throw new RuntimeException($"{Input.Location}: Index {Index} outside of string");
                }
                return StringInstance;
            }
            public static async Task<Instance> _LessThan(MethodInput Input) {
                Instance Left = Input.Instance;
                Instance Right = Input.Arguments[0];
                if (Right is StringInstance && string.Compare(Left.String, Right.String) < 0) {
                    return Input.Api.True;
                }
                else {
                    return Input.Api.False;
                }
            }
            public static async Task<Instance> _GreaterThan(MethodInput Input) {
                Instance Left = Input.Instance;
                Instance Right = Input.Arguments[0];
                if (Right is StringInstance && string.Compare(Left.String, Right.String) > 0) {
                    return Input.Api.True;
                }
                else {
                    return Input.Api.False;
                }
            }
            public static async Task<Instance> _LessThanOrEqualTo(MethodInput Input) {
                Instance Left = Input.Instance;
                Instance Right = Input.Arguments[0];
                if (Right is StringInstance && string.Compare(Left.String, Right.String) <= 0) {
                    return Input.Api.True;
                }
                else {
                    return Input.Api.False;
                }
            }
            public static async Task<Instance> _GreaterThanOrEqualTo(MethodInput Input) {
                Instance Left = Input.Instance;
                Instance Right = Input.Arguments[0];
                if (Right is StringInstance && string.Compare(Left.String, Right.String) >= 0) {
                    return Input.Api.True;
                }
                else {
                    return Input.Api.False;
                }
            }
            public static async Task<Instance> _Spaceship(MethodInput Input) {
                if ((await _LessThan(Input)).IsTruthy) {
                    return Input.Api.GetInteger(-1);
                }
                else if ((await _Equals(Input)).IsTruthy) {
                    return Input.Api.GetInteger(0);
                }
                else if ((await _GreaterThan(Input)).IsTruthy) {
                    return Input.Api.GetInteger(1);
                }
                return Input.Api.Nil;
            }
            public static async Task<Instance> initialize(MethodInput Input) {
                if (Input.Arguments.Count == 1) {
                    ((StringInstance)Input.Instance).SetValue(Input.Arguments[0].String);
                }
                return Input.Api.Nil;
            }
            public static async Task<Instance> to_str(MethodInput Input) {
                return await _Object.to_s(Input);
            }
            public static async Task<Instance> to_i(MethodInput Input) {
                string IntegerAsString = Input.Instance.LightInspect();
                StringBuilder IntegerString = new();
                for (int i = 0; i < IntegerAsString.Length; i++) {
                    char Chara = IntegerAsString[i];

                    if (Chara.IsAsciiDigit()) {
                        IntegerString.Append(Chara);
                    }
                    else {
                        break;
                    }
                }
                if (IntegerString.Length == 0) return Input.Api.GetInteger(0);
                return Input.Api.GetInteger(IntegerString.ToString().ParseInteger());
            }
            public static async Task<Instance> to_f(MethodInput Input) {
                string FloatAsString = Input.Instance.LightInspect();
                StringBuilder FloatString = new();
                bool SeenDot = false;
                for (int i = 0; i < FloatAsString.Length; i++) {
                    char Chara = FloatAsString[i];

                    if (Chara.IsAsciiDigit()) {
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
                if (FloatString.Length == 0) return Input.Api.GetFloat(0);
                if (!SeenDot) FloatString.Append(".0");
                return Input.Api.GetFloat(FloatString.ToString().ParseFloat());
            }
            public static async Task<Instance> to_sym(MethodInput Input) {
                return Input.Api.GetSymbol(Input.Instance.String);
            }
            public static async Task<Instance> to_a(MethodInput Input) {
                List<Instance> Array = new();
                foreach (char Chara in Input.Instance.String) {
                    Array.Add(new StringInstance(Input.Api.String, Chara.ToString()));
                }
                return new ArrayInstance(Input.Api.Array, Array);
            }
            private static StringInstance _CreateOrSetString(MethodInput Input, string Value, bool Exclaim) {
                if (Exclaim) {
                    StringInstance String = (StringInstance)Input.Instance;
                    String.SetValue(Value);
                    return String;
                }
                else {
                    return new StringInstance(Input.Api.String, Value);
                }
            }
            private static async Task<Instance> _chomp(MethodInput Input, bool Exclaim = false) {
                string String = Input.Instance.String;
                if (Input.Arguments.Count == 0) {
                    if (String.EndsWith("\r\n")) {
                        return _CreateOrSetString(Input, String[0..^2], Exclaim);
                    }
                    else if (String.EndsWith('\n') || String.EndsWith('\r')) {
                        return _CreateOrSetString(Input, String[0..^1], Exclaim);
                    }
                }
                else {
                    string RemoveFromEnd = Input.Arguments[0].String;
                    if (String.EndsWith(RemoveFromEnd)) {
                        return _CreateOrSetString(Input, String[0..^RemoveFromEnd.Length], Exclaim);
                    }
                }
                return Input.Instance;
            }
            public static async Task<Instance> length(MethodInput Input) {
                return Input.Api.GetInteger(Input.Instance.String.Length);
            }
            public static async Task<Instance> chomp(MethodInput Input) => await _chomp(Input, false);
            public static async Task<Instance> chomp1(MethodInput Input) => await _chomp(Input, true);
            static async Task<Instance> ModifyString(MethodInput Input, Func<string, string> Modifier, bool Exclaim) {
                string OriginalString = Input.Instance.String;
                string ModifiedString = Modifier(OriginalString);
                if (ModifiedString != OriginalString) {
                    return _CreateOrSetString(Input, ModifiedString, Exclaim);
                }
                return Input.Instance;
            }
            private static async Task<Instance> _strip(MethodInput Input, bool Exclaim = false) {
                return await ModifyString(Input, Str => Str.Trim(), Exclaim);
            }
            public static async Task<Instance> strip(MethodInput Input) => await _strip(Input, false);
            public static async Task<Instance> strip1(MethodInput Input) => await _strip(Input, true);
            private static async Task<Instance> _lstrip(MethodInput Input, bool Exclaim = false) {
                return await ModifyString(Input, Str => Str.TrimStart(), Exclaim);
            }
            public static async Task<Instance> lstrip(MethodInput Input) => await _lstrip(Input, false);
            public static async Task<Instance> lstrip1(MethodInput Input) => await _lstrip(Input, true);
            private static async Task<Instance> _rstrip(MethodInput Input, bool Exclaim = false) {
                return await ModifyString(Input, Str => Str.TrimEnd(), Exclaim);
            }
            public static async Task<Instance> rstrip(MethodInput Input) => await _rstrip(Input, false);
            public static async Task<Instance> rstrip1(MethodInput Input) => await _rstrip(Input, true);
            private static async Task<Instance> _squeeze(MethodInput Input, bool Exclaim = false) {
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
                }, Exclaim);
            }
            public static async Task<Instance> squeeze(MethodInput Input) => await _squeeze(Input, false);
            public static async Task<Instance> squeeze1(MethodInput Input) => await _squeeze(Input, true);
            private static async Task<Instance> _chop(MethodInput Input, bool Exclaim = false) {
                return await ModifyString(Input, Str => Str.Length != 0 ? Str[..^1] : Str, Exclaim);
            }
            public static async Task<Instance> chop(MethodInput Input) => await _chop(Input, false);
            public static async Task<Instance> chop1(MethodInput Input) => await _chop(Input, true);
            public static async Task<Instance> chr(MethodInput Input) {
                return await ModifyString(Input, Str => Str.Length != 0 ? Str[0].ToString() : Str, false);
            }
            private static async Task<Instance> _capitalize(MethodInput Input, bool Exclaim = false) {
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
                }, Exclaim);
            }
            public static async Task<Instance> capitalize(MethodInput Input) => await _capitalize(Input, false);
            public static async Task<Instance> capitalize1(MethodInput Input) => await _capitalize(Input, true);
            private static async Task<Instance> _upcase(MethodInput Input, bool Exclaim = false) {
                return await ModifyString(Input, Str => Str.ToUpperInvariant(), Exclaim);
            }
            public static async Task<Instance> upcase(MethodInput Input) => await _upcase(Input, false);
            public static async Task<Instance> upcase1(MethodInput Input) => await _upcase(Input, true);
            private static async Task<Instance> _downcase(MethodInput Input, bool Exclaim = false) {
                return await ModifyString(Input, Str => Str.ToLowerInvariant(), Exclaim);
            }
            public static async Task<Instance> downcase(MethodInput Input) => await _downcase(Input, false);
            public static async Task<Instance> downcase1(MethodInput Input) => await _downcase(Input, true);
            private static async Task<Instance> _sub(MethodInput Input, bool Exclaim = false) {
                string Replace = Input.Arguments[0].String;
                string With = Input.Arguments[1].String;
                return await ModifyString(Input, Str => Str.ReplaceFirst(Replace, With), Exclaim);
            }
            public static async Task<Instance> sub(MethodInput Input) => await _sub(Input, false);
            public static async Task<Instance> sub1(MethodInput Input) => await _sub(Input, true);
            private static async Task<Instance> _gsub(MethodInput Input, bool Exclaim = false) {
                string Replace = Input.Arguments[0].String;
                string With = Input.Arguments[1].String;
                return await ModifyString(Input, Str => Str.Replace(Replace, With), Exclaim);
            }
            public static async Task<Instance> gsub(MethodInput Input) => await _gsub(Input, false);
            public static async Task<Instance> gsub1(MethodInput Input) => await _gsub(Input, true);
            public static async Task<Instance> eql7(MethodInput Input) {
                return await _Equals(Input);
            }
        }
        static class _Integer {
            private static Instance _GetResult(MethodInput Input, DynFloat Result, bool RightIsInteger) {
                if (RightIsInteger) {
                    return Input.Api.GetInteger((DynInteger)Result);
                }
                else {
                    return Input.Api.GetFloat(Result);
                }
            }
            public static async Task<Instance> _Add(MethodInput Input) {
                Instance Right = Input.Arguments[0];
                return _GetResult(Input, Input.Instance.Integer + Right.Float, Right is IntegerInstance);
            }
            public static async Task<Instance> _Subtract(MethodInput Input) {
                Instance Right = Input.Arguments[0];
                return _GetResult(Input, Input.Instance.Integer - Right.Float, Right is IntegerInstance);
            }
            public static async Task<Instance> _Multiply(MethodInput Input) {
                Instance Right = Input.Arguments[0];
                return _GetResult(Input, Input.Instance.Integer * Right.Float, Right is IntegerInstance);
            }
            public static async Task<Instance> _Divide(MethodInput Input) {
                Instance Right = Input.Arguments[0];
                if (Right.Float == 0) throw new DivideByZeroException();
                return _GetResult(Input, Input.Instance.Integer / Right.Float, Right is IntegerInstance);
            }
            public static async Task<Instance> _Modulo(MethodInput Input) {
                Instance Right = Input.Arguments[0];
                return _GetResult(Input, Input.Instance.Integer % Right.Float, Right is IntegerInstance);
            }
            public static async Task<Instance> _Exponentiate(MethodInput Input) {
                Instance Right = Input.Arguments[0];
                return _GetResult(Input, System.Math.Pow((long)Input.Instance.Integer, (double)Right.Float), Right is IntegerInstance);
            }
            public static async Task<Instance> _UnaryPlus(MethodInput Input) {
                return Input.Instance;
            }
            public static async Task<Instance> _UnaryMinus(MethodInput Input) {
                return Input.Api.GetInteger(-Input.Instance.Integer);
            }
            public static async Task<Instance> to_i(MethodInput Input) {
                return Input.Instance;
            }
            public static async Task<Instance> to_f(MethodInput Input) {
                return Input.Api.GetFloat(Input.Instance.Float);
            }
            public static async Task<Instance> clamp(MethodInput Input) {
                DynInteger Number = Input.Instance.Integer;
                Instance Min = Input.Arguments[0];
                Instance Max = Input.Arguments[1];
                if ((DynFloat)Number < Min.Float) {
                    if (Min is IntegerInstance)
                        return Input.Api.GetInteger(Min.Integer);
                    else
                        return Input.Api.GetFloat(Min.Float);
                }
                else if ((DynFloat)Number > Max.Float) {
                    if (Max is IntegerInstance)
                        return Input.Api.GetInteger(Max.Integer);
                    else
                        return Input.Api.GetFloat(Max.Float);
                }
                else {
                    return Input.Instance;
                }
            }
            public static async Task<Instance> times(MethodInput Input) {
                if (Input.OnYield != null) {
                    DynInteger Times = Input.Instance.Integer;
                    bool TakesArgument = Input.OnYield.ArgumentNames.Count == 1;

                    for (DynInteger i = 0; i < Times; i++) {
                        try {
                            // x.times do |n|
                            if (TakesArgument) {
                                await Input.OnYield.Call(Input.Script, null, Input.Api.GetInteger(i), BreakHandleType: BreakHandleType.Rethrow, CatchReturn: false);
                            }
                            // x.times do
                            else {
                                await Input.OnYield.Call(Input.Script, null, BreakHandleType: BreakHandleType.Rethrow, CatchReturn: false);
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
                            throw new SyntaxErrorException($"{Input.Location}: {Ex.GetType().Name} not valid in {Times}.times");
                        }
                    }
                }
                return Input.Api.Nil;
            }
        }
        static class _Float {
            public static async Task<Instance> _Add(MethodInput Input) {
                Instance Right = Input.Arguments[0];
                return Input.Api.GetFloat(Input.Instance.Float + Right.Float);
            }
            public static async Task<Instance> _Subtract(MethodInput Input) {
                Instance Right = Input.Arguments[0];
                return Input.Api.GetFloat(Input.Instance.Float - Right.Float);
            }
            public static async Task<Instance> _Multiply(MethodInput Input) {
                Instance Right = Input.Arguments[0];
                return Input.Api.GetFloat(Input.Instance.Float * Right.Float);
            }
            public static async Task<Instance> _Divide(MethodInput Input) {
                Instance Right = Input.Arguments[0];
                return Input.Api.GetFloat(Input.Instance.Float / Right.Float);
            }
            public static async Task<Instance> _Modulo(MethodInput Input) {
                Instance Right = Input.Arguments[0];
                return Input.Api.GetFloat(Input.Instance.Float % Right.Float);
            }
            public static async Task<Instance> _Exponentiate(MethodInput Input) {
                Instance Right = Input.Arguments[0];
                return Input.Api.GetFloat(System.Math.Pow((double)Input.Instance.Float, (double)Right.Float));
            }
            public static async Task<Instance> _Equals(MethodInput Input) {
                Instance Left = Input.Instance;
                Instance Right = Input.Arguments[0];
                if ((Right is IntegerInstance or FloatInstance) && Left.Float == Right.Float) {
                    return Input.Api.True;
                }
                else {
                    return Input.Api.False;
                }
            }
            public static async Task<Instance> _LessThan(MethodInput Input) {
                Instance Left = Input.Instance;
                Instance Right = Input.Arguments[0];
                if ((Right is IntegerInstance or FloatInstance) && Left.Float < Right.Float) {
                    return Input.Api.True;
                }
                else {
                    return Input.Api.False;
                }
            }
            public static async Task<Instance> _GreaterThan(MethodInput Input) {
                Instance Left = Input.Instance;
                Instance Right = Input.Arguments[0];
                if ((Right is IntegerInstance or FloatInstance) && Left.Float > Right.Float) {
                    return Input.Api.True;
                }
                else {
                    return Input.Api.False;
                }
            }
            public static async Task<Instance> _LessThanOrEqualTo(MethodInput Input) {
                Instance Left = Input.Instance;
                Instance Right = Input.Arguments[0];
                if ((Right is IntegerInstance or FloatInstance) && Left.Float <= Right.Float) {
                    return Input.Api.True;
                }
                else {
                    return Input.Api.False;
                }
            }
            public static async Task<Instance> _GreaterThanOrEqualTo(MethodInput Input) {
                Instance Left = Input.Instance;
                Instance Right = Input.Arguments[0];
                if ((Right is IntegerInstance or FloatInstance) && Left.Float >= Right.Float) {
                    return Input.Api.True;
                }
                else {
                    return Input.Api.False;
                }
            }
            public static async Task<Instance> _Spaceship(MethodInput Input) {
                if ((await _LessThan(Input)).IsTruthy) {
                    return Input.Api.GetInteger(-1);
                }
                else if ((await _Equals(Input)).IsTruthy) {
                    return Input.Api.GetInteger(0);
                }
                else if ((await _GreaterThan(Input)).IsTruthy) {
                    return Input.Api.GetInteger(1);
                }
                return Input.Api.Nil;
            }
            public static async Task<Instance> _UnaryPlus(MethodInput Input) {
                return Input.Instance;
            }
            public static async Task<Instance> _UnaryMinus(MethodInput Input) {
                return Input.Api.GetFloat(-Input.Instance.Float);
            }
            public static async Task<Instance> to_i(MethodInput Input) {
                return Input.Api.GetInteger(Input.Instance.Integer);
            }
            public static async Task<Instance> to_f(MethodInput Input) {
                return Input.Instance;
            }
            public static async Task<Instance> clamp(MethodInput Input) {
                DynFloat Number = Input.Instance.Float;
                DynFloat Min = Input.Arguments[0].Float;
                DynFloat Max = Input.Arguments[1].Float;
                if (Number < Min) {
                    return Input.Api.GetFloat(Min);
                }
                else if (Number > Max) {
                    return Input.Api.GetFloat(Max);
                }
                else {
                    return Input.Instance;
                }
            }
            public static async Task<Instance> round(MethodInput Input) {
                // Get number and number of decimal places
                double Number = (double)Input.Instance.Float;
                int DecimalPlaces = 0;
                if (Input.Arguments.Count == 1)
                    DecimalPlaces = (int)System.Math.Min((long)Input.Arguments[0].Integer, 15);
                // Round
                double Result;
                if (DecimalPlaces >= 0) {
                    // Round decimal places
                    Result = System.Math.Round(Number, DecimalPlaces);
                }
                else {
                    // Round digits before dot
                    double Factor = System.Math.Pow(10, -DecimalPlaces);
                    Result = System.Math.Round(Number / Factor) * Factor;
                }
                long ResultAsLong = (long)Result;
                // Return result
                if (Result == ResultAsLong) {
                    return Input.Api.GetInteger(ResultAsLong);
                }
                else {
                    return Input.Api.GetFloat(Result);
                }
            }
            public static async Task<Instance> floor(MethodInput Input) {
                long Result = (long)System.Math.Floor((double)Input.Instance.Float);
                return Input.Api.GetInteger(Result);
            }
            public static async Task<Instance> ceil(MethodInput Input) {
                long Result = (long)System.Math.Ceiling((double)Input.Instance.Float);
                return Input.Api.GetInteger(Result);
            }
            public static async Task<Instance> truncate(MethodInput Input) {
                long Result = (long)System.Math.Truncate((double)Input.Instance.Float);
                return Input.Api.GetInteger(Result);
            }
        }
        static class _File {
            public static async Task<Instance> read(MethodInput Input) {
                string FilePath = Input.Arguments[0].String;
                try {
                    string FileContents = System.IO.File.ReadAllText(FilePath);
                    return new StringInstance(Input.Api.String, FileContents);
                }
                catch (FileNotFoundException) {
                    throw new RuntimeException($"{Input.Location}: No such file or directory: '{FilePath}'");
                }
                catch (Exception Ex) {
                    throw new RuntimeException($"{Input.Location}: Error reading file: '{Ex.Message}'");
                }
            }
            public static async Task<Instance> write(MethodInput Input) {
                string FilePath = Input.Arguments[0].String;
                string Text = Input.Arguments[1].String;
                try {
                    System.IO.File.WriteAllText(FilePath, Text);
                    return Input.Api.Nil;
                }
                catch (Exception Ex) {
                    throw new RuntimeException($"{Input.Location}: Error writing file: '{Ex.Message}'");
                }
            }
            public static async Task<Instance> append(MethodInput Input) {
                string FilePath = Input.Arguments[0].String;
                string Text = Input.Arguments[1].String;
                try {
                    System.IO.File.AppendAllText(FilePath, Text);
                    return Input.Api.Nil;
                }
                catch (Exception Ex) {
                    throw new RuntimeException($"{Input.Location}: Error appending file: '{Ex.Message}'");
                }
            }
            public static async Task<Instance> delete(MethodInput Input) {
                string FilePath = Input.Arguments[0].String;
                System.IO.File.Delete(FilePath);
                return Input.Api.Nil;
            }
            public static async Task<Instance> exist7(MethodInput Input) {
                string FilePath = Input.Arguments[0].String;
                bool Exists = System.IO.File.Exists(FilePath);
                return Exists ? Input.Api.True : Input.Api.False;
            }
            public static async Task<Instance> absolute_path(MethodInput Input) {
                string FilePath = Input.Arguments[0].String;
                string FullFilePath = Path.GetFullPath(FilePath);
                return new StringInstance(Input.Api.String, FullFilePath);
            }
            public static async Task<Instance> absolute_path7(MethodInput Input) {
                string FilePath = Input.Arguments[0].String;
                string FullFilePath = Path.GetFullPath(FilePath);
                return FilePath == FullFilePath ? Input.Api.True : Input.Api.False;
            }
            public static async Task<Instance> basename(MethodInput Input) {
                string FilePath = Input.Arguments[0].String;
                string FileName = Path.GetFileName(FilePath);
                return new StringInstance(Input.Api.String, FileName);
            }
            public static async Task<Instance> dirname(MethodInput Input) {
                string FilePath = Input.Arguments[0].String;
                string DirectoryName = Path.GetDirectoryName(FilePath) ?? "";
                return new StringInstance(Input.Api.String, DirectoryName);
            }
        }
        static class _Proc {
            public static async Task<Instance> call(MethodInput Input) {
                return await Input.Instance.Proc.Call(Input.Script, null, Input.Arguments, Input.OnYield);
            }
        }
        static class _Range {
            public static async Task<Instance> _TripleEquals(MethodInput Input) {
                IntegerRange Range = Input.Instance.Range;
                Instance Value = Input.Arguments[0];
                if (Value is IntegerInstance or FloatInstance) {
                    return Range.IsInRange(Value.Float) ? Input.Api.True : Input.Api.False;
                }
                else {
                    return Input.Api.False;
                }
            }
            public static async Task<Instance> min(MethodInput Input) {
                return ((RangeInstance)Input.Instance).AppliedMin;
            }
            public static async Task<Instance> max(MethodInput Input) {
                return ((RangeInstance)Input.Instance).AppliedMax;
            }
            public static async Task<Instance> each(MethodInput Input) {
                if (Input.OnYield != null) {
                    IntegerRange Range = Input.Instance.Range;
                    long Min = (long)(Range.Min != null ? Range.Min : 0);
                    long Max = (long)(Range.Max != null ? Range.Max : throw new RuntimeException($"{Input.Location}: Cannot call 'each' on range if max is endless"));
                    
                    bool TakesArgument = Input.OnYield.ArgumentNames.Count == 1;
                    for (long i = Min; i <= Max; i++) {
                        try {
                            // x.each do |n|
                            if (TakesArgument) {
                                await Input.OnYield.Call(Input.Script, null, Input.Api.GetInteger(i), BreakHandleType: BreakHandleType.Rethrow, CatchReturn: false);
                            }
                            // x.each do
                            else {
                                await Input.OnYield.Call(Input.Script, null, BreakHandleType: BreakHandleType.Rethrow, CatchReturn: false);
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
                            throw new SyntaxErrorException($"{Input.Location}: {Ex.GetType().Name} not valid in range.each");
                        }
                    }
                }
                return Input.Api.Nil;
            }
            public static async Task<Instance> reverse_each(MethodInput Input) {
                if (Input.OnYield != null) {
                    IntegerRange Range = Input.Instance.Range;
                    long Min = (long)(Range.Min != null ? Range.Min : 0);
                    long Max = (long)(Range.Max != null ? Range.Max : throw new RuntimeException($"{Input.Location}: Cannot call 'reverse_each' on range if max is endless"));
                    
                    bool TakesArgument = Input.OnYield.ArgumentNames.Count == 1;
                    for (long i = Max; i >= Min; i--) {
                        try {
                            // x.reverse_each do |n|
                            if (TakesArgument) {
                                await Input.OnYield.Call(Input.Script, null, Input.Api.GetInteger(i), BreakHandleType: BreakHandleType.Rethrow, CatchReturn: false);
                            }
                            // x.reverse_each do
                            else {
                                await Input.OnYield.Call(Input.Script, null, BreakHandleType: BreakHandleType.Rethrow, CatchReturn: false);
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
                            throw new SyntaxErrorException($"{Input.Location}: {Ex.GetType().Name} not valid in range.reverse_each");
                        }
                    }
                }
                return Input.Api.Nil;
            }
            public static async Task<Instance> to_a(MethodInput Input) {
                List<Instance> Array = new();
                IntegerRange Range = Input.Instance.Range;
                long Min = (long)(Range.Min != null ? Range.Min : 0);
                long Max = (long)(Range.Max != null ? Range.Max : throw new RuntimeException($"{Input.Location}: Cannot call 'to_a' on range if max is endless"));
                for (long i = Min; i <= Max; i++) {
                    Array.Add(Input.Api.GetInteger(i));
                }
                return new ArrayInstance(Input.Api.Array, Array);
            }
            public static async Task<Instance> length(MethodInput Input) {
                IntegerRange Range = Input.Instance.Range;
                long? Count = Range.Count;
                if (Count != null) {
                    return Input.Api.GetInteger(Count.Value);
                }
                else {
                    return Input.Api.Nil;
                }
            }
        }
        static class _Array {
            private static async Task<Instance> _GetIndex(MethodInput Input, int ArrayIndex) {
                Instance Index = Input.Api.GetInteger(ArrayIndex);
                return await Input.Instance.InstanceMethods["[]"].Call(Input.Script, Input.Instance, new Instances(Index));
            }
            private static int _RealisticIndex(MethodInput Input, DynInteger RawIndex) {
                if (RawIndex < int.MinValue || RawIndex > int.MaxValue) {
                    throw new RuntimeException($"{Input.Location}: Index ({RawIndex}) is too large for array.");
                }
                int Index = (int)RawIndex;
                return Index;
            }
            public static async Task<Instance> _Indexer(MethodInput Input) {
                // Get array and index
                List<Instance> Array = Input.Instance.Array;
                Instance Indexer = Input.Arguments[0];

                if (Indexer is RangeInstance RangeIndexer) {
                    // Return values in range
                    int StartIndex = RangeIndexer.Min != null ? _RealisticIndex(Input, RangeIndexer.Min.Integer) : 0;
                    int EndIndex = RangeIndexer.Max != null ? _RealisticIndex(Input, RangeIndexer.Max.Integer) : Array.Count - 1;
                    if (StartIndex < 0) StartIndex = 0;
                    if (EndIndex >= Array.Count) EndIndex = Array.Count - 1;

                    return new ArrayInstance(Input.Api.Array, Array.GetIndexRange(StartIndex, EndIndex));
                }
                else {
                    int Index = _RealisticIndex(Input, Indexer.Integer);

                    // Return value at array index or nil
                    if (Index >= 0 && Index < Array.Count) {
                        return Array[Index];
                    }
                    else if (Index < 0 && Index > -Array.Count) {
                        return Array[^-Index];
                    }
                    else {
                        return Input.Api.Nil;
                    }
                }
            }
            public static async Task<Instance> _IndexEquals(MethodInput Input) {
                // Get array, index and value
                List<Instance> Array = Input.Instance.Array;
                int Index = _RealisticIndex(Input, Input.Arguments[0].Integer);
                Instance Value = Input.Arguments[1];

                // Set value
                if (Index >= 0) {
                    lock (Array) {
                        Array.EnsureArrayIndex(Input.Api, Index);
                        return Array[Index] = Value;
                    }
                }
                else {
                    lock (Array) {
                        Array.EnsureArrayIndex(Input.Api, Index);
                        return Array[^-Index] = Value;
                    }
                }
            }
            public static async Task<Instance> _Multiply(MethodInput Input) {
                // Get array and repeat count
                List<Instance> Array = Input.Instance.Array;
                List<Instance> NewArray = new();
                int Repeat = _RealisticIndex(Input, Input.Arguments[0].Integer);
                int InitialCount = Array.Count;
                // Repeat the items in the array
                NewArray.EnsureCapacity(InitialCount * Repeat);
                for (int i = 0; i < Repeat; i++) {
                    for (int i2 = 0; i2 < InitialCount; i2++) {
                        NewArray.Add(Array[i2]);
                    }
                }
                // Return the new array
                return new ArrayInstance(Input.Api.Array, NewArray);
            }
            public static async Task<Instance> _Equals(MethodInput Input) {
                Instance Left = Input.Instance;
                Instance Right = Input.Arguments[0];
                if (Right is ArrayInstance) {
                    int Count = Left.Array.Count;
                    if (Count != Right.Array.Count) return Input.Api.False;

                    for (int i = 0; i < Left.Array.Count; i++) {
                        bool ValuesEqual = (await Left.Array[i].CallInstanceMethod(Input.Script, "==", Right.Array[i])).IsTruthy;
                        if (!ValuesEqual) return Input.Api.False;
                    }
                    return Input.Api.True;
                }
                else {
                    return Input.Api.False;
                }
            }
            public static async Task<Instance> _Append(MethodInput Input) {
                // Get array and index
                List<Instance> Array = Input.Instance.Array;
                Instance ItemToAppend = Input.Arguments[0];
                // Append item
                Array.Add(ItemToAppend);
                // Return array
                return Input.Instance;
            }
            public static async Task<Instance> length(MethodInput Input) {
                List<Instance> Items = Input.Instance.Array;
                return Input.Api.GetInteger(Items.Count);
            }
            public static async Task<Instance> count(MethodInput Input) {
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
                    return Input.Api.GetInteger(Count);
                }
            }
            public static async Task<Instance> first(MethodInput Input) {
                return await _GetIndex(Input, 0);
            }
            public static async Task<Instance> last(MethodInput Input) {
                return await _GetIndex(Input, -1);
            }
            public static async Task<Instance> forty_two(MethodInput Input) {
                return await _GetIndex(Input, 41);
            }
            public static async Task<Instance> sample(MethodInput Input) {
                List<Instance> Items = Input.Instance.Array;
                if (Items.Count != 0) {
                    return Items[Input.Interpreter.InternalRandom.Next(0, Items.Count)];
                }
                else {
                    return Input.Api.Nil;
                }
            }
            public static async Task<Instance> min(MethodInput Input) {
                List<Instance> Items = Input.Instance.Array;
                if (Items.Count != 0) {
                    Instance Minimum = Items[0];
                    for (int i = 1; i < Items.Count; i++) {
                        Instance Current = Items[i];
                        if ((await Current.CallInstanceMethod(Input.Script, "<", Minimum)).IsTruthy) {
                            Minimum = Current;
                        }
                    }
                    return Minimum;
                }
                else {
                    return Input.Api.Nil;
                }
            }
            public static async Task<Instance> max(MethodInput Input) {
                List<Instance> Items = Input.Instance.Array;
                if (Items.Count != 0) {
                    Instance Maximum = Items[0];
                    for (int i = 1; i < Items.Count; i++) {
                        Instance Current = Items[i];
                        if ((await Current.CallInstanceMethod(Input.Script, ">", Maximum)).IsTruthy) {
                            Maximum = Current;
                        }
                    }
                    return Maximum;
                }
                else {
                    return Input.Api.Nil;
                }
            }
            public static async Task<Instance> insert(MethodInput Input) {
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
            public static async Task<Instance> each(MethodInput Input) {
                if (Input.OnYield != null) {
                    List<Instance> Array = Input.Instance.Array;
                    
                    int TakesArguments = Input.OnYield.ArgumentNames.Count;
                    for (int i = 0; i < Array.Count; i++) {
                        try {
                            // x.each do |n, i|
                            if (TakesArguments == 2) {
                                await Input.OnYield.Call(Input.Script, null, new List<Instance>() { Array[i], Input.Api.GetInteger(i) }, BreakHandleType: BreakHandleType.Rethrow, CatchReturn: false);
                            }
                            // x.each do |n|
                            else if (TakesArguments == 1) {
                                await Input.OnYield.Call(Input.Script, null, Array[i], BreakHandleType: BreakHandleType.Rethrow, CatchReturn: false);
                            }
                            // x.each do
                            else {
                                await Input.OnYield.Call(Input.Script, null, BreakHandleType: BreakHandleType.Rethrow, CatchReturn: false);
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
                            throw new SyntaxErrorException($"{Input.Location}: {Ex.GetType().Name} not valid in array.each");
                        }
                    }
                }
                return Input.Api.Nil;
            }
            public static async Task<Instance> reverse_each(MethodInput Input) {
                if (Input.OnYield != null) {
                    List<Instance> Array = Input.Instance.Array;
                    
                    int TakesArguments = Input.OnYield.ArgumentNames.Count;
                    for (int i = Array.Count - 1; i >= 0; i--) {
                        try {
                            // x.reverse_each do |n, i|
                            if (TakesArguments == 2) {
                                await Input.OnYield.Call(Input.Script, null, new List<Instance>() { Array[i], Input.Api.GetInteger(i) }, BreakHandleType: BreakHandleType.Rethrow, CatchReturn: false);
                            }
                            // x.reverse_each do |n|
                            else if (TakesArguments == 1) {
                                await Input.OnYield.Call(Input.Script, null, Array[i], BreakHandleType: BreakHandleType.Rethrow, CatchReturn: false);
                            }
                            // x.reverse_each do
                            else {
                                await Input.OnYield.Call(Input.Script, null, BreakHandleType: BreakHandleType.Rethrow, CatchReturn: false);
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
                            throw new SyntaxErrorException($"{Input.Location}: {Ex.GetType().Name} not valid in array.reverse_each");
                        }
                    }
                }
                return Input.Api.Nil;
            }
            private static async Task<Instance> _map(MethodInput Input, bool Exclaim) {
                if (Input.OnYield != null) {
                    List<Instance> Array = Input.Instance.Array;
                    List<Instance> MappedArray = new();
                    for (int i = 0; i < Array.Count; i++) {
                        Instance Item = Array[i];
                        Instance MappedItem = await Input.OnYield.Call(Input.Script, null, Item, CatchReturn: false);
                        MappedArray.Add(MappedItem);
                    }
                    if (Exclaim) {
                        ((ArrayInstance)Input.Instance).SetValue(MappedArray);
                        return Input.Instance;
                    }
                    else {
                        return new ArrayInstance(Input.Api.Array, MappedArray);
                    }
                }
                return Input.Instance;
            }
            public static async Task<Instance> map(MethodInput Input) => await _map(Input, false);
            public static async Task<Instance> map1(MethodInput Input) => await _map(Input, true);
            private static async Task<Instance> _sort(MethodInput Input, bool Exclaim) {
                // Get sort methods
                List<Instance> Array = Input.Instance.Array;
                Func<Instance, Instance, Task<bool>> SortFunction;
                if (Input.OnYield != null) {
                    SortFunction = async (A, B) => {
                        return (await Input.OnYield.Call(Input.Script, null, new Instances(A, B), CatchReturn: false)).Integer < 0;
                    };
                }
                else {
                    SortFunction = async (A, B) => {
                        return (await A.CallInstanceMethod(Input.Script, "<=>", B)).Integer < 0;
                    };
                }
                // Sort array
                List<Instance> SortedArray = new(Array);
                for (int i = 0; i < SortedArray.Count; i++) {
                    for (int i2 = i + 1; i2 < SortedArray.Count; i2++) {
                        if (!await SortFunction(SortedArray[i], SortedArray[i2])) {
                            // Swap elements if they are out of order
                            (SortedArray[i2], SortedArray[i]) = (SortedArray[i], SortedArray[i2]);
                        }
                    }
                }
                // Return sorted array
                if (Exclaim) {
                    ((ArrayInstance)Input.Instance).SetValue(SortedArray);
                    return Input.Instance;
                }
                else {
                    return new ArrayInstance(Input.Api.Array, Array);
                }
            }
            public static async Task<Instance> sort(MethodInput Input) => await _sort(Input, false);
            public static async Task<Instance> sort1(MethodInput Input) => await _sort(Input, true);
            public static async Task<Instance> include7(MethodInput Input) {
                Instance ItemToFind = Input.Arguments[0];
                foreach (Instance Item in Input.Instance.Array) {
                    if ((await Item.InstanceMethods["=="].Call(Input.Script, Item, ItemToFind)).IsTruthy) {
                        return Input.Api.True;
                    }
                }
                return Input.Api.False;
            }
            public static async Task<Instance> delete(MethodInput Input) {
                List<Instance> Array = Input.Instance.Array;
                Instance DeleteItem = Input.Arguments[0];
                Instance LastDeletedItem = Input.Api.Nil;
                for (int i = 0; i < Array.Count; i++) {
                    Instance Item = Array[i];
                    if ((await Item.CallInstanceMethod(Input.Script, "==", DeleteItem)).IsTruthy) {
                        Array.RemoveAt(i);
                        LastDeletedItem = Item;
                    }
                }
                return LastDeletedItem;
            }
            public static async Task<Instance> delete_at(MethodInput Input) {
                List<Instance> Array = Input.Instance.Array;
                int DeleteIndex = _RealisticIndex(Input, Input.Arguments[0].Integer);
                if (DeleteIndex >= 0 && DeleteIndex < Array.Count) {
                    Instance RemovedItem = Array[DeleteIndex];
                    Array.RemoveAt(DeleteIndex);
                    return RemovedItem;
                }
                else if (DeleteIndex < 0 && DeleteIndex > -Array.Count) {
                    Instance RemovedItem = Array[^-DeleteIndex];
                    Array.RemoveAt(Array.Count - DeleteIndex);
                    return RemovedItem;
                }
                else {
                    return Input.Api.Nil;
                }
            }
            public static async Task<Instance> clear(MethodInput Input) {
                Input.Instance.Array.Clear();
                return Input.Api.Nil;
            }
            public static async Task<Instance> empty7(MethodInput Input) {
                return Input.Instance.Array.Count == 0 ? Input.Api.True : Input.Api.False;
            }
            public static async Task<Instance> reverse(MethodInput Input) {
                List<Instance> ReversedArray = new(Input.Instance.Array);
                ReversedArray.Reverse();
                return new ArrayInstance(Input.Api.Array, ReversedArray);
            }
            public static async Task<Instance> reverse1(MethodInput Input) {
                Input.Instance.Array.Reverse();
                return Input.Instance;
            }
        }
        static class _Hash {
            public static async Task<Instance> _Indexer(MethodInput Input) {
                // Get hash and key
                HashDictionary Hash = Input.Instance.Hash;
                Instance Key = Input.Arguments[0];

                // Return value at hash index or default value
                Instance? Value = await Hash.Lookup(Input.Script, Key);
                if (Value != null) {
                    return Value;
                }
                else {
                    return ((HashInstance)Input.Instance).DefaultValue;
                }
            }
            public static async Task<Instance> _IndexEquals(MethodInput Input) {
                // Get hash, key and value
                HashDictionary Hash = Input.Instance.Hash;
                Instance Key = Input.Arguments[0];
                Instance Value = Input.Arguments[1];

                // Store value
                await Hash.Store(Input.Script, Key, Value);
                return Value;
            }
            public static async Task<Instance> _Equals(MethodInput Input) {
                Instance Left = Input.Instance;
                Instance Right = Input.Arguments[0];
                if (Right is HashInstance) {
                    foreach (KeyValuePair<Instance, Instance> LeftKeyValue in Left.Hash.KeyValues) {
                        foreach (KeyValuePair<Instance, Instance> RightKeyValue in Right.Hash.KeyValues) {
                            bool KeysEqual = (await LeftKeyValue.Key.CallInstanceMethod(Input.Script, "==", RightKeyValue.Key)).IsTruthy;
                            if (!KeysEqual) return Input.Api.False;
                            bool ValuesEqual = (await LeftKeyValue.Value.CallInstanceMethod(Input.Script, "==", RightKeyValue.Value)).IsTruthy;
                            if (!ValuesEqual) return Input.Api.False;
                        }
                    }
                    return Input.Api.True;
                }
                else {
                    return Input.Api.False;
                }
            }
            public static async Task<Instance> initialize(MethodInput Input) {
                if (Input.Arguments.Count == 1) {
                    ((HashInstance)Input.Instance).SetValue(Input.Instance.Hash, Input.Arguments[0]);
                }
                return Input.Api.Nil;
            }
            public static async Task<Instance> length(MethodInput Input) {
                HashDictionary Items = Input.Instance.Hash;
                return Input.Api.GetInteger(Items.Count);
            }
            public static async Task<Instance> has_key7(MethodInput Input) {
                Instance ItemToFind = Input.Arguments[0];
                Instance? Found = await Input.Instance.Hash.Lookup(Input.Script, ItemToFind);
                return Found != null ? Input.Api.True : Input.Api.False;
            }
            public static async Task<Instance> has_value7(MethodInput Input) {
                Instance ItemToFind = Input.Arguments[0];
                Instance? Found = await Input.Instance.Hash.ReverseLookup(Input.Script, ItemToFind);
                return Found != null ? Input.Api.True : Input.Api.False;
            }
            public static async Task<Instance> keys(MethodInput Input) {
                return new ArrayInstance(Input.Api.Array, Input.Instance.Hash.Keys);
            }
            public static async Task<Instance> values(MethodInput Input) {
                return new ArrayInstance(Input.Api.Array, Input.Instance.Hash.Values);
            }
            public static async Task<Instance> delete(MethodInput Input) {
                HashDictionary Hash = Input.Instance.Hash;
                Instance Key = Input.Arguments[0];
                return await Hash.Remove(Input.Script, Key) ?? Input.Api.Nil;
            }
            public static async Task<Instance> clear(MethodInput Input) {
                Input.Instance.Hash.Clear();
                return Input.Api.Nil;
            }
            public static async Task<Instance> each(MethodInput Input) {
                if (Input.OnYield != null) {
                    HashDictionary Hash = Input.Instance.Hash;
                    
                    int TakesArguments = Input.OnYield.ArgumentNames.Count;
                    foreach (KeyValuePair<Instance, Instance> Match in Hash.KeyValues) {
                        Redo:
                        try {
                            // x.each do |key, value|
                            if (TakesArguments == 2) {
                                await Input.OnYield.Call(Input.Script, null, new List<Instance>() {Match.Key, Match.Value}, BreakHandleType: BreakHandleType.Rethrow, CatchReturn: false);
                            }
                            // x.each do |key|
                            else if (TakesArguments == 1) {
                                await Input.OnYield.Call(Input.Script, null, Match.Key, BreakHandleType: BreakHandleType.Rethrow, CatchReturn: false);
                            }
                            // x.each do
                            else {
                                await Input.OnYield.Call(Input.Script, null, BreakHandleType: BreakHandleType.Rethrow, CatchReturn: false);
                            }
                        }
                        catch (BreakException) {
                            break;
                        }
                        catch (RedoException) {
                            goto Redo;
                        }
                        catch (NextException) {
                            continue;
                        }
                        catch (LoopControlException Ex) {
                            throw new SyntaxErrorException($"{Input.Location}: {Ex.GetType().Name} not valid in hash.each");
                        }
                    }
                }
                return Input.Api.Nil;
            }
            public static async Task<Instance> invert(MethodInput Input) {
                HashInstance Hash = (HashInstance)Input.Instance;
                HashDictionary Inverted = new();
                foreach (KeyValuePair<Instance, Instance> Match in Hash.Hash.KeyValues) {
                    await Inverted.Store(Input.Script, Match.Value, Match.Key);
                }
                return new HashInstance(Input.Api.Hash, Inverted, Hash.DefaultValue);
            }
            public static async Task<Instance> to_a(MethodInput Input) {
                List<Instance> Array = new();
                foreach (KeyValuePair<Instance, Instance> Item in Input.Instance.Hash.KeyValues) {
                    Array.Add(new ArrayInstance(Input.Api.Array, new List<Instance>() { Item.Key, Item.Value }));
                }
                return new ArrayInstance(Input.Api.Array, Array);
            }
            public static async Task<Instance> to_hash(MethodInput Input) {
                return Input.Instance;
            }
            public static async Task<Instance> empty7(MethodInput Input) {
                return Input.Instance.Hash.Count == 0 ? Input.Api.True : Input.Api.False;
            }
        }
        static class _Random {
            public static async Task<Instance> rand(MethodInput Input) {
                // Integer random
                if (Input.Arguments.Count == 1 && Input.Arguments[0] is IntegerInstance Integer) {
                    long IncludingMin = 0;
                    long ExcludingMax = (long)Integer.Integer;
                    long RandomNumber = Input.Interpreter.Random.NextInt64(IncludingMin, ExcludingMax);
                    return Input.Api.GetInteger(RandomNumber);
                }
                // Range random
                else if (Input.Arguments.Count == 1 && Input.Arguments[0] is RangeInstance Range) {
                    long IncludingMin = (long)Range.AppliedMin.Integer;
                    long IncludingMax = (long)Range.AppliedMax.Integer;
                    long RandomNumber = Input.Interpreter.Random.NextInt64(IncludingMin, IncludingMax + 1);
                    return Input.Api.GetInteger(RandomNumber);
                }
                // Float random
                else {
                    const double IncludingMin = 0;
                    double ExcludingMax;
                    if (Input.Arguments.Count == 0) {
                        ExcludingMax = 1;
                    }
                    else {
                        ExcludingMax = (double)Input.Arguments[0].Float;
                    }
                    double RandomNumber = Input.Interpreter.Random.NextDouble() * (ExcludingMax - IncludingMin) + IncludingMin;
                    return Input.Api.GetFloat(RandomNumber);
                }
            }
            public static async Task<Instance> srand(MethodInput Input) {
                long PreviousSeed = Input.Interpreter.RandomSeed;
                long NewSeed;
                if (Input.Arguments.Count == 1) {
                    NewSeed = (long)Input.Arguments[0].Integer;
                }
                else {
                    NewSeed = Input.Interpreter.InternalRandom.NextInt64();
                }

                Input.Interpreter.RandomSeed = NewSeed;
                Input.Interpreter.Random = new Random(NewSeed.GetHashCode());

                return Input.Api.GetInteger(PreviousSeed);
            }
        }
        static class _Math {
            public static async Task<Instance> sin(MethodInput Input) {
                return Input.Api.GetFloat(System.Math.Sin((double)Input.Arguments[0].Float));
            }
            public static async Task<Instance> cos(MethodInput Input) {
                return Input.Api.GetFloat(System.Math.Cos((double)Input.Arguments[0].Float));
            }
            public static async Task<Instance> tan(MethodInput Input) {
                return Input.Api.GetFloat(System.Math.Tan((double)Input.Arguments[0].Float));
            }
            public static async Task<Instance> asin(MethodInput Input) {
                return Input.Api.GetFloat(System.Math.Asin((double)Input.Arguments[0].Float));
            }
            public static async Task<Instance> acos(MethodInput Input) {
                return Input.Api.GetFloat(System.Math.Acos((double)Input.Arguments[0].Float));
            }
            public static async Task<Instance> atan(MethodInput Input) {
                return Input.Api.GetFloat(System.Math.Atan((double)Input.Arguments[0].Float));
            }
            public static async Task<Instance> atan2(MethodInput Input) {
                return Input.Api.GetFloat(System.Math.Atan2((double)Input.Arguments[0].Float, (double)Input.Arguments[1].Float));
            }
            public static async Task<Instance> sinh(MethodInput Input) {
                return Input.Api.GetFloat(System.Math.Sinh((double)Input.Arguments[0].Float));
            }
            public static async Task<Instance> cosh(MethodInput Input) {
                return Input.Api.GetFloat(System.Math.Cosh((double)Input.Arguments[0].Float));
            }
            public static async Task<Instance> tanh(MethodInput Input) {
                return Input.Api.GetFloat(System.Math.Tanh((double)Input.Arguments[0].Float));
            }
            public static async Task<Instance> asinh(MethodInput Input) {
                return Input.Api.GetFloat(System.Math.Asinh((double)Input.Arguments[0].Float));
            }
            public static async Task<Instance> acosh(MethodInput Input) {
                return Input.Api.GetFloat(System.Math.Acosh((double)Input.Arguments[0].Float));
            }
            public static async Task<Instance> atanh(MethodInput Input) {
                return Input.Api.GetFloat(System.Math.Atanh((double)Input.Arguments[0].Float));
            }
            public static async Task<Instance> exp(MethodInput Input) {
                return Input.Api.GetFloat(System.Math.Exp((double)Input.Arguments[0].Float));
            }
            public static async Task<Instance> log(MethodInput Input) {
                return Input.Api.GetFloat(System.Math.Log((double)Input.Arguments[0].Float, (double)Input.Arguments[1].Float));
            }
            public static async Task<Instance> log10(MethodInput Input) {
                return Input.Api.GetFloat(System.Math.Log10((double)Input.Arguments[0].Float));
            }
            public static async Task<Instance> log2(MethodInput Input) {
                return Input.Api.GetFloat(System.Math.Log((double)Input.Arguments[0].Float, 2));
            }
            public static async Task<Instance> frexp(MethodInput Input) {
                double Value = (double)Input.Arguments[0].Float;

                // Calculate fractional exponent
                // From https://stackoverflow.com/a/390072
                long Bits = BitConverter.DoubleToInt64Bits(Value);
                bool Negative = Bits < 0;
                int Exponent = (int)((Bits >> 52) & 0x7ffL);
                long Mantissa = Bits & 0xfffffffffffffL;
                if (Exponent == 0) Exponent++;
                else Mantissa |= 1L << 52;
                if (Mantissa == 0)
                    return new ArrayInstance(Input.Api.Array, new List<Instance>() {
                        Input.Api.GetFloat(0),
                        Input.Api.GetInteger(0)
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
                return new ArrayInstance(Input.Api.Array, new List<Instance>() {
                    Input.Api.GetFloat(M),
                    Input.Api.GetInteger(E)
                });
            }
            public static async Task<Instance> ldexp(MethodInput Input) {
                double Fraction = (double)Input.Arguments[0].Float;
                long Exponent = (long)Input.Arguments[1].Integer;
                return Input.Api.GetFloat(Fraction * System.Math.Pow(2, Exponent));
            }
            public static async Task<Instance> sqrt(MethodInput Input) {
                return Input.Api.GetFloat(System.Math.Sqrt((double)Input.Arguments[0].Float));
            }
            public static async Task<Instance> cbrt(MethodInput Input) {
                return Input.Api.GetFloat(System.Math.Cbrt((double)Input.Arguments[0].Float));
            }
            public static async Task<Instance> hypot(MethodInput Input) {
                double A = (double)Input.Arguments[0].Float;
                double B = (double)Input.Arguments[1].Float;
                return Input.Api.GetFloat(System.Math.Sqrt(System.Math.Pow(A, 2) + System.Math.Pow(B, 2)));
            }
            private static double _Erf(double x) {
                // Approximate error function
                // From https://www.johndcook.com/blog/csharp_erf

                // constants
                const double a1 = 0.254829592;
                const double a2 = -0.284496736;
                const double a3 = 1.421413741;
                const double a4 = -1.453152027;
                const double a5 = 1.061405429;
                const double p = 0.3275911;

                // Save the sign of x
                int sign = x >= 0 ? 1 : -1;
                x = System.Math.Abs(x);

                // A&S formula 7.1.26
                double t = 1.0 / (1.0 + p * x);
                double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * System.Math.Exp(-x * x);

                return sign * y;
            }
            public static async Task<Instance> erf(MethodInput Input) {
                return Input.Api.GetFloat(_Erf((double)Input.Arguments[0].Float));
            }
            public static async Task<Instance> erfc(MethodInput Input) {
                return Input.Api.GetFloat(1.0 - _Erf((double)Input.Arguments[0].Float));
            }
            private static double _Gamma(double Z) {
                // Approximate gamma
                // From https://stackoverflow.com/a/66193379
                const int G = 7;
                double[] P = { 0.99999999999980993, 676.5203681218851, -1259.1392167224028, 771.32342877765313, -176.61502916214059, 12.507343278686905, -0.13857109526572012, 9.9843695780195716e-6, 1.5056327351493116e-7 };
                if (Z < 0.5)
                    return System.Math.PI / (System.Math.Sin(System.Math.PI * Z) * _Gamma(1 - Z));
                Z -= 1;
                double X = P[0];
                for (var i = 1; i < G + 2; i++)
                    X += P[i] / (Z + i);
                double T = Z + G + 0.5;
                return System.Math.Sqrt(2 * System.Math.PI) * (System.Math.Pow(T, Z + 0.5)) * System.Math.Exp(-T) * X;
            }
            public static async Task<Instance> gamma(MethodInput Input) {
                return Input.Api.GetFloat(_Gamma((double)Input.Arguments[0].Float));
            }
            public static async Task<Instance> lgamma(MethodInput Input) {
                double Value = (double)Input.Arguments[0].Float;
                double GammaValue = _Gamma(Value);
                double A = System.Math.Log(System.Math.Abs(GammaValue));
                long B = GammaValue < 0 ? -1 : 1;
                return new ArrayInstance(Input.Api.Array, new List<Instance>() {
                    Input.Api.GetFloat(A),
                    Input.Api.GetInteger(B)
                });
            }
            public static async Task<Instance> to_rad(MethodInput Input) {
                double Degrees = (double)Input.Arguments[0].Float;
                return Input.Api.GetFloat(Degrees * (System.Math.PI / 180));
            }
            public static async Task<Instance> to_deg(MethodInput Input) {
                double Radians = (double)Input.Arguments[0].Float;
                return Input.Api.GetFloat(Radians / (System.Math.PI / 180));
            }
            public static async Task<Instance> lerp(MethodInput Input) {
                DynFloat A = Input.Arguments[0].Float;
                DynFloat B = Input.Arguments[1].Float;
                DynFloat T = Input.Arguments[2].Float;
                return Input.Api.GetFloat(A * (1 - T) + (B * T));
            }
        }
        static class _Exception {
            public static async Task<Instance> initialize(MethodInput Input) {
                if (Input.Arguments.Count == 1) {
                    ((ExceptionInstance)Input.Instance).SetValue(Input.Arguments[0].String);
                }
                return Input.Api.Nil;
            }
            public static async Task<Instance> message(MethodInput Input) {
                return new StringInstance(Input.Api.String, Input.Instance.Exception.Message);
            }
            public static async Task<Instance> backtrace(MethodInput Input) {
                static string SimplifyStackTrace(string StackTrace) {
                    const string Pattern = @"[A-Za-z]:[\\\/]\S+"; // Match file paths
                    return System.Text.RegularExpressions.Regex.Replace(StackTrace, Pattern, FilePath => Path.GetFileName(FilePath.Value)); // Shorten file paths
                }
                return new StringInstance(Input.Api.String, SimplifyStackTrace(Input.Instance.Exception.StackTrace ?? ""));
            }
        }
        static class _Thread {
            public static async Task<Instance> initialize(MethodInput Input) {
                Method? OnYield = Input.OnYield ?? throw new RuntimeException($"{Input.Location}: No block given for Thread.new");
                
                ThreadInstance Thread = (ThreadInstance)Input.Instance;
                Thread.SetMethod(OnYield);
                _ = Thread.Thread.Run(Input.Arguments, Input.OnYield);

                return Input.Api.Nil;
            }
            public static async Task<Instance> join(MethodInput Input) {
                ThreadInstance Thread = (ThreadInstance)Input.Instance;
                await Thread.Thread.Run(OnYield: Input.OnYield);
                return Input.Api.Nil;
            }
            public static async Task<Instance> stop(MethodInput Input) {
                ThreadInstance Thread = (ThreadInstance)Input.Instance;
                Thread.Thread.Stop();
                return Input.Api.Nil;
            }
        }
        static class _Parallel {
            public static async Task<Instance> each(MethodInput Input) {
                if (Input.OnYield != null) {
                    Instance[] Array = Input.Arguments[0].Array.ToArray();
                    Action[] Methods = new Action[Array.Length];
                    
                    int TakesArguments = Input.OnYield.ArgumentNames.Count;
                    for (int i = 0; i < Array.Length; i++) {
                        Instance Current = Array[i];
                        int CurrentIndex = i;

                        Methods[i] = async () => {
                            ThreadInstance Thread = new(Input.Api.Thread, Input.Script);
                            Thread.Thread.Method = Input.OnYield;

                            // Parallel.each do |n, i|
                            if (TakesArguments == 2) {
                                await Thread.Thread.Run(new List<Instance>() { Current, Input.Api.GetInteger(CurrentIndex) }, Input.OnYield);
                            }
                            // Parallel.each do |n|
                            else if (TakesArguments == 1) {
                                await Thread.Thread.Run(Current, Input.OnYield);
                            }
                            // Parallel.each do
                            else {
                                await Thread.Thread.Run(OnYield: Input.OnYield);
                            }
                        };
                    }

                    System.Threading.Tasks.Parallel.Invoke(Methods);
                }
                return Input.Api.Nil;
            }
            public static async Task<Instance> times(MethodInput Input) {
                if (Input.OnYield != null) {
                    Instance Argument = Input.Arguments[0];
                    IntegerRange Times = Argument is RangeInstance ? Argument.Range : new IntegerRange(0, Argument.Integer);
                    int TimesCount = (int)(Times.Count ?? throw new Exception($"{Input.Location}: Expected finite range for Parallel.times(range)"));
                    Action[] Methods = new Action[TimesCount];

                    int TakesArguments = Input.OnYield.ArgumentNames.Count;
                    int Counter = 0;
                    for (DynInteger i = Times.Min!.Value; i <= Times.Max!.Value; i++) {
                        DynInteger CurrentIndex = i;

                        Methods[Counter] = async () => {
                            ThreadInstance Thread = new(Input.Api.Thread, Input.Script);
                            Thread.Thread.Method = Input.OnYield;

                            // Parallel.times do |n|
                            if (TakesArguments == 1) {
                                await Thread.Thread.Run(Input.Api.GetInteger(CurrentIndex), Input.OnYield);
                            }
                            // Parallel.times do
                            else {
                                await Thread.Thread.Run(OnYield: Input.OnYield);
                            }
                        };
                        Counter++;
                    }

                    System.Threading.Tasks.Parallel.Invoke(Methods);
                }
                return Input.Api.Nil;
            }
        }
        static class _Time {
            public static async Task<Instance> now(MethodInput Input) {
                return new TimeInstance(Input.Api.Time, DateTime.Now);
            }
            public static async Task<Instance> initialize(MethodInput Input) {
                int ArgsCount = Input.Arguments.Count;

                int Year = ArgsCount >= 1 ? (int)Input.Arguments[0].Integer : DateTime.Now.Year;
                int Month = ArgsCount >= 2 ? (int)Input.Arguments[1].Integer : ArgsCount == 0 ? DateTime.Now.Month : 0;
                double Day = ArgsCount >= 3 ? (double)Input.Arguments[2].Float : ArgsCount == 0 ? DateTime.Now.Day : 0;
                double Hour = ArgsCount >= 4 ? (double)Input.Arguments[3].Float : ArgsCount == 0 ? DateTime.Now.Hour : 0;
                double Minute = ArgsCount >= 5 ? (double)Input.Arguments[4].Float : ArgsCount == 0 ? DateTime.Now.Minute : 0;
                double Second = ArgsCount >= 6 ? (double)Input.Arguments[5].Float : ArgsCount == 0 ? DateTime.Now.Second : 0;
                TimeSpan UtcOffset = ArgsCount >= 7 ? TimeSpan.FromHours((double)Input.Arguments[6].Float) : DateTimeOffset.Now.Offset;

                DateTimeOffset Time = new(Year, Month, (int)Day, (int)Hour, (int)Minute, (int)Second, UtcOffset);
                Time = Time
                    .AddDays(Day - Time.Day)
                    .AddHours(Hour - Time.Hour)
                    .AddMinutes(Minute - Time.Minute)
                    .AddSeconds(Second - Time.Second);
                ((TimeInstance)Input.Instance).SetValue(Time);

                return Input.Api.Nil;
            }
            public static async Task<Instance> to_i(MethodInput Input) {
                return Input.Api.GetInteger(Input.Instance.Time.ToUnixTimeSeconds());
            }
            public static async Task<Instance> to_f(MethodInput Input) {
                return Input.Api.GetFloat(Input.Instance.Time.ToUnixTimeSecondsDouble());
            }
            public static async Task<Instance> at(MethodInput Input) {
                double Seconds = (double)Input.Arguments[0].Float;
                long TruncatedSeconds = (long)Seconds;

                DateTimeOffset DateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(TruncatedSeconds);
                TimeSpan TimeZoneOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow);
                DateTimeOffset = DateTimeOffset.ToOffset(TimeZoneOffset);
                DateTime Time = DateTimeOffset.DateTime;
                Time = Time.AddSeconds(Seconds - TruncatedSeconds);

                return new TimeInstance(Input.Api.Time, Time);
            }
        }
        static class _WeakRef {
            public static async Task<Instance> initialize(MethodInput Input) {
                ((WeakRefInstance)Input.Instance).SetValue(new WeakReference<Instance>(Input.Arguments[0]));
                return Input.Api.Nil;
            }
            public static async Task<Instance> method_missing(MethodInput Input) {
                WeakRefInstance WeakRef = (WeakRefInstance)Input.Instance;
                string MethodName = Input.Arguments[0].String;
                Instances Arguments = Input.Arguments.MultiInstance.GetIndexRange(1);
                if (WeakRef.WeakRef.TryGetTarget(out Instance? Target)) {
                    return await Target.CallInstanceMethod(Input.Script, MethodName, Arguments, Input.OnYield);
                }
                else {
                    throw new RuntimeException($"{Input.Location}: Cannot call method on WeakRef because it is dead");
                }
            }
            public static async Task<Instance> weakref_alive7(MethodInput Input) {
                WeakRefInstance WeakRef = (WeakRefInstance)Input.Instance;
                return WeakRef.WeakRef.TryGetTarget(out _) ? Input.Api.True : Input.Api.False;
            }
        }
        static class _Net {
            public static class _HTTP {
                public static async Task<Instance> get(MethodInput Input) {
                    Uri Uri = new(Input.Arguments[0].String, UriKind.RelativeOrAbsolute);
                    if (!Uri.IsAbsoluteUri) Uri = new("https://" + Uri.OriginalString);

                    HttpClient Client = new();
                    HttpResponseMessage Response = await Client.GetAsync(Uri);

                    return new HttpResponseInstance(Input.Api.HTTPResponse, Response);
                }
                public static class _HTTPResponse {
                    public static async Task<Instance> body(MethodInput Input) {
                        return new StringInstance(Input.Api.String, await Input.Instance.HttpResponse.Content.ReadAsStringAsync());
                    }
                    public static async Task<Instance> code(MethodInput Input) {
                        return Input.Api.GetInteger((int)Input.Instance.HttpResponse.StatusCode);
                    }
                }
            }
        }
        static class _Class {
            public static async Task<Instance> _TripleEquals(MethodInput Input) {
                Instance Left = Input.Instance;
                Instance Right = Input.Arguments[0];
                return Right.Module!.InheritsFrom(Left.Module) ? Input.Api.True : Input.Api.False;
            }
            public static async Task<Instance> name(MethodInput Input) {
                return new StringInstance(Input.Api.String, Input.Instance.Module!.Name);
            }
        }

        //
        // INSTANCES
        //

        public class NilInstance : Instance {
            public override string Inspect() {
                return "nil";
            }
            public override string LightInspect() {
                return "";
            }
            public NilInstance(Class fromClass) : base(fromClass) { }
        }
        public class TrueInstance : Instance {
            public override object? Object { get { return true; } }
            public override bool Boolean { get { return true; } }
            public override string Inspect() {
                return "true";
            }
            public TrueInstance(Class fromClass) : base(fromClass) { }
        }
        public class FalseInstance : Instance {
            public override object? Object { get { return false; } }
            public override bool Boolean { get { return false; } }
            public override string Inspect() {
                return "false";
            }
            public FalseInstance(Class fromClass) : base(fromClass) { }
        }
        public class StringInstance : Instance {
            string Value;
            public override object? Object { get { return Value; } }
            public override string String { get { return Value; } }
            public override string Inspect() {
                return '"' + Value.Replace("\n", "\\n").Replace("\r", "\\r") + '"';
            }
            public override string LightInspect() {
                return Value;
            }
            public StringInstance(Class fromClass, string value) : base(fromClass) {
                Value = value;
            }
            public void SetValue(string value) {
                Value = value;
            }
            public override int GetHashCode() {
                return Value.GetHashCode();
            }
        }
        public class SymbolInstance : Instance {
            readonly string Value;
            readonly bool IsStringSymbol;
            public override object? Object { get { return Value; } }
            public override string String { get { return Value; } }
            public override string Inspect() {
                if (IsStringSymbol) {
                    return ":\"" + Value.Replace("\n", "\\n").Replace("\r", "\\r") + "\"";
                }
                else {
                    return ":" + Value;
                }
            }
            public override string LightInspect() {
                return Value;
            }
            public SymbolInstance(Class fromClass, string value) : base(fromClass) {
                Value = value;
                IsStringSymbol = Value.Any("(){}[]<>=+-*/%.,;@#&|~^$".Contains) || Value.Any(char.IsWhiteSpace) || (Value.Length != 0 && Value[0].IsAsciiDigit()) || Value[..^1].Any("?!".Contains);
            }
        }
        public class IntegerInstance : Instance {
            readonly DynInteger Value;
            public override object? Object { get { return Value; } }
            public override DynInteger Integer { get { return Value; } }
            public override DynFloat Float { get { return Value; } }
            public override string Inspect() {
                return Value.ToString();
            }
            public IntegerInstance(Class fromClass, DynInteger value) : base(fromClass) {
                Value = value;
            }
        }
        public class FloatInstance : Instance {
            readonly DynFloat Value;
            public override object? Object { get { return Value; } }
            public override DynFloat Float { get { return Value; } }
            public override DynInteger Integer { get { return (DynInteger)Value; } }
            public override string Inspect() {
                if (Value.IsDouble) {
                    if (double.IsPositiveInfinity(Value.Double))
                        return "Infinity";
                    else if (double.IsNegativeInfinity(Value.Double))
                        return "-Infinity";
                }

                string FloatString = Value.ToString();
                if (!FloatString.Contains('.'))
                    FloatString += ".0";
                return FloatString;
            }
            public FloatInstance(Class fromClass, DynFloat value) : base(fromClass) {
                Value = value;
            }
        }
        public class ProcInstance : Instance {
            Method Value;
            public override object? Object { get { return Value; } }
            public override Method Proc { get { return Value; } }
            public ProcInstance(Class fromClass, Method value) : base(fromClass) {
                Value = value;
            }
            public void SetValue(Method value) {
                Value = value;
            }
        }
        public class ThreadInstance : Instance {
            public readonly ScriptThread ScriptThread;
            public override object? Object { get { return ScriptThread; } }
            public override ScriptThread Thread { get { return ScriptThread; } }
            public ThreadInstance(Class fromClass, Script fromScript) : base(fromClass) {
                ScriptThread = new(fromScript);
            }
            public void SetMethod(Method method) {
                Thread.Method = method;
            }
        }
        public class RangeInstance : Instance {
            public IntegerInstance? Min;
            public IntegerInstance? Max;
            public Instance AppliedMin;
            public Instance AppliedMax;
            public bool IncludesMax;
            public override object? Object { get { return ToIntegerRange; } }
            public override IntegerRange Range { get { return ToIntegerRange; } }
            public override string Inspect() {
                return $"{(Min != null ? Min.Inspect() : "")}{(IncludesMax ? ".." : "...")}{(Max != null ? Max.Inspect() : "")}";
            }
            public RangeInstance(Class fromClass, IntegerInstance? min, IntegerInstance? max, bool includesMax) : base(fromClass) {
                Min = min;
                Max = max;
                IncludesMax = includesMax;
                (AppliedMin, AppliedMax) = Setup();
                Setup();
            }
            public void SetValue(IntegerInstance min, IntegerInstance max, bool includesMax) {
                Min = min;
                Max = max;
                IncludesMax = includesMax;
                Setup();
            }
            (Instance, Instance) Setup() {
                if (Min == null) {
                    AppliedMin = Max!.Module!.Interpreter.Api.Nil;
                    AppliedMax = IncludesMax ? Max : Max.Module!.Interpreter.Api.GetInteger(Max.Integer - 1);
                }
                else if (Max == null) {
                    AppliedMin = Min;
                    AppliedMax = Min!.Module!.Interpreter.Api.Nil;
                }
                else {
                    AppliedMin = Min;
                    AppliedMax = IncludesMax ? Max : Max.Module!.Interpreter.Api.GetInteger(Max.Integer - 1);
                }
                return (AppliedMin, AppliedMax);
            }
            IntegerRange ToIntegerRange => new(AppliedMin is IntegerInstance ? (long)AppliedMin.Integer : null, AppliedMax is IntegerInstance ? (long)AppliedMax.Integer : null);
        }
        public class ArrayInstance : Instance {
            List<Instance> Value;
            public override object? Object { get { return Value; } }
            public override List<Instance> Array { get { return Value; } }
            public override string Inspect() {
                return $"[{Value.InspectInstances()}]";
            }
            public override string LightInspect() {
                return Value.LightInspectInstances("\n");
            }
            public ArrayInstance(Class fromClass, List<Instance> value) : base(fromClass) {
                Value = value;
            }
            public void SetValue(List<Instance> value) {
                Value = value;
            }
            public override int GetHashCode() {
                unchecked {
                    int CurrentHash = 19;
                    foreach (Instance Item in Value) {
                        CurrentHash = CurrentHash * 31 + Item.GetHashCode();
                    }
                    return CurrentHash;
                }
            }
        }
        public class HashInstance : Instance {
            HashDictionary Value;
            public Instance DefaultValue;
            public override object? Object { get { return Value; } }
            public override HashDictionary Hash { get { return Value; } }
            public override string Inspect() {
                return $"{{{Value.InspectHash()}}}";
            }
            public HashInstance(Class fromClass, HashDictionary value, Instance defaultValue) : base(fromClass) {
                Value = value;
                DefaultValue = defaultValue;
            }
            public void SetValue(HashDictionary value, Instance defaultValue) {
                Value = value;
                DefaultValue = defaultValue;
            }
            public void SetValue(HashDictionary value) {
                Value = value;
            }
            public override int GetHashCode() {
                unchecked {
                    int CurrentHash = 0;
                    foreach (KeyValuePair<Instance, Instance> Item in Value.KeyValues) {
                        CurrentHash ^= Item.Key.GetHashCode() ^ Item.Value.GetHashCode();
                    }
                    return CurrentHash;
                }
            }
        }
        public class HashArgumentsInstance : Instance {
            public readonly HashInstance Value;
            public override string Inspect() {
                return $"Hash arguments instance: {{{Value.Inspect()}}}";
            }
            public HashArgumentsInstance(HashInstance value, Interpreter interpreter) : base(interpreter) {
                Value = value;
            }
        }
        public class ExceptionInstance : Instance {
            Exception Value;
            public override object? Object { get { return Value; } }
            public override Exception Exception { get { return Value; } }
            public ExceptionInstance(Class fromClass, string message) : base(fromClass) {
                Value = new Exception(message);
            }
            public ExceptionInstance(Class fromClass, Exception exception) : base(fromClass) {
                Value = exception;
            }
            public void SetValue(string message) {
                Value = new Exception(message);
            }
            public void SetValue(Exception exception) {
                Value = exception;
            }
        }
        public class TimeInstance : Instance {
            DateTimeOffset Value;
            public override object? Object { get { return Value; } }
            public override DateTimeOffset Time { get { return Value; } }
            public override string Inspect() {
                return Value.ToString(System.Globalization.CultureInfo.GetCultureInfo("ja-JP")); // yyyy/mm/dd format
            }
            public TimeInstance(Class fromClass, DateTimeOffset value) : base(fromClass) {
                Value = value;
            }
            public void SetValue(DateTimeOffset value) {
                Value = value;
            }
        }
        public class WeakRefInstance : Instance {
            WeakReference<Instance> Value;
            public override object? Object { get { return Value; } }
            public override WeakReference<Instance> WeakRef { get { return Value; } }
            public WeakRefInstance(Class fromClass, WeakReference<Instance> value) : base(fromClass) {
                Value = value;
            }
            public void SetValue(WeakReference<Instance> value) {
                Value = value;
            }
        }
        public class HttpResponseInstance : Instance {
            HttpResponseMessage Value;
            public override object? Object { get { return Value; } }
            public override HttpResponseMessage HttpResponse { get { return Value; } }
            public HttpResponseInstance(Class fromClass, HttpResponseMessage value) : base(fromClass) {
                Value = value;
            }
            public void SetValue(HttpResponseMessage value) {
                Value = value;
            }
        }
        public class ModuleReference : Instance {
            public override object? Object { get { return Module!; } }
            public override string Inspect() {
                return Module!.Name;
            }
            public override string LightInspect() {
                return Module!.Name;
            }
            public ModuleReference(Module module) : base(module) {
                // Copy changes to the parent module
                InstanceMethods = module.InstanceMethods;
            }
        }
    }
}
