using System;
using System.Security.Cryptography;

namespace CSharp.Ulid
{
    public struct Ulid : IComparable, IComparable<Ulid>, IEquatable<Ulid>
    {
        public byte TimeStamp_0 { get; set; }
        public byte TimeStamp_1 { get; set; }
        public byte TimeStamp_2 { get; set; }
        public byte TimeStamp_3 { get; set; }
        public byte TimeStamp_4 { get; set; }
        public byte TimeStamp_5 { get; set; }

        public long TimeStamp { get; set; }

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

        private static readonly object LOCK = "THREAD_SAFE_LOCK";

        private static readonly DateTime EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        private const long TIMESTAMP_MASK_0 = 255;
        private const long TIMESTAMP_MASK_1 = 65280;
        private const long TIMESTAMP_MASK_2 = 16711680;
        private const long TIMESTAMP_MASK_3 = 4278190080;
        private const long TIMESTAMP_MASK_4 = 1095216660480;
        private const long TIMESTAMP_MASK_5 = 280375465082880;

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

                byte[] randomness = new byte[10];

                if (timestamp == LastUsedTimeStamp)
                {
                    //Increment by one
                    byte[] lastUsedRandomness = AddOne(LastUsedRandomness);
                    lastUsedRandomness.CopyTo(randomness, 0);
                }
                else
                {
                    //Use Crypto random
                    using (RNGCryptoServiceProvider RNG = new RNGCryptoServiceProvider())
                    {
                        RNG.GetBytes(randomness);
                    }
                }

                LastUsedTimeStamp = timestamp;
                randomness.CopyTo(LastUsedRandomness, 0);

                return new Ulid
                {
                    TimeStamp_0 = (byte)(timestamp & TIMESTAMP_MASK_0 >> 0),
                    TimeStamp_1 = (byte)(timestamp & TIMESTAMP_MASK_1 >> 8),
                    TimeStamp_2 = (byte)(timestamp & TIMESTAMP_MASK_2 >> 16),
                    TimeStamp_3 = (byte)(timestamp & TIMESTAMP_MASK_3 >> 24),
                    TimeStamp_4 = (byte)(timestamp & TIMESTAMP_MASK_4 >> 32),
                    TimeStamp_5 = (byte)(timestamp & TIMESTAMP_MASK_5 >> 40),
                    TimeStamp = timestamp,
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

        private static byte[] AddOne(byte[] bytes)
        {
            byte[] data = new byte[bytes.Length];
            bytes.CopyTo(data, 0);
            AddOne(data, bytes.Length - 1);
            return data;
        }

        private static void AddOne(byte[] data, int index)
        {
            if (index < 0)
            {
                throw new OverflowException($"{nameof(LastUsedRandomness)} overflowed within same millisecond");
            }

            if (data[index] == byte.MaxValue)
            {
                data[index] = 0;
                AddOne(data, index - 1);
            }
            else
            {
                ++data[index];
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
            return new byte[]
            {
                this.TimeStamp_0,
                this.TimeStamp_1,
                this.TimeStamp_2,
                this.TimeStamp_3,
                this.TimeStamp_4,
                this.TimeStamp_5,
                this.Randomness_0,
                this.Randomness_1,
                this.Randomness_2,
                this.Randomness_3,
                this.Randomness_4,
                this.Randomness_5,
                this.Randomness_6,
                this.Randomness_7,
                this.Randomness_8,
                this.Randomness_9,
            };
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

        public static explicit operator byte[] (Ulid ulid)
        {
            return ulid.ToByteArray();
        }
    }
}
