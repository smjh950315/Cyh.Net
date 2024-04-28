#pragma warning disable CS8604
using Cyh.Net.Data.Logs;
using Cyh.Net.Data.Models;

namespace Cyh.Net.Data
{
    public static class Pipeline
    {
        public static unsafe bool SaveResult<T>(delegate*<T, void> save, T? saver, DataTransResult? result) {
            if (saver == null) {
                result?.OnTransact(Data.Logs.FAILURE_REASON.INV_SRCS);
                result?.OnFinish(false);
                return false;
            }
            try {
                save(saver);
                result?.OnFinish(true);
                return true;
            } catch (Exception ex) {
                if (result != null) {
                    result.Message = $"Exception: {ex.Message}," +
#if DEBUG
                        $" InnerException: {ex.InnerException?.Message}";
#endif
                }
                result?.OnFinish(false);
                return false;
            }
        }

        private static NEXT_ACTION GetNextAction<T, U, V>(T? source, U? data, V? saver, DataTransResult? result) {
            NEXT_ACTION retVal = NEXT_ACTION.NORM;
            if (data == null) {
                result?.OnTransact(FAILURE_REASON.INV_DATA);
                retVal = NEXT_ACTION.PASS;
            }
            if (source == null || saver == null) {
                result?.OnTransact(FAILURE_REASON.INV_SRCS);
                retVal = NEXT_ACTION.HALT;
            }
            return retVal;
        }

        public static unsafe bool Execute<T, U, V>(
            delegate*<T, U, void> exec, delegate*<V, void> save,
            T? source, U? data, V? saver,
            DataTransResult? result, bool exec_now) where T : class {

            NEXT_ACTION act = GetNextAction(source, data, saver, result);

            switch (act) {
                case NEXT_ACTION.NORM: {
                    exec(source, data);
                    break;
                }
                case NEXT_ACTION.PASS: {
                    break;
                }
                case NEXT_ACTION.HALT: {
                    result?.OnFinish(false);
                    return false;
                }
            }

            if (exec_now) {
                return SaveResult(save, saver, result);
            }
            return true;
        }

        public static unsafe bool Execute<T, U, V>(
            delegate*<T, U, void> exec, delegate*<V, void> save,
            T? source, IEnumerable<U> datas, V? saver,
            DataTransResult? result, bool exec_now) where T : class {

            NEXT_ACTION act = GetNextAction(source, datas, saver, result);

            switch (act) {
                case NEXT_ACTION.NORM: {
                    foreach (U? data in datas) {
                        exec(source, data);
                    }
                    break;
                }
                case NEXT_ACTION.PASS: {
                    break;
                }
                case NEXT_ACTION.HALT: {
                    result?.OnFinish(false);
                    return false;
                }
            }

            if (exec_now) {
                return SaveResult(save, saver, result);
            }
            return true;
        }
    }
}
