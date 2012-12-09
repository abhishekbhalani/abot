using Abot.Poco;
using NUnit.Framework;

namespace Abot.Tests.Unit.Poco
{
    [TestFixture]
    public class CrawlConfigurationTest
    {
        [Test]
        public void Constructor_ValidUri_CreatesInstance()
        {
            CrawlConfiguration unitUnderTest = new CrawlConfiguration();

            Assert.AreEqual(10, unitUnderTest.MaxConcurrentThreads);
            Assert.AreEqual("abot v1.1 http://code.google.com/p/abot", unitUnderTest.UserAgentString);
            Assert.AreEqual(1000, unitUnderTest.MaxPagesToCrawl);
        }
    }
}
