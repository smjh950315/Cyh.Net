using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Cyh.Net {
    public static class Lib {

        public delegate void NoReturn();
        public delegate void NoReturn<T>(T? val);
        public delegate void NoReturn<T, U>(T? val1, U? val2);
        public delegate void NoReturn<T, U, V>(T? val1, U? val2, V? val3);

        /// <summary>
        /// Try to execute a function and get the result.
        /// </summary>
        /// <param name="func">The function to execute</param>
        /// <param name="val">The result of function</param>
        /// <returns>Whether executed without exception</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryExecute<T>(Func<T?> func, out T? val) {
            try {
                val = func();
                return true;
            } catch {
                val = default;
                return false;
            }
        }

        /// <summary>
        /// Try to execute a function and ignore any exception.
        /// </summary>
        /// <param name="func">The function to execute</param>
        /// <returns>Whether executed without exception</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryExecute(NoReturn func) {
            try {
                func();
                return true;
            } catch { return false; }
        }

        /// <summary>
        /// Try to execute a function and ignore any exception.
        /// </summary>
        /// <param name="func">The function to execute</param>
        /// <param name="val">The value to pass to the function</param>
        /// <returns>Whether executed without exception</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryExecute<T>(NoReturn<T> func, T? val) {
            try {
                func(val);
                return true;
            } catch { return false; }
        }

        /// <summary>
        /// Try to execute a function and ignore any exception.
        /// </summary>
        /// <param name="func">The function to execute</param>
        /// <param name="val1">The value to pass to the function</param>
        /// <param name="val2">The value to pass to the function</param>
        /// <returns>Whether executed without exception</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryExecute<T, U>(NoReturn<T, U> func, T? val1, U? val2) {
            try {
                func(val1, val2);
                return true;
            } catch { return false; }
        }

        /// <summary>
        /// Try to execute a function and ignore any exception.
        /// </summary>
        /// <param name="func">The function to execute</param>
        /// <param name="val1">The value to pass to the function</param>
        /// <param name="val2">The value to pass to the function</param>
        /// <param name="val3">The value to pass to the function</param>
        /// <returns>Whether executed without exception</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryExecute<T, U, V>(NoReturn<T, U, V> func, T? val1, U? val2, V? val3) {
            try {
                func(val1, val2, val3);
                return true;
            } catch { return false; }
        }

        /// <summary>
        /// Try to execute a function and get the result.
        /// </summary>
        /// <param name="func">The function to execute</param>
        /// <param name="val">The result of function</param>
        /// <returns>Whether executed without exception</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static bool TryExecute<T>(delegate*<T> func, out T? val) {
            try {
                val = func();
                return true;
            } catch {
                val = default;
                return false;
            }
        }

        /// <summary>
        /// Try to execute a function and ignore any exception.
        /// </summary>
        /// <param name="func">The function to execute</param>
        /// <returns>Whether executed without exception</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static bool TryExecute(delegate*<void> func) {
            try {
                func();
                return true;
            } catch { return false; }
        }

        /// <summary>
        /// Try to execute a function and ignore any exception.
        /// </summary>
        /// <param name="func">The function to execute</param>
        /// <param name="val">The value to pass to the function</param>
        /// <returns>Whether executed without exception</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static bool TryExecute<T>(delegate*<T?, void> func, T? val) {
            try {
                func(val);
                return true;
            } catch { return false; }
        }

        /// <summary>
        /// Try to execute a function and ignore any exception.
        /// </summary>
        /// <param name="func">The function to execute</param>
        /// <param name="val1">The value to pass to the function</param>
        /// <param name="val2">The value to pass to the function</param>
        /// <returns>Whether executed without exception</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static bool TryExecute<T, U>(delegate*<T?, U?, void> func, T? val1, U? val2) {
            try {
                func(val1, val2);
                return true;
            } catch { return false; }
        }

        /// <summary>
        /// Try to execute a function and ignore any exception.
        /// </summary>
        /// <param name="func">The function to execute</param>
        /// <param name="val1">The value to pass to the function</param>
        /// <param name="val2">The value to pass to the function</param>
        /// <param name="val3">The value to pass to the function</param>
        /// <returns>Whether executed without exception</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static bool TryExecute<T, U, V>(delegate*<T?, U?, V?, void> func, T? val1, U? val2, V? val3) {
            try {
                func(val1, val2, val3);
                return true;
            } catch { return false; }
        }

        /// <summary>
        /// Throw an ArgumentNullException if the value is null.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowNull<T>([NotNull] T? value) {
            if (value == null) {
                throw new ArgumentNullException(nameof(value));
            }
        }

        /// <summary>
        /// Throw an ArgumentNullException if the value is null.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowNull<T, U>([NotNull] T? value1, [NotNull] U? value2) {
            ThrowNull(value1);
            ThrowNull(value2);
        }

        /// <summary>
        /// Throw an ArgumentNullException if the value is null.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowNull<T, U, V>([NotNull] T? value1, [NotNull] U? value2, [NotNull] V? value3) {
            ThrowNull(value1, value2);
            ThrowNull(value3);
        }

        /// <summary>
        /// Throw an ArgumentNullException if the value is null.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowNull<T, U, V, W>([NotNull] T? value1, [NotNull] U? value2, [NotNull] V? value3, [NotNull] W? value4) {
            ThrowNull(value1, value2, value3);
            ThrowNull(value4);
        }
    }
}
