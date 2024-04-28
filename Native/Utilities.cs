using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Cyh.Net.Native
{
    /// <summary>
    /// 輔助原生資料相關操作的類別
    /// </summary>
    public unsafe static class Utilities
    {
        static delegate*<nuint, void*> _CustomAlloc;
        static delegate*<void*, nuint, void*> _CustomRealloc;
        static delegate*<void*, void> _CustomFree;
        static void* __allocate(nuint size) {
            if (_CustomAlloc != null) {
                return _CustomAlloc(size);
            }
            return (void*)Marshal.AllocHGlobal((int)size);
        }
        static void* __realloc(void* old, nuint size) {
            if (_CustomRealloc != null) {
                return _CustomRealloc(old, size);
            }
            return (void*)Marshal.ReAllocHGlobal((IntPtr)old, (int)size);
        }
        static void __free(void* ptr) {
            if (_CustomFree != null) {
                _CustomFree(ptr);
            }
            Marshal.FreeHGlobal((IntPtr)ptr);
        }

        /// <summary>
        /// 設定自訂的記憶體管理函數
        /// </summary>
        /// <param name="alloc">分配記憶體的函數</param>
        /// <param name="realloc">調整記憶體大小的函數</param>
        /// <param name="free">釋放記憶體的函數</param>
        /// <exception cref="ArgumentException"></exception>
        public static void SetCustomMemCallback(delegate*<nuint, void*> alloc, delegate*<void*, nuint, void*> realloc, delegate*<void*, void> free) {
            if (alloc == null && realloc == null && free == null) { return; }
            if (alloc != null && realloc != null && free != null) {
                _CustomAlloc = alloc;
                _CustomRealloc = realloc;
                _CustomFree = free;
            }
            throw new ArgumentException("custom callbacks : alloc, realloc and free should be set together!");
        }

        /// <summary>
        /// 分配 大小為 <paramref name="size"/> 個 <typeparamref name="T"/> 的非託管的記憶體區塊
        /// </summary>
        public static void* Allocate<T>(nuint size) {
            // zero size : return nullptr
            if (size == 0) { return null; }
            return __allocate(size * (nuint)Marshal.SizeOf<T>());
        }

        /// <summary>
        /// 將已經分配的非託管的記憶體區塊調整大小到 <paramref name="size"/> 個 <typeparamref name="T"/>
        /// </summary>
        public static void* Reallocate<T>(void* old, nuint size) {
            // old == nullptr : alloc directly
            if (old == null) {
                return Allocate<T>(size * (nuint)Marshal.SizeOf<T>());
            }
            // size == 0 && old != nullptr : free directly
            if (size == 0) {
                __free(old);
                return null;
            }
            // size != 0 && old != nullptr : reallocate
            return __realloc(old, size * (nuint)Marshal.SizeOf<T>());
        }

        /// <summary>
        /// 釋放已分配的非託管記憶體區塊
        /// </summary>
        /// <param name="ptr"></param>
        public static void Free(void* ptr) {
            // ptr == nullptr : free directly
            if (ptr == null) { return; }
            __free(ptr);
        }

        /// <summary>
        /// 分配 <paramref name="size"/>個 byte 的非託管記憶體區塊
        /// </summary>
        public static void* Allocate(nuint size) => Allocate<byte>(size);

        /// <summary>
        /// 將已經分配的非託管的記憶體區塊調整大小到 <paramref name="size"/> 個 byte
        /// </summary>
        public static void* Reallocate(void* old, nuint size) => Reallocate<byte>(old, size);

        /// <summary>
        /// 是否是有效的指標
        /// </summary>
        /// <param name="addr">指標</param>
        /// <returns>指標是否有效</returns>
        private static bool IsValidAddress([NotNullWhen(true)] void* addr) => addr != null;

        /// <summary>
        /// 當指標為空的時候丟出例外
        /// </summary>
        /// <param name="addr">指標</param>
        /// <exception cref="ArgumentNullException"></exception>
        private static void ThrowNullPointer([NotNull] void* addr) {
            if (addr == null) { throw new ArgumentNullException(nameof(addr)); }
        }

        /// <summary>
        /// 將指標的位址偏移到 <paramref name="offset"/> 個物件  <typeparamref name="T"/> 的位址
        /// </summary>
        /// <param name="addr">偏移開始的位址</param>
        /// <param name="offset">偏移量</param>
        /// <returns>偏移後的指標</returns>
        public static void* Shift<T>(void* addr, int offset) where T : unmanaged {
            ThrowNullPointer(addr);
            return (T*)addr + offset;
        }

        /// <summary>
        /// 將指標的位址偏移到 <paramref name="offset"/> 個物件  <typeparamref name="T"/> 的位址，並以 <typeparamref name="T"/>* 的形式取得指向該位址的指標
        /// </summary>
        /// <param name="addr">偏移開始的位址</param>
        /// <param name="offset">偏移量</param>
        /// <returns>偏移後的指標 <typeparamref name="T"/>*</returns>
        public static T* GetValuePtr<T>(void* addr, int offset) where T : unmanaged {
            ThrowNullPointer(addr);
            return (T*)Shift<T>(addr, offset);
        }

        /// <summary>
        /// 將指標的位址偏移到 <paramref name="offset"/> 個物件  <typeparamref name="T"/> 的位址，並將該位址的物件值設定為 <paramref name="value"/>
        /// </summary>
        /// <param name="addr">偏移開始的位址</param>
        /// <param name="offset">偏移量</param>
        /// <param name="value">要設定的值</param>
        public static void SetValue<T>(void* addr, int offset, T value) where T : unmanaged {
            *GetValuePtr<T>(addr, offset) = value;
        }

        /// <summary>
        /// 將指標的位址偏移到 <paramref name="offset"/> 個物件  <typeparamref name="T"/> 的位址，並取得該位址的儲存的值
        /// </summary>
        /// <param name="addr">偏移開始的位址</param>
        /// <param name="offset">偏移量</param>
        /// <returns>偏移後的位址的儲存的值</returns>
        public static T GetValue<T>(void* addr, int offset) where T : unmanaged {
            return *GetValuePtr<T>(addr, offset);
        }

        /// <summary>
        /// 以Byte為單位來比較兩個記憶體位置特定長度的資料是否相等
        /// </summary>
        /// <returns>是否相等</returns>
        public static bool IsBytesEqual(void* lhs, void* rhs, int byteLength) {
            if (byteLength == 0) return true;
            if (lhs == null && rhs == null) return true;
            if (lhs != null && rhs != null) {
                byte* lhsPtr = (byte*)lhs;
                byte* rhsPtr = (byte*)rhs;
                for (int i = 0; i < byteLength; i++) {
                    if (lhsPtr[i] != rhsPtr[i]) return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 從指標取得特定類型的託管陣列
        /// </summary>
        public static bool GetManagedArray<T>(void* src, ulong length, out T[] array) where T : unmanaged {
            if (length == 0) {
                array = Array.Empty<T>();
                return false;
            }
            try {
                array = new T[length];
                fixed (T* ptr = array) {
                    Buffer.MemoryCopy(src, ptr, length, length);
                }
                return true;
            } catch {
                array = Array.Empty<T>();
                return false;
            }
        }
    }
}
