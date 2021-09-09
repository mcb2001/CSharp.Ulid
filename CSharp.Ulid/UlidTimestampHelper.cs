using System;
using System.Runtime.CompilerServices;

namespace CSharp.Ulid
{
    internal readonly struct UlidTimestampHelper
    {
        private const long TIMESTAMP_MASK_0 = 255;
        private const long TIMESTAMP_MASK_1 = 65280;
        private const long TIMESTAMP_MASK_2 = 16711680;
        private const long TIMESTAMP_MASK_3 = 4278190080;
        private const long TIMESTAMP_MASK_4 = 1095216660480;
        private const long TIMESTAMP_MASK_5 = 280375465082880;

        public UlidTimestampHelper(long timeStamp)
        {
            TimeStamp_0 = (byte)((timeStamp & TIMESTAMP_MASK_5) >> 40);
            TimeStamp_1 = (byte)((timeStamp & TIMESTAMP_MASK_4) >> 32);
            TimeStamp_2 = (byte)((timeStamp & TIMESTAMP_MASK_3) >> 24);
            TimeStamp_3 = (byte)((timeStamp & TIMESTAMP_MASK_2) >> 16);
            TimeStamp_4 = (byte)((timeStamp & TIMESTAMP_MASK_1) >> 8);
            TimeStamp_5 = (byte)((timeStamp & TIMESTAMP_MASK_0) >> 0);
        }

        public UlidTimestampHelper(ReadOnlySpan<byte> timestamp)
        {
            if (timestamp.Length != 6)
            {
                throw new ArgumentException($"Expected a length of {6}, received {timestamp.Length}", nameof(timestamp));
            }

            TimeStamp_0 = timestamp[0];
            TimeStamp_1 = timestamp[1];
            TimeStamp_2 = timestamp[2];
            TimeStamp_3 = timestamp[3];
            TimeStamp_4 = timestamp[4];
            TimeStamp_5 = timestamp[5];
        }

        public byte TimeStamp_0 { get; }
        public byte TimeStamp_1 { get; }
        public byte TimeStamp_2 { get; }
        public byte TimeStamp_3 { get; }
        public byte TimeStamp_4 { get; }
        public byte TimeStamp_5 { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Ulid Create(ReadOnlySpan<byte> randomness) => new Ulid
        {
            TimeStamp_0 = TimeStamp_0,
            TimeStamp_1 = TimeStamp_1,
            TimeStamp_2 = TimeStamp_2,
            TimeStamp_3 = TimeStamp_3,
            TimeStamp_4 = TimeStamp_4,
            TimeStamp_5 = TimeStamp_5,
            Randomness_0 = randomness[0],
            Randomness_1 = randomness[1],
            Randomness_2 = randomness[2],
            Randomness_3 = randomness[3],
            Randomness_4 = randomness[4],
            Randomness_5 = randomness[5],
            Randomness_6 = randomness[6],
            Randomness_7 = randomness[7],
            Randomness_8 = randomness[8],
            Randomness_9 = randomness[9],
        };
    }
}