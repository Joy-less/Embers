using static Embers.Script;

namespace Embers
{
    public class Interpreter
    {
        public readonly Class RootModule;
        public readonly Instance RootInstance;
        public readonly Scope RootScope;

        public readonly Dictionary<string, Instance> GlobalVariables = new();
        public readonly Dictionary<string, SymbolInstance> Symbols = new();

        public readonly Class NilClass;
        public readonly Class TrueClass;
        public readonly Class FalseClass;
        public readonly Class String;
        public readonly Class Symbol;
        public readonly Class Integer;
        public readonly Class Float;
        public readonly Class Proc;
        public readonly Class Array;

        public readonly NilInstance Nil;
        public readonly TrueInstance True;
        public readonly FalseInstance False;

        public readonly Random InternalRandom = new();
        public long RandomSeed;
        public Random Random;

        public Interpreter() {
            RootModule = new("main", null);
            RootInstance = new Instance(RootModule);
            RootScope = new Scope(RootInstance);

            Script MainScript = new(this);

            NilClass = MainScript.CreateClass("NilClass"); Nil = new NilInstance(NilClass);
            TrueClass = MainScript.CreateClass("TrueClass"); True = new TrueInstance(TrueClass);
            FalseClass = MainScript.CreateClass("FalseClass"); False = new FalseInstance(FalseClass);
            String = MainScript.CreateClass("String");
            Symbol = MainScript.CreateClass("Symbol");
            Integer = MainScript.CreateClass("Integer");
            Float = MainScript.CreateClass("Float");
            Proc = MainScript.CreateClass("Proc");
            Array = MainScript.CreateClass("Array");

            RandomSeed = InternalRandom.NextInt64();
            Random = new Random(RandomSeed.GetHashCode());

            Api.Setup(MainScript);
        }
    }
}
