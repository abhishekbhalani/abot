using Abot.Poco;
using NUnit.Framework;

namespace Abot.Tests.Unit.Poco
{
    [TestFixture]
    public class CrawlContextTest
    {
        [Test]
        public void Constructor_ValidUri_CreatesInstance()
        {
            CrawlContext unitUnderTest = new CrawlContext();
            Assert.AreEqual(null, unitUnderTest.RootUri);
            Assert.IsNotNull(unitUnderTest.CrawledUrls);
            Assert.AreEqual(0, unitUnderTest.CrawledUrls.Count);
            Assert.IsNull(unitUnderTest.CrawlConfiguration);
        }   
    }
}
