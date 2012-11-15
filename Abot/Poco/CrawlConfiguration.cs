
namespace Abot.Poco
{
    public class CrawlConfiguration
    {
        public CrawlConfiguration()
        {
            MaxConcurrentThreads = 10;
            UserAgentString = "abot v1.0 http://code.google.com/p/abot";
            MaxPagesToCrawl = 1000;
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
    }
}
