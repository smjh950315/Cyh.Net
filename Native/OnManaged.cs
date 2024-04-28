namespace Cyh.Net.Native
{
    public abstract unsafe class OnManaged : IDisposable
    {
        protected void* _Instance;
        private bool disposedValue;

        protected abstract void Release();

        protected virtual void Dispose(bool disposing) {
            if (!this.disposedValue) {
                if (disposing) { }
                this.Release();
                this._Instance = null;
                this.disposedValue = true;
            }
        }

        ~OnManaged() {
            // 請勿變更此程式碼。請將清除程式碼放入 'Dispose(bool disposing)' 方法
            this.Dispose(disposing: false);
        }

        void IDisposable.Dispose() {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
