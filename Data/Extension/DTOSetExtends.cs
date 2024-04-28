using Cyh.Net.Data.Internal;
using Cyh.Net.Data.Logs;
using Cyh.Net.Data.Models;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Cyh.Net.Data.Extension
{
    public static class DTOSetExtends
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryActivate<T>(this IDTOHelper<T> dto, [NotNullWhen(true)] out IMyDataSource<T>? dataSource) {
            if (dto.DataSource == null) {
                dto.DataSource = dto.Activator.GetDataSource<T>();
                if (dto.DataSource == null) {
                    dataSource = null;
                    return false;
                }
            }
            dataSource = dto.DataSource;
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool ShouldReturn<T, V>(this IDTOHelper<T, V> dto, [NotNullWhen(false)] V? view, [NotNull] ref DataTransResult? result, bool exec_now, [NotNullWhen(false)] out IMyDataSource<T>? dataSource) {
            if (result != null) {
                if (result.IsFinished) {
                    dataSource = null;
                    return true;
                }
            } else {
                result = new DataTransResult();
            }

            if (view != null) {
                return !TryActivate(dto, out dataSource);
            }

            if (exec_now) {
                if (!TryActivate(dto, out dataSource)) {
                    result.OnTransact(FAILURE_REASON.INV_SRCS);
                    return true;
                }
                result.OnTransact(FAILURE_REASON.INV_DATA);
                bool is_succeed = dataSource.ApplyChanges(result);
                result.OnFinish(is_succeed);
                return true;
            } else {
                return !TryActivate(dto, out dataSource);
            }
        }

        public static DataTransResult TryAdd<T, V>(this IDTOHelper<T, V> dto, V? view, DataTransResult? result, bool exec_now) {
            if (ShouldReturn(dto, view, ref result, exec_now, out IMyDataSource<T>? dataSource)) { return result; }
            dataSource.AddSingle(dto.GetExprToData().Compile(), view, result, exec_now);
            return result;
        }
        public static DataTransResult TryUpdate<T, V>(this IDTOHelper<T, V> dto, V? view, DataTransResult? result, bool exec_now) {
            if (ShouldReturn(dto, view, ref result, exec_now, out IMyDataSource<T>? dataSource)) { return result; }
            dataSource.UpdateFromSingle(dto.UpdateToData, view, dto.GetExprToFindData(view), result, exec_now);
            return result;
        }

    }
}
