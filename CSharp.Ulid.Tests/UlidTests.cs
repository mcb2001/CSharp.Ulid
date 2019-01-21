using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace CSharp.Ulid.Tests
{
    [TestClass]
    public class UlidTests
    {
        private const int ULID_LENGTH = 26;
        private const int TEST_COUNT = 1000;
        private readonly Ulid[] ulids = new Ulid[TEST_COUNT];

        public UlidTests()
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

        [TestMethod]
        public void TestGenerate()
        {
            for (int i = 0; i < TEST_COUNT - 1; ++i)
            {
                TestUlidAgainstOther(ulids[i], ulids[i + 1]);
            }
        }

        [TestMethod]
        public void TestTryParseExceptions()
        {
            //one too few chars
            Assert.IsFalse(Ulid.TryParse("0000000000000000000000000", out Ulid error0));

            //one too many chars
            Assert.IsFalse(Ulid.TryParse("000000000000000000000000000", out Ulid error1));

            //null
            Assert.IsFalse(Ulid.TryParse(null, out Ulid error2));

            //Invalid char
            Assert.IsFalse(Ulid.TryParse("000*0000000000000000000000", out Ulid error3));
        }

        [TestMethod]
        public void TestTryParseDefault()
        {
            Assert.IsTrue(Ulid.TryParse("00000000000000000000000000", out Ulid valid0));
            Assert.AreEqual(default(Ulid), valid0);
        }

        [TestMethod]
        private void TestTryParse()
        {
            for (int i = 0; i < TEST_COUNT; ++i)
            {
                Ulid expected = ulids[i];
                Assert.IsTrue(Ulid.TryParse(expected.ToString(), out Ulid actual));
                Assert.AreEqual<Ulid>(expected, actual);
            }
        }

        private void TestUlidAgainstOther(Ulid a, Ulid b)
        {
            string sa = a.ToString();
            string sb = b.ToString();

            Assert.AreEqual<int>(ULID_LENGTH, sa.Length);
            Assert.AreEqual<int>(ULID_LENGTH, sb.Length);

            if (a.TimeStamp == b.TimeStamp)
            {
                Assert.AreEqual(sa.Substring(0, 9), sb.Substring(0, 9));
            }
        }
    }
}
