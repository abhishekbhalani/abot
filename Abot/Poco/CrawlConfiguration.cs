
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
            Data = new Dictionary<string, string>();
        }

        /// <summary>
        /// Max concurrent threads to use for http requests
        /// </summary>
        public int MaxConcurrentThreads { get; set; }

        /// <summary>
        /// The user agent string to use for http requests
        /// </summary>
        public string UserAgentString { get; set; }

        /// <summary>
        /// Maximum number of pages to crawl
        /// </summary>
        public long MaxPagesToCrawl { get; set; }

        /// <summary>
        /// Maximum seconds before the crawl times out and stops. A value of zero means no timeout
        /// </summary>
        public long CrawlTimeoutSeconds { get; set; }

        /// <summary>
        /// Dictionary that stores additional keyvalue pairs that can be accessed throught the crawl pipeline
        /// </summary>
        public Dictionary<string, string> Data { get; set; }
    }
}
