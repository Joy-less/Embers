namespace Embers {
    public sealed class ControlCode : Instance {
        public readonly ControlType Type;
        public readonly CodeLocation Location;
        public readonly Instance? Argument;
        public ControlCode(Axis axis, CodeLocation location, ControlType type, Instance? argument) : base(axis.ControlCode, argument?.Value) {
            Type = type;
            Location = location;
            Argument = argument;
        }
        public override string ToString() {
            string String = $"{Type}".ToLower();
            if (Argument is not null) {
                String += $" {Argument}";
            }
            return String;
        }
    }
}