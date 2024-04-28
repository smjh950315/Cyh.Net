namespace Cyh.Net.Reflection
{
    public class RunTimeObject : System.Dynamic.DynamicObject
    {
        private System.Collections.Generic.Dictionary<string, object?>? _PROPERTIES;
        private System.Collections.Generic.Dictionary<string, object?> _Properties {
            get {
                this._PROPERTIES ??= new System.Collections.Generic.Dictionary<string, object?>();
                return this._PROPERTIES;
            }
        }

        public static dynamic GetRuntimeObject() => new RunTimeObject();

        public object? this[string name] {
            get {
                if (!this._Properties.TryGetValue(name, out object? result)) {
                    return null;
                } else {
                    return result;
                }
            }
            set {
                this._Properties[name] = value;
            }
        }

        public RunTimeObject() { }

        public override bool TryGetMember(System.Dynamic.GetMemberBinder binder, out object? result) {
            if (!this._Properties.TryGetValue(binder.Name, out result)) { result = null; }
            return true;
        }

        public override bool TrySetMember(System.Dynamic.SetMemberBinder binder, object? value) {
            this._Properties[binder.Name] = value;
            return true;
        }
    }
};