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
        /// <summary>Object is the superclass of all classes and modules.</summary>
        public readonly Module Object;
        /// <summary>Class is the class of all classes and modules.</summary>
        public readonly Class Class;

        public readonly Module RootModule;
        public readonly Instance RootInstance;
        public readonly Scope RootScope;

        public readonly LockingDictionary<string, Instance> GlobalVariables = new();
        public readonly Cache<string, SymbolInstance> Symbols = new();
        public readonly Cache<DynInteger, IntegerInstance> Integers = new();
        public readonly Cache<DynFloat, FloatInstance> Floats = new();

        public readonly Api Api;

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

            /*NilClass = MainScript.CreateClass("NilClass"); Nil = new NilInstance(NilClass); NilClass.InstanceMethods.Remove("initialize"); NilClass.Methods.Remove("new");
            TrueClass = MainScript.CreateClass("TrueClass"); True = new TrueInstance(TrueClass); TrueClass.InstanceMethods.Remove("initialize"); TrueClass.Methods.Remove("new");
            FalseClass = MainScript.CreateClass("FalseClass"); False = new FalseInstance(FalseClass); FalseClass.InstanceMethods.Remove("initialize"); FalseClass.Methods.Remove("new");
            String = MainScript.CreateClass("String");
            Symbol = MainScript.CreateClass("Symbol"); Symbol.InstanceMethods.Remove("initialize"); Symbol.Methods.Remove("new");
            Integer = MainScript.CreateClass("Integer");
            Float = MainScript.CreateClass("Float");
            Proc = MainScript.CreateClass("Proc");
            Range = MainScript.CreateClass("Range");
            Array = MainScript.CreateClass("Array");
            Hash = MainScript.CreateClass("Hash");
            Exception = MainScript.CreateClass("Exception");
            StandardError = MainScript.CreateClass("StandardError", InheritsFrom: Exception);
            RuntimeError = MainScript.CreateClass("RuntimeError", InheritsFrom: StandardError);
            Thread = MainScript.CreateClass("Thread");
            Time = MainScript.CreateClass("Time");
            WeakRef = MainScript.CreateClass("WeakRef");*/

            RandomSeed = InternalRandom.NextInt64();
            Random = new Random(RandomSeed.GetHashCode());
        }
    }
}
