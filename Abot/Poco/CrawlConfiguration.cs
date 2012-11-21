
using Abot.Core;
using System.Collections.Generic;
namespace Abot.Poco
{
    public class CrawlConfiguration
    {
        public CrawlConfiguration()
        {
            MaxConcurrentThreads = 10;
            UserAgentString = "abot v1.0 http://code.google.com/p/abot";
            MaxPagesToCrawl = 1000;
            ConfigurationExtensions = new Dictionary<string, string>();
        }

        public CrawlConfiguration(ConfigurationSectionHandler section)
        {
            //TODO convert section to this
            //TODO use automapper to copy the properties
            //TODO also see if anything else is manually mapping values
        }

        #region crawlBehavior

        /// <summary>
        /// Max concurrent threads to use for http requests
        /// </summary>
        public int MaxConcurrentThreads { get; set; }

        /// <summary>
        /// Maximum number of pages to crawl
        /// </summary>
        public long MaxPagesToCrawl { get; set; }

        public int MaxDomainDiscoveryLevel { get; set; }
        //    0 = internal links only, 
        //    1 = internal + external, 
        //    2 = internal + external + external
        //    IsWwwSameAsNonWww
        //    IsSubdomainSameAsRoot
        //    IsHttpSameAsHttps

        /// <summary>
        /// The user agent string to use for http requests
        /// </summary>
        public string UserAgentString { get; set; }

        /// <summary>
        /// Maximum seconds before the crawl times out and stops. A value of zero means no timeout
        /// </summary>
        public long CrawlTimeoutSeconds { get; set; }

        /// <summary>
        /// Dictionary that stores additional keyvalue pairs that can be accessed throught the crawl pipeline
        /// </summary>
        public Dictionary<string, string> ConfigurationExtensions { get; set; }

        public bool IsUriRecrawlingEnabled { get; set; }

        public string DownloadableContentTypes { get; set; }//text/html,application/xyz	
        
        #endregion

        #region politeness

        public bool IsThrottlingEnabled { get; set; }

        public long ManualCrawlDelayMilliSeconds { get; set; }
        
        #endregion
    }
}
