using System.Runtime.InteropServices;
using System.Text;

namespace Cyh.Net.Native {

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct Utf8String
        : IEquatable<string>
        , IEquatable<Utf8String>
        , IDisposable {

        void* m_chars;
        nuint m_length;
        nuint m_disposed = 0;
        void load_from_bytes(void* ptr, nuint count) {
            if (this.m_chars != null) { throw new InvalidOperationException(""); }
            this.m_chars = Utilities.Allocate(count);
            Buffer.MemoryCopy(ptr, this.m_chars, count, count);
            this.m_length = count;
        }

        /// <summary>
        /// 字串資料是否被釋放
        /// </summary>
        public readonly bool IsDisposed => this.m_disposed != 0;

        /// <summary>
        /// 字串資料的位元長度
        /// </summary>
        public readonly int Length => (int)this.m_length;

        public Utf8String(string? @string) {
            if (@string.IsNullOrEmpty()) { return; }
            byte[] u8str = Encoding.UTF8.GetBytes(@string);
            fixed (byte* ptr = u8str) {
                this.load_from_bytes(ptr, (nuint)u8str.Length);
            }
        }

        public Utf8String(Utf8String? other) {
            if (other == null) { return; }
            if (other.Value.IsDisposed) { return; }
            if (other.Value.Length != 0 && other.Value.m_chars != null) {
                this.load_from_bytes(other.Value.m_chars, other.Value.m_length);
            }
        }

        public override readonly string ToString() {
            if (this.m_length == 0) { return String.Empty; }
            return Encoding.UTF8.GetString((byte*)this.m_chars, this.Length);
        }

        public bool Equals(string? other) {
            if (other == null) { return false; }
            return this.ToString() == other.ToString();
        }

        public bool Equals(Utf8String other) {
            if (other.Length != this.Length) { return false; }
            return Utilities.IsBytesEqual(this.m_chars, other.m_chars, this.Length);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public void Dispose() {
            Utilities.Free(this.m_chars);
            this.m_chars = null;
            this.m_length = 0;
            this.m_disposed = 1;
        }

        public static implicit operator Utf8String(string? value) {
            return new Utf8String(value ?? String.Empty);
        }
    }
}
