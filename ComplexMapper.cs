using Cyh.Net.Reflection;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Cyh.Net
{
    public interface IComplexMapper<TSource>
    {
        IQueryable<TResult> GetResultQueryable<TResult>(string mark);
        TResult GetResult<TResult>(TSource source, string mark);
        void SetResult<TResult>(TSource source, TResult result, string mark);
    }
    public static class ComplexMapper
    {
        internal static readonly Type[] BuiltinDataTypes =
        [
            typeof(string),
            typeof(int),
            typeof(int?),
            typeof(long),
            typeof(long?),
            typeof(float),
            typeof(float?),
            typeof(double),
            typeof(double?),
            typeof(decimal),
            typeof(decimal?),
            typeof(DateTime),
            typeof(DateTime?)
        ];
        internal static readonly MethodInfo ExpressionLambda__Expr_Bool_ParamExprEnumerable__;
        internal static readonly MethodInfo GetSetter__;
        internal static readonly MethodInfo GetGetter__;
        static readonly Dictionary<PropertyInfo, IEnumerable<MapFromAttribute>> CachedMapAttribute;
        static IEnumerable<MapFromAttribute> GetMapFromAttributes(PropertyInfo propertyInfo)
        {
            if (!CachedMapAttribute.TryGetValue(propertyInfo, out IEnumerable<MapFromAttribute>? values))
            {
                values = propertyInfo.GetCustomAttributes<MapFromAttribute>();
                CachedMapAttribute[propertyInfo] = values;
            }
            return values;
        }

        static ComplexMapper()
        {
            MethodInfo? methodForSelectorExpr = typeof(Expression).GetMethod("Lambda", 1, [typeof(Expression), typeof(bool), typeof(IEnumerable<ParameterExpression>)]);
            Debug.Assert(methodForSelectorExpr != null);
            ExpressionLambda__Expr_Bool_ParamExprEnumerable__ = methodForSelectorExpr;
            CachedMapAttribute = new();
            GetSetter__ = typeof(ComplexMapper).GetMethod("Impl_GetSetter", BindingFlags.Static | BindingFlags.NonPublic)!;
            GetGetter__ = typeof(ComplexMapper).GetMethod("Impl_GetGetter", BindingFlags.Static | BindingFlags.NonPublic)!;
            Debug.Assert(GetSetter__ != null);
            Debug.Assert(GetGetter__ != null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void GetSetDefaultMemberAssignment(ref List<MemberAssignment> assignments, List<PropertyInfo> setDefaultProperties)
        {
            for (int i = 0; i < setDefaultProperties.Count; ++i)
            {
                PropertyInfo setDefaultProperty = setDefaultProperties[i];
                Type unusedPropertyType = setDefaultProperty.PropertyType;
                Expression defaultExpression;
                if (unusedPropertyType.IsValueType)
                {
                    ObjectHelper.ConstructBy(unusedPropertyType, out object? inst);
                    defaultExpression = Expression.Constant(inst, unusedPropertyType);
                }
                else
                {
                    defaultExpression = Expression.Constant(null, unusedPropertyType);
                }
                assignments.Add(Expression.Bind(setDefaultProperty, defaultExpression));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool AddAssignment(ref List<MemberAssignment> assignments, ref List<Action<object?, object?>> actions, Func<object?, object?>? srcGetter, ref List<PropertyInfo> unusedTargetProperties, Expression parameter, PropertyInfo dstProperty, Type sourceType, string mark)
        {
            if (sourceType.IsBuiltinType()) return false;
            PropertyInfo[] sourceProperties = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < sourceProperties.Length; i++)
            {
                PropertyInfo sourceProperty = sourceProperties[i];
                Type sourceType_ = sourceProperty.PropertyType;
                MapFromAttribute? mapFromAttribute = null;

                IEnumerable<MapFromAttribute> mapFromAttributes = GetMapFromAttributes(dstProperty);
                if (mapFromAttributes.Any())
                {
                    mapFromAttribute = mapFromAttributes.FirstOrDefault(x => x.SourceType == sourceType && x.SourcePropertyName == sourceProperty.Name && !x.IsTargetReadOnly);
                    mapFromAttribute ??= mapFromAttributes.FirstOrDefault(x => x.SourceType == null && x.SourcePropertyName == sourceProperty.Name && !x.IsTargetReadOnly);
                }
                if (mapFromAttribute == null)
                {
                    MemberExpression parameter_ = Expression.Property(parameter, sourceProperty);
                    bool hasFound = AddAssignment(ref assignments, ref actions, sourceProperty.GetValue, ref unusedTargetProperties, parameter_, dstProperty, sourceType_, mark);
                    if (hasFound) return true;
                }
                else
                {
                    IEnumerable<string> marks = mapFromAttribute.Mark?.Split(',')?.Select(x => x.Trim())?.Where(x => !x.IsNullOrEmpty()) ?? [];
                    if (marks.Any())
                    {
                        if (!marks.Contains(mark))
                        {
                            return true;
                        }
                    }
                    Action<object?, object?> action = (src, dst) =>
                    {
                        object? src_ = srcGetter == null ? src : srcGetter(src);
                        dstProperty.SetValueEx(dst, sourceProperty.GetValue(src_));
                    };
                    actions.Add(action);
                    MemberExpression parameter_ = Expression.Property(parameter, sourceProperty);
                    assignments.Add(Expression.Bind(dstProperty, parameter_));
                    unusedTargetProperties.Remove(dstProperty);
                    return true;
                }
            }

            return false;
        }

        class MappingInformations
        {
            public required object Expression { get; set; }
            public required object Getter { get; set; }
            public required object Setter { get; set; }
        }

        static Action<TSource, TResult> Impl_GetSetter<TSource, TResult>(List<Action<object?, object?>> actions)
        {
            return (src, dst) =>
            {
                foreach (Action<object?, object?> action in actions)
                {
                    action(src, dst);
                }
            };
        }
        static Func<TSource, TResult?> Impl_GetGetter<TSource, TResult>(List<Action<object?, object?>> actions)
        {
            return (src) =>
            {
                if (!ObjectHelper.ConstructBy(out TResult? dst)) return default;
                foreach (Action<object?, object?> action in actions)
                {
                    action(src, dst);
                }
                return dst;
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static MappingInformations CreateMappingInformationsOfComplexType(Type sourceType, Type targetType, string mark)
        {
            PropertyInfo[] targetProps = targetType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            ParameterExpression parameter = Expression.Parameter(sourceType, "x");
            List<MemberAssignment> assignments = [];
            List<PropertyInfo> setDefaultProperties = targetProps.ToList();
            List<Action<object?, object?>> actions = [];
            for (int i = 0; i < targetProps.Length; i++)
            {
                PropertyInfo targetProperty = targetProps[i];
                AddAssignment(ref assignments, ref actions, null, ref setDefaultProperties, parameter, targetProperty, sourceType, mark);
            }
            GetSetDefaultMemberAssignment(ref assignments, setDefaultProperties);
            Type funcType = typeof(Func<,>).MakeGenericType(sourceType, targetType);
            NewExpression newExpression = Expression.New(targetType);
            MemberInitExpression memberInit = Expression.MemberInit(newExpression, assignments);
            object expr = ExpressionLambda__Expr_Bool_ParamExprEnumerable__.MakeGenericMethod(funcType).Invoke(null, [memberInit, false, new ParameterExpression[] { parameter }])!;
            object? getter = GetGetter__.MakeGenericMethod(sourceType, targetType).Invoke(null, [actions]);
            object? setter = GetSetter__.MakeGenericMethod(sourceType, targetType).Invoke(null, [actions]);
            Debug.Assert(setter != null);
            Debug.Assert(getter != null);
            return new MappingInformations()
            {
                Expression = expr,
                Getter = getter,
                Setter = setter
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Expression<Func<TSource, TResult>> CreateMappingExpressionOfComplexType<TSource, TResult>(string mark)
        {
            return (Expression<Func<TSource, TResult>>)CreateMappingInformationsOfComplexType(typeof(TSource), typeof(TResult), mark).Expression;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Func<TSource, TResult> CreateMappingGetterOfComplexType<TSource, TResult>(string mark)
        {
            return (Func<TSource, TResult>)CreateMappingInformationsOfComplexType(typeof(TSource), typeof(TResult), mark).Getter;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Action<TSource, TResult> CreateMappingSetterOfComplexType<TSource, TResult>(string mark)
        {
            return (Action<TSource, TResult>)CreateMappingInformationsOfComplexType(typeof(TSource), typeof(TResult), mark).Setter;
        }
        public static IComplexMapper<TSource> GetComplexMapper<TSource>(IQueryable<TSource> sources) => new ComplexMapper<TSource>(sources);
    }

    internal interface IComplexMapperImpl
    {
        public object GetExpressionByMark(string mark);
        public object GetGetterByMark(string mark);
        public object GetSetterByMark(string mark);
    }
    internal class ComplexMapperImpl<TSource, TResult> : IComplexMapperImpl
    {
        static readonly Dictionary<string, Expression<Func<TSource, TResult>>> _cacheExpressions;
        static readonly Dictionary<string, Func<TSource, TResult>> _cacheGetters;
        static readonly Dictionary<string, Action<TSource, TResult>> _cacheSetters;

        static ComplexMapperImpl()
        {
            _cacheExpressions = [];
            _cacheGetters = [];
            _cacheSetters = [];
        }
        public object GetExpressionByMark(string mark)
        {
            if (!_cacheExpressions.TryGetValue(mark, out Expression<Func<TSource, TResult>>? expr))
            {
                expr = ComplexMapper.CreateMappingExpressionOfComplexType<TSource, TResult>(mark);
                _cacheExpressions[mark] = expr;
            }
            return expr;
        }

        public object GetGetterByMark(string mark)
        {
            if (!_cacheGetters.TryGetValue(mark, out var expr))
            {
                expr = ComplexMapper.CreateMappingGetterOfComplexType<TSource, TResult>(mark);
                _cacheGetters[mark] = expr;
            }
            return expr;
        }

        public object GetSetterByMark(string mark)
        {
            if (!_cacheSetters.TryGetValue(mark, out var expr))
            {
                expr = ComplexMapper.CreateMappingSetterOfComplexType<TSource, TResult>(mark);
                _cacheSetters[mark] = expr;
            }
            return expr;
        }
    }
    internal class ComplexMapper<TSource> : IComplexMapper<TSource>
    {
        static readonly Dictionary<Type, IComplexMapperImpl> _cacheImplementations;
        static ComplexMapper()
        {
            _cacheImplementations = [];
        }
        internal IQueryable<TSource> _source;
        internal ComplexMapper(IQueryable<TSource> source) => this._source = source;
        public IQueryable<TResult> GetResultQueryable<TResult>(string mark)
        {
            if (!_cacheImplementations.TryGetValue(typeof(TResult), out IComplexMapperImpl? impl))
            {
                impl = new ComplexMapperImpl<TSource, TResult>();
                _cacheImplementations[typeof(TResult)] = impl;
            }
            Expression<Func<TSource, TResult>> expr = (Expression<Func<TSource, TResult>>)impl.GetExpressionByMark(mark);
            return _source.Select(expr);
        }

        public TResult GetResult<TResult>(TSource source, string mark)
        {
            if (!_cacheImplementations.TryGetValue(typeof(TResult), out IComplexMapperImpl? impl))
            {
                impl = new ComplexMapperImpl<TSource, TResult>();
                _cacheImplementations[typeof(TResult)] = impl;
            }
            var expr = (Func<TSource, TResult>)impl.GetGetterByMark(mark);
            return expr(source);
        }

        public void SetResult<TResult>(TSource source, TResult result, string mark)
        {
            if (!_cacheImplementations.TryGetValue(typeof(TResult), out IComplexMapperImpl? impl))
            {
                impl = new ComplexMapperImpl<TSource, TResult>();
                _cacheImplementations[typeof(TResult)] = impl;
            }
            var expr = (Action<TSource, TResult>)impl.GetSetterByMark(mark);
            expr(source, result);
        }
    }
}
