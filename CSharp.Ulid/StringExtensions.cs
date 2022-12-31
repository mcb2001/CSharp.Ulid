using System;
using System.Buffers;
using System.Runtime.CompilerServices;

#if NETSTANDARD2_0_OR_GREATER

namespace CSharp.Ulid
{
    internal static class StringExtensions
    {
#if NETSTANDARD2_1_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static string Create<TState>(int length, TState state, SpanAction<char, TState> action)
        {
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
            if (action is null) throw new ArgumentNullException(nameof(action));
            if (length == 0) return string.Empty;

            var chars = new char[length];
            action(chars, state);
            return new string(chars);
        }
    }
}

#endif
