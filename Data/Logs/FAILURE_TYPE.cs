namespace Cyh.Net.Data.Logs {
    public enum FAILURE_TYPE {
        /// <summary>
        /// No error happend.
        /// </summary>
        NONE = 0b0000_0000,

        /// <summary>
        /// Error caused by not invoked.
        /// </summary>
        NOT_INVOKED = 0b0000_0010,

        /// <summary>
        /// Error caused by not saved.
        /// </summary>
        NOT_SAVED = 0b0000_0100,

        /// <summary>
        /// Error caused by null data input.
        /// </summary>
        INV_DATA = 0b0001_0000,

        /// <summary>
        /// Error caused by invalid convert process.
        /// </summary>
        INV_CONV = 0b0010_0000,

        /// <summary>
        /// Error caused by invalid data source.
        /// </summary>
        INV_SRCS = 0b0100_0000,

        /// <summary>
        /// Error caused by unknow reason.
        /// </summary>
        UNKNOWN = 0b1000_0000,

        /// <summary>
        /// [Do not call directly](Only used on database) Error caused by database roll back action.
        /// </summary>
        ROLL_BACK = 0b00000001_00000000
    }

    public static class LogFlagExtends {
        private static int InnerJoin(this FAILURE_TYPE reason, int mask) {
            return (int)reason & mask;
        }

        /// <summary>
        /// Indicate weather the failure reason is ignorable.  
        /// </summary>
        public static bool Ignorable(this FAILURE_TYPE reason) {
            return reason.InnerJoin(0b0000_1111 | (int)FAILURE_TYPE.INV_DATA) != 0;
        }
    }
}
