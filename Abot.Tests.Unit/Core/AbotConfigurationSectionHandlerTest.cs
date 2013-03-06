
using Abot.Core;
using Abot.Poco;
using NUnit.Framework;
namespace Abot.Tests.Unit.Core
{
    [TestFixture]
    public class AbotConfigurationSectionHandlerTest
    {
        AbotConfigurationSectionHandler _config = AbotConfigurationSectionHandler.LoadFromXml();

        [Test]
        public void GetSetion_FillsConfigValuesFromAppConfigFile()
        {
            Assert.IsNotNull(_config.CrawlBehavior);
            Assert.AreEqual(44, _config.CrawlBehavior.CrawlTimeoutSeconds);
            Assert.AreEqual("bbbb", _config.CrawlBehavior.DownloadableContentTypes);
            Assert.AreEqual(true, _config.CrawlBehavior.IsUriRecrawlingEnabled); 
            Assert.AreEqual(11, _config.CrawlBehavior.MaxConcurrentThreads);
            Assert.AreEqual(33, _config.CrawlBehavior.MaxPagesToCrawl);
            Assert.AreEqual(333, _config.CrawlBehavior.MaxPagesToCrawlPerDomain);
            Assert.AreEqual(4444, _config.CrawlBehavior.MaxPageSizeInBytes);
            Assert.AreEqual("aaaa", _config.CrawlBehavior.UserAgentString);
            Assert.AreEqual(true, _config.CrawlBehavior.IsExternalPageCrawlingEnabled);
            Assert.AreEqual(true, _config.CrawlBehavior.IsExternalPageLinksCrawlingEnabled);
            
            Assert.IsNotNull(_config.Politeness);
            Assert.AreEqual(true, _config.Politeness.IsThrottlingEnabled);
            Assert.AreEqual(true, _config.Politeness.IsRespectRobotsDotTextEnabled);
            Assert.AreEqual("zzzz", _config.Politeness.RobotsDotTextUserAgentString);
            Assert.AreEqual(55, _config.Politeness.MinCrawlDelayPerDomainMilliSeconds);
            Assert.AreEqual(5, _config.Politeness.MaxRobotsDotTextCrawlDelayInSeconds); 

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
            Assert.AreEqual(result.MaxPagesToCrawlPerDomain, _config.CrawlBehavior.MaxPagesToCrawlPerDomain);
            Assert.AreEqual(result.MaxPageSizeInBytes, _config.CrawlBehavior.MaxPageSizeInBytes);
            Assert.AreEqual(result.UserAgentString, _config.CrawlBehavior.UserAgentString);
            Assert.AreEqual(result.IsExternalPageCrawlingEnabled, _config.CrawlBehavior.IsExternalPageCrawlingEnabled);
            Assert.AreEqual(result.IsExternalPageLinksCrawlingEnabled, _config.CrawlBehavior.IsExternalPageLinksCrawlingEnabled);

            Assert.AreEqual(result.IsThrottlingEnabled, _config.Politeness.IsThrottlingEnabled);
            Assert.AreEqual(result.IsRespectRobotsDotTextEnabled, _config.Politeness.IsRespectRobotsDotTextEnabled);
            Assert.AreEqual(result.RobotsDotTextUserAgentString, _config.Politeness.RobotsDotTextUserAgentString);
            Assert.AreEqual(result.MinCrawlDelayPerDomainMilliSeconds, _config.Politeness.MinCrawlDelayPerDomainMilliSeconds);
            Assert.AreEqual(result.MaxRobotsDotTextCrawlDelayInSeconds, _config.Politeness.MaxRobotsDotTextCrawlDelayInSeconds);

            Assert.IsNotNull(result.ConfigurationExtensions);
            Assert.AreEqual(result.ConfigurationExtensions["key1"], _config.ExtensionValues[0].Value);
            Assert.AreEqual(result.ConfigurationExtensions["key2"], _config.ExtensionValues[1].Value);
        }
    }
}
