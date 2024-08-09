namespace Cyh.Net {
    public static class CyhTypeExtensions {
        /// <summary>
        /// Set message of current <see cref="IExecuteResult"/> instance if not null
        /// </summary>
        /// <param name="message">Message to write into the <see cref="IExecuteResult"/> instance</param>
        public static void TrySetMessage(this IExecuteResult? result, string? message) {
            if (result == null) { return; }
            result.Message = message;
        }

        /// <summary>
        /// Indicate whether current <see cref="IExecuteResult"/> instance has finished without error if not null
        /// </summary>
        public static bool IsSucceed(this IExecuteResult? result) {
            if (result == null) { return false; }
            return result.Executed != null && result.Finished != null;
        }

        /// <summary>
        /// Set current <see cref="IExecuteResult"/> instance 's state into begin
        /// </summary>
        public static void OnBegin(this IExecuteResult? result) {
            if (result == null) { return; }
            result.Executed = DateTime.Now;
        }

        /// <summary>
        /// Set current <see cref="IExecuteResult"/> instance 's state into finished
        /// </summary>
        public static void OnFinish(this IExecuteResult? result) {
            if (result == null) { return; }
            if (result.Executed == null) { return; }
            result.Finished = DateTime.Now;
        }
    }
}
