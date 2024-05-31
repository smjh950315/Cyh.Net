using System.Xml.Linq;

namespace Cyh.Net.Native {
    /// <summary>
    /// 處理非託管資源生命週期的類別
    /// </summary>
    public class LifeTimeHandler : IDisposable {
        private bool m_is_disposed;
        /// <summary>
        /// 釋放函數
        /// </summary>
        private unsafe delegate*<void*, void> m_release_callback;
        /// <summary>
        /// 非託管資源指標的記憶體位址
        /// </summary>
        private unsafe void** m_data_address;

        protected virtual void Dispose(bool disposing) {
            if (!this.m_is_disposed) {
                if (disposing) { }

                unsafe {
                    if (this.m_release_callback != null && this.m_data_address != null) {
                        if(*this.m_data_address != null) {
                            this.m_release_callback(*this.m_data_address);
                            *this.m_data_address = null;
                        }
                        this.m_release_callback = null;
                        this.m_data_address = null;
                    }
                }

                this.m_is_disposed = true;
            }
        }

        ~LifeTimeHandler() {
            this.Dispose(disposing: false);
        }

        public void Dispose() {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 處理非託管資源生命週期的類別
        /// </summary>
        /// <param name="address_of_ptr">非託管資源指標的記憶體位址</param>
        /// <param name="release">釋放函數</param>
        unsafe public LifeTimeHandler(void** address_of_ptr, delegate*<void*, void> release) {
            this.m_data_address = address_of_ptr;
            this.m_release_callback = release;
        }
    }
}
