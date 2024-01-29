namespace Embers {
    public sealed class CodeLocation {
        public readonly Axis Axis;
        public readonly int Line = 1;
        public readonly int Column = 0;
        public readonly bool IsUnknown = false;
        public CodeLocation(Axis axis, int line, int column) {
            Axis = axis;
            Line = line;
            Column = column;
        }
        public CodeLocation(Axis axis) {
            Axis = axis;
            IsUnknown = true;
        }
        public override string ToString() {
            return IsUnknown ? "?" : (Axis.Options.ShowErrorColumn ? $"{Line}:{Column}" : $"{Line}");
        }
        public string Serialise() {
            return IsUnknown
                ? "new CodeLocation()"
                : $"new CodeLocation(Axis, {Line}, {Column})";
        }
        public static readonly CodeLocation Invalid = new(null!);
    }
}
