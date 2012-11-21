using System.Configuration;

namespace Abot.Core
{
    public class ConfigurationSectionHandler : ConfigurationSection
    {
        public ConfigurationSectionHandler()
        {
        }

        [ConfigurationProperty("crawlBehavior")]
        public CrawlBehaviorElement CrawlBehavior
        {
            get
            {
                return (CrawlBehaviorElement)this["crawlBehavior"];
            }
            set
            { this["crawlBehavior"] = value; }
        }

        [ConfigurationProperty("politeness")]
        public PolitenessElement Politeness
        {
            get
            {
                return (PolitenessElement)this["politeness"];
            }
            set
            { this["politeness"] = value; }
        }

        [ConfigurationProperty("extensionValues")]
        [ConfigurationCollection(typeof(ExtensionValueCollection), AddItemName = "add")]
        public ExtensionValueCollection ExtensionValues
        {
            get
            {
                return (ExtensionValueCollection)this["extensionValues"];
            }
            set
            { this["extensionValues"] = value; }
        }
    }

    public class PolitenessElement : ConfigurationElement
    {
        [ConfigurationProperty("isThrottlingEnabled", DefaultValue = "false", IsRequired = true)]
        public bool IsThrottlingEnabled
        {
            get { return (bool)this["isThrottlingEnabled"]; }
            set { this["isThrottlingEnabled"] = value; }
        }

        [ConfigurationProperty("manualCrawlDelayMilliSeconds", DefaultValue = "0", IsRequired = true)]
        [LongValidator]
        public long ManualCrawlDelayMilliSeconds
        {
            get { return (long)this["manualCrawlDelayMilliSeconds"]; }
            set { this["manualCrawlDelayMilliSeconds"] = value; }
        }
    }

    public class CrawlBehaviorElement : ConfigurationElement
    {
        [ConfigurationProperty("maxConcurrentThreads", DefaultValue = "10", IsRequired = true)]
        [IntegerValidator(MinValue = 1, MaxValue = 100)]
        public int MaxConcurrentThreads
        {
            get { return (int)this["maxConcurrentThreads"]; }
            set { this["maxConcurrentThreads"] = value; }
        }

        [ConfigurationProperty("maxDomainDiscoveryLevel", DefaultValue = "0", IsRequired = true)]
        [IntegerValidator]
        public int MaxDomainDiscoveryLevel
        {
            get { return (int)this["maxDomainDiscoveryLevel"]; }
            set { this["maxDomainDiscoveryLevel"] = value; }
        }

        [ConfigurationProperty("maxPagesToCrawl", DefaultValue = "1000", IsRequired = true)]
        [IntegerValidator]
        public int MaxPagesToCrawl
        {
            get { return (int)this["maxPagesToCrawl"]; }
            set { this["maxPagesToCrawl"] = value; }
        }

        [ConfigurationProperty("userAgentString", DefaultValue = "", IsRequired = true)]
        public string UserAgentString
        {
            get { return (string)this["userAgentString"]; }
            set { this["userAgentString"] = value; }
        }

        [ConfigurationProperty("crawlTimeoutSeconds", DefaultValue = "0", IsRequired = true)]
        public int CrawlTimeoutSeconds
        {
            get { return (int)this["crawlTimeoutSeconds"]; }
            set { this["crawlTimeoutSeconds"] = value; }
        }

        [ConfigurationProperty("downloadableContentTypes", DefaultValue = "text/html", IsRequired = true)]
        public string DownloadableContentTypes
        {
            get { return (string)this["downloadableContentTypes"]; }
            set { this["downloadableContentTypes"] = value; }
        }

        [ConfigurationProperty("isUriRecrawlingEnabled", DefaultValue = "false", IsRequired = true)]
        public bool IsUriRecrawlingEnabled
        {
            get { return (bool)this["isUriRecrawlingEnabled"]; }
            set { this["isUriRecrawlingEnabled"] = value; }
        }
    }

    public class ExtensionValueElement : ConfigurationElement
    {
        [ConfigurationProperty("key", DefaultValue = "", IsRequired = true, IsKey = true)]
        public string Key
        {
            get { return (string)this["key"]; }
            set { this["key"] = value; }
        }

        [ConfigurationProperty("value", DefaultValue = "", IsRequired = true, IsKey = false)]
        public string Value
        {
            get { return (string)this["value"]; }
            set { this["value"] = value; }
        }

    }

    public class ExtensionValueCollection : ConfigurationElementCollection
    {
        public ExtensionValueElement this[int index]
        {
            get { return (ExtensionValueElement)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public void Add(ExtensionValueElement serviceConfig)
        {
            BaseAdd(serviceConfig);
        }

        //public void Clear()
        //{
        //    BaseClear();
        //}

        protected override ConfigurationElement CreateNewElement()
        {
            return new ExtensionValueElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ExtensionValueElement)element).Key;
        }

        //public void Remove(ServiceConfig serviceConfig)
        //{
        //    BaseRemove(serviceConfig.Port);
        //}

        //public void RemoveAt(int index)
        //{
        //    BaseRemoveAt(index);
        //}

        //public void Remove(string name)
        //{
        //    BaseRemove(name);
        //}
    }
}
