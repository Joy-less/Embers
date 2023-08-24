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
                {"sleep", new Method(Sleep, 0..1)}
            };
            return Methods;
        }

        async Task<Instance> Puts(Interpreter Interpreter, InstanceOrBlock InstanceOrBlock, List<Instance> Messages) {
            foreach (Instance Message in Messages) {
                Console.WriteLine(Message.Object);
            }
            return Interpreter.Nil;
        }
        async Task<Instance> Print(Interpreter Interpreter, InstanceOrBlock InstanceOrBlock, List<Instance> Messages) {
            foreach (Instance Message in Messages) {
                Console.Write(Message.Object);
            }
            return Interpreter.Nil;
        }
        async Task<Instance> P(Interpreter Interpreter, InstanceOrBlock InstanceOrBlock, List<Instance> Messages) {
            foreach (Instance Message in Messages) {
                Console.WriteLine(Message.Inspect());
            }
            return Interpreter.Nil;
        }
        async Task<Instance> Sleep(Interpreter Interpreter, InstanceOrBlock InstanceOrBlock, List<Instance> Time) {
            if (Time.Count == 1) {
                await Task.Delay(1);
            }
            else {
                await Task.Delay(Timeout.Infinite);
            }
            return Interpreter.Nil;
        }
    }
}
