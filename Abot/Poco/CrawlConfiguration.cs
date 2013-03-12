using System.Collections.Generic;

namespace Abot.Poco
{
    public class CrawlConfiguration
    {
        public CrawlConfiguration()
        {
            MaxConcurrentThreads = 10;
            UserAgentString = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; abot v@ABOTASSEMBLYVERSION@ http://code.google.com/p/abot)";
            RobotsDotTextUserAgentString = "abot";
            MaxPagesToCrawl = 1000;
            DownloadableContentTypes = "text/html";
            ConfigurationExtensions = new Dictionary<string, string>();
            MaxRobotsDotTextCrawlDelayInSeconds = 5;
            HttpRequestMaxAutoRedirects = 7;
            IsHttpRequestAutoRedirectsEnabled = true;
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
        /// Maximum size of page. If the page size is above this value, it will not be downloaded or processed
        /// </summary>
        public long MaxPageSizeInBytes { get; set; }

        /// <summary>
        /// The maximum numer of seconds to respect in the robots.txt "Crawl-delay: X" directive. If set to 0 will always follow this directive no matter how high the value. 
        /// IsRespectRobotsDotTextEnabled must be true for this value to be used.
        /// </summary>
        public int MaxRobotsDotTextCrawlDelayInSeconds { get; set; }

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
        /// A comma seperated string that has content types that should have their page content downloaded. For each page, the content type is checked to see if it contains any of the values defined here.
        /// </summary>
        public string DownloadableContentTypes { get; set; } 

        /// <summary>
        /// Gets or sets the maximum number of concurrent connections allowed by a System.Net.ServicePoint. The system default is 2. This means that only 2 concurrent http connections can be open to the same host.
        /// If zero, this setting has no effect.
        /// </summary>
        public int HttpServicePointConnectionLimit { get; set; }

        /// <summary>
        /// Gets or sets the time-out value in milliseconds for the System.Net.HttpWebRequest.GetResponse() and System.Net.HttpWebRequest.GetRequestStream() methods.
        /// If zero, this setting has no effect.
        /// </summary>
        public int HttpRequestTimeoutInSeconds { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of redirects that the request follows.
        /// If zero, this setting has no effect.
        /// </summary>
        public int HttpRequestMaxAutoRedirects { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the request should follow redirection
        /// </summary>
        public bool IsHttpRequestAutoRedirectsEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates gzip and deflate will be automatically accepted and decompressed
        /// </summary>
        public bool IsHttpRequestAutomaticDecompressionEnabled { get; set; }

        #endregion

        #region politeness

        /// <summary>
        /// Whether the crawler should attempt to slow down http web requests if it detects the website is under stress.
        /// </summary>
        public bool IsThrottlingEnabled { get; set; }

        /// <summary>
        /// Whether the crawler should retrieve and respect the robotsdottext file.
        /// </summary>
        public bool IsRespectRobotsDotTextEnabled { get; set; }

        /// <summary>
        /// The user agent string to use when checking robots.txt file for specific directives.  Some examples of other crawler's user agent values are "googlebot", "slurp" etc...
        /// </summary>
        public string RobotsDotTextUserAgentString { get; set; }

        /// <summary>
        /// The number of milliseconds to wait in between http requests to the same domain. Note: This will set the crawl to a single thread no matter what the MaxConcurrentThreads value is.
        /// </summary>
        public long MinCrawlDelayPerDomainMilliSeconds { get; set; }
        
        #endregion
    }
}
