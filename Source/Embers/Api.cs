using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Embers.Interpreter;

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

        async Task<RubyObject?> Puts(Interpreter Interpreter, List<RubyObject?> Messages) {
            foreach (RubyObject? Message in Messages) {
                Console.WriteLine(Message?.Object);
            }
            return null;
        }
        async Task<RubyObject?> Print(Interpreter Interpreter, List<RubyObject?> Messages) {
            foreach (RubyObject? Message in Messages) {
                Console.Write(Message?.Object);
            }
            return null;
        }
        async Task<RubyObject?> P(Interpreter Interpreter, List<RubyObject?> Messages) {
            foreach (RubyObject? Message in Messages) {
                Console.WriteLine(Message?.Inspect());
            }
            return null;
        }
        async Task<RubyObject?> Sleep(Interpreter Interpreter, List<RubyObject?> Time) {
            if (Time.Count == 1) {
                await Task.Delay(1);
            }
            else {
                await Task.Delay(Timeout.Infinite);
            }
            return null;
        }
    }
}
