﻿using System;
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
    public static class Api
    {
        public static void Setup(Script Script) {
            Interpreter Interpreter = Script.Interpreter;

            // Object
            Interpreter.Object.InstanceMethods["==", "==="] = Interpreter.Object.Methods["==", "==="] = Script.CreateMethod(ClassInstance._Equals, 1);
            Interpreter.Object.InstanceMethods["!="] = Interpreter.Object.Methods["!="] = Script.CreateMethod(ClassInstance._NotEquals, 1);
            Interpreter.Object.InstanceMethods["<=>"] = Interpreter.Object.Methods["<=>"] = Script.CreateMethod(ClassInstance._Spaceship, 1);
            Interpreter.Object.InstanceMethods["inspect"] = Interpreter.Object.Methods["inspect"] = Script.CreateMethod(ClassInstance.inspect, 0);
            Interpreter.Object.InstanceMethods["class"] = Interpreter.Object.Methods["class"] = Script.CreateMethod(ClassInstance.@class, 0);
            Interpreter.Object.InstanceMethods["to_s"] = Interpreter.Object.Methods["to_s"] = Script.CreateMethod(ClassInstance.to_s, 0);
            Interpreter.Object.InstanceMethods["method"] = Interpreter.Object.Methods["method"] = Script.CreateMethod(ClassInstance.method, 1);
            Interpreter.Object.InstanceMethods["constants"] = Interpreter.Object.Methods["constants"] = Script.CreateMethod(ClassInstance.constants, 0);
            Interpreter.Object.InstanceMethods["object_id"] = Interpreter.Object.Methods["object_id"] = Script.CreateMethod(ClassInstance.object_id, 0);
            Interpreter.Object.InstanceMethods["hash"] = Interpreter.Object.Methods["hash"] = Script.CreateMethod(ClassInstance.hash, 0);
            Interpreter.Object.InstanceMethods["eql?"] = Interpreter.Object.Methods["eql?"] = Script.CreateMethod(ClassInstance.eql7, 1);
            Interpreter.Object.InstanceMethods["methods"] = Interpreter.Object.Methods["methods"] = Script.CreateMethod(ClassInstance.methods, 0);
            Interpreter.Object.InstanceMethods["is_a?"] = Interpreter.Object.Methods["is_a?"] = Script.CreateMethod(ClassInstance.is_a7, 1);
            Interpreter.Object.InstanceMethods["instance_of?"] = Interpreter.Object.Methods["instance_of?"] = Script.CreateMethod(ClassInstance.instance_of7, 1);
            Interpreter.Object.InstanceMethods["in?"] = Interpreter.Object.Methods["in?"] = Script.CreateMethod(ClassInstance.in7, 1);
            Interpreter.Object.InstanceMethods["clone"] = Interpreter.Object.Methods["clone"] = Script.CreateMethod(ClassInstance.clone, 0);

            Script.CurrentAccessModifier = AccessModifier.Protected;
            Interpreter.Object.InstanceMethods["puts"] = Interpreter.Object.Methods["puts"] = Script.CreateMethod(puts, null);
            Interpreter.Object.InstanceMethods["print"] = Interpreter.Object.Methods["print"] = Script.CreateMethod(print, null);
            Interpreter.Object.InstanceMethods["p"] = Interpreter.Object.Methods["p"] = Script.CreateMethod(p, null);
            Interpreter.Object.InstanceMethods["gets"] = Interpreter.Object.Methods["gets"] = Script.CreateMethod(gets, 0);
            Interpreter.Object.InstanceMethods["getc"] = Interpreter.Object.Methods["getc"] = Script.CreateMethod(getc, 0);
            Interpreter.Object.InstanceMethods["warn"] = Interpreter.Object.Methods["warn"] = Script.CreateMethod(warn, null);
            Interpreter.Object.InstanceMethods["sleep"] = Interpreter.Object.Methods["sleep"] = Script.CreateMethod(sleep, 0..1);
            Interpreter.Object.InstanceMethods["raise"] = Interpreter.Object.Methods["raise"] = Script.CreateMethod(raise, 0..1);
            Interpreter.Object.InstanceMethods["throw"] = Interpreter.Object.Methods["throw"] = Script.CreateMethod(@throw, 1);
            Interpreter.Object.InstanceMethods["catch"] = Interpreter.Object.Methods["catch"] = Script.CreateMethod(@catch, 1);
            Interpreter.Object.InstanceMethods["lambda"] = Interpreter.Object.Methods["lambda"] = Script.CreateMethod(lambda, 0);
            Interpreter.Object.InstanceMethods["loop"] = Interpreter.Object.Methods["loop"] = Script.CreateMethod(loop, 0);
            Interpreter.Object.InstanceMethods["rand"] = Interpreter.Object.Methods["rand"] = Script.CreateMethod(_Random.rand, 0..1);
            Interpreter.Object.InstanceMethods["srand"] = Interpreter.Object.Methods["srand"] = Script.CreateMethod(_Random.srand, 0..1);
            Interpreter.Object.InstanceMethods["exit"] = Interpreter.Object.Methods["exit"] = Script.CreateMethod(exit, 0);
            Interpreter.Object.InstanceMethods["quit"] = Interpreter.Object.Methods["quit"] = Script.CreateMethod(exit, 0);
            Interpreter.Object.InstanceMethods["eval"] = Interpreter.Object.Methods["eval"] = Script.CreateMethod(eval, 1);
            Interpreter.Object.InstanceMethods["local_variables"] = Interpreter.Object.Methods["local_variables"] = Script.CreateMethod(local_variables, 0);
            Interpreter.Object.InstanceMethods["global_variables"] = Interpreter.Object.Methods["global_variables"] = Script.CreateMethod(global_variables, 0);

            Interpreter.Object.InstanceMethods["attr_reader"] = Script.CreateMethod(ClassInstance.attr_reader, 1);
            Interpreter.Object.InstanceMethods["attr_writer"] = Script.CreateMethod(ClassInstance.attr_writer, 1);
            Interpreter.Object.InstanceMethods["attr_accessor"] = Script.CreateMethod(ClassInstance.attr_accessor, 1);
            Interpreter.Object.InstanceMethods["public"] = Script.CreateMethod(ClassInstance.@public, 0);
            Interpreter.Object.InstanceMethods["private"] = Script.CreateMethod(ClassInstance.@private, 0);
            Interpreter.Object.InstanceMethods["protected"] = Script.CreateMethod(ClassInstance.@protected, 0);
            Script.CurrentAccessModifier = AccessModifier.Public;

            // Global constants
            Interpreter.RootScope.Constants["EMBERS_VERSION"] = new StringInstance(Interpreter.String, Info.Version);
            Interpreter.RootScope.Constants["EMBERS_RELEASE_DATE"] = new StringInstance(Interpreter.String, Info.ReleaseDate);
            Interpreter.RootScope.Constants["EMBERS_PLATFORM"] = new StringInstance(Interpreter.String, $"{RuntimeInformation.OSArchitecture}-{RuntimeInformation.OSDescription}");
            Interpreter.RootScope.Constants["EMBERS_COPYRIGHT"] = new StringInstance(Interpreter.String, Info.Copyright);
            Interpreter.RootScope.Constants["RUBY_COPYRIGHT"] = new StringInstance(Interpreter.String, Info.RubyCopyright);

            // Class
            Interpreter.Class.Methods["name"] = Script.CreateMethod(_Class.name, 0);
            Interpreter.Class.Methods["==="] = Script.CreateMethod(_Class._TripleEquals, 1);

            // String
            Interpreter.String.InstanceMethods["[]"] = Script.CreateMethod(String._Indexer, 1);
            Interpreter.String.InstanceMethods["[]="] = Script.CreateMethod(String._IndexEquals, 2);
            Interpreter.String.InstanceMethods["+"] = Script.CreateMethod(String._Add, 1);
            Interpreter.String.InstanceMethods["*"] = Script.CreateMethod(String._Multiply, 1);
            Interpreter.String.InstanceMethods["==", "==="] = Script.CreateMethod(String._Equals, 1);
            Interpreter.String.InstanceMethods["<"] = Script.CreateMethod(String._LessThan, 1);
            Interpreter.String.InstanceMethods[">"] = Script.CreateMethod(String._GreaterThan, 1);
            Interpreter.String.InstanceMethods["<="] = Script.CreateMethod(String._LessThanOrEqualTo, 1);
            Interpreter.String.InstanceMethods[">="] = Script.CreateMethod(String._GreaterThanOrEqualTo, 1);
            Interpreter.String.InstanceMethods["<=>"] = Script.CreateMethod(String._Spaceship, 1);
            Interpreter.String.InstanceMethods["initialize"] = Script.CreateMethod(String.initialize, 0..1);
            Interpreter.String.InstanceMethods["to_str"] = Script.CreateMethod(String.to_str, 0);
            Interpreter.String.InstanceMethods["to_i"] = Script.CreateMethod(String.to_i, 0);
            Interpreter.String.InstanceMethods["to_f"] = Script.CreateMethod(String.to_f, 0);
            Interpreter.String.InstanceMethods["to_sym"] = Script.CreateMethod(String.to_sym, 0);
            Interpreter.String.InstanceMethods["to_a"] = Script.CreateMethod(String.to_a, 0);
            Interpreter.String.InstanceMethods["chomp"] = Script.CreateMethod(String.chomp, 0..1);
            Interpreter.String.InstanceMethods["chomp!"] = Script.CreateMethod(String.chomp1, 0..1);
            Interpreter.String.InstanceMethods["strip"] = Script.CreateMethod(String.strip, 0);
            Interpreter.String.InstanceMethods["strip!"] = Script.CreateMethod(String.strip1, 0);
            Interpreter.String.InstanceMethods["lstrip"] = Script.CreateMethod(String.lstrip, 0);
            Interpreter.String.InstanceMethods["lstrip!"] = Script.CreateMethod(String.lstrip1, 0);
            Interpreter.String.InstanceMethods["rstrip"] = Script.CreateMethod(String.rstrip, 0);
            Interpreter.String.InstanceMethods["rstrip!"] = Script.CreateMethod(String.rstrip1, 0);
            Interpreter.String.InstanceMethods["squeeze"] = Script.CreateMethod(String.squeeze, 0);
            Interpreter.String.InstanceMethods["squeeze!"] = Script.CreateMethod(String.squeeze1, 0);
            Interpreter.String.InstanceMethods["chop"] = Script.CreateMethod(String.chop, 0);
            Interpreter.String.InstanceMethods["chop!"] = Script.CreateMethod(String.chop1, 0);
            Interpreter.String.InstanceMethods["chr"] = Script.CreateMethod(String.chr, 0);
            Interpreter.String.InstanceMethods["capitalize"] = Script.CreateMethod(String.capitalize, 0);
            Interpreter.String.InstanceMethods["capitalize!"] = Script.CreateMethod(String.capitalize1, 0);
            Interpreter.String.InstanceMethods["upcase"] = Script.CreateMethod(String.upcase, 0);
            Interpreter.String.InstanceMethods["upcase!"] = Script.CreateMethod(String.upcase1, 0);
            Interpreter.String.InstanceMethods["downcase"] = Script.CreateMethod(String.downcase, 0);
            Interpreter.String.InstanceMethods["downcase!"] = Script.CreateMethod(String.downcase1, 0);
            Interpreter.String.InstanceMethods["sub"] = Script.CreateMethod(String.sub, 2);
            Interpreter.String.InstanceMethods["sub!"] = Script.CreateMethod(String.sub1, 2);
            Interpreter.String.InstanceMethods["gsub"] = Script.CreateMethod(String.gsub, 2);
            Interpreter.String.InstanceMethods["gsub!"] = Script.CreateMethod(String.gsub1, 2);
            Interpreter.String.InstanceMethods["eql?"] = Script.CreateMethod(String.eql7, 1);

            // Integer
            Interpreter.Integer.InstanceMethods["+"] = Script.CreateMethod(Integer._Add, 1);
            Interpreter.Integer.InstanceMethods["-"] = Script.CreateMethod(Integer._Subtract, 1);
            Interpreter.Integer.InstanceMethods["*"] = Script.CreateMethod(Integer._Multiply, 1);
            Interpreter.Integer.InstanceMethods["/"] = Script.CreateMethod(Integer._Divide, 1);
            Interpreter.Integer.InstanceMethods["%"] = Script.CreateMethod(Integer._Modulo, 1);
            Interpreter.Integer.InstanceMethods["**"] = Script.CreateMethod(Integer._Exponentiate, 1);
            Interpreter.Integer.InstanceMethods["==", "==="] = Script.CreateMethod(Float._Equals, 1);
            Interpreter.Integer.InstanceMethods["<"] = Script.CreateMethod(Float._LessThan, 1);
            Interpreter.Integer.InstanceMethods[">"] = Script.CreateMethod(Float._GreaterThan, 1);
            Interpreter.Integer.InstanceMethods["<="] = Script.CreateMethod(Float._LessThanOrEqualTo, 1);
            Interpreter.Integer.InstanceMethods[">="] = Script.CreateMethod(Float._GreaterThanOrEqualTo, 1);
            Interpreter.Integer.InstanceMethods["<=>"] = Script.CreateMethod(Float._Spaceship, 1);
            Interpreter.Integer.InstanceMethods["+@"] = Script.CreateMethod(Integer._UnaryPlus, 0);
            Interpreter.Integer.InstanceMethods["-@"] = Script.CreateMethod(Integer._UnaryMinus, 0);
            Interpreter.Integer.InstanceMethods["to_i"] = Script.CreateMethod(Integer.to_i, 0);
            Interpreter.Integer.InstanceMethods["to_f"] = Script.CreateMethod(Integer.to_f, 0);
            Interpreter.Integer.InstanceMethods["times"] = Script.CreateMethod(Integer.times, 0);
            Interpreter.Integer.InstanceMethods["clamp"] = Script.CreateMethod(Integer.clamp, 2);
            Interpreter.Integer.InstanceMethods["round"] = Script.CreateMethod(Float.round, 0..1);
            Interpreter.Integer.InstanceMethods["floor"] = Script.CreateMethod(Float.floor, 0);
            Interpreter.Integer.InstanceMethods["ceil"] = Script.CreateMethod(Float.ceil, 0);
            Interpreter.Integer.InstanceMethods["truncate"] = Script.CreateMethod(Float.truncate, 0);

            // Float
            Interpreter.Float.Constants["INFINITY"] = Interpreter.GetFloat(double.PositiveInfinity);
            Interpreter.Float.InstanceMethods["+"] = Script.CreateMethod(Float._Add, 1);
            Interpreter.Float.InstanceMethods["-"] = Script.CreateMethod(Float._Subtract, 1);
            Interpreter.Float.InstanceMethods["*"] = Script.CreateMethod(Float._Multiply, 1);
            Interpreter.Float.InstanceMethods["/"] = Script.CreateMethod(Float._Divide, 1);
            Interpreter.Float.InstanceMethods["%"] = Script.CreateMethod(Float._Modulo, 1);
            Interpreter.Float.InstanceMethods["**"] = Script.CreateMethod(Float._Exponentiate, 1);
            Interpreter.Float.InstanceMethods["==", "==="] = Script.CreateMethod(Float._Equals, 1);
            Interpreter.Float.InstanceMethods["<"] = Script.CreateMethod(Float._LessThan, 1);
            Interpreter.Float.InstanceMethods[">"] = Script.CreateMethod(Float._GreaterThan, 1);
            Interpreter.Float.InstanceMethods["<="] = Script.CreateMethod(Float._LessThanOrEqualTo, 1);
            Interpreter.Float.InstanceMethods[">="] = Script.CreateMethod(Float._GreaterThanOrEqualTo, 1);
            Interpreter.Float.InstanceMethods["<=>"] = Script.CreateMethod(Float._Spaceship, 1);
            Interpreter.Float.InstanceMethods["+@"] = Script.CreateMethod(Float._UnaryPlus, 0);
            Interpreter.Float.InstanceMethods["-@"] = Script.CreateMethod(Float._UnaryMinus, 0);
            Interpreter.Float.InstanceMethods["to_i"] = Script.CreateMethod(Float.to_i, 0);
            Interpreter.Float.InstanceMethods["to_f"] = Script.CreateMethod(Float.to_f, 0);
            Interpreter.Float.InstanceMethods["clamp"] = Script.CreateMethod(Float.clamp, 2);
            Interpreter.Float.InstanceMethods["round"] = Script.CreateMethod(Float.round, 0..1);
            Interpreter.Float.InstanceMethods["floor"] = Script.CreateMethod(Float.floor, 0);
            Interpreter.Float.InstanceMethods["ceil"] = Script.CreateMethod(Float.ceil, 0);
            Interpreter.Float.InstanceMethods["truncate"] = Script.CreateMethod(Float.truncate, 0);

            // Proc
            Interpreter.Proc.InstanceMethods["call"] = Script.CreateMethod(Proc.call, null);

            // Range
            Interpreter.Range.InstanceMethods["==="] = Script.CreateMethod(Range._TripleEquals, 1);
            Interpreter.Range.InstanceMethods["min"] = Script.CreateMethod(Range.min, 0);
            Interpreter.Range.InstanceMethods["max"] = Script.CreateMethod(Range.max, 0);
            Interpreter.Range.InstanceMethods["each"] = Script.CreateMethod(Range.each, 0);
            Interpreter.Range.InstanceMethods["reverse_each"] = Script.CreateMethod(Range.reverse_each, 0);
            Interpreter.Range.InstanceMethods["length", "count"] = Script.CreateMethod(Range.length, 0);
            Interpreter.Range.InstanceMethods["to_a"] = Script.CreateMethod(Range.to_a, 0);

            // Array
            Interpreter.Array.InstanceMethods["[]"] = Script.CreateMethod(Array._Indexer, 1);
            Interpreter.Array.InstanceMethods["[]="] = Script.CreateMethod(Array._IndexEquals, 2);
            Interpreter.Array.InstanceMethods["*"] = Script.CreateMethod(Array._Multiply, 1);
            Interpreter.Array.InstanceMethods["==", "==="] = Script.CreateMethod(Array._Equals, 1);
            Interpreter.Array.InstanceMethods["<<"] = Script.CreateMethod(Array._Append, 1);
            Interpreter.Array.InstanceMethods["length"] = Script.CreateMethod(Array.length, 0);
            Interpreter.Array.InstanceMethods["count"] = Script.CreateMethod(Array.count, 0..1);
            Interpreter.Array.InstanceMethods["first"] = Script.CreateMethod(Array.first, 0);
            Interpreter.Array.InstanceMethods["last"] = Script.CreateMethod(Array.last, 0);
            Interpreter.Array.InstanceMethods["forty_two"] = Script.CreateMethod(Array.forty_two, 0);
            Interpreter.Array.InstanceMethods["sample"] = Script.CreateMethod(Array.sample, 0);
            Interpreter.Array.InstanceMethods["min"] = Script.CreateMethod(Array.min, 0);
            Interpreter.Array.InstanceMethods["max"] = Script.CreateMethod(Array.max, 0);
            Interpreter.Array.InstanceMethods["insert"] = Script.CreateMethod(Array.insert, 1..);
            Interpreter.Array.InstanceMethods["each"] = Script.CreateMethod(Array.each, 0);
            Interpreter.Array.InstanceMethods["reverse_each"] = Script.CreateMethod(Array.reverse_each, 0);
            Interpreter.Array.InstanceMethods["map"] = Script.CreateMethod(Array.map, 0);
            Interpreter.Array.InstanceMethods["map!"] = Script.CreateMethod(Array.map1, 0);
            Interpreter.Array.InstanceMethods["sort"] = Script.CreateMethod(Array.sort, 0);
            Interpreter.Array.InstanceMethods["sort!"] = Script.CreateMethod(Array.sort1, 0);
            Interpreter.Array.InstanceMethods["include?", "includes?", "contain?", "contains?"] = Script.CreateMethod(Array.include7, 1);
            Interpreter.Array.InstanceMethods["delete", "remove"] = Script.CreateMethod(Array.delete, 1);
            Interpreter.Array.InstanceMethods["delete_at", "remove_at"] = Script.CreateMethod(Array.delete_at, 1);
            Interpreter.Array.InstanceMethods["clear"] = Script.CreateMethod(Array.clear, 0);
            Interpreter.Array.InstanceMethods["empty?"] = Script.CreateMethod(Array.empty7, 0);
            Interpreter.Array.InstanceMethods["reverse"] = Script.CreateMethod(Array.reverse, 0);
            Interpreter.Array.InstanceMethods["reverse!"] = Script.CreateMethod(Array.reverse1, 0);

            // Hash
            Interpreter.Hash.InstanceMethods["[]"] = Script.CreateMethod(Hash._Indexer, 1);
            Interpreter.Hash.InstanceMethods["[]="] = Script.CreateMethod(Hash._IndexEquals, 2);
            Interpreter.Hash.InstanceMethods["==", "==="] = Script.CreateMethod(Hash._Equals, 1);
            Interpreter.Hash.InstanceMethods["initialize"] = Script.CreateMethod(Hash.initialize, 0..1);
            Interpreter.Hash.InstanceMethods["has_key?"] = Script.CreateMethod(Hash.has_key7, 1);
            Interpreter.Hash.InstanceMethods["has_value?"] = Script.CreateMethod(Hash.has_value7, 1);
            Interpreter.Hash.InstanceMethods["keys"] = Script.CreateMethod(Hash.keys, 0);
            Interpreter.Hash.InstanceMethods["values"] = Script.CreateMethod(Hash.values, 0);
            Interpreter.Hash.InstanceMethods["delete", "remove"] = Script.CreateMethod(Hash.delete, 1);
            Interpreter.Hash.InstanceMethods["clear"] = Script.CreateMethod(Hash.clear, 0);
            Interpreter.Hash.InstanceMethods["each"] = Script.CreateMethod(Hash.each, 0);
            Interpreter.Hash.InstanceMethods["invert"] = Script.CreateMethod(Hash.invert, 0);
            Interpreter.Hash.InstanceMethods["to_a"] = Script.CreateMethod(Hash.to_a, 0);
            Interpreter.Hash.InstanceMethods["to_hash"] = Script.CreateMethod(Hash.to_hash, 0);
            Interpreter.Hash.InstanceMethods["empty?"] = Script.CreateMethod(Hash.empty7, 0);

            // Random
            Module RandomModule = Script.CreateModule("Random");
            RandomModule.Methods["rand"] = Script.CreateMethod(_Random.rand, 0..1);
            RandomModule.Methods["srand"] = Script.CreateMethod(_Random.srand, 0..1);

            // Math
            Module MathModule = Script.CreateModule("Math");
            MathModule.Constants["PI"] = Interpreter.GetFloat(Math.PI);
            MathModule.Constants["E"] = Interpreter.GetFloat(Math.E);
            MathModule.Methods["sin"] = Script.CreateMethod(_Math.sin, 1);
            MathModule.Methods["cos"] = Script.CreateMethod(_Math.cos, 1);
            MathModule.Methods["tan"] = Script.CreateMethod(_Math.tan, 1);
            MathModule.Methods["asin"] = Script.CreateMethod(_Math.asin, 1);
            MathModule.Methods["acos"] = Script.CreateMethod(_Math.acos, 1);
            MathModule.Methods["atan"] = Script.CreateMethod(_Math.atan, 1);
            MathModule.Methods["atan2"] = Script.CreateMethod(_Math.atan2, 2);
            MathModule.Methods["sinh"] = Script.CreateMethod(_Math.sinh, 1);
            MathModule.Methods["cosh"] = Script.CreateMethod(_Math.cosh, 1);
            MathModule.Methods["tanh"] = Script.CreateMethod(_Math.tanh, 1);
            MathModule.Methods["asinh"] = Script.CreateMethod(_Math.asinh, 1);
            MathModule.Methods["acosh"] = Script.CreateMethod(_Math.acosh, 1);
            MathModule.Methods["atanh"] = Script.CreateMethod(_Math.atanh, 1);
            MathModule.Methods["exp"] = Script.CreateMethod(_Math.exp, 1);
            MathModule.Methods["log"] = Script.CreateMethod(_Math.log, 2);
            MathModule.Methods["log10"] = Script.CreateMethod(_Math.log10, 1);
            MathModule.Methods["log2"] = Script.CreateMethod(_Math.log2, 1);
            MathModule.Methods["frexp"] = Script.CreateMethod(_Math.frexp, 1);
            MathModule.Methods["ldexp"] = Script.CreateMethod(_Math.ldexp, 2);
            MathModule.Methods["sqrt"] = Script.CreateMethod(_Math.sqrt, 1);
            MathModule.Methods["cbrt"] = Script.CreateMethod(_Math.cbrt, 1);
            MathModule.Methods["hypot"] = Script.CreateMethod(_Math.hypot, 2);
            MathModule.Methods["erf"] = Script.CreateMethod(_Math.erf, 1);
            MathModule.Methods["erfc"] = Script.CreateMethod(_Math.erfc, 1);
            MathModule.Methods["gamma"] = Script.CreateMethod(_Math.gamma, 1);
            MathModule.Methods["lgamma"] = Script.CreateMethod(_Math.lgamma, 1);
            MathModule.Methods["to_rad"] = Script.CreateMethod(_Math.to_rad, 1);
            MathModule.Methods["to_deg"] = Script.CreateMethod(_Math.to_deg, 1);
            MathModule.Methods["lerp"] = Script.CreateMethod(_Math.lerp, 3);

            // Exception
            Interpreter.Exception.InstanceMethods["initialize"] = Script.CreateMethod(_Exception.initialize, 0..1);
            Interpreter.Exception.InstanceMethods["message"] = Script.CreateMethod(_Exception.message, 0);
            Interpreter.Exception.InstanceMethods["backtrace"] = Script.CreateMethod(_Exception.backtrace, 0);

            // Thread
            Interpreter.Thread.InstanceMethods["initialize"] = Script.CreateMethod(_Thread.initialize, null);
            Interpreter.Thread.InstanceMethods["join"] = Script.CreateMethod(_Thread.join, 0);
            Interpreter.Thread.InstanceMethods["stop"] = Script.CreateMethod(_Thread.stop, 0);

            // Parallel
            Module ParallelModule = Script.CreateModule("Parallel");
            ParallelModule.Methods["each"] = Script.CreateMethod(_Parallel.each, 1);
            ParallelModule.Methods["times"] = Script.CreateMethod(_Parallel.times, 1);

            // Time
            Interpreter.Time.Methods["now"] = Script.CreateMethod(Time.now, 0);
            Interpreter.Time.Methods["at"] = Script.CreateMethod(Time.at, 1);
            Interpreter.Time.InstanceMethods["initialize"] = Script.CreateMethod(Time.initialize, 0..7);
            Interpreter.Time.InstanceMethods["to_i"] = Script.CreateMethod(Time.to_i, 0);
            Interpreter.Time.InstanceMethods["to_f"] = Script.CreateMethod(Time.to_f, 0);

            //
            // UNSAFE APIS
            //

            // Global methods
            Script.CurrentAccessModifier = AccessModifier.Protected;
            Interpreter.Object.InstanceMethods["system"] = Interpreter.Object.Methods["system"] = Script.CreateMethod(system, 1, IsUnsafe: true);
            Script.CurrentAccessModifier = AccessModifier.Public;

            // File
            Module FileModule = Script.CreateModule("File");
            FileModule.Methods["read"] = Script.CreateMethod(_File.read, 1, IsUnsafe: true);
            FileModule.Methods["write"] = Script.CreateMethod(_File.write, 2, IsUnsafe: true);
            FileModule.Methods["append"] = Script.CreateMethod(_File.append, 2, IsUnsafe: true);
            FileModule.Methods["delete"] = Script.CreateMethod(_File.delete, 1, IsUnsafe: true);
            FileModule.Methods["exist?", "exists?"] = Script.CreateMethod(_File.exist7, 1, IsUnsafe: true);
            FileModule.Methods["absolute_path"] = Script.CreateMethod(_File.absolute_path, 1, IsUnsafe: true);
            FileModule.Methods["absolute_path?"] = Script.CreateMethod(_File.absolute_path7, 1, IsUnsafe: true);
            FileModule.Methods["basename"] = Script.CreateMethod(_File.basename, 1, IsUnsafe: true);
            FileModule.Methods["dirname"] = Script.CreateMethod(_File.dirname, 1, IsUnsafe: true);

            // Net
            Module NetModule = Script.CreateModule("Net");
            // Net::HTTP
            Module NetHTTPModule = Script.CreateModule("HTTP", NetModule);
            NetHTTPModule.Methods["get"] = Script.CreateMethod(Net.HTTP.get, 1, IsUnsafe: true);
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
            return Input.Interpreter.Nil;
        }
        static async Task<Instance> print(MethodInput Input) {
            foreach (Instance Message in Input.Arguments) {
                Console.Write(Message.LightInspect());
            }
            return Input.Interpreter.Nil;
        }
        static async Task<Instance> p(MethodInput Input) {
            foreach (Instance Message in Input.Arguments) {
                Console.WriteLine(Message.Inspect());
            }
            return Input.Interpreter.Nil;
        }
        static async Task<Instance> gets(MethodInput Input) {
            string? UserInput = Console.ReadLine();
            UserInput = UserInput != null ? UserInput + "\n" : "";
            return new StringInstance(Input.Interpreter.String, UserInput);
        }
        static async Task<Instance> getc(MethodInput Input) {
            string UserInput = Console.ReadKey().KeyChar.ToString();
            return new StringInstance(Input.Interpreter.String, UserInput);
        }
        static async Task<Instance> warn(MethodInput Input) {
            ConsoleColor PreviousForegroundColour = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            foreach (Instance Message in Input.Arguments) {
                Console.WriteLine(Message.Object);
            }
            Console.ForegroundColor = PreviousForegroundColour;
            return Input.Interpreter.Nil;
        }
        static async Task<Instance> sleep(MethodInput Input) {
            if (Input.Arguments.Count == 1) {
                DynFloat SecondsToSleep = Input.Arguments[0].Float;
                await Task.Delay((int)(SecondsToSleep * 1000));
            }
            else {
                await Task.Delay(Timeout.Infinite);
            }
            return Input.Interpreter.Nil;
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
                    Input.Script.ExceptionsTable.TryAdd(NewExceptionToRaise, new ExceptionInstance(Input.Interpreter.RuntimeError, Argument.String));
                    throw NewExceptionToRaise;
                }
            }
            else {
                // raise
                Exception NewExceptionToRaise = new RuntimeException("");
                Input.Script.ExceptionsTable.TryAdd(NewExceptionToRaise, new ExceptionInstance(Input.Interpreter.RuntimeError, ""));
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
            return Input.Interpreter.Nil;
        }
        static async Task<Instance> lambda(MethodInput Input) {
            Method? OnYield = Input.OnYield ?? throw new RuntimeException($"{Input.Location}: No block given for lambda");

            Instance NewProc = new ProcInstance(Input.Interpreter.Proc, Input.Script.CreateMethod(
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
            return Input.Interpreter.Nil;
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
            return new StringInstance(Input.Interpreter.String, Output);
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
                GlobalVariables.Add(Input.Interpreter.GetSymbol(GlobalVariable.Key));
            }
            return new ArrayInstance(Input.Interpreter.Array, GlobalVariables);
        }
        static async Task<Instance> global_variables(MethodInput Input) {
            List<Instance> GlobalVariables = new();
            foreach (KeyValuePair<string, Instance> GlobalVariable in Input.Interpreter.GlobalVariables) {
                GlobalVariables.Add(Input.Interpreter.GetSymbol(GlobalVariable.Key));
            }
            return new ArrayInstance(Input.Interpreter.Array, GlobalVariables);
        }
        static class ClassInstance {
            public static async Task<Instance> _Equals(MethodInput Input) {
                Instance Left = Input.Instance;
                Instance Right = Input.Arguments[0];
                if (Left is ModuleReference LeftModule && Right is ModuleReference RightModule) {
                    return LeftModule.Module == RightModule.Module ? Input.Interpreter.True : Input.Interpreter.False;
                }
                else {
                    return Left == Right ? Input.Interpreter.True : Input.Interpreter.False;
                }
            }
            public static async Task<Instance> _NotEquals(MethodInput Input) {
                Instance Left = Input.Instance;
                Instance Right = Input.Arguments[0];
                return (await Left.CallInstanceMethod(Input.Script, "==", Right)).IsTruthy ? Input.Interpreter.False : Input.Interpreter.True;
            }
            public static async Task<Instance> _Spaceship(MethodInput Input) {
                return Input.Interpreter.Nil;
            }
            public static async Task<Instance> inspect(MethodInput Input) {
                return new StringInstance(Input.Interpreter.String, Input.Instance.Inspect());
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
                return new StringInstance(Input.Interpreter.String, Input.Instance.LightInspect());
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
                    return new ProcInstance(Input.Interpreter.Proc, FindMethod!);
                }
                else {
                    throw new RuntimeException($"{Input.Location}: Undefined method '{MethodName}' for {Input.Instance.LightInspect()}");
                }
            }
            public static async Task<Instance> constants(MethodInput Input) {
                List<Instance> Constants = new();
                foreach (KeyValuePair<string, Instance> Constant in Input.Script.GetAllLocalConstants()) {
                    Constants.Add(Input.Interpreter.GetSymbol(Constant.Key));
                }
                return new ArrayInstance(Input.Interpreter.Array, Constants);
            }
            public static async Task<Instance> object_id(MethodInput Input) {
                return Input.Interpreter.GetInteger(Input.Instance.ObjectId);
            }
            public static async Task<Instance> hash(MethodInput Input) {
                DynInteger Hash = (Input.Instance.GetHashCode().ToString() + ((DynInteger)31 * Input.Instance.Module!.GetHashCode()).ToString()).ParseInteger();
                return Input.Interpreter.GetInteger(Hash);
            }
            public static async Task<Instance> eql7(MethodInput Input) {
                Instance Other = Input.Arguments[0];
                return (await Input.Instance.CallInstanceMethod(Input.Script, "hash")).Integer == (await Other.CallInstanceMethod(Input.Script, "hash")).Integer
                    ? Input.Interpreter.True : Input.Interpreter.False;
            }
            public static async Task<Instance> methods(MethodInput Input) {
                List<Instance> MethodsDictToSymbolsArray(LockingDictionary<string, Method> MethodDict) {
                    List<Instance> Symbols = new();
                    foreach (string MethodName in MethodDict.Keys) {
                        Symbols.Add(Input.Interpreter.GetSymbol(MethodName));
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
            public static async Task<Instance> is_a7(MethodInput Input) {
                Instance Argument = Input.Arguments[0];
                if (Argument is ModuleReference ModuleRef && Input.Instance is not (PseudoInstance or ModuleReference)) {
                    return Input.Instance.Module!.InheritsFrom(ModuleRef.Module!) ? Input.Interpreter.True : Input.Interpreter.False;
                }
                else {
                    throw new RuntimeException($"{Input.Location}: Expected class/module for is_a?");
                }
            }
            public static async Task<Instance> instance_of7(MethodInput Input) {
                Instance Argument = Input.Arguments[0];
                if (Argument is ModuleReference ModuleRef && Input.Instance is not (PseudoInstance or ModuleReference)) {
                    return Input.Instance.Module! == ModuleRef.Module! ? Input.Interpreter.True : Input.Interpreter.False;
                }
                else {
                    throw new RuntimeException($"{Input.Location}: Expected class/module for instance_of?");
                }
            }
            public static async Task<Instance> in7(MethodInput Input) {
                List<Instance> Array = Input.Arguments[0].Array;
                foreach (Instance Item in Array) {
                    if ((await Item.InstanceMethods["=="].Call(Input.Script, Item, Input.Instance)).IsTruthy) {
                        return Input.Interpreter.True;
                    }
                }
                return Input.Interpreter.False;
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
                    return Value ?? Input.Interpreter.Nil;
                }, 0);

                return Input.Interpreter.Nil;
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

                return Input.Interpreter.Nil;
            }
            public static async Task<Instance> attr_accessor(MethodInput Input) {
                await attr_writer(Input);
                await attr_reader(Input);
                return Input.Interpreter.Nil;
            }
            public static async Task<Instance> @public(MethodInput Input) {
                Input.Script.CurrentAccessModifier = AccessModifier.Public;
                return Input.Interpreter.Nil;
            }
            public static async Task<Instance> @private(MethodInput Input) {
                Input.Script.CurrentAccessModifier = AccessModifier.Private;
                return Input.Interpreter.Nil;
            }
            public static async Task<Instance> @protected(MethodInput Input) {
                Input.Script.CurrentAccessModifier = AccessModifier.Protected;
                return Input.Interpreter.Nil;
            }
        }
        static class String {
            public static async Task<Instance> _Add(MethodInput Input) {
                Instance Right = Input.Arguments[0];
                return new StringInstance(Input.Interpreter.String, Input.Instance.String + Right.String);
            }
            public static async Task<Instance> _Multiply(MethodInput Input) {
                Instance Right = Input.Arguments[0];
                StringBuilder JoinedString = new();
                long RepeatCount = (long)Right.Integer;
                for (long i = 0; i < RepeatCount; i++) {
                    JoinedString.Append(Input.Instance.String);
                }
                return new StringInstance(Input.Interpreter.String, JoinedString.ToString());
            }
            public static async Task<Instance> _Equals(MethodInput Input) {
                Instance Left = Input.Instance;
                Instance Right = Input.Arguments[0];
                if (Right is StringInstance RightString && Left.String == RightString.String) {
                    return Input.Interpreter.True;
                }
                else {
                    return Input.Interpreter.False;
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

                    return new StringInstance(Input.Interpreter.String, String[StartIndex..(EndIndex + 1)]);
                }
                else {
                    int Index = _RealisticIndex(Input, Input.Arguments[0].Integer);

                    // Return character at string index or nil
                    if (Index >= 0 && Index < String.Length) {
                        return new StringInstance(Input.Interpreter.String, String[Index].ToString());
                    }
                    else if (Index < 0 && Index > -String.Length) {
                        return new StringInstance(Input.Interpreter.String, String[^-Index].ToString());
                    }
                    else {
                        return Input.Interpreter.Nil;
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
                    return Input.Interpreter.True;
                }
                else {
                    return Input.Interpreter.False;
                }
            }
            public static async Task<Instance> _GreaterThan(MethodInput Input) {
                Instance Left = Input.Instance;
                Instance Right = Input.Arguments[0];
                if (Right is StringInstance && string.Compare(Left.String, Right.String) > 0) {
                    return Input.Interpreter.True;
                }
                else {
                    return Input.Interpreter.False;
                }
            }
            public static async Task<Instance> _LessThanOrEqualTo(MethodInput Input) {
                Instance Left = Input.Instance;
                Instance Right = Input.Arguments[0];
                if (Right is StringInstance && string.Compare(Left.String, Right.String) <= 0) {
                    return Input.Interpreter.True;
                }
                else {
                    return Input.Interpreter.False;
                }
            }
            public static async Task<Instance> _GreaterThanOrEqualTo(MethodInput Input) {
                Instance Left = Input.Instance;
                Instance Right = Input.Arguments[0];
                if (Right is StringInstance && string.Compare(Left.String, Right.String) >= 0) {
                    return Input.Interpreter.True;
                }
                else {
                    return Input.Interpreter.False;
                }
            }
            public static async Task<Instance> _Spaceship(MethodInput Input) {
                if ((await _LessThan(Input)).IsTruthy) {
                    return Input.Interpreter.GetInteger(-1);
                }
                else if ((await _Equals(Input)).IsTruthy) {
                    return Input.Interpreter.GetInteger(0);
                }
                else if ((await _GreaterThan(Input)).IsTruthy) {
                    return Input.Interpreter.GetInteger(1);
                }
                return Input.Interpreter.Nil;
            }
            public static async Task<Instance> initialize(MethodInput Input) {
                if (Input.Arguments.Count == 1) {
                    ((StringInstance)Input.Instance).SetValue(Input.Arguments[0].String);
                }
                return Input.Interpreter.Nil;
            }
            public static async Task<Instance> to_str(MethodInput Input) {
                return await ClassInstance.to_s(Input);
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
                if (IntegerString.Length == 0) return Input.Interpreter.GetInteger(0);
                return Input.Interpreter.GetInteger(IntegerString.ToString().ParseInteger());
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
                if (FloatString.Length == 0) return Input.Interpreter.GetFloat(0);
                if (!SeenDot) FloatString.Append(".0");
                return Input.Interpreter.GetFloat(FloatString.ToString().ParseFloat());
            }
            public static async Task<Instance> to_sym(MethodInput Input) {
                return Input.Interpreter.GetSymbol(Input.Instance.String);
            }
            public static async Task<Instance> to_a(MethodInput Input) {
                List<Instance> Array = new();
                foreach (char Chara in Input.Instance.String) {
                    Array.Add(new StringInstance(Input.Interpreter.String, Chara.ToString()));
                }
                return new ArrayInstance(Input.Interpreter.Array, Array);
            }
            private static StringInstance _CreateOrSetString(MethodInput Input, string Value, bool Exclaim) {
                if (Exclaim) {
                    StringInstance String = (StringInstance)Input.Instance;
                    String.SetValue(Value);
                    return String;
                }
                else {
                    return new StringInstance(Input.Interpreter.String, Value);
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
        static class Integer {
            private static Instance _GetResult(Interpreter Interpreter, DynFloat Result, bool RightIsInteger) {
                if (RightIsInteger) {
                    return Interpreter.GetInteger((DynInteger)Result);
                }
                else {
                    return Interpreter.GetFloat(Result);
                }
            }
            public static async Task<Instance> _Add(MethodInput Input) {
                Instance Right = Input.Arguments[0];
                return _GetResult(Input.Interpreter, Input.Instance.Integer + Right.Float, Right is IntegerInstance);
            }
            public static async Task<Instance> _Subtract(MethodInput Input) {
                Instance Right = Input.Arguments[0];
                return _GetResult(Input.Interpreter, Input.Instance.Integer - Right.Float, Right is IntegerInstance);
            }
            public static async Task<Instance> _Multiply(MethodInput Input) {
                Instance Right = Input.Arguments[0];
                return _GetResult(Input.Interpreter, Input.Instance.Integer * Right.Float, Right is IntegerInstance);
            }
            public static async Task<Instance> _Divide(MethodInput Input) {
                Instance Right = Input.Arguments[0];
                if (Right.Float == 0) throw new DivideByZeroException();
                return _GetResult(Input.Interpreter, Input.Instance.Integer / Right.Float, Right is IntegerInstance);
            }
            public static async Task<Instance> _Modulo(MethodInput Input) {
                Instance Right = Input.Arguments[0];
                return _GetResult(Input.Interpreter, Input.Instance.Integer % Right.Float, Right is IntegerInstance);
            }
            public static async Task<Instance> _Exponentiate(MethodInput Input) {
                Instance Right = Input.Arguments[0];
                return _GetResult(Input.Interpreter, Math.Pow((long)Input.Instance.Integer, (double)Right.Float), Right is IntegerInstance);
            }
            public static async Task<Instance> _UnaryPlus(MethodInput Input) {
                return Input.Instance;
            }
            public static async Task<Instance> _UnaryMinus(MethodInput Input) {
                return Input.Interpreter.GetInteger(-Input.Instance.Integer);
            }
            public static async Task<Instance> to_i(MethodInput Input) {
                return Input.Instance;
            }
            public static async Task<Instance> to_f(MethodInput Input) {
                return Input.Interpreter.GetFloat(Input.Instance.Float);
            }
            public static async Task<Instance> clamp(MethodInput Input) {
                DynInteger Number = Input.Instance.Integer;
                Instance Min = Input.Arguments[0];
                Instance Max = Input.Arguments[1];
                if ((DynFloat)Number < Min.Float) {
                    if (Min is IntegerInstance)
                        return Input.Interpreter.GetInteger(Min.Integer);
                    else
                        return Input.Interpreter.GetFloat(Min.Float);
                }
                else if ((DynFloat)Number > Max.Float) {
                    if (Max is IntegerInstance)
                        return Input.Interpreter.GetInteger(Max.Integer);
                    else
                        return Input.Interpreter.GetFloat(Max.Float);
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
                                await Input.OnYield.Call(Input.Script, null, Input.Interpreter.GetInteger(i), BreakHandleType: BreakHandleType.Rethrow, CatchReturn: false);
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
                return Input.Interpreter.Nil;
            }
        }
        static class Float {
            public static async Task<Instance> _Add(MethodInput Input) {
                Instance Right = Input.Arguments[0];
                return Input.Interpreter.GetFloat(Input.Instance.Float + Right.Float);
            }
            public static async Task<Instance> _Subtract(MethodInput Input) {
                Instance Right = Input.Arguments[0];
                return Input.Interpreter.GetFloat(Input.Instance.Float - Right.Float);
            }
            public static async Task<Instance> _Multiply(MethodInput Input) {
                Instance Right = Input.Arguments[0];
                return Input.Interpreter.GetFloat(Input.Instance.Float * Right.Float);
            }
            public static async Task<Instance> _Divide(MethodInput Input) {
                Instance Right = Input.Arguments[0];
                return Input.Interpreter.GetFloat(Input.Instance.Float / Right.Float);
            }
            public static async Task<Instance> _Modulo(MethodInput Input) {
                Instance Right = Input.Arguments[0];
                return Input.Interpreter.GetFloat(Input.Instance.Float % Right.Float);
            }
            public static async Task<Instance> _Exponentiate(MethodInput Input) {
                Instance Right = Input.Arguments[0];
                return Input.Interpreter.GetFloat(Math.Pow((double)Input.Instance.Float, (double)Right.Float));
            }
            public static async Task<Instance> _Equals(MethodInput Input) {
                Instance Left = Input.Instance;
                Instance Right = Input.Arguments[0];
                if ((Right is IntegerInstance or FloatInstance) && Left.Float == Right.Float) {
                    return Input.Interpreter.True;
                }
                else {
                    return Input.Interpreter.False;
                }
            }
            public static async Task<Instance> _LessThan(MethodInput Input) {
                Instance Left = Input.Instance;
                Instance Right = Input.Arguments[0];
                if ((Right is IntegerInstance or FloatInstance) && Left.Float < Right.Float) {
                    return Input.Interpreter.True;
                }
                else {
                    return Input.Interpreter.False;
                }
            }
            public static async Task<Instance> _GreaterThan(MethodInput Input) {
                Instance Left = Input.Instance;
                Instance Right = Input.Arguments[0];
                if ((Right is IntegerInstance or FloatInstance) && Left.Float > Right.Float) {
                    return Input.Interpreter.True;
                }
                else {
                    return Input.Interpreter.False;
                }
            }
            public static async Task<Instance> _LessThanOrEqualTo(MethodInput Input) {
                Instance Left = Input.Instance;
                Instance Right = Input.Arguments[0];
                if ((Right is IntegerInstance or FloatInstance) && Left.Float <= Right.Float) {
                    return Input.Interpreter.True;
                }
                else {
                    return Input.Interpreter.False;
                }
            }
            public static async Task<Instance> _GreaterThanOrEqualTo(MethodInput Input) {
                Instance Left = Input.Instance;
                Instance Right = Input.Arguments[0];
                if ((Right is IntegerInstance or FloatInstance) && Left.Float >= Right.Float) {
                    return Input.Interpreter.True;
                }
                else {
                    return Input.Interpreter.False;
                }
            }
            public static async Task<Instance> _Spaceship(MethodInput Input) {
                if ((await _LessThan(Input)).IsTruthy) {
                    return Input.Interpreter.GetInteger(-1);
                }
                else if ((await _Equals(Input)).IsTruthy) {
                    return Input.Interpreter.GetInteger(0);
                }
                else if ((await _GreaterThan(Input)).IsTruthy) {
                    return Input.Interpreter.GetInteger(1);
                }
                return Input.Interpreter.Nil;
            }
            public static async Task<Instance> _UnaryPlus(MethodInput Input) {
                return Input.Instance;
            }
            public static async Task<Instance> _UnaryMinus(MethodInput Input) {
                return Input.Interpreter.GetFloat(-Input.Instance.Float);
            }
            public static async Task<Instance> to_i(MethodInput Input) {
                return Input.Interpreter.GetInteger(Input.Instance.Integer);
            }
            public static async Task<Instance> to_f(MethodInput Input) {
                return Input.Instance;
            }
            public static async Task<Instance> clamp(MethodInput Input) {
                DynFloat Number = Input.Instance.Float;
                DynFloat Min = Input.Arguments[0].Float;
                DynFloat Max = Input.Arguments[1].Float;
                if (Number < Min) {
                    return Input.Interpreter.GetFloat(Min);
                }
                else if (Number > Max) {
                    return Input.Interpreter.GetFloat(Max);
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
                    DecimalPlaces = (int)Math.Min((long)Input.Arguments[0].Integer, 15);
                // Round
                double Result;
                if (DecimalPlaces >= 0) {
                    // Round decimal places
                    Result = Math.Round(Number, DecimalPlaces);
                }
                else {
                    // Round digits before dot
                    double Factor = Math.Pow(10, -DecimalPlaces);
                    Result = Math.Round(Number / Factor) * Factor;
                }
                long ResultAsLong = (long)Result;
                // Return result
                if (Result == ResultAsLong) {
                    return Input.Interpreter.GetInteger(ResultAsLong);
                }
                else {
                    return Input.Interpreter.GetFloat(Result);
                }
            }
            public static async Task<Instance> floor(MethodInput Input) {
                long Result = (long)Math.Floor((double)Input.Instance.Float);
                return Input.Interpreter.GetInteger(Result);
            }
            public static async Task<Instance> ceil(MethodInput Input) {
                long Result = (long)Math.Ceiling((double)Input.Instance.Float);
                return Input.Interpreter.GetInteger(Result);
            }
            public static async Task<Instance> truncate(MethodInput Input) {
                long Result = (long)Math.Truncate((double)Input.Instance.Float);
                return Input.Interpreter.GetInteger(Result);
            }
        }
        static class _File {
            public static async Task<Instance> read(MethodInput Input) {
                string FilePath = Input.Arguments[0].String;
                try {
                    string FileContents = File.ReadAllText(FilePath);
                    return new StringInstance(Input.Interpreter.String, FileContents);
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
                    File.WriteAllText(FilePath, Text);
                    return Input.Interpreter.Nil;
                }
                catch (Exception Ex) {
                    throw new RuntimeException($"{Input.Location}: Error writing file: '{Ex.Message}'");
                }
            }
            public static async Task<Instance> append(MethodInput Input) {
                string FilePath = Input.Arguments[0].String;
                string Text = Input.Arguments[1].String;
                try {
                    File.AppendAllText(FilePath, Text);
                    return Input.Interpreter.Nil;
                }
                catch (Exception Ex) {
                    throw new RuntimeException($"{Input.Location}: Error appending file: '{Ex.Message}'");
                }
            }
            public static async Task<Instance> delete(MethodInput Input) {
                string FilePath = Input.Arguments[0].String;
                File.Delete(FilePath);
                return Input.Interpreter.Nil;
            }
            public static async Task<Instance> exist7(MethodInput Input) {
                string FilePath = Input.Arguments[0].String;
                bool Exists = File.Exists(FilePath);
                return Exists ? Input.Interpreter.True : Input.Interpreter.False;
            }
            public static async Task<Instance> absolute_path(MethodInput Input) {
                string FilePath = Input.Arguments[0].String;
                string FullFilePath = Path.GetFullPath(FilePath);
                return new StringInstance(Input.Interpreter.String, FullFilePath);
            }
            public static async Task<Instance> absolute_path7(MethodInput Input) {
                string FilePath = Input.Arguments[0].String;
                string FullFilePath = Path.GetFullPath(FilePath);
                return FilePath == FullFilePath ? Input.Interpreter.True : Input.Interpreter.False;
            }
            public static async Task<Instance> basename(MethodInput Input) {
                string FilePath = Input.Arguments[0].String;
                string FileName = Path.GetFileName(FilePath);
                return new StringInstance(Input.Interpreter.String, FileName);
            }
            public static async Task<Instance> dirname(MethodInput Input) {
                string FilePath = Input.Arguments[0].String;
                string DirectoryName = Path.GetDirectoryName(FilePath) ?? "";
                return new StringInstance(Input.Interpreter.String, DirectoryName);
            }
        }
        static class Proc {
            public static async Task<Instance> call(MethodInput Input) {
                return await Input.Instance.Proc.Call(Input.Script, null, Input.Arguments, Input.OnYield);
            }
        }
        static class Range {
            public static async Task<Instance> _TripleEquals(MethodInput Input) {
                IntegerRange Range = Input.Instance.Range;
                Instance Value = Input.Arguments[0];
                if (Value is IntegerInstance or FloatInstance) {
                    return Range.IsInRange(Value.Float) ? Input.Interpreter.True : Input.Interpreter.False;
                }
                else {
                    return Input.Interpreter.False;
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
                                await Input.OnYield.Call(Input.Script, null, Input.Interpreter.GetInteger(i), BreakHandleType: BreakHandleType.Rethrow, CatchReturn: false);
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
                return Input.Interpreter.Nil;
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
                                await Input.OnYield.Call(Input.Script, null, Input.Interpreter.GetInteger(i), BreakHandleType: BreakHandleType.Rethrow, CatchReturn: false);
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
                return Input.Interpreter.Nil;
            }
            public static async Task<Instance> to_a(MethodInput Input) {
                List<Instance> Array = new();
                IntegerRange Range = Input.Instance.Range;
                long Min = (long)(Range.Min != null ? Range.Min : 0);
                long Max = (long)(Range.Max != null ? Range.Max : throw new RuntimeException($"{Input.Location}: Cannot call 'to_a' on range if max is endless"));
                for (long i = Min; i <= Max; i++) {
                    Array.Add(Input.Interpreter.GetInteger(i));
                }
                return new ArrayInstance(Input.Interpreter.Array, Array);
            }
            public static async Task<Instance> length(MethodInput Input) {
                IntegerRange Range = Input.Instance.Range;
                long? Count = Range.Count;
                if (Count != null) {
                    return Input.Interpreter.GetInteger(Count.Value);
                }
                else {
                    return Input.Interpreter.Nil;
                }
            }
        }
        static class Array {
            private static async Task<Instance> _GetIndex(MethodInput Input, int ArrayIndex) {
                Instance Index = Input.Interpreter.GetInteger(ArrayIndex);
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

                    return new ArrayInstance(Input.Interpreter.Array, Array.GetIndexRange(StartIndex, EndIndex));
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
                        return Input.Interpreter.Nil;
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
                        Array.EnsureArrayIndex(Input.Interpreter, Index);
                        return Array[Index] = Value;
                    }
                }
                else {
                    lock (Array) {
                        Array.EnsureArrayIndex(Input.Interpreter, Index);
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
                return new ArrayInstance(Input.Interpreter.Array, NewArray);
            }
            public static async Task<Instance> _Equals(MethodInput Input) {
                Instance Left = Input.Instance;
                Instance Right = Input.Arguments[0];
                if (Right is ArrayInstance) {
                    int Count = Left.Array.Count;
                    if (Count != Right.Array.Count) return Input.Interpreter.False;

                    for (int i = 0; i < Left.Array.Count; i++) {
                        bool ValuesEqual = (await Left.Array[i].CallInstanceMethod(Input.Script, "==", Right.Array[i])).IsTruthy;
                        if (!ValuesEqual) return Input.Interpreter.False;
                    }
                    return Input.Interpreter.True;
                }
                else {
                    return Input.Interpreter.False;
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
                return Input.Interpreter.GetInteger(Items.Count);
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
                    return Input.Interpreter.GetInteger(Count);
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
                    return Input.Interpreter.Nil;
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
                    return Input.Interpreter.Nil;
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
                    return Input.Interpreter.Nil;
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
                                await Input.OnYield.Call(Input.Script, null, new List<Instance>() { Array[i], Input.Interpreter.GetInteger(i) }, BreakHandleType: BreakHandleType.Rethrow, CatchReturn: false);
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
                return Input.Interpreter.Nil;
            }
            public static async Task<Instance> reverse_each(MethodInput Input) {
                if (Input.OnYield != null) {
                    List<Instance> Array = Input.Instance.Array;
                    
                    int TakesArguments = Input.OnYield.ArgumentNames.Count;
                    for (int i = Array.Count - 1; i >= 0; i--) {
                        try {
                            // x.reverse_each do |n, i|
                            if (TakesArguments == 2) {
                                await Input.OnYield.Call(Input.Script, null, new List<Instance>() { Array[i], Input.Interpreter.GetInteger(i) }, BreakHandleType: BreakHandleType.Rethrow, CatchReturn: false);
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
                return Input.Interpreter.Nil;
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
                        return new ArrayInstance(Input.Interpreter.Array, MappedArray);
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
                    return new ArrayInstance(Input.Interpreter.Array, Array);
                }
            }
            public static async Task<Instance> sort(MethodInput Input) => await _sort(Input, false);
            public static async Task<Instance> sort1(MethodInput Input) => await _sort(Input, true);
            public static async Task<Instance> include7(MethodInput Input) {
                Instance ItemToFind = Input.Arguments[0];
                foreach (Instance Item in Input.Instance.Array) {
                    if ((await Item.InstanceMethods["=="].Call(Input.Script, Item, ItemToFind)).IsTruthy) {
                        return Input.Interpreter.True;
                    }
                }
                return Input.Interpreter.False;
            }
            public static async Task<Instance> delete(MethodInput Input) {
                List<Instance> Array = Input.Instance.Array;
                Instance DeleteItem = Input.Arguments[0];
                Instance LastDeletedItem = Input.Interpreter.Nil;
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
                    return Input.Interpreter.Nil;
                }
            }
            public static async Task<Instance> clear(MethodInput Input) {
                Input.Instance.Array.Clear();
                return Input.Interpreter.Nil;
            }
            public static async Task<Instance> empty7(MethodInput Input) {
                return Input.Instance.Array.Count == 0 ? Input.Interpreter.True : Input.Interpreter.False;
            }
            public static async Task<Instance> reverse(MethodInput Input) {
                List<Instance> ReversedArray = new(Input.Instance.Array);
                ReversedArray.Reverse();
                return new ArrayInstance(Input.Interpreter.Array, ReversedArray);
            }
            public static async Task<Instance> reverse1(MethodInput Input) {
                Input.Instance.Array.Reverse();
                return Input.Instance;
            }
        }
        static class Hash {
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
                            if (!KeysEqual) return Input.Interpreter.False;
                            bool ValuesEqual = (await LeftKeyValue.Value.CallInstanceMethod(Input.Script, "==", RightKeyValue.Value)).IsTruthy;
                            if (!ValuesEqual) return Input.Interpreter.False;
                        }
                    }
                    return Input.Interpreter.True;
                }
                else {
                    return Input.Interpreter.False;
                }
            }
            public static async Task<Instance> initialize(MethodInput Input) {
                if (Input.Arguments.Count == 1) {
                    ((HashInstance)Input.Instance).SetValue(Input.Instance.Hash, Input.Arguments[0]);
                }
                return Input.Interpreter.Nil;
            }
            public static async Task<Instance> has_key7(MethodInput Input) {
                Instance ItemToFind = Input.Arguments[0];
                Instance? Found = await Input.Instance.Hash.Lookup(Input.Script, ItemToFind);
                return Found != null ? Input.Interpreter.True : Input.Interpreter.False;
            }
            public static async Task<Instance> has_value7(MethodInput Input) {
                Instance ItemToFind = Input.Arguments[0];
                Instance? Found = await Input.Instance.Hash.ReverseLookup(Input.Script, ItemToFind);
                return Found != null ? Input.Interpreter.True : Input.Interpreter.False;
            }
            public static async Task<Instance> keys(MethodInput Input) {
                return new ArrayInstance(Input.Interpreter.Array, Input.Instance.Hash.Keys);
            }
            public static async Task<Instance> values(MethodInput Input) {
                return new ArrayInstance(Input.Interpreter.Array, Input.Instance.Hash.Values);
            }
            public static async Task<Instance> delete(MethodInput Input) {
                HashDictionary Hash = Input.Instance.Hash;
                Instance Key = Input.Arguments[0];
                return await Hash.Remove(Input.Script, Key) ?? Input.Interpreter.Nil;
            }
            public static async Task<Instance> clear(MethodInput Input) {
                Input.Instance.Hash.Clear();
                return Input.Interpreter.Nil;
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
                return Input.Interpreter.Nil;
            }
            public static async Task<Instance> invert(MethodInput Input) {
                HashInstance Hash = (HashInstance)Input.Instance;
                HashDictionary Inverted = new();
                foreach (KeyValuePair<Instance, Instance> Match in Hash.Hash.KeyValues) {
                    await Inverted.Store(Input.Script, Match.Value, Match.Key);
                }
                return new HashInstance(Input.Interpreter.Hash, Inverted, Hash.DefaultValue);
            }
            public static async Task<Instance> to_a(MethodInput Input) {
                List<Instance> Array = new();
                foreach (KeyValuePair<Instance, Instance> Item in Input.Instance.Hash.KeyValues) {
                    Array.Add(new ArrayInstance(Input.Interpreter.Array, new List<Instance>() { Item.Key, Item.Value }));
                }
                return new ArrayInstance(Input.Interpreter.Array, Array);
            }
            public static async Task<Instance> to_hash(MethodInput Input) {
                return Input.Instance;
            }
            public static async Task<Instance> empty7(MethodInput Input) {
                return Input.Instance.Hash.Count == 0 ? Input.Interpreter.True : Input.Interpreter.False;
            }
        }
        static class _Random {
            public static async Task<Instance> rand(MethodInput Input) {
                // Integer random
                if (Input.Arguments.Count == 1 && Input.Arguments[0] is IntegerInstance Integer) {
                    long IncludingMin = 0;
                    long ExcludingMax = (long)Integer.Integer;
                    long RandomNumber = Input.Interpreter.Random.NextInt64(IncludingMin, ExcludingMax);
                    return Input.Interpreter.GetInteger(RandomNumber);
                }
                // Range random
                else if (Input.Arguments.Count == 1 && Input.Arguments[0] is RangeInstance Range) {
                    long IncludingMin = (long)Range.AppliedMin.Integer;
                    long IncludingMax = (long)Range.AppliedMax.Integer;
                    long RandomNumber = Input.Interpreter.Random.NextInt64(IncludingMin, IncludingMax + 1);
                    return Input.Interpreter.GetInteger(RandomNumber);
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
                    return Input.Interpreter.GetFloat(RandomNumber);
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

                return Input.Interpreter.GetInteger(PreviousSeed);
            }
        }
        static class _Math {
            public static async Task<Instance> sin(MethodInput Input) {
                return Input.Interpreter.GetFloat(Math.Sin((double)Input.Arguments[0].Float));
            }
            public static async Task<Instance> cos(MethodInput Input) {
                return Input.Interpreter.GetFloat(Math.Cos((double)Input.Arguments[0].Float));
            }
            public static async Task<Instance> tan(MethodInput Input) {
                return Input.Interpreter.GetFloat(Math.Tan((double)Input.Arguments[0].Float));
            }
            public static async Task<Instance> asin(MethodInput Input) {
                return Input.Interpreter.GetFloat(Math.Asin((double)Input.Arguments[0].Float));
            }
            public static async Task<Instance> acos(MethodInput Input) {
                return Input.Interpreter.GetFloat(Math.Acos((double)Input.Arguments[0].Float));
            }
            public static async Task<Instance> atan(MethodInput Input) {
                return Input.Interpreter.GetFloat(Math.Atan((double)Input.Arguments[0].Float));
            }
            public static async Task<Instance> atan2(MethodInput Input) {
                return Input.Interpreter.GetFloat(Math.Atan2((double)Input.Arguments[0].Float, (double)Input.Arguments[1].Float));
            }
            public static async Task<Instance> sinh(MethodInput Input) {
                return Input.Interpreter.GetFloat(Math.Sinh((double)Input.Arguments[0].Float));
            }
            public static async Task<Instance> cosh(MethodInput Input) {
                return Input.Interpreter.GetFloat(Math.Cosh((double)Input.Arguments[0].Float));
            }
            public static async Task<Instance> tanh(MethodInput Input) {
                return Input.Interpreter.GetFloat(Math.Tanh((double)Input.Arguments[0].Float));
            }
            public static async Task<Instance> asinh(MethodInput Input) {
                return Input.Interpreter.GetFloat(Math.Asinh((double)Input.Arguments[0].Float));
            }
            public static async Task<Instance> acosh(MethodInput Input) {
                return Input.Interpreter.GetFloat(Math.Acosh((double)Input.Arguments[0].Float));
            }
            public static async Task<Instance> atanh(MethodInput Input) {
                return Input.Interpreter.GetFloat(Math.Atanh((double)Input.Arguments[0].Float));
            }
            public static async Task<Instance> exp(MethodInput Input) {
                return Input.Interpreter.GetFloat(Math.Exp((double)Input.Arguments[0].Float));
            }
            public static async Task<Instance> log(MethodInput Input) {
                return Input.Interpreter.GetFloat(Math.Log((double)Input.Arguments[0].Float, (double)Input.Arguments[1].Float));
            }
            public static async Task<Instance> log10(MethodInput Input) {
                return Input.Interpreter.GetFloat(Math.Log10((double)Input.Arguments[0].Float));
            }
            public static async Task<Instance> log2(MethodInput Input) {
                return Input.Interpreter.GetFloat(Math.Log((double)Input.Arguments[0].Float, 2));
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
                    return new ArrayInstance(Input.Interpreter.Array, new List<Instance>() {
                        Input.Interpreter.GetFloat(0),
                        Input.Interpreter.GetInteger(0)
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
                    Input.Interpreter.GetFloat(M),
                    Input.Interpreter.GetInteger(E)
                });
            }
            public static async Task<Instance> ldexp(MethodInput Input) {
                double Fraction = (double)Input.Arguments[0].Float;
                long Exponent = (long)Input.Arguments[1].Integer;
                return Input.Interpreter.GetFloat(Fraction * Math.Pow(2, Exponent));
            }
            public static async Task<Instance> sqrt(MethodInput Input) {
                return Input.Interpreter.GetFloat(Math.Sqrt((double)Input.Arguments[0].Float));
            }
            public static async Task<Instance> cbrt(MethodInput Input) {
                return Input.Interpreter.GetFloat(Math.Cbrt((double)Input.Arguments[0].Float));
            }
            public static async Task<Instance> hypot(MethodInput Input) {
                double A = (double)Input.Arguments[0].Float;
                double B = (double)Input.Arguments[1].Float;
                return Input.Interpreter.GetFloat(Math.Sqrt(Math.Pow(A, 2) + Math.Pow(B, 2)));
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
                x = Math.Abs(x);

                // A&S formula 7.1.26
                double t = 1.0 / (1.0 + p * x);
                double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x);

                return sign * y;
            }
            public static async Task<Instance> erf(MethodInput Input) {
                return Input.Interpreter.GetFloat(_Erf((double)Input.Arguments[0].Float));
            }
            public static async Task<Instance> erfc(MethodInput Input) {
                return Input.Interpreter.GetFloat(1.0 - _Erf((double)Input.Arguments[0].Float));
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
            public static async Task<Instance> gamma(MethodInput Input) {
                return Input.Interpreter.GetFloat(_Gamma((double)Input.Arguments[0].Float));
            }
            public static async Task<Instance> lgamma(MethodInput Input) {
                double Value = (double)Input.Arguments[0].Float;
                double GammaValue = _Gamma(Value);
                double A = Math.Log(Math.Abs(GammaValue));
                long B = GammaValue < 0 ? -1 : 1;
                return new ArrayInstance(Input.Interpreter.Array, new List<Instance>() {
                    Input.Interpreter.GetFloat(A),
                    Input.Interpreter.GetInteger(B)
                });
            }
            public static async Task<Instance> to_rad(MethodInput Input) {
                double Degrees = (double)Input.Arguments[0].Float;
                return Input.Interpreter.GetFloat(Degrees * (Math.PI / 180));
            }
            public static async Task<Instance> to_deg(MethodInput Input) {
                double Radians = (double)Input.Arguments[0].Float;
                return Input.Interpreter.GetFloat(Radians / (Math.PI / 180));
            }
            public static async Task<Instance> lerp(MethodInput Input) {
                DynFloat A = Input.Arguments[0].Float;
                DynFloat B = Input.Arguments[1].Float;
                DynFloat T = Input.Arguments[2].Float;
                return Input.Interpreter.GetFloat(A * (1 - T) + (B * T));
            }
        }
        static class _Exception {
            public static async Task<Instance> initialize(MethodInput Input) {
                if (Input.Arguments.Count == 1) {
                    ((ExceptionInstance)Input.Instance).SetValue(Input.Arguments[0].String);
                }
                return Input.Interpreter.Nil;
            }
            public static async Task<Instance> message(MethodInput Input) {
                return new StringInstance(Input.Interpreter.String, Input.Instance.Exception.Message);
            }
            public static async Task<Instance> backtrace(MethodInput Input) {
                static string SimplifyStackTrace(string StackTrace) {
                    const string Pattern = @"[A-Za-z]:[\\\/]\S+"; // Match file paths
                    return System.Text.RegularExpressions.Regex.Replace(StackTrace, Pattern, FilePath => Path.GetFileName(FilePath.Value)); // Shorten file paths
                }
                return new StringInstance(Input.Interpreter.String, SimplifyStackTrace(Input.Instance.Exception.StackTrace ?? ""));
            }
        }
        static class _Thread {
            public static async Task<Instance> initialize(MethodInput Input) {
                Method? OnYield = Input.OnYield ?? throw new RuntimeException($"{Input.Location}: No block given for Thread.new");
                
                ThreadInstance Thread = (ThreadInstance)Input.Instance;
                Thread.SetMethod(OnYield);
                _ = Thread.Thread.Run(Input.Arguments, Input.OnYield);

                return Input.Interpreter.Nil;
            }
            public static async Task<Instance> join(MethodInput Input) {
                ThreadInstance Thread = (ThreadInstance)Input.Instance;
                await Thread.Thread.Run(OnYield: Input.OnYield);
                return Input.Interpreter.Nil;
            }
            public static async Task<Instance> stop(MethodInput Input) {
                ThreadInstance Thread = (ThreadInstance)Input.Instance;
                Thread.Thread.Stop();
                return Input.Interpreter.Nil;
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
                            ThreadInstance Thread = new(Input.Interpreter.Thread, Input.Script);
                            Thread.Thread.Method = Input.OnYield;

                            // Parallel.each do |n, i|
                            if (TakesArguments == 2) {
                                await Thread.Thread.Run(new List<Instance>() { Current, Input.Interpreter.GetInteger(CurrentIndex) }, Input.OnYield);
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

                    Parallel.Invoke(Methods);
                }
                return Input.Interpreter.Nil;
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
                            ThreadInstance Thread = new(Input.Interpreter.Thread, Input.Script);
                            Thread.Thread.Method = Input.OnYield;

                            // Parallel.times do |n|
                            if (TakesArguments == 1) {
                                await Thread.Thread.Run(Input.Interpreter.GetInteger(CurrentIndex), Input.OnYield);
                            }
                            // Parallel.times do
                            else {
                                await Thread.Thread.Run(OnYield: Input.OnYield);
                            }
                        };
                        Counter++;
                    }

                    Parallel.Invoke(Methods);
                }
                return Input.Interpreter.Nil;
            }
        }
        static class Time {
            public static async Task<Instance> now(MethodInput Input) {
                return new TimeInstance(Input.Interpreter.Time, DateTime.Now);
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

                return Input.Interpreter.Nil;
            }
            public static async Task<Instance> to_i(MethodInput Input) {
                return Input.Interpreter.GetInteger(Input.Instance.Time.ToUnixTimeSeconds());
            }
            public static async Task<Instance> to_f(MethodInput Input) {
                return Input.Interpreter.GetFloat(Input.Instance.Time.ToUnixTimeSecondsDouble());
            }
            public static async Task<Instance> at(MethodInput Input) {
                double Seconds = (double)Input.Arguments[0].Float;
                long TruncatedSeconds = (long)Seconds;

                DateTimeOffset DateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(TruncatedSeconds);
                TimeSpan TimeZoneOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow);
                DateTimeOffset = DateTimeOffset.ToOffset(TimeZoneOffset);
                DateTime Time = DateTimeOffset.DateTime;
                Time = Time.AddSeconds(Seconds - TruncatedSeconds);

                return new TimeInstance(Input.Interpreter.Time, Time);
            }
        }
        static class Net {
            public static class HTTP {
                public static async Task<Instance> get(MethodInput Input) {
                    Uri Uri = new(Input.Arguments[0].String, UriKind.RelativeOrAbsolute);
                    if (!Uri.IsAbsoluteUri) Uri = new("https://" + Uri.OriginalString);

                    HttpClient Client = new();
                    HttpResponseMessage Response = await Client.GetAsync(Uri);
                    string ResponseString = await Response.Content.ReadAsStringAsync();

                    return new StringInstance(Input.Interpreter.String, ResponseString);
                }
            }
        }
        static class _Class {
            public static async Task<Instance> _TripleEquals(MethodInput Input) {
                Instance Left = Input.Instance;
                Instance Right = Input.Arguments[0];
                return Right.Module!.InheritsFrom(Left.Module) ? Input.Interpreter.True : Input.Interpreter.False;
            }
            public static async Task<Instance> name(MethodInput Input) {
                return new StringInstance(Input.Interpreter.String, Input.Instance.Module!.Name);
            }
        }
    }
}
