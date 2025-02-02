using Cyh.Net.Reflection;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Cyh.Net
{
    public interface IQueryableHelper<TSource>
    {
        IQueryable<TResult> GetResultQueryable<TResult>(string mark);
    }
    public class QueryableHelper
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

        static QueryableHelper()
        {
            MethodInfo? methodForSelectorExpr = typeof(Expression).GetMethod("Lambda", 1, [typeof(Expression), typeof(bool), typeof(IEnumerable<ParameterExpression>)]);
            Debug.Assert(methodForSelectorExpr != null);
            ExpressionLambda__Expr_Bool_ParamExprEnumerable__ = methodForSelectorExpr;
            CachedMapAttribute = new();
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
        static bool AddAssignment(ref List<MemberAssignment> assignments, ref List<PropertyInfo> unusedTargetProperties, Expression parameter, PropertyInfo dstProperty, Type sourceType, string mark)
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
                    PropertyInfo[] sourceProperties_ = sourceType_.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                    bool hasFound = AddAssignment(ref assignments, ref unusedTargetProperties, parameter_, dstProperty, sourceType_, mark);
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
                    MemberExpression parameter_ = Expression.Property(parameter, sourceProperty);
                    assignments.Add(Expression.Bind(dstProperty, parameter_));
                    unusedTargetProperties.Remove(dstProperty);
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static object CreateMappingExpressionOfComplexType(Type sourceType, Type targetType, string mark)
        {
            PropertyInfo[] targetProps = targetType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            ParameterExpression parameter = Expression.Parameter(sourceType, "x");
            List<MemberAssignment> assignments = [];
            List<PropertyInfo> setDefaultProperties = targetProps.ToList();
            for (int i = 0; i < targetProps.Length; i++)
            {
                PropertyInfo targetProperty = targetProps[i];
                AddAssignment(ref assignments, ref setDefaultProperties, parameter, targetProperty, sourceType, mark);
            }
            GetSetDefaultMemberAssignment(ref assignments, setDefaultProperties);
            Type funcType = typeof(Func<,>).MakeGenericType(sourceType, targetType);
            NewExpression newExpression = Expression.New(targetType);
            MemberInitExpression memberInit = Expression.MemberInit(newExpression, assignments);
            return ExpressionLambda__Expr_Bool_ParamExprEnumerable__.MakeGenericMethod(funcType).Invoke(null, [memberInit, false, new ParameterExpression[] { parameter }])!;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Expression<Func<TSource, TResult>> CreateMappingExpressionOfComplexType<TSource, TResult>(string mark)
        {
            return (Expression<Func<TSource, TResult>>)CreateMappingExpressionOfComplexType(typeof(TSource), typeof(TResult), mark);
        }

        public static IQueryableHelper<TSource> GetQueryableHelper<TSource>(IQueryable<TSource> sources) => new QueryableHelper<TSource>(sources);
    }

    internal interface IQueryableHelperImpl
    {
        public abstract object GetExpressionByMark(string mark);
    }
    internal class QueryableHelperImpl<TSource, TResult> : QueryableHelper, IQueryableHelperImpl
    {
        static readonly Dictionary<string, Expression<Func<TSource, TResult>>> _cacheExpressions;
        static QueryableHelperImpl()
        {
            _cacheExpressions = [];
        }
        public object GetExpressionByMark(string mark)
        {
            if (!_cacheExpressions.TryGetValue(mark, out Expression<Func<TSource, TResult>>? expr))
            {
                expr = CreateMappingExpressionOfComplexType<TSource, TResult>(mark);
                _cacheExpressions[mark] = expr;
            }
            return expr;
        }
    }
    internal class QueryableHelper<TSource> : QueryableHelper, IQueryableHelper<TSource>
    {
        static readonly Dictionary<Type, IQueryableHelperImpl> _cacheImplementations;
        static QueryableHelper()
        {
            _cacheImplementations = [];
        }
        internal IQueryable<TSource> _source;
        internal QueryableHelper(IQueryable<TSource> source) => this._source = source;
        public IQueryable<TResult> GetResultQueryable<TResult>(string mark)
        {
            if (!_cacheImplementations.TryGetValue(typeof(TResult), out IQueryableHelperImpl? impl))
            {
                impl = new QueryableHelperImpl<TSource, TResult>();
                _cacheImplementations[typeof(TResult)] = impl;
            }
            Expression<Func<TSource, TResult>> expr = (Expression<Func<TSource, TResult>>)impl.GetExpressionByMark(mark);
            return _source.Select(expr);
        }
    }
}
