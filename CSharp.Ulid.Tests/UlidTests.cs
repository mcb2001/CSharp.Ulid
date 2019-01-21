using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSharp.Ulid.Tests
{
    [TestClass]
    public class UlidTests
    {
        private const int ULID_LENGTH = 26;
        private const int TEST_COUNT = 100;

        [TestMethod]
        public void TestGenerate()
        {
            Ulid[] ulids = new Ulid[TEST_COUNT];

            for (int i = 0; i < TEST_COUNT; ++i)
            {
                ulids[i] = Ulid.NewUlid();
            }

            for (int i = 0; i < TEST_COUNT - 1; ++i)
            {
                TestUlidAgainstOther(ulids[i], ulids[i + 1]);
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
