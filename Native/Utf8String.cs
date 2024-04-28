using System.Runtime.InteropServices;
using System.Text;

namespace Cyh.Net.Native
{

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct Utf8String
        : IEquatable<string>
        , IDisposable
    {
        void* _Chars;
        nuint _Length;

        void _Load_From_Bytes(void* ptr, nuint count) {
            if (this._Chars != null) { throw new InvalidOperationException(""); }
            this._Chars = Utilities.Allocate(count);
            Buffer.MemoryCopy(ptr, this._Chars, count, count);
            this._Length = count;
        }

        public bool IsDisposed = false;
        public readonly int Length => (int)this._Length;
        public readonly int Count => this.Length;

        public Utf8String(string? @string) {
            if (@string.IsNullOrEmpty()) { return; }
            byte[] u8str = Encoding.UTF8.GetBytes(@string);
            fixed (byte* ptr = u8str) {
                this._Load_From_Bytes(ptr, (nuint)u8str.Length);
            }
        }
        public Utf8String(Utf8String? other) {
            if (other == null) { return; }
            if (other.Value.IsDisposed) { return; }
            if (other.Value.Length != 0 && other.Value._Chars != null) {
                this._Load_From_Bytes(other.Value._Chars, other.Value._Length);
            }
        }
        public override readonly string ToString() {
            if (this._Length == 0) { return String.Empty; }
            return Encoding.UTF8.GetString((byte*)this._Chars, this.Length);
        }
        public bool Equals(string? other) {
            if (other == null) { return false; }
            return this.ToString() == other.ToString();
        }
        public bool Equals(Utf8String other) {
            if (other.Length != this.Length) { return false; }
            return Utilities.IsBytesEqual(this._Chars, other._Chars, this.Length);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
        public void Dispose() {
            Utilities.Free(this._Chars);
            this._Chars = null;
            this._Length = 0;
            this.IsDisposed = true;
        }

        public static implicit operator Utf8String(string? value) {
            return new Utf8String(value ?? String.Empty);
        }
    }
}
