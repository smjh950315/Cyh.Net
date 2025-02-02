using System.Reflection;

namespace Cyh.Net.Reflection
{
    public class MemberBindingFlags
    {
        public const BindingFlags StaticMethod = BindingFlags.Static | BindingFlags.InvokeMethod;
        public const BindingFlags StaticMember = BindingFlags.Static;
        public const BindingFlags InstanceMethod = BindingFlags.Instance | BindingFlags.InvokeMethod;
        public const BindingFlags InstanceMember = BindingFlags.Instance;
        public const BindingFlags Accessable_Public = BindingFlags.Public;
        public const BindingFlags Accessable_NoPublic = BindingFlags.NonPublic;
        public const BindingFlags Accessable_All = BindingFlags.Public | BindingFlags.NonPublic;
    }
}
