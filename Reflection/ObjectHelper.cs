using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Cyh.Net.Reflection {
    public static class ObjectHelper {
        class _struct<T> where T : struct { }
        class _unmanaged<T> where T : unmanaged { }
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
                throw new InvalidOperationException("nuknow member information");
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

        /// <summary>
        /// Get value of member named <paramref name="name"/> in <paramref name="obj"/>
        /// </summary>
        public static bool TryGetMember(object? obj, string name, out object? output) {
            if (name.IsNullOrEmpty() || obj == null) {
                output = null;
                return false;
            } else {
                MemberInfo? mi = GetMemberInfo(obj.GetType(), name);
                try {
                    if (mi != null) {
                        output = GetValue(obj, mi);
                        return true;
                    } else {
                        output = null;
                        return false;
                    }
                } catch {
                    output = null;
                    return false;
                }
            }
        }

        /// <summary>
        /// Set value of member named <paramref name="name"/> in <paramref name="obj"/>
        /// </summary>
        public static bool TrySetMember(object? obj, string name, object? input) {
            if (name.IsNullOrEmpty() || obj == null) {
                return false;
            } else {
                MemberInfo? mi = GetMemberInfo(obj.GetType(), name);
                try {
                    if (mi != null) {
                        SetValue(obj, input, mi);
                        return true;
                    } else {
                        return false;
                    }
                } catch {
                    return false;
                }
            }
        }

        /// <summary>
        /// Get value of member match the <paramref name="info"/> in <paramref name="obj"/>
        /// </summary>
        public static bool TryGetMember(object? obj, MemberInfo? info, out object? output) {
            output = null;
            if (obj == null || info == null) { return false; }
            try {
                if (info != null) {
                    output = GetValue(obj, info);
                    return true;
                } else {
                    return false;
                }
            } catch {
                return false;
            }
        }

        /// <summary>
        /// Set value of member match the <paramref name="info"/> in <paramref name="obj"/>
        /// </summary>
        public static bool TrySetMember(object? obj, MemberInfo? info, object? input) {
            if (obj == null || info == null) { return false; }
            try {
                if (info != null) {
                    SetValue(obj, input, info);
                    return true;
                } else {
                    return false;
                }
            } catch {
                return false;
            }
        }

        /// <summary>
        /// Get value of static member named <paramref name="name"/> in <typeparamref name="T"/>
        /// </summary>
        public static object? GetStaticMember<T>(string name) {
            MemberInfo? member = GetStaticMemberInfo(typeof(T), name);
            if (member != null) {
                return GetValue(null, member);
            }
            return default;
        }

        /// <summary>
        /// Set value of static member named <paramref name="name"/> in <typeparamref name="T"/> to <paramref name="value"/>
        /// </summary>
        public static bool SetStaticMember<T>(string name, object? value) {
            MemberInfo? member = GetStaticMemberInfo(typeof(T), name);
            if (member != null) {
                SetValue(null, value, member);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Indicate whether exist a static member named <paramref name="name"/> in <typeparamref name="T"/>
        /// </summary>
        public static bool HasStaticMember<T>(string name) {
            return GetStaticMemberInfo(typeof(T), name) != null;
        }

        /// <summary>
        /// Get the member informations with custom attribute of <paramref name="attrType"/>
        /// </summary>
        public static IEnumerable<PropertyInfo> GetPropertiesWithCustomAttribute(Type type, Type attrType) {
            return type.GetProperties().Where(p => p.IsDefined(attrType));
        }

        /// <summary>
        /// Get the member informations with custom attribute of <typeparamref name="T"/>
        /// </summary>
        public static IEnumerable<PropertyInfo> GetPropertiesWithCustomAttribute<T>(Type type) {
            return type.GetProperties().Where(p => p.IsDefined(typeof(T)));
        }

        public static bool IsStruct(Type type) {
            try {
                typeof(_struct<>).MakeGenericType(type);
                return true;
            } catch {
                return false;
            }
        }

        public static bool IsUnmanaged(Type type) {
            try {
                typeof(_unmanaged<>).MakeGenericType(type);
                return true;
            } catch {
                return false;
            }
        }

        public static bool IsStruct<T>() => IsStruct(typeof(T));

        public static bool IsUnmanaged<T>() => IsUnmanaged(typeof(T));


        public static bool ConstructBy<T>([NotNullWhen(true)] out T? output, params object[] args) {
            try {
                Type[] types = new Type[args.Length];

                for (int i = 0; i < args.Length; i++) {
#pragma warning disable CS8602
                    types[i] = args[i].GetType();
#pragma warning restore CS8602
                }

                var constructor = typeof(T).GetConstructor(types);

                output = (T?)constructor?.Invoke(args) ?? default;

            } catch { output = default; }

            return false;
        }

        public static T? ConstructBy<T>(params object[] args) {
            ConstructBy(out T? result, args);
            return result;
        }
    }
}
