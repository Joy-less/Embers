using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Embers
{
    public class SyntaxErrorException : Exception {
        public SyntaxErrorException(string Message) : base(Message) { }
    }
    public class InternalErrorException : Exception {
        public InternalErrorException(string Message) : base(Message) { }
    }
    public class ScriptErrorException : Exception {
        public ScriptErrorException(string Message) : base(Message) { }
    }
    public class ApiException : Exception {
        public ApiException(string Message) : base(Message) { }
    }
}
