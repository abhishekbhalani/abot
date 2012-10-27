using Abot.Poco;
using NUnit.Framework;

namespace Abot.Tests.Poco
{
    [TestFixture]
    public class CrawlContextTest
    {
        [Test]
        public void Constructor_ValidUri_CreatesInstance()
        {
            CrawlContext unitUnderTest = new CrawlContext();
            Assert.AreEqual(null, unitUnderTest.RootUri);
        }   
    }
}
