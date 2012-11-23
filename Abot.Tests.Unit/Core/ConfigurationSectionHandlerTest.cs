
using Abot.Core;
using Abot.Poco;
using NUnit.Framework;
namespace Abot.Tests.Unit.Core
{
    [TestFixture]
    public class ConfigurationSectionHandlerTest
    {
        ConfigurationSectionHandler _config = (ConfigurationSectionHandler)System.Configuration.ConfigurationManager.GetSection("abot");

        [Test]
        public void GetSetion_FillsConfigValuesFromAppConfigFile()
        {
            Assert.IsNotNull(_config.CrawlBehavior);
            Assert.AreEqual(44, _config.CrawlBehavior.CrawlTimeoutSeconds);
            Assert.AreEqual("bbbb", _config.CrawlBehavior.DownloadableContentTypes);
            Assert.AreEqual(true, _config.CrawlBehavior.IsUriRecrawlingEnabled); 
            Assert.AreEqual(11, _config.CrawlBehavior.MaxConcurrentThreads);
            Assert.AreEqual(33, _config.CrawlBehavior.MaxPagesToCrawl);
            Assert.AreEqual("aaaa", _config.CrawlBehavior.UserAgentString);
            Assert.AreEqual(true, _config.CrawlBehavior.IsExternalPageCrawlingEnabled);
            Assert.AreEqual(true, _config.CrawlBehavior.IsExternalPageLinksCrawlingEnabled);
            
            Assert.IsNotNull(_config.Politeness);
            Assert.AreEqual(true, _config.Politeness.IsThrottlingEnabled);
            Assert.AreEqual(55, _config.Politeness.ManualCrawlDelayMilliSeconds); 

            Assert.IsNotNull(_config.ExtensionValues);
            Assert.AreEqual("key1", _config.ExtensionValues[0].Key);
            Assert.AreEqual("key2", _config.ExtensionValues[1].Key);
            Assert.AreEqual("value1", _config.ExtensionValues[0].Value);
            Assert.AreEqual("value2", _config.ExtensionValues[1].Value);
        }

        [Test]
        public void Convert_CovertsFromSectionObjectToDtoObject()
        {
            CrawlConfiguration result = _config.Convert();

            Assert.IsNotNull(result);
            Assert.AreEqual(result.CrawlTimeoutSeconds, _config.CrawlBehavior.CrawlTimeoutSeconds);
            Assert.AreEqual(result.DownloadableContentTypes, _config.CrawlBehavior.DownloadableContentTypes);
            Assert.AreEqual(result.IsUriRecrawlingEnabled, _config.CrawlBehavior.IsUriRecrawlingEnabled);
            Assert.AreEqual(result.MaxConcurrentThreads, _config.CrawlBehavior.MaxConcurrentThreads);
            Assert.AreEqual(result.MaxPagesToCrawl, _config.CrawlBehavior.MaxPagesToCrawl);
            Assert.AreEqual(result.UserAgentString, _config.CrawlBehavior.UserAgentString);
            Assert.AreEqual(result.IsExternalPageCrawlingEnabled, _config.CrawlBehavior.IsExternalPageCrawlingEnabled);
            Assert.AreEqual(result.IsExternalPageLinksCrawlingEnabled, _config.CrawlBehavior.IsExternalPageLinksCrawlingEnabled);

            Assert.AreEqual(result.IsThrottlingEnabled, _config.Politeness.IsThrottlingEnabled);
            Assert.AreEqual(result.ManualCrawlDelayMilliSeconds, _config.Politeness.ManualCrawlDelayMilliSeconds);

            Assert.IsNotNull(result.ConfigurationExtensions);
            Assert.AreEqual(result.ConfigurationExtensions["key1"], _config.ExtensionValues[0].Value);
            Assert.AreEqual(result.ConfigurationExtensions["key2"], _config.ExtensionValues[1].Value);
        }
    }
}
