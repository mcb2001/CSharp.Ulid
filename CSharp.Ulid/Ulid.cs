using System;
using System.Security.Cryptography;

namespace CSharp.Ulid
{
    public struct Ulid : IComparable, IComparable<Ulid>, IEquatable<Ulid>
    {
        private const int VALID_ULID_STRING_LENGTH = 26;

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

        private static readonly char[] CrockfordsBase32 = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K', 'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'V', 'W', 'X', 'Y', 'Z' };

        private static long LastUsedTimeStamp = 0;
        private static readonly byte[] LastUsedRandomness = new byte[10];

        private static readonly object LOCK = new object();

        private static readonly DateTime EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        private const long TIMESTAMP_MASK_0 = 255;
        private const long TIMESTAMP_MASK_1 = 65280;
        private const long TIMESTAMP_MASK_2 = 16711680;
        private const long TIMESTAMP_MASK_3 = 4278190080;
        private const long TIMESTAMP_MASK_4 = 1095216660480;
        private const long TIMESTAMP_MASK_5 = 280375465082880;

        public Ulid(long timestamp, byte[] randomness)
            : this(new byte[]
            {
                (byte)((timestamp & TIMESTAMP_MASK_5) >> 40),
                (byte)((timestamp & TIMESTAMP_MASK_4) >> 32),
                (byte)((timestamp & TIMESTAMP_MASK_3) >> 24),
                (byte)((timestamp & TIMESTAMP_MASK_2) >> 16),
                (byte)((timestamp & TIMESTAMP_MASK_1) >> 8),
                (byte)((timestamp & TIMESTAMP_MASK_0) >> 0)},
            randomness)
        {
        }

