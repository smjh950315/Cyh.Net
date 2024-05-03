using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Cyh.Net.Native {
    /// <summary>
    /// The method collection to handle unmanaged resources
    /// </summary>
    public unsafe static class Utilities {
        static delegate*<nuint, void*> _CustomAlloc = null;
        static delegate*<void*, nuint, void*> _CustomRealloc = null;
        static delegate*<void*, void> _CustomFree = null;
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
        /// Set custom memory allocation callbacks.
        /// </summary>
        /// <param name="alloc">The function pointer to allocate memory.</param>
        /// <param name="realloc">The function pointer to reallocate memory.</param>
        /// <param name="free">The function pointer to free the allocated memory.</param>
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
        /// Allocate a unmanaged memory block with size of <typeparamref name="T"/> * <paramref name="count"/> bytes.
        /// </summary>
        public static void* Allocate<T>(nuint count) {
            // zero size : return nullptr
            if (count == 0) { return null; }
            return __allocate(count * (nuint)Marshal.SizeOf<T>());
        }

        /// <summary>
        /// Reallocate a unmanaged memory block to the size of <typeparamref name="T"/> * <paramref name="count"/> bytes.
        /// </summary>
        public static void* Reallocate<T>(void* old, nuint count) {
            // old == nullptr : alloc directly
            if (old == null) {
                return Allocate<T>(count * (nuint)Marshal.SizeOf<T>());
            }
            // count == 0 && old != nullptr : free directly
            if (count == 0) {
                __free(old);
                return null;
            }
            // count != 0 && old != nullptr : reallocate
            return __realloc(old, count * (nuint)Marshal.SizeOf<T>());
        }

        /// <summary>
        /// Free the unmanaged memory block.
        /// </summary>
        /// <param name="ptr"></param>
        public static void Free(void* ptr) {
            // ptr == nullptr : free directly
            if (ptr == null) { return; }
            __free(ptr);
        }

        /// <summary>
        /// Allocate a unmanaged memory block with <paramref name="size"/> bytes.
        /// </summary>
        public static void* Allocate(nuint size) => Allocate<byte>(size);

        /// <summary>
        /// Reallocate a unmanaged memory block to <paramref name="size"/> bytes.
        /// </summary>
        public static void* Reallocate(void* old, nuint size) => Reallocate<byte>(old, size);

        /// <summary>
        /// Indicate whether the pointer is valid.
        /// </summary>
        /// <param name="addr">pointer</param>
        /// <returns>Return true if the pointer input is valid.</returns>
        private static bool IsValidAddress([NotNullWhen(true)] void* addr) => addr != null;

        /// <summary>
        /// Throw an exception if the pointer is null.
        /// </summary>
        /// <param name="addr">pointer</param>
        /// <exception cref="ArgumentNullException"></exception>
        private static void ThrowNullPointer([NotNull] void* addr) {
            if (addr == null) { throw new ArgumentNullException(nameof(addr)); }
        }

        /// <summary>
        /// Get the pointer of the value at the offset of the pointer.
        /// </summary>
        /// <param name="addr">The origin pointer.</param>
        /// <param name="offset">The count of <typeparamref name="T"/> to shift.</param>
        /// <returns>Pointer after shifting.</returns>
        public static void* Shift<T>(void* addr, int offset) where T : unmanaged {
            ThrowNullPointer(addr);
            return (T*)addr + offset;
        }

        /// <summary>
        /// Get the pointer of the value at the offset of the pointer.
        /// </summary>
        /// <param name="addr">The origin pointer.</param>
        /// <param name="offset">The count of <typeparamref name="T"/> to shift.</param>
        /// <returns><typeparamref name="T"/>* after shifting.</returns>
        public static T* GetValuePtr<T>(void* addr, int offset) where T : unmanaged {
            ThrowNullPointer(addr);
            return (T*)Shift<T>(addr, offset);
        }

        /// <summary>
        /// Set the value at the offset of the pointer.
        /// </summary>
        /// <param name="addr">The origin pointer.</param>
        /// <param name="offset">The count of <typeparamref name="T"/> to shift.</param>
        /// <param name="value">The value to set to the memory.</param>
        public static void SetValue<T>(void* addr, int offset, T value) where T : unmanaged {
            *GetValuePtr<T>(addr, offset) = value;
        }

        /// <summary>
        /// Get the value at the offset of the pointer.
        /// </summary>
        /// <param name="addr">The origin pointer.</param>
        /// <param name="offset">The count of <typeparamref name="T"/> to shift.</param>
        /// <returns>The value on the memory offset.</returns>
        public static T GetValue<T>(void* addr, int offset) where T : unmanaged {
            return *GetValuePtr<T>(addr, offset);
        }

        /// <summary>
        /// Compare two memory blocks in bytes.
        /// </summary>
        /// <returns>Whether same in bytes</returns>
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
        /// Get the managed array from the unmanaged memory block.
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
