using ObjectDict = System.Collections.Generic.Dictionary<string, object?>;
namespace Cyh.Net.Reflection {
    public class RunTimeObject : System.Dynamic.DynamicObject {

        private ObjectDict? m_proterpties;

        public static dynamic GetRuntimeObject() => new RunTimeObject();

        public object? this[string name] {
            get {
                if (name.IsNullOrEmpty()) { return null; }
                if (this.m_proterpties.IsNullOrEmpty()) { return null; }
                return this.m_proterpties.TryGetValue(name, out object? result) ? result : null;
            }
            set {
                if (!name.IsNullOrEmpty()) {
                    this.m_proterpties ??= new ObjectDict();
                    this.m_proterpties[name] = value;
                }
            }
        }

        public RunTimeObject() { }

        public override bool TryGetMember(System.Dynamic.GetMemberBinder binder, out object? result) {
            if (!this.m_proterpties.IsNullOrEmpty()) {
                this.m_proterpties.TryGetValue(binder.Name, out result);
            } else {
                result = null;
            }
            return true;
        }

        public override bool TrySetMember(System.Dynamic.SetMemberBinder binder, object? value) {
            this[binder.Name] = value;
            return true;
        }
    }
};