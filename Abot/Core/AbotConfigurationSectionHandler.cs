using Abot.Poco;
using System.Configuration;

namespace Abot.Core
{
    public class AbotConfigurationSectionHandler : ConfigurationSection
    {
        public AbotConfigurationSectionHandler()
        {
        }

        [ConfigurationProperty("crawlBehavior")]
        public CrawlBehaviorElement CrawlBehavior
        {
            get { return (CrawlBehaviorElement)this["crawlBehavior"]; }
        }

        [ConfigurationProperty("politeness")]
        public PolitenessElement Politeness
        {
            get { return (PolitenessElement)this["politeness"]; }
        }

        [ConfigurationProperty("extensionValues")]
        [ConfigurationCollection(typeof(ExtensionValueCollection), AddItemName = "add")]
        public ExtensionValueCollection ExtensionValues
        {
            get { return (ExtensionValueCollection)this["extensionValues"]; }
        }

        public CrawlConfiguration Convert()
        {
            AutoMapper.Mapper.CreateMap<CrawlBehaviorElement, CrawlConfiguration>();
            AutoMapper.Mapper.CreateMap<PolitenessElement, CrawlConfiguration>();


            CrawlConfiguration config = new CrawlConfiguration();
            AutoMapper.Mapper.Map<CrawlBehaviorElement, CrawlConfiguration>(CrawlBehavior, config);
            AutoMapper.Mapper.Map<PolitenessElement, CrawlConfiguration>(Politeness, config);

            foreach (ExtensionValueElement element in ExtensionValues)
                config.ConfigurationExtensions.Add(element.Key, element.Value);

            return config;
        }

        public static AbotConfigurationSectionHandler LoadFromXml()
        {
            return ((AbotConfigurationSectionHandler)System.Configuration.ConfigurationManager.GetSection("abot"));
        }
    }

    public class PolitenessElement : ConfigurationElement
    {
        [ConfigurationProperty("isThrottlingEnabled", IsRequired = true)]
        public bool IsThrottlingEnabled
        {
            get { return (bool)this["isThrottlingEnabled"]; }
        }

        [ConfigurationProperty("isRespectRobotsDotTextEnabled", IsRequired = true)]
        public bool IsRespectRobotsDotTextEnabled
        {
            get { return (bool)this["isRespectRobotsDotTextEnabled"]; }
        }

        [ConfigurationProperty("maxRobotsDotTextCrawlDelayInSeconds", IsRequired = true)]
        public int MaxRobotsDotTextCrawlDelayInSeconds
        {
            get { return (int)this["maxRobotsDotTextCrawlDelayInSeconds"]; }
        }

        [ConfigurationProperty("minCrawlDelayPerDomainMilliSeconds", IsRequired = true)]
        public long MinCrawlDelayPerDomainMilliSeconds
        {
            get { return (long)this["minCrawlDelayPerDomainMilliSeconds"]; }
        }
    }

    public class CrawlBehaviorElement : ConfigurationElement
    {
        [ConfigurationProperty("maxConcurrentThreads", IsRequired = true)]
        public int MaxConcurrentThreads
        {
            get { return (int)this["maxConcurrentThreads"]; }
        }

        [ConfigurationProperty("maxPagesToCrawl", IsRequired = true)]
        public int MaxPagesToCrawl
        {
            get { return (int)this["maxPagesToCrawl"]; }
        }

        [ConfigurationProperty("maxPagesToCrawlPerDomain", IsRequired = true)]
        public int MaxPagesToCrawlPerDomain
        {
            get { return (int)this["maxPagesToCrawlPerDomain"]; }
        }

        [ConfigurationProperty("maxPageSizeInBytes", IsRequired = true)]
        public int MaxPageSizeInBytes
        {
            get { return (int)this["maxPageSizeInBytes"]; }
        }

        [ConfigurationProperty("userAgentString", IsRequired = true)]
        public string UserAgentString
        {
            get { return (string)this["userAgentString"]; }
        }

        [ConfigurationProperty("crawlTimeoutSeconds", IsRequired = true)]
        public int CrawlTimeoutSeconds
        {
            get { return (int)this["crawlTimeoutSeconds"]; }
        }

        [ConfigurationProperty("downloadableContentTypes", IsRequired = true)]
        public string DownloadableContentTypes
        {
            get { return (string)this["downloadableContentTypes"]; }
        }

        [ConfigurationProperty("isUriRecrawlingEnabled", IsRequired = true)]
        public bool IsUriRecrawlingEnabled
        {
            get { return (bool)this["isUriRecrawlingEnabled"]; }
        }

        [ConfigurationProperty("isExternalPageCrawlingEnabled", IsRequired = true)]
        public bool IsExternalPageCrawlingEnabled
        {
            get { return (bool)this["isExternalPageCrawlingEnabled"]; }
        }

        [ConfigurationProperty("isExternalPageLinksCrawlingEnabled", IsRequired = true)]
        public bool IsExternalPageLinksCrawlingEnabled
        {
            get { return (bool)this["isExternalPageLinksCrawlingEnabled"]; }
        }
    }

    public class ExtensionValueElement : ConfigurationElement
    {
        [ConfigurationProperty("key", IsRequired = true, IsKey = true)]
        public string Key
        {
            get { return (string)this["key"]; }
        }

        [ConfigurationProperty("value", IsRequired = true, IsKey = false)]
        public string Value
        {
            get { return (string)this["value"]; }
        }

    }

    public class ExtensionValueCollection : ConfigurationElementCollection
    {
        public ExtensionValueElement this[int index]
        {
            get { return (ExtensionValueElement)BaseGet(index); }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ExtensionValueElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ExtensionValueElement)element).Key;
        }
    }
}
