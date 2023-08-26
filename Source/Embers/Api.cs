using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Embers.Interpreter;

#pragma warning disable CS1998

namespace Embers
{
    public class Api
    {
        public Dictionary<string, Method> GetBuiltInMethods() {
            Dictionary<string, Method> Methods = new() {
                {"puts", new Method(Puts, null)},
                {"print", new Method(Print, null)},
                {"p", new Method(P, null)},
                {"warn", new Method(Warn, null)},
                {"sleep", new Method(Sleep, 0..1)}
            };
            return Methods;
        }

        async Task<Instance> Puts(MethodInput Input) {
            foreach (Instance Message in Input.Arguments) {
                Console.WriteLine(Message.Object);
            }
            return Input.Interpreter.Nil;
        }
        async Task<Instance> Print(MethodInput Input) {
            foreach (Instance Message in Input.Arguments) {
                Console.Write(Message.Object);
            }
            return Input.Interpreter.Nil;
        }
        async Task<Instance> P(MethodInput Input) {
            foreach (Instance Message in Input.Arguments) {
                Console.WriteLine(Message.Inspect());
            }
            return Input.Interpreter.Nil;
        }
        async Task<Instance> Warn(MethodInput Input) {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            foreach (Instance Message in Input.Arguments) {
                Console.WriteLine(Message.Object);
            }
            Console.ResetColor();
            return Input.Interpreter.Nil;
        }
        async Task<Instance> Sleep(MethodInput Input) {
            if (Input.Arguments.Count == 1) {
                await Task.Delay(1);
            }
            else {
                await Task.Delay(Timeout.Infinite);
            }
            return Input.Interpreter.Nil;
        }
    }
}
