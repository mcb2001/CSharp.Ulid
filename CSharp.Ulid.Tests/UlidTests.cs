using Xunit;
using System.Threading;

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
