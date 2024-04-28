using Cyh.Net.Native.Prototypes;

namespace Cyh.Net.Native.OS
{
    public class Status : IDisposable
    {
        public Quantity<nuint> PhysicalMemory;
        public Quantity<nuint> Volumn;
        public Quantity<float> Processer;
        private bool disposedValue;

        protected virtual void Dispose(bool disposing) {
            if (!this.disposedValue) {
                if (disposing) {
                    // TODO: 處置受控狀態 (受控物件)
                }

                // TODO: 釋出非受控資源 (非受控物件) 並覆寫完成項
                // TODO: 將大型欄位設為 Null
                this.disposedValue = true;
            }
        }

        ~Status() {
            // 請勿變更此程式碼。請將清除程式碼放入 'Dispose(bool disposing)' 方法
            this.Dispose(disposing: false);
        }

        public void Dispose() {
            // 請勿變更此程式碼。請將清除程式碼放入 'Dispose(bool disposing)' 方法
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
