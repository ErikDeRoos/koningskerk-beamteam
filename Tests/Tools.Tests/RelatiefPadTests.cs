using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tools.Tests
{
    class RelatiefPadTests
    {
        [TestInitialize]
        public void Initialise()
        {
        }

        [TestClass]
        public class ReplaceWithRelativePathMethod : RelatiefPadTests
        {
            [DataTestMethod]
            [DataRow(@"C:\program\ProgramDir", @"C:\program\", @".ProgramDir")]
            [DataRow(@"D:\program\ProgramDir", @"C:\program\", @"D:\program\ProgramDir")]
            [DataRow(@"C:\program\tool\", @"C:\program\exe\", @"...\tool")]
            //[DataRow(@"C:\program\tool 2", @"C:\program\tool\", @".ProgramDir")]
            public void Klopt(string path, string exePath, string expectedResult)
            {
                var relativePath = RelatiefPad.ReplaceWithRelativePath(exePath, path);

                Assert.AreEqual(expectedResult, relativePath);
            }
        }

        [TestClass]
        public class ReplaceWithNormalPathMethod : RelatiefPadTests
        {
            [DataTestMethod]
            [DataRow(@".ProgramDir", @"C:\program\", @"C:\program\ProgramDir")]
            [DataRow(@"D:\program\ProgramDir", @"C:\program\", @"D:\program\ProgramDir")]
            public void Klopt(string path, string exePath, string expectedResult)
            {
                var normalPath = RelatiefPad.ReplaceWithNormalPath(exePath, path);

                Assert.AreEqual(expectedResult, normalPath);
            }
        }
    }
}
