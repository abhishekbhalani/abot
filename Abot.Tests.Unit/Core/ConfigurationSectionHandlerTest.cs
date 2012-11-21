
using Abot.Core;
using NUnit.Framework;
namespace Abot.Tests.Unit.Core
{
    [TestFixture]
    public class ConfigurationSectionHandlerTest
    {
        [Test]
        public void GetSetion_FillsConfigValuesFromAppConfigFile()
        {
            ConfigurationSectionHandler config = (ConfigurationSectionHandler)System.Configuration.ConfigurationManager.GetSection("abot");

            Assert.IsNotNull(config.CrawlBehavior);
            Assert.AreEqual(44, config.CrawlBehavior.CrawlTimeoutSeconds);
            Assert.AreEqual("bbbb", config.CrawlBehavior.DownloadableContentTypes);
            Assert.AreEqual(true, config.CrawlBehavior.IsUriRecrawlingEnabled); 
            Assert.AreEqual(11, config.CrawlBehavior.MaxConcurrentThreads);
            Assert.AreEqual(22, config.CrawlBehavior.MaxDomainDiscoveryLevel);
            Assert.AreEqual(33, config.CrawlBehavior.MaxPagesToCrawl);
            Assert.AreEqual("aaaa", config.CrawlBehavior.UserAgentString);
            
            Assert.IsNotNull(config.Politeness);
            Assert.AreEqual(true, config.Politeness.IsThrottlingEnabled);
            Assert.AreEqual(55, config.Politeness.ManualCrawlDelayMilliSeconds); 

            Assert.IsNotNull(config.ExtensionValues);
            Assert.AreEqual("key1", config.ExtensionValues[0].Key);
            Assert.AreEqual("key2", config.ExtensionValues[1].Key);
            Assert.AreEqual("value1", config.ExtensionValues[0].Value);
            Assert.AreEqual("value2", config.ExtensionValues[1].Value);
        }
    }
}
