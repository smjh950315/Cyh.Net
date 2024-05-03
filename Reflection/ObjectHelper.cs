using System.Reflection;

namespace Cyh.Net.Reflection {
    public static class ObjectHelper {
        private static object? GetValue(object? instance, MemberInfo memberInfo) {
            if (memberInfo is PropertyInfo propertyInfo) {
                return propertyInfo.GetValue(instance);
            } else if (memberInfo is FieldInfo fieldInfo) {
                return fieldInfo.GetValue(instance);
            } else {
                return null;
            }
        }
        private static void SetValue(object? instance, object? value, MemberInfo memberInfo) {
            if (memberInfo is PropertyInfo propertyInfo) {
                propertyInfo.SetValue(instance, value);
            } else if (memberInfo is FieldInfo fieldInfo) {
                fieldInfo.SetValue(instance, value);
            } else {
            }
        }
        private static MemberInfo[] GetMemberInfos(Type type, BindingFlags bindingFlags = MemberBindingFlags.InstanceMember | MemberBindingFlags.Accessable_All) {
            return type.GetMembers(bindingFlags);
        }
        private static MemberInfo? GetMemberInfo(Type type, string name, BindingFlags bindingFlags = MemberBindingFlags.InstanceMember | MemberBindingFlags.Accessable_All) {
            return type.GetMember(name, bindingFlags).FirstOrDefault();
        }
        private static MemberInfo? GetStaticMemberInfo(Type type, string name) {
            return GetMemberInfo(type, name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        }
        public static bool TryGetMember(object? obj, string name, out object? value) {
            if (name.IsNullOrEmpty() || obj == null) {
                value = null;
                return false;
            } else {
                MemberInfo? mi = GetMemberInfo(obj.GetType(), name);
                try {
                    if (mi != null) {
                        value = GetValue(obj, mi);
                        return true;
                    } else {
                        value = null;
                        return false;
                    }
                } catch {
                    value = null;
                    return false;
                }
            }
        }
        public static bool TrySetMember(object? obj, string name, object? value) {
            if (name.IsNullOrEmpty() || obj == null) {
                return false;
            } else {
                MemberInfo? mi = GetMemberInfo(obj.GetType(), name);
                try {
                    if (mi != null) {
                        SetValue(obj, value, mi);
                        return true;
                    } else {
                        return false;
                    }
                } catch {
                    return false;
                }
            }
        }
        public static object? GetStaticMember<T>(string name) {
            MemberInfo? member = GetStaticMemberInfo(typeof(T), name);
            if (member != null) {
                return GetValue(null, member);
            }
            return default;
        }
        public static bool SetStaticMember<T>(string name, object? value) {
            MemberInfo? member = GetStaticMemberInfo(typeof(T), name);
            if (member != null) {
                SetValue(null, value, member);
                return true;
            }
            return false;
        }
        public static bool HasStaticMember<T>(string name) {
            return GetStaticMemberInfo(typeof(T), name) != null;
        }
        public static IEnumerable<PropertyInfo> GetPropertiesOfCustomAttribute(Type type, Type attrType) {
            return type.GetProperties().Where(p => p.IsDefined(attrType));
        }
    }
}
