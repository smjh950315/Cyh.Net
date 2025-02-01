namespace Cyh.Net.Reflection
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class MapFromAttribute : Attribute
    {
        /// <summary>
        /// 是否唯讀
        /// </summary>
        public bool IsSourceReadOnly { get; }
        /// <summary>
        /// 是否唯讀
        /// </summary>
        public bool IsTargetReadOnly { get; }
        /// <summary>
        /// 特殊標示
        /// </summary>
        public string Mark { get; }
        /// <summary>
        /// 來源型別
        /// </summary>
        public virtual Type? SourceType { get; }

        /// <summary>
        /// 目標欄位名稱
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

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class MapFromAttribute<TSource> : MapFromAttribute
    {
        /// <summary>
        /// 來源型別
        /// </summary>
        public override Type? SourceType => typeof(TSource);

        public MapFromAttribute(string sourcePropertyName, string mark = "", bool isSourceReadOnly = false, bool isTargetReadOnly = false) : base(sourcePropertyName, mark, isSourceReadOnly, isTargetReadOnly)
        {

        }
    }
}
