using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static Embers.Phase2;

#nullable enable

namespace Embers
{
    public sealed class Interpreter {
        public readonly Api Api;

        /// <summary>Object is the superclass of all classes and modules.</summary>
        public readonly Module Object;
        /// <summary>Class is the class of all classes and modules.</summary>
        public readonly Class Class;

        public readonly LockingDictionary<string, Instance> GlobalVariables = new();
        public readonly WeakCache<string, SymbolInstance> Symbols = new();

        internal readonly ConditionalWeakTable<Exception, ExceptionInstance> ExceptionsTable = new();
        public readonly Random InternalRandom = new();
        public long RandomSeed;
        public Random Random;

        public long NewObjectId() => NextObjectId++;
        private long NextObjectId = 0;

        public static string Serialise(string Code) {
            List<Phase1.Phase1Token> Tokens = Phase1.GetPhase1Tokens(Code);
            List<Expression> Statements = ObjectsToExpressions(Tokens, ExpressionsType.Statements);

            return Statements.Serialise();
        }

        public Interpreter(Scope RootScope) {
            Object = new Module("Object", this, null);
            Class = new Class("Class", this, Object);

            RandomSeed = InternalRandom.NextInt64();
            Random = new Random(RandomSeed.GetHashCode());

            RootScope.SetInterpreter(this);
            RootScope.CurrentModule = new Module("main", this, Object);
            RootScope.CurrentModule.Constants["Object"] = new ModuleReference(Object);
            RootScope.CurrentModule.Constants["Class"] = new ModuleReference(Class);
            RootScope.CurrentInstance = new ModuleReference(RootScope.CurrentModule);

            Api = new Api(RootScope);
        }
    }
}
