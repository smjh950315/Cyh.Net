namespace Cyh.Net.Data.Logs {
    public enum NEXT_ACTION {
        /// <summary>
        /// 照常執行
        /// </summary>
        NORM,

        /// <summary>
        /// 略過該筆
        /// </summary>
        PASS,

        /// <summary>
        /// 中止交易
        /// </summary>
        HALT
    }
}
