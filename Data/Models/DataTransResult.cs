using Cyh.Net.Data.Logs;
using System.Runtime.CompilerServices;

namespace Cyh.Net.Data.Models
{
    /// <summary>
    /// Results of data transaction.
    /// </summary>
    public class DataTransResult
    {
        private List<TransactionDetail>? _Details;

        private void OnSucceess() {
            if (this.IsFinished) { return; }
            this.EndTime = DateTime.Now;
            this.IsFinished = true;
            if (this._Details != null) {
                foreach (TransactionDetail detail in this._Details) {
                    detail.IsSaved = true;
                    detail.Saved = this.EndTime;
                    detail.FailedReason = FAILURE_REASON.NONE;
                }
            }
        }
        private void OnFail() {
            if (this.IsFinished) { return; }
            this.EndTime = DateTime.Now;
            this.IsFinished = true;
            if (this._Details != null) {
                if (this.UseRollback) {
                    foreach (TransactionDetail detail in this._Details) {
                        if (detail.FailedReason == FAILURE_REASON.NOT_SAVED) {
                            detail.FailedReason = FAILURE_REASON.ROLL_BACK;
                        }
                    }
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
        /// <param name="reason"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int OnTransact(FAILURE_REASON reason = FAILURE_REASON.NOT_SAVED, string? message = null) {
            int index = this.TotalCount;
            this.Details.Add(new TransactionDetail
            {
                Index = index,
                IsInvoked = true,
                Invoked = DateTime.Now,
                FailedReason = reason,
                Message = message,
            });
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