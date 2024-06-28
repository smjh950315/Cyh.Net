using System.Runtime.InteropServices;

namespace Cyh.Net.Native {
    [StructLayout(LayoutKind.Sequential)]
    public struct CArray<T> where T : unmanaged {
        public unsafe T* m_data;
        public nuint m_length;
    }
}
