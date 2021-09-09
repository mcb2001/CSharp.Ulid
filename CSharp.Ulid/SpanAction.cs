#if NETSTANDARD2_0

using System;

namespace System.Buffers
{
    internal delegate void SpanAction<T, in TArg>(Span<T> span, TArg arg);
}

#endif