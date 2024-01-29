namespace Embers {
    public abstract class RubyObject {
        public readonly CodeLocation Location;
        public Axis Axis => Location.Axis;
        public RubyObject(CodeLocation location) {
            Location = location;
        }
        public abstract override string ToString();
    }
}
