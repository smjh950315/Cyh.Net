using Cyh.Net.Data.Logs;

namespace Cyh.Net.Data.Models {
    public class TransactionDetail {
        internal TransactionDetail() { }

        /// <summary>
        /// Index of data in the transaction sequence.
        /// </summary>
        public int Index { get; internal set; }

        /// <summary>
        /// Whether transaction is been proccessed.
        /// </summary>
        public bool IsProccessed => this.Proccessed < DateTime.Now;

        /// <summary>
        /// Whether transaction is launched and saved without error.
        /// </summary>
        public bool IsSucceed => this.FailedReason == FAILURE_TYPE.NONE;

        /// <summary>
        /// The time when transaction is added to queue.
        /// </summary>
        public DateTime Proccessed { get; internal set; } = DateTime.MinValue;

        /// <summary>
        /// The time when transaction is launched.
        /// </summary>
        public DateTime Succeed { get; internal set; } = DateTime.MinValue;

        /// <summary>
        /// The reason why transaction is failed.
        /// <para>Without the rollback machanism, 
        /// this value will be marked FAILURE_TYPE.NONE as a succeed result,
        /// otherwise it will marked as FAILURE.* to represent a specific failure reason.</para>
        /// 
        /// <para>With the rollback machanism, 
        /// this value will be marked FAILURE.NOT_SAVED if it is added to the queue without error and
        /// marked FAILURE_TYPE.NONE when current batch of transaction is saved successfully,
        /// otherwise marked FAILURE_TYPE.ROLL_BACK to represnet a whole batch transaction is been rollback.
        /// </para>
        /// </summary>
        public FAILURE_TYPE FailedReason { get; internal set; } = FAILURE_TYPE.NOT_INVOKED;

        /// <summary>
        /// The additional message from transaction process
        /// </summary>
        public string? Message { get; internal set; }
    }
    internal static class TransDetailExtends {
        internal static TransactionDetail OnProcess(this TransactionDetail detail, FAILURE_TYPE type) {

            detail.Proccessed = DateTime.Now;

            detail.FailedReason = type;
            if (type == FAILURE_TYPE.NONE) {
                detail.Succeed = detail.Proccessed;
            } else {
                detail.Succeed = DateTime.MinValue;
            }

            return detail;
        }
    }
}