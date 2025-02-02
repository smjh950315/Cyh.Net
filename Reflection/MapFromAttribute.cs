namespace Cyh.Net.Reflection
{
    /// <summary>
    /// Attribute used for map property value with specific name from specific source type
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class MapFromAttribute : Attribute
    {
        /// <summary>
        /// Whether the source property is readonly
        /// </summary>
        public bool IsSourceReadOnly { get; }

        /// <summary>
        /// Whether the target property is readonly
        /// </summary>
        public bool IsTargetReadOnly { get; }

        /// <summary>
        /// Customized mark, split by ","
        /// </summary>
        public string Mark { get; }

        /// <summary>
        /// Type of source
        /// </summary>
        public virtual Type? SourceType { get; }

        /// <summary>
        /// Property name of source type
        /// </summary>
        public string SourcePropertyName { get; set; }

        public MapFromAttribute(string sourcePropertyName, string mark = "", bool isSourceReadOnly = false, bool isTargetReadOnly = false)
        {
            this.SourcePropertyName = sourcePropertyName;
            this.IsSourceReadOnly = isSourceReadOnly;
            this.IsTargetReadOnly = isTargetReadOnly;
            this.Mark = mark;
        }
    }

    /// <summary>
    /// Attribute used for map property value with specific name from <typeparamref name="TSource"/> 
    /// </summary>
    /// <typeparam name="TSource">Type of source</typeparam>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class MapFromAttribute<TSource> : MapFromAttribute
    {
        /// <summary>
        /// Type of source
        /// </summary>
        public override Type? SourceType => typeof(TSource);

        public MapFromAttribute(string sourcePropertyName, string mark = "", bool isSourceReadOnly = false, bool isTargetReadOnly = false) : base(sourcePropertyName, mark, isSourceReadOnly, isTargetReadOnly)
        {

        }
    }
}
