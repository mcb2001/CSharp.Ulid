using System;
using System.Security.Cryptography;

namespace CSharp.Ulid
{
    public struct Ulid : IComparable, IComparable<Ulid>, IEquatable<Ulid>
    {
        private const int VALID_ULID_STRING_LENGTH = 26;
        private static readonly char[] CrockfordsBase32 = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K', 'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'V', 'W', 'X', 'Y', 'Z' };

        private const int TIMESTAMP_LENGTH = 6;
        private const int RANDOMNESS_LENGTH = 10;
        private const int DATA_LENGTH = TIMESTAMP_LENGTH + RANDOMNESS_LENGTH;

        private static long LastUsedTimeStamp /* = 0 */;
        private static readonly byte[] LastUsedRandomness = new byte[RANDOMNESS_LENGTH];
        private static readonly DateTime EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        private static readonly object LOCK = new object();

        public byte TimeStamp_0 { get; set; }
        public byte TimeStamp_1 { get; set; }
        public byte TimeStamp_2 { get; set; }
        public byte TimeStamp_3 { get; set; }
        public byte TimeStamp_4 { get; set; }
        public byte TimeStamp_5 { get; set; }

        public long TimeStamp
        {
            get
            {
                long t0 = TimeStamp_0;
                long t1 = TimeStamp_1;
                long t2 = TimeStamp_2;
                long t3 = TimeStamp_3;
                long t4 = TimeStamp_4;
                long t5 = TimeStamp_5;

                return (t0 << 40)
                    | (t1 << 32)
                    | (t2 << 24)
                    | (t3 << 16)
                    | (t4 << 8)
                    | (t5);
            }
        }

        public byte Randomness_0 { get; set; }
        public byte Randomness_1 { get; set; }
        public byte Randomness_2 { get; set; }
        public byte Randomness_3 { get; set; }
        public byte Randomness_4 { get; set; }
        public byte Randomness_5 { get; set; }
        public byte Randomness_6 { get; set; }
        public byte Randomness_7 { get; set; }
        public byte Randomness_8 { get; set; }
        public byte Randomness_9 { get; set; }

        private Ulid(UlidTimestampHelper timestamp, ReadOnlySpan<byte> randomness)
        {
            if (randomness.Length != RANDOMNESS_LENGTH)
            {
                throw new ArgumentException($"Expected a length of {RANDOMNESS_LENGTH}, received {randomness.Length}", nameof(randomness));
            }

            TimeStamp_0 = timestamp.TimeStamp_0;
            TimeStamp_1 = timestamp.TimeStamp_1;
            TimeStamp_2 = timestamp.TimeStamp_2;
            TimeStamp_3 = timestamp.TimeStamp_3;
            TimeStamp_4 = timestamp.TimeStamp_4;
            TimeStamp_5 = timestamp.TimeStamp_5;
            Randomness_0 = randomness[0];
            Randomness_1 = randomness[1];
            Randomness_2 = randomness[2];
            Randomness_3 = randomness[3];
            Randomness_4 = randomness[4];
            Randomness_5 = randomness[5];
            Randomness_6 = randomness[6];
            Randomness_7 = randomness[7];
            Randomness_8 = randomness[8];
            Randomness_9 = randomness[9];
        }

        public Ulid(long timestamp, ReadOnlySpan<byte> randomness) : this(new UlidTimestampHelper(timestamp), randomness)
        {
        }

        public Ulid(ReadOnlySpan<byte> timestamp, ReadOnlySpan<byte> randomness) : this(new UlidTimestampHelper(timestamp), randomness)
        {
        }

        public Ulid(ReadOnlySpan<byte> data)
        {
            if (data.Length != DATA_LENGTH)
            {
                throw new ArgumentException($"Expected a length of {DATA_LENGTH}, received {data.Length}", nameof(data));
            }

            this = new Ulid(new UlidTimestampHelper(data.Slice(0, TIMESTAMP_LENGTH)), data.Slice(TIMESTAMP_LENGTH));
        }

        public static Ulid NewUlid()
        {
            /*
             * Ensure thread safety and monotonicity
             * Using lock instead of mutex, making thread keep spinning, as this process should be over quickly
             */
            lock (LOCK)
            {
                DateTime now = DateTime.UtcNow;
                long timestamp = (long)(now - EPOCH).TotalMilliseconds;

#if NETSTANDARD2_1_OR_GREATER
                Span<byte> randomness = stackalloc byte[LastUsedRandomness.Length];
#else
                byte[] randomness = new byte[LastUsedRandomness.Length];
#endif

                if (timestamp == LastUsedTimeStamp)
                {
                    //Increment by one
#if NETSTANDARD2_1_OR_GREATER
                    LastUsedRandomness.AsSpan().CopyTo(randomness);
#else
                    LastUsedRandomness.CopyTo(randomness, 0);
#endif
                    AddOne(randomness);
                }
                else
                {
                    //Use Crypto random
                    using (var rng = new RNGCryptoServiceProvider())
                    {
                        rng.GetBytes(randomness);
                    }

                    LastUsedTimeStamp = timestamp;
                }

#if NETSTANDARD2_1_OR_GREATER
                randomness.CopyTo(LastUsedRandomness);
#else
                randomness.CopyTo(LastUsedRandomness, 0);
#endif
                return new UlidTimestampHelper(timestamp).Create(randomness);
            }
        }

        private static void AddOne(Span<byte> bytes)
        {
            for (int index = bytes.Length - 1; index >= 0; index--)
            {
                if (bytes[index] < byte.MaxValue)
                {
                    ++bytes[index];
                    return;
                }

                bytes[index] = 0;
            }
        }

