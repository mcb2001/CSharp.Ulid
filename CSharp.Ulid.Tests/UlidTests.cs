using System;
using System.Threading;
using Xunit;

namespace CSharp.Ulid.Tests
{
    public class UlidTest
    {
        private const int ULID_LENGTH = 26;
        private const int TEST_COUNT = 1000;
        private readonly Ulid[] ulids = new Ulid[TEST_COUNT];

        public UlidTest()
        {
            for (int i = 0; i < TEST_COUNT / 2; ++i)
            {
                ulids[i] = Ulid.NewUlid();
            }

            Thread.Sleep(100); //Ensure not all are at the same millisecond

            for (int i = TEST_COUNT / 2; i < TEST_COUNT; ++i)
            {
                ulids[i] = Ulid.NewUlid();
            }
        }

        [Fact]
        public void TestGenerate()
        {
            for (int i = 0; i < TEST_COUNT - 1; ++i)
            {
                TestUlidAgainstOther(ulids[i], ulids[i + 1]);
            }
        }

        [Fact]
        public void TestTryParseExceptions()
        {
            //one too few chars
            Assert.False(Ulid.TryParse("0000000000000000000000000", out _));

            //one too many chars
            Assert.False(Ulid.TryParse("000000000000000000000000000", out _));

            //null
            Assert.False(Ulid.TryParse(null, out _));

            //Invalid char
            Assert.False(Ulid.TryParse("000*0000000000000000000000", out _));
        }

        [Fact]
        public void TestTryParseDefault()
        {
            Assert.True(Ulid.TryParse("00000000000000000000000000", out Ulid valid0));
            Assert.Equal(default, valid0);
        }

        [Fact]
        private void TestTryParse()
        {
            for (int i = 0; i < TEST_COUNT; ++i)
            {
                Ulid expected = ulids[i];
                Assert.True(Ulid.TryParse(expected.ToString(), out Ulid actual));
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        private void TestConstructFromByteArray()
        {
            byte[] array1 = Ulid.NewUlid().ToByteArray();
            byte[] array2 = Ulid.NewUlid().ToByteArray();

            Assert.Equal(array1, new Ulid(array1).ToByteArray());
            Assert.Equal(array2, new Ulid(array2).ToByteArray());
        }

        [Fact]
        private void TestConstructFromROS()
        {
            ReadOnlySpan<byte> span1 = Ulid.NewUlid().ToByteArray().AsSpan();
            ReadOnlySpan<byte> span2 = Ulid.NewUlid().ToByteArray().AsSpan();

            byte[] result1 = new Ulid(span1).ToByteArray();
            byte[] result2 = new Ulid(span2).ToByteArray();

            // We need to manually test byte-by-byte, because Span/ReadOnlySpan is not allowed as generic type (for Assert.Equal<T>()).
            for (int i = 0; i < 16; ++i)
            {
                Assert.True(span1[i] == result1[i], $"Expected {span1[i]} at index {i} but found {result1[i]}");
                Assert.True(span2[i] == result2[i], $"Expected {span2[i]} at index {i} but found {result2[i]}");
            }
        }

        [Fact]
        private void TestTryWriteBytesValid()
        {
            Ulid ulid = Ulid.NewUlid();
            byte[] bytes = ulid.ToByteArray();
            Span<byte> resultBuffer = stackalloc byte[100];

            int offset = 55;
            Assert.True(ulid.TryWriteBytes(resultBuffer.Slice(start: offset)));

            // We need to manually test byte-by-byte, because Span/ReadOnlySpan is not allowed as generic type (for Assert.Equal<T>())..
            for (int i = 0, j = offset; i < 16; ++i, ++j)
            {
                Assert.True(bytes[i] == resultBuffer[j], $"Expected {bytes[i]} at index {j} but found {resultBuffer[j]}");
            }
        }

        [Fact]
        private void TestTryWriteBytesInvalid()
        {
            Ulid ulid = Ulid.NewUlid();
            Span<byte> resultBuffer = stackalloc byte[100];

            int offset = 92; // invalid offset
            Assert.False(ulid.TryWriteBytes(resultBuffer.Slice(start: offset)));
        }

        private void TestUlidAgainstOther(Ulid a, Ulid b)
        {
            string sa = a.ToString();
            string sb = b.ToString();

            Assert.Equal(ULID_LENGTH, sa.Length);
            Assert.Equal(ULID_LENGTH, sb.Length);

            if (a.TimeStamp == b.TimeStamp)
            {
                Assert.Equal(sa.Substring(0, 9), sb.Substring(0, 9));
            }
        }
    }
}
