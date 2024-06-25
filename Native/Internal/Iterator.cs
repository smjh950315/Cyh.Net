using System.Collections;

namespace Cyh.Net.Native.Internal {
    internal unsafe class Iterator<T> : IEnumerator<T> where T : unmanaged {

        T* m_begin;
        T* m_end;
        T* m_current;

        public T Current => *this.m_current;

        object IEnumerator.Current => this.Current;

        internal Iterator(T* ptr_begin, int length) {
            this.m_begin = ptr_begin;
            this.m_end = ptr_begin + length;
            this.m_current = ptr_begin - 1;
        }

        public void Dispose() { }

        public bool MoveNext() => ++this.m_current < this.m_end;

        public void Reset() => this.m_current = this.m_begin;
    }
}
