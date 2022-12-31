using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace CSharp.Ulid
{
    internal static class StringExtensions
    {
#if NETSTANDARD2_1_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static string Create<TState>(int length, TState state, SpanAction<char, TState> action)
        {
#if NETSTANDARD2_0
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
            if (action is null) throw new ArgumentNullException(nameof(action));
            if (length == 0) return string.Empty;

            var chars = new char[length];
            action(chars, state);
            return new string(chars);
#else
            return string.Create(length, state, action);
#endif
        }
    }
}
