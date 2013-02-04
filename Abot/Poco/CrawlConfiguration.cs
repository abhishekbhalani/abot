using System.Collections.Generic;

namespace Abot.Poco
{
    public class CrawlConfiguration
    {
        public CrawlConfiguration()
        {
            MaxConcurrentThreads = 10;
            UserAgentString = "abot v1.1 http://code.google.com/p/abot";
            MaxPagesToCrawl = 1000;
            DownloadableContentTypes = "text/html";
            ConfigurationExtensions = new Dictionary<string, string>();
            ShouldLoadHtmlAgilityPackForEachCrawledPage = true;//required for the default IHyperlinkParser HapHyperlinkParser
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

        /// <summary>
        /// Maximum number of pages to crawl per domain
        /// </summary>
        public long MaxPagesToCrawlPerDomain { get; set; }

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

        /// <summary>
        /// Whether Uris should be crawled more than once. This is not common and should be false for most scenarios
        /// </summary>
        public bool IsUriRecrawlingEnabled { get; set; }

        /// <summary>
        /// Whether pages external to the root uri should be crawled
        /// </summary>
        public bool IsExternalPageCrawlingEnabled { get; set; }

        /// <summary>
        /// Whether pages external to the root uri should have their links crawled. NOTE: IsExternalPageCrawlEnabled must be true for this setting to have any effect
        /// </summary>
        public bool IsExternalPageLinksCrawlingEnabled { get; set; }

        /// <summary>
        // Whether an Html Agility Pack HtmlDocument is loaded with the raw content of each page. . Allows you to use CrawledPage.HtmlDocument to search/modify raw html.
        /// </summary>
        public bool ShouldLoadHtmlAgilityPackForEachCrawledPage { get; set; }

        /// <summary>
        // Whether a CsQuery CQ is loaded with the raw content of each page. Allows you to use CrawledPage.CsQueryDocument to search/modify raw html.
        /// </summary>
        public bool ShouldLoadCsQueryForEachCrawledPage { get; set; }

        /// <summary>
        /// A comma seperated string that has content types that should have their page content downloaded. For each page, the content type is checked to see if it contains any of the values defined here.
        /// </summary>
        public string DownloadableContentTypes { get; set; }
        
        #endregion

        #region politeness

        /// <summary>
        /// Whether the crawler should attempt to slow down http web requests if it detects the website is under stress.
        /// </summary>
        public bool IsThrottlingEnabled { get; set; }

        /// <summary>
        /// The number of milliseconds to wait in between http requests to the same domain. Note: This will set the crawl to a single thread no matter what the MaxConcurrentThreads value is.
        /// </summary>
        public long MinCrawlDelayPerDomainMilliSeconds { get; set; }
        
        #endregion
    }
}
