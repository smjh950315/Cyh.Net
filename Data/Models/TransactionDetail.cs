using Cyh.Net.Data.Logs;

namespace Cyh.Net.Data.Models
{
    public class TransactionDetail
    {
        internal TransactionDetail() { }

        /// <summary>
        /// Index of data in the transaction sequence.
        /// </summary>
        public int Index { get; internal set; }

        /// <summary>
        /// Whether transaction is launched without error.
        /// </summary>
        public bool IsInvoked { get; internal set; } = false;

        /// <summary>
        /// Whether transaction is saved without error.
        /// </summary>
        public bool IsSaved { get; internal set; } = false;

        /// <summary>
        /// Whether transaction is launched and saved without error.
        /// </summary>
        public bool IsSucceed { get => this.IsInvoked && this.IsSaved; }

        /// <summary>
        /// The time when transaction is added to queue.
        /// </summary>
        public DateTime Invoked { get; internal set; } = DateTime.MinValue;

        /// <summary>
        /// The time when transaction is launched.
        /// </summary>
        public DateTime Saved { get; internal set; } = DateTime.MinValue;

        /// <summary>
        /// The additional message from transaction process
        /// </summary>
        public string? Message { get; internal set; }

        /// <summary>
        /// The reason why transaction is failed.
        /// </summary>
        public FAILURE_REASON FailedReason { get; internal set; } = FAILURE_REASON.NOT_INVOKED;
    }
}