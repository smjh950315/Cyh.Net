using Cyh.Net.Data.Logs;
using System.Runtime.CompilerServices;

namespace Cyh.Net.Data.Models {
    /// <summary>
    /// Results of data transaction.
    /// </summary>
    public class DataTransResult {

        private List<TransactionDetail>? _Details;

        /// <summary>
        /// [Only use when apply rollback machanism]Change the FailedReason mark of details from NOT_SAVED to ROLL_BACK.
        /// </summary>
        private static void on_rollback(List<TransactionDetail> details) {
            foreach (TransactionDetail detail in details) {
                if (detail.FailedReason == FAILURE_TYPE.NOT_SAVED) {
                    detail.FailedReason = FAILURE_TYPE.ROLL_BACK;
                }
            }
        }

        /// <summary>
        /// [Only use when apply rollback machanism]Change the FailedReason mark of details from NOT_SAVED to NONE.
        /// </summary>
        private static void on_save(List<TransactionDetail> details, DateTime end_time) {
            foreach (TransactionDetail detail in details) {
                if (detail.FailedReason == FAILURE_TYPE.NOT_SAVED) {
                    detail.FailedReason = FAILURE_TYPE.NONE;
                }
            }
        }

        private TransactionDetail CreateDetail(string? msg = null) {
            var index = this.Details.Count;
            return new TransactionDetail {
                Index = index,
                Message = msg
            };
        }

        /// <summary>
        /// Mark all NOT_SAVED transaction to succeed state and finish the transactions.
        /// <para>This function will and should be call internally when current batch of transaction is finished.</para>
        /// </summary>
        private void OnSucceess() {
            if (this.IsFinished) { return; }
            this.EndTime = DateTime.Now;
            this.IsFinished = true;
            if (this._Details != null) {
                if (this.UseRollback) {
                    on_save(this._Details, this.EndTime);
                }
            }
        }

        /// <summary>
        /// Mark all NOT_SAVED transaction to falied state and finish the transactions.
        /// <para>This function will and should be call internally when current batch of transaction is finished.</para>
        /// </summary>
        private void OnFail() {
            if (this.IsFinished) { return; }
            this.EndTime = DateTime.Now;
            this.IsFinished = true;
            if (this._Details != null) {
                if (this.UseRollback) {
                    on_rollback(this._Details);
                }
            }
        }

        public DataTransResult() { this.BeginTime = DateTime.Now; }

        /// <summary>
        /// Undo all transactions when any failure happend. Only work while data source is database now.
        /// </summary>
        public bool UseRollback { get; set; } = true;

        /// <summary>
        /// Log a transaction to details
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int OnTransact(FAILURE_TYPE reason, string? message = null) {
            if (this.IsFinished) {
                throw new InvalidOperationException("Current transaction is finished and should not be used anymore!");
            }
            int index = this.TotalCount;
            this.CreateDetail(message).OnProcess(reason);
            if (!reason.Ignorable()) {
                this.OnFinish(false);
            }
            return index;
        }

        /// <summary>
        /// Finish the transaction.
        /// </summary>
        public void OnFinish(bool succeed) {
            if (succeed) {
                this.OnSucceess();
            } else {
                this.OnFail();
            }
        }

        /// <summary>
        /// The user who invoke the transaction.
        /// </summary>
        public string? Invoker { get; set; }

        /// <summary>
        /// Total count of transaction in this batch.
        /// </summary>
        public int TotalCount => this._Details?.Count ?? 0;

        /// <summary>
        /// Succeed count of transaction in this batch.
        /// </summary>
        public int SuccedCount => this._Details?.Count(x => x.IsSucceed) ?? 0;

        /// <summary>
        /// Whether this batch of transaction has failure.
        /// </summary>
        public bool HasFailure => this.TotalCount != this.SuccedCount;

        /// <summary>
        /// The time of transaction begin.
        /// </summary>
        public DateTime BeginTime { get; set; }

        /// <summary>
        /// The time of transaction end.
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Whether this batch of transaction is finished.
        /// </summary>
        public bool IsFinished { get; set; }

        /// <summary>
        /// Additional message generated in this batch of transactions.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Details of each transaction.
        /// </summary>
        public List<TransactionDetail> Details {
            get {
                this._Details ??= new List<TransactionDetail>();
                return this._Details;
            }
        }

        /// <summary>
        /// Serialized additional model ( optional )
        /// </summary>
        public string? SerializedModel { get; set; }
    }
}