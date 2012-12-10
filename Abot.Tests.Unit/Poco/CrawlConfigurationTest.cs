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

            Assert.IsNotNull(unitUnderTest.ConfigurationExtensions);
            Assert.AreEqual(0, unitUnderTest.ConfigurationExtensions.Count);
            Assert.AreEqual(0, unitUnderTest.CrawlTimeoutSeconds);
            Assert.AreEqual("text/html", unitUnderTest.DownloadableContentTypes);
            Assert.AreEqual(false, unitUnderTest.IsExternalPageCrawlingEnabled);
            Assert.AreEqual(false, unitUnderTest.IsExternalPageLinksCrawlingEnabled);
            Assert.AreEqual(false, unitUnderTest.IsThrottlingEnabled);
            Assert.AreEqual(false, unitUnderTest.IsUriRecrawlingEnabled);
            Assert.AreEqual(10, unitUnderTest.MaxConcurrentThreads);
            Assert.AreEqual(1000, unitUnderTest.MaxPagesToCrawl);
            Assert.AreEqual(0, unitUnderTest.MaxPagesToCrawlPerDomain);
            Assert.AreEqual(0, unitUnderTest.MinCrawlDelayPerDomainMilliSeconds);
            Assert.AreEqual("abot v1.1 http://code.google.com/p/abot", unitUnderTest.UserAgentString);
        }
    }
}
