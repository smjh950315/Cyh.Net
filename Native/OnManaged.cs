namespace Cyh.Net.Native
{
    public abstract unsafe class OnManaged : IDisposable
    {
        protected void* m_instance;
        private bool m_disposed;

        protected abstract void Release();

        protected virtual void Dispose(bool disposing)
        {
            if (!this.m_disposed)
            {
                if (disposing) { }
                this.Release();
                this.m_instance = null;
                this.m_disposed = true;
            }
        }

        ~OnManaged()
        {
            // 請勿變更此程式碼。請將清除程式碼放入 'Dispose(bool disposing)' 方法
            this.Dispose(disposing: false);
        }

        void IDisposable.Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