        public override string ToString() => StringExtensions.Create(VALID_ULID_STRING_LENGTH, new UlidStringHelper(this), (buffer, value) =>
        {
            for (int index = 0; index < buffer.Length; ++index)
            {
                int i = value[index];
                buffer[index] = CrockfordsBase32[i];
            }
        });

        public byte[] ToByteArray()
        {
            var destination = new byte[16];
            TryWriteBytes(destination);
            return destination;
        }

        public bool TryWriteBytes(Span<byte> destination)
        {
            if (destination.Length < 16)
            {
                return false;
            }

            destination[0] = TimeStamp_0;
            destination[1] = TimeStamp_1;
            destination[2] = TimeStamp_2;
            destination[3] = TimeStamp_3;
            destination[4] = TimeStamp_4;
            destination[5] = TimeStamp_5;
            destination[6] = Randomness_0;
            destination[7] = Randomness_1;
            destination[8] = Randomness_2;
            destination[9] = Randomness_3;
            destination[10] = Randomness_4;
            destination[11] = Randomness_5;
            destination[12] = Randomness_6;
            destination[13] = Randomness_7;
            destination[14] = Randomness_8;
            destination[15] = Randomness_9;

            return true;
        }

        public bool Equals(Ulid other)
        {
            return this.TimeStamp_0 == other.TimeStamp_0
                && this.TimeStamp_1 == other.TimeStamp_1
                && this.TimeStamp_2 == other.TimeStamp_2
                && this.TimeStamp_3 == other.TimeStamp_3
                && this.TimeStamp_4 == other.TimeStamp_4
                && this.TimeStamp_5 == other.TimeStamp_5
                && this.Randomness_0 == other.Randomness_0
                && this.Randomness_1 == other.Randomness_1
                && this.Randomness_2 == other.Randomness_2
                && this.Randomness_3 == other.Randomness_3
                && this.Randomness_4 == other.Randomness_4
                && this.Randomness_5 == other.Randomness_5
                && this.Randomness_6 == other.Randomness_6
                && this.Randomness_7 == other.Randomness_7
                && this.Randomness_8 == other.Randomness_8
                && this.Randomness_9 == other.Randomness_9;
        }

        public int CompareTo(Ulid other)
        {
            int result = 0, index = 0;
            var thisHelper = new UlidStringHelper(this);
            var otherHelper = new UlidStringHelper(other);
            while (result == 0 && index < VALID_ULID_STRING_LENGTH)
            {
                result = thisHelper[index].CompareTo(otherHelper[index]);
                ++index;
            }

            return result;
        }

        public int CompareTo(object obj)
        {
            if (obj is Ulid other)
            {
                return CompareTo(other);
            }

            throw new ArgumentException($"Expected type {typeof(Ulid).FullName}, received {obj.GetType().FullName}", nameof(obj));
        }

        public override int GetHashCode()
        {
            //Auto generated
            var hashCode = -597700488;
            hashCode = hashCode * -1521134295 + TimeStamp_0.GetHashCode();
            hashCode = hashCode * -1521134295 + TimeStamp_1.GetHashCode();
            hashCode = hashCode * -1521134295 + TimeStamp_2.GetHashCode();
            hashCode = hashCode * -1521134295 + TimeStamp_3.GetHashCode();
            hashCode = hashCode * -1521134295 + TimeStamp_4.GetHashCode();
            hashCode = hashCode * -1521134295 + TimeStamp_5.GetHashCode();
            hashCode = hashCode * -1521134295 + Randomness_0.GetHashCode();
            hashCode = hashCode * -1521134295 + Randomness_1.GetHashCode();
            hashCode = hashCode * -1521134295 + Randomness_2.GetHashCode();
            hashCode = hashCode * -1521134295 + Randomness_3.GetHashCode();
            hashCode = hashCode * -1521134295 + Randomness_4.GetHashCode();
            hashCode = hashCode * -1521134295 + Randomness_5.GetHashCode();
            hashCode = hashCode * -1521134295 + Randomness_6.GetHashCode();
            hashCode = hashCode * -1521134295 + Randomness_7.GetHashCode();
            hashCode = hashCode * -1521134295 + Randomness_8.GetHashCode();
            hashCode = hashCode * -1521134295 + Randomness_9.GetHashCode();
            return hashCode;
        }

        public override bool Equals(object obj)
        {
            if (obj is Ulid other)
            {
                return Equals(other);
            }

            return false;
        }

        public static explicit operator byte[](Ulid ulid)
        {
            return ulid.ToByteArray();
        }

        public static bool TryParseSpan(ReadOnlySpan<char> input, out Ulid ulid)
        {
            if (input.Length != VALID_ULID_STRING_LENGTH)
            {
                ulid = default;
                return false;
            }

            Span<int> index = stackalloc int[VALID_ULID_STRING_LENGTH];
            for (int i = 0; i < input.Length; ++i)
            {
                char c = char.ToUpperInvariant(input[i]);
                bool found = false;

                for (int v = 0; v < CrockfordsBase32.Length; ++v)
                {
                    if (CrockfordsBase32[v] == c)
                    {
                        index[i] = v;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    ulid = default;
                    return false;
                }
            }

            ulid = UlidStringHelper.FromString(index);
            return true;
        }

        public static bool TryParse(string input, out Ulid ulid) => TryParseSpan(input.AsSpan(), out ulid);
    }
}
