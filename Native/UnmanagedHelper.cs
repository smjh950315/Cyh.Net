using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Cyh.Net.Native {
    /// <summary>
    /// The method collection to handle unmanaged resources
    /// </summary>
    public unsafe static class UnmanagedHelper {
        static delegate*<nuint, void*> m_customAllocCallback = null;
        static delegate*<void*, nuint, void*> m_customReallocCallback = null;
        static delegate*<void*, void> m_customFreeCallback = null;
        static void* __allocate(nuint size) {
            if (m_customAllocCallback != null) {
                return m_customAllocCallback(size);
            }
            return (void*)Marshal.AllocHGlobal((int)size);
        }
        static void* __realloc(void* old, nuint size) {
            if (m_customReallocCallback != null) {
                return m_customReallocCallback(old, size);
            }
            return (void*)Marshal.ReAllocHGlobal((IntPtr)old, (int)size);
        }
        static void __free(void* ptr) {
            if (ptr == null) { return; }
            if (m_customFreeCallback != null) {
                m_customFreeCallback(ptr);
                return;
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
                m_customAllocCallback = alloc;
                m_customReallocCallback = realloc;
                m_customFreeCallback = free;
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
        /// Set the value at the offset of the pointer.
        /// </summary>
        /// <param name="addr">The origin pointer.</param>
        /// <param name="offset">The count of <typeparamref name="T"/> to shift.</param>
        /// <param name="value">The value to set to the memory.</param>
        public static void SetValueByteField<T>(void* addr, int offset, T value) where T : unmanaged {
            void* _addr = Shift<T>(addr, offset);
            Buffer.MemoryCopy(&value, _addr, sizeof(T), sizeof(T));
        }

        /// <summary>
        /// Get the value at the offset of the pointer.
        /// </summary>
        /// <param name="addr">The origin pointer.</param>
        /// <param name="offset">The count of <typeparamref name="T"/> to shift.</param>
        /// <returns>The value on the memory offset.</returns>
        public static T GetValueByteField<T>(void* addr, int offset) where T : unmanaged {
            T result = default;
            void* _addr = Shift<T>(addr, offset);
            Buffer.MemoryCopy(_addr, &result, sizeof(T), sizeof(T));
            return result;
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
        public static bool GetManagedArray<T>(void* src, ulong length, [NotNull] out T[]? array) where T : unmanaged {
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

        public static IEnumerable<byte> GetBytesOfLsb<T>(ref T value) where T : unmanaged {
            int typeSize = sizeof(T);
            byte[] result = new byte[typeSize];
            fixed (T* addr = &value) {
                byte* byteField = (byte*)addr;
                if (BitConverter.IsLittleEndian) {
                    for (int i = 0; i < typeSize; i++) {
                        result[i] = byteField[i];
                    }
                } else {
                    for (int i = 0; i < typeSize; i++) {
                        result[i] = byteField[typeSize - i];
                    }
                }
            }
            return result;
        }

        public static void SetByteOfLsbIndex<T>(ref T src, int index, byte value) where T : unmanaged {
            int typeSize = sizeof(T);
            if (index < 0 || index > typeSize) throw new ArgumentOutOfRangeException(nameof(index));
            fixed (T* addr = &src) {
                byte* byteField = (byte*)addr;
                if (BitConverter.IsLittleEndian) {
                    byteField[index] = value;
                } else {
                    byteField[typeSize - index] = value;
                }
            }
        }

        public static byte GetByteOfLsbIndex<T>(ref T src, int index) where T : unmanaged {
            int typeSize = sizeof(T);
            fixed (T* addr = &src) {
                byte* byteField = (byte*)addr;
                if (BitConverter.IsLittleEndian) {
                    return byteField[index];
                } else {
                    return byteField[sizeof(ulong) - index];
                }
            }
        }

#pragma warning disable CS8500, CS8600, CS8603

        /// <summary>
        /// Get the pointer of the value at the offset of the pointer.
        /// </summary>
        /// <param name="addr">The origin pointer.</param>
        /// <param name="offset">The count of <typeparamref name="T"/> to shift.</param>
        /// <returns>Pointer after shifting.</returns>
        public static void* Shift_Unchecked<T>(void* addr, int offset) {
            ThrowNullPointer(addr);
            return (T*)addr + offset;
        }

        /// <summary>
        /// Set the value at the offset of the pointer.
        /// </summary>
        /// <param name="addr">The origin pointer.</param>
        /// <param name="offset">The count of <typeparamref name="T"/> to shift.</param>
        /// <param name="value">The value to set to the memory.</param>
        public static void SetValueByteField_Unchecked<T>(void* addr, int offset, T value) {
            void* _addr = Shift_Unchecked<T>(addr, offset);
            Buffer.MemoryCopy(&value, _addr, sizeof(T), sizeof(T));
        }

        /// <summary>
        /// Get the value at the offset of the pointer.
        /// </summary>
        /// <param name="addr">The origin pointer.</param>
        /// <param name="offset">The count of <typeparamref name="T"/> to shift.</param>
        /// <returns>The value on the memory offset.</returns>
        public static T GetValueByteField_Unchecked<T>(void* addr, int offset) {
            T result = default;
            void* _addr = Shift_Unchecked<T>(addr, offset);
            Buffer.MemoryCopy(_addr, &result, sizeof(T), sizeof(T));
            return result;
        }

        /// <summary>
        /// Get the managed array from the unmanaged memory block.
        /// </summary>
        public static bool GetManagedArray_Unchecked<T>(void* src, ulong length, [NotNull] out T[]? array) {
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

#pragma warning restore CS8500, CS8600, CS8603
    }
}
