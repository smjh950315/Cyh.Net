using Cyh.Net.Native.Internal;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Cyh.Net.Native {

    [StructLayout(LayoutKind.Sequential)]
    public struct u8string
        : IEnumerable<byte>
        , IEquatable<u8string>
        , IEquatable<string>
        , IDisposable {

        nuint m_data;
        nuint m_length;

        public int Length => (int)this.m_length;

        public void Dispose() {
            unsafe {
                Native.UnmanagedHelper.Free((void*)this.m_data);
                this.m_data = 0;
                this.m_length = 0;
            }
        }

        public bool Equals(u8string other) {
            if (this.Length == other.Length) {
                if (this.Length != 0) {
                    unsafe {
                        return Native.UnmanagedHelper.IsBytesEqual((void*)this.m_data, (void*)other.m_data, this.Length);
                    }
                }
                return true;
            }
            return false;
        }

        public bool Equals(string? other) => this.ToString() == other;

        public IEnumerator<byte> GetEnumerator() {
            unsafe { return new Iterator<byte>((byte*)this.m_data, this.Length); }
        }

        public override string ToString() {
            if (this.Length != 0) {
                unsafe {
                    return Encoding.UTF8.GetString((byte*)this.m_data, this.Length);
                }
            }
            return string.Empty;
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public u8string() { }

        public u8string(byte[] bytes) {
            if (!bytes.IsNullOrEmpty()) {
                unsafe {
                    fixed (byte* ptr = bytes) {
                        this.m_data = unchecked((nuint)Native.UnmanagedHelper.Allocate((nuint)bytes.Length));
                        this.m_length = (nuint)bytes.Length;
                        Buffer.MemoryCopy(ptr, (void*)this.m_data, bytes.Length, bytes.Length);
                    }
                }
            }
        }

        public u8string(string str) : this(str.GetBytesUtf8() ?? []) { }

        public static implicit operator u8string(byte[] bytes) {
            return new u8string(bytes);
        }

        public static implicit operator u8string(string str) {
            return new u8string(str);
        }

        public static implicit operator string(u8string str) => str.ToString();

        public static implicit operator byte[](u8string str) { return [.. str]; }
    }
}
