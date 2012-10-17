using Abot.Core;
using Abot.Poco;
using NUnit.Framework;

namespace Abot.Tests.Core
{
    [TestFixture]
    public class CrawlConfigurationProviderTest
    {
        CrawlConfigurationProvider _unitUnderTest;

        [SetUp]
        public void SetUp()
        {
            _unitUnderTest = new CrawlConfigurationProvider();
        }

        [Test]
        public void GetConfiguration_ReturnsDefaultConfiguration()
        {
            CrawlConfiguration defaultConfiguration = new CrawlConfiguration();

            CrawlConfiguration actualConfiguration = _unitUnderTest.GetConfiguration();

            Assert.AreEqual(defaultConfiguration.MaxConcurrentThreads, actualConfiguration.MaxConcurrentThreads);
            Assert.AreEqual(defaultConfiguration.UserAgentString, actualConfiguration.UserAgentString);
        }
    }
}
