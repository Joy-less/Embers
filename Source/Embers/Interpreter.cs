using System;
using System.Collections.Generic;
using static Embers.Phase2;
using static Embers.Script;
using static Embers.Api;
using static Embers.SpecialTypes;

namespace Embers
{
    public class Interpreter
    {
        public readonly Api Api;

        /// <summary>Object is the superclass of all classes and modules.</summary>
        public readonly Module Object;
        /// <summary>Class is the class of all classes and modules.</summary>
        public readonly Class Class;

        public readonly Module RootModule;
        public readonly Instance RootInstance;
        public readonly Scope RootScope;

        public readonly LockingDictionary<string, Instance> GlobalVariables = new();
        public readonly WeakCache<string, SymbolInstance> Symbols = new();

        public readonly Random InternalRandom = new();
        public long RandomSeed;
        public Random Random;
        public long GenerateObjectId() => NextObjectId++;
        private long NextObjectId = 0;

        public static string Serialise(string Code) {
            List<Phase1.Phase1Token> Tokens = Phase1.GetPhase1Tokens(Code);
            List<Expression> Statements = ObjectsToExpressions(Tokens, ExpressionsType.Statements);

            return Statements.Serialise();
        }

        public Interpreter() {
            Object = new Module("Object", this); Object.InstanceMethods.Remove("initialize"); Object.Methods.Remove("new");
            RootModule = new Module("main", this, Object);
            Class = new Class("Class", RootModule, Object); RootModule.Constants["Class"] = new ModuleReference(Class);
            RootInstance = new ModuleReference(RootModule);
            RootScope = new Scope();

            Api = new Api(this);

            RandomSeed = InternalRandom.NextInt64();
            Random = new Random(RandomSeed.GetHashCode());
        }
    }
}