        public Ulid(byte[] timestamp, byte[] randomness)
        {
            const int TIMESTAMP_LENGTH = 6;
            const int RANDOMNESS_LENGTH = 10;

            if (timestamp.Length != TIMESTAMP_LENGTH)
            {
                throw new ArgumentException($"Expected a length of {TIMESTAMP_LENGTH}, received {timestamp.Length}", nameof(timestamp));
            }

            if (randomness.Length != RANDOMNESS_LENGTH)
            {
                throw new ArgumentException($"Expected a length of {RANDOMNESS_LENGTH}, received {randomness.Length}", nameof(randomness));
            }

            TimeStamp_0 = timestamp[0];
            TimeStamp_1 = timestamp[1];
            TimeStamp_2 = timestamp[2];
            TimeStamp_3 = timestamp[3];
            TimeStamp_4 = timestamp[4];
            TimeStamp_5 = timestamp[5];
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

        public Ulid(byte[] data)
            : this(new ReadOnlySpan<byte>(data)) { }

        public Ulid(ReadOnlySpan<byte> data)
        {
            const int DATA_LENGTH = 16;

            if (data.Length != DATA_LENGTH)
            {
                throw new ArgumentException($"Expected a length of {DATA_LENGTH}, received {data.Length}", nameof(data));
            }

            TimeStamp_0 = data[0];
            TimeStamp_1 = data[1];
            TimeStamp_2 = data[2];
            TimeStamp_3 = data[3];
            TimeStamp_4 = data[4];
            TimeStamp_5 = data[5];
            Randomness_0 = data[6];
            Randomness_1 = data[7];
            Randomness_2 = data[8];
            Randomness_3 = data[9];
            Randomness_4 = data[10];
            Randomness_5 = data[11];
            Randomness_6 = data[12];
            Randomness_7 = data[13];
            Randomness_8 = data[14];
            Randomness_9 = data[15];
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

                byte[] randomness = new byte[LastUsedRandomness.Length];

                if (timestamp == LastUsedTimeStamp)
                {
                    //Increment by one
                    LastUsedRandomness.CopyTo(randomness, 0);
                    AddOne(randomness);
                }
                else
                {
                    //Use Crypto random
                    using (RNGCryptoServiceProvider RNG = new RNGCryptoServiceProvider())
                    {
                        RNG.GetBytes(randomness);
                    }

                    LastUsedTimeStamp = timestamp;
                }
                
                randomness.CopyTo(LastUsedRandomness, 0);

                //Use explicit version, as the constructor have bounds checks
                return new Ulid()
                {
                    TimeStamp_0 = (byte)((timestamp & TIMESTAMP_MASK_5) >> 40),
                    TimeStamp_1 = (byte)((timestamp & TIMESTAMP_MASK_4) >> 32),
                    TimeStamp_2 = (byte)((timestamp & TIMESTAMP_MASK_3) >> 24),
                    TimeStamp_3 = (byte)((timestamp & TIMESTAMP_MASK_2) >> 16),
                    TimeStamp_4 = (byte)((timestamp & TIMESTAMP_MASK_1) >> 8),
                    TimeStamp_5 = (byte)((timestamp & TIMESTAMP_MASK_0) >> 0),
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

        private static void AddOne(byte[] bytes)
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

        public override string ToString()
        {
            int index0 = (TimeStamp_0 & 224) >> 5;
            char char0 = CrockfordsBase32[index0];

            int index1 = TimeStamp_0 & 31;
            char char1 = CrockfordsBase32[index1];

            int index2 = (TimeStamp_1 & 248) >> 3;
            char char2 = CrockfordsBase32[index2];

            int index3 = ((TimeStamp_1 & 7) << 2) | ((TimeStamp_2 & 192) >> 6);
            char char3 = CrockfordsBase32[index3];

            int index4 = (TimeStamp_2 & 62) >> 1;
            char char4 = CrockfordsBase32[index4];

            int index5 = ((TimeStamp_2 & 1) << 4) | ((TimeStamp_3 & 240) >> 4);
            char char5 = CrockfordsBase32[index5];

            int index6 = ((TimeStamp_3 & 15) << 1) | ((TimeStamp_4 & 128) >> 7);
            char char6 = CrockfordsBase32[index6];

            int index7 = (TimeStamp_4 & 124) >> 2;
            char char7 = CrockfordsBase32[index7];

            int index8 = ((TimeStamp_4 & 3) << 3) | ((TimeStamp_5 & 224) >> 5);
            char char8 = CrockfordsBase32[index8];

            int index9 = TimeStamp_5 & 31;
            char char9 = CrockfordsBase32[index9];

            int index10 = (Randomness_0 & 248) >> 3;
            char char10 = CrockfordsBase32[index10];

            int index11 = ((Randomness_0 & 7) << 2) | ((Randomness_1 & 192) >> 6);
            char char11 = CrockfordsBase32[index11];

            int index12 = (Randomness_1 & 62) >> 1;
            char char12 = CrockfordsBase32[index12];

            int index13 = ((Randomness_1 & 1) << 4) | ((Randomness_2 & 240) >> 4);
            char char13 = CrockfordsBase32[index13];

            int index14 = ((Randomness_2 & 15) << 1) | ((Randomness_3 & 128) >> 7);
            char char14 = CrockfordsBase32[index14];

            int index15 = (Randomness_3 & 124) >> 2;
            char char15 = CrockfordsBase32[index15];

            int index16 = ((Randomness_3 & 3) << 3) | ((Randomness_4 & 224) >> 5);
            char char16 = CrockfordsBase32[index16];

            int index17 = Randomness_4 & 31;
            char char17 = CrockfordsBase32[index17];

            int index18 = (Randomness_5 & 248) >> 3;
            char char18 = CrockfordsBase32[index18];

            int index19 = ((Randomness_5 & 7) << 2) | ((Randomness_6 & 192) >> 6);
            char char19 = CrockfordsBase32[index19];

            int index20 = (Randomness_6 & 62) >> 1;
            char char20 = CrockfordsBase32[index20];

            int index21 = ((Randomness_6 & 1) << 4) | ((Randomness_7 & 240) >> 4);
            char char21 = CrockfordsBase32[index21];

            int index22 = ((Randomness_7 & 15) << 1) | ((Randomness_8 & 128) >> 7);
            char char22 = CrockfordsBase32[index22];

            int index23 = (Randomness_8 & 124) >> 2;
            char char23 = CrockfordsBase32[index23];

            int index24 = ((Randomness_8 & 3) << 3) | ((Randomness_9 & 224) >> 5);
            char char24 = CrockfordsBase32[index24];

            int index25 = Randomness_9 & 31;
            char char25 = CrockfordsBase32[index25];

            return new string(new char[]
            {
                char0,
                char1,
                char2,
                char3,
                char4,
                char5,
                char6,
                char7,
                char8,
                char9,
                char10,
                char11,
                char12,
                char13,
                char14,
                char15,
                char16,
                char17,
                char18,
                char19,
                char20,
                char21,
                char22,
                char23,
                char24,
                char25,
            });
        }

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
            return this.ToString().CompareTo(other.ToString());
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

        public static bool TryParse(string input, out Ulid ulid)
        {
            if (input is null)
            {
                ulid = default;
                return false;
            }

            if (input.Length != VALID_ULID_STRING_LENGTH)
            {
                ulid = default;
                return false;
            }

            input = input.ToUpperInvariant();

            int[] index = new int[VALID_ULID_STRING_LENGTH];

            for (int i = 0; i < VALID_ULID_STRING_LENGTH; ++i)
            {
                char c = input[i];
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

            /*
             * // timestamp
             * ulid.data[0] = (dec[int(str[0])] << 5) | dec[int(str[1])];
             * ulid.data[1] = (dec[int(str[2])] << 3) | (dec[int(str[3])] >> 2);
             * ulid.data[2] = (dec[int(str[3])] << 6) | (dec[int(str[4])] << 1) | (dec[int(str[5])] >> 4);
             * ulid.data[3] = (dec[int(str[5])] << 4) | (dec[int(str[6])] >> 1);
             * ulid.data[4] = (dec[int(str[6])] << 7) | (dec[int(str[7])] << 2) | (dec[int(str[8])] >> 3);
             * ulid.data[5] = (dec[int(str[8])] << 5) | dec[int(str[9])];
             * ulid.data[6] = (dec[int(str[10])] << 3) | (dec[int(str[11])] >> 2);
             * ulid.data[7] = (dec[int(str[11])] << 6) | (dec[int(str[12])] << 1) | (dec[int(str[13])] >> 4);
             * ulid.data[8] = (dec[int(str[13])] << 4) | (dec[int(str[14])] >> 1);
             * ulid.data[9] = (dec[int(str[14])] << 7) | (dec[int(str[15])] << 2) | (dec[int(str[16])] >> 3);
             * ulid.data[10] = (dec[int(str[16])] << 5) | dec[int(str[17])];
             * ulid.data[11] = (dec[int(str[18])] << 3) | (dec[int(str[19])] >> 2);
             * ulid.data[12] = (dec[int(str[19])] << 6) | (dec[int(str[20])] << 1) | (dec[int(str[21])] >> 4);
             * ulid.data[13] = (dec[int(str[21])] << 4) | (dec[int(str[22])] >> 1);
             * ulid.data[14] = (dec[int(str[22])] << 7) | (dec[int(str[23])] << 2) | (dec[int(str[24])] >> 3);
             * ulid.data[15] = (dec[int(str[24])] << 5) | dec[int(str[25])];
             */

            //01D1R8YYT5BHVZ1MBM8RHG35AD
            //01D1R8YYT5BHVF1MBM8RHG35AD
            ulid = new Ulid
            {
                TimeStamp_0 = (byte)(index[0] << 5 | index[1]),
                TimeStamp_1 = (byte)(index[2] << 3 | index[3] >> 2),
                TimeStamp_2 = (byte)(index[3] << 6 | index[4] << 1 | index[5] >> 4),
                TimeStamp_3 = (byte)(index[5] << 4 | index[6] >> 1),
                TimeStamp_4 = (byte)(index[6] << 7 | index[7] << 2 | index[8] >> 3),
                TimeStamp_5 = (byte)(index[8] << 5 | index[9]),
                Randomness_0 = (byte)(index[10] << 3 | index[11] >> 2),
                Randomness_1 = (byte)(index[11] << 6 | index[12] << 1 | index[13] >> 4),
                Randomness_2 = (byte)(index[13] << 4 | index[14] >> 1),
                Randomness_3 = (byte)(index[14] << 7 | index[15] << 2 | index[16] >> 3),
                Randomness_4 = (byte)(index[16] << 5 | index[17]),
                Randomness_5 = (byte)(index[18] << 3 | index[19] >> 2),
                Randomness_6 = (byte)(index[19] << 6 | index[20] << 1 | index[21] >> 4),
                Randomness_7 = (byte)(index[21] << 4 | index[22] >> 1),
                Randomness_8 = (byte)(index[22] << 7 | index[23] << 2 | index[24] >> 3),
                Randomness_9 = (byte)(index[24] << 5 | index[25]),
            };

            return true;
        }
    }
}
