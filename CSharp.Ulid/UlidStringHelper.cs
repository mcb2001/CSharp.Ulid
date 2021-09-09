using System;

namespace CSharp.Ulid
{
    internal readonly struct UlidStringHelper
    {
        public UlidStringHelper(Ulid value)
        {
            Value = value;
        }

        private Ulid Value { get; }

        public int this[int index] => index switch
        {
            0 => (Value.TimeStamp_0 & 224) >> 5,
            1 => Value.TimeStamp_0 & 31,
            2 => (Value.TimeStamp_1 & 248) >> 3,
            3 => ((Value.TimeStamp_1 & 7) << 2) | ((Value.TimeStamp_2 & 192) >> 6),
            4 => (Value.TimeStamp_2 & 62) >> 1,
            5 => ((Value.TimeStamp_2 & 1) << 4) | ((Value.TimeStamp_3 & 240) >> 4),
            6 => ((Value.TimeStamp_3 & 15) << 1) | ((Value.TimeStamp_4 & 128) >> 7),
            7 => (Value.TimeStamp_4 & 124) >> 2,
            8 => ((Value.TimeStamp_4 & 3) << 3) | ((Value.TimeStamp_5 & 224) >> 5),
            9 => Value.TimeStamp_5 & 31,
            10 => (Value.Randomness_0 & 248) >> 3,
            11 => ((Value.Randomness_0 & 7) << 2) | ((Value.Randomness_1 & 192) >> 6),
            12 => (Value.Randomness_1 & 62) >> 1,
            13 => ((Value.Randomness_1 & 1) << 4) | ((Value.Randomness_2 & 240) >> 4),
            14 => ((Value.Randomness_2 & 15) << 1) | ((Value.Randomness_3 & 128) >> 7),
            15 => (Value.Randomness_3 & 124) >> 2,
            16 => ((Value.Randomness_3 & 3) << 3) | ((Value.Randomness_4 & 224) >> 5),
            17 => Value.Randomness_4 & 31,
            18 => (Value.Randomness_5 & 248) >> 3,
            19 => ((Value.Randomness_5 & 7) << 2) | ((Value.Randomness_6 & 192) >> 6),
            20 => (Value.Randomness_6 & 62) >> 1,
            21 => ((Value.Randomness_6 & 1) << 4) | ((Value.Randomness_7 & 240) >> 4),
            22 => ((Value.Randomness_7 & 15) << 1) | ((Value.Randomness_8 & 128) >> 7),
            23 => (Value.Randomness_8 & 124) >> 2,
            24 => ((Value.Randomness_8 & 3) << 3) | ((Value.Randomness_9 & 224) >> 5),
            25 => Value.Randomness_9 & 31,
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
    }
}