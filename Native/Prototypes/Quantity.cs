using System.Runtime.InteropServices;

namespace Cyh.Net.Native.Prototypes
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Quantity<T> where T : unmanaged
    {
        public T Total;
        public T Usage;
        public T Avail;
    }
}
