namespace Cyh.Net {
    /// <summary>
    /// 執行結果
    /// </summary>
    public partial interface IExecuteResult {
        /// <summary>
        /// 代表執行結果的代號
        /// </summary>
        int ResultCode { get; set; }
        /// <summary>
        /// 執行的時間
        /// </summary>
        DateTime? Executed { get; set; }
        /// <summary>
        /// 完成的時間
        /// </summary>
        DateTime? Finished { get; set; }
        /// <summary>
        /// 執行過程返回的訊息
        /// </summary>
        string? Message { get; set; }
    }
}
