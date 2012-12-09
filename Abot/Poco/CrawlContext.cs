using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Abot.Poco
{
    public class CrawlContext
    {
        public CrawlContext()
        {
            CrawledUrls = new ConcurrentBag<string>();
            CrawlCountByDomain = new Dictionary<string, int>();
        }

        /// <summary>
        /// The root of the crawl
        /// </summary>
        public Uri RootUri { get; set; }

        /// <summary>
        /// The datetime of the last unsuccessful http status (non 200) was requested
        /// </summary>
        public DateTime CrawlStartDate { get; set; }

        ///// <summary>
        ///// The datetime of the last successful http status (200) was requested
        ///// </summary>
        //public DateTime LastSuccessfulHttpRequestDate { get; set; }

        ///// <summary>
        ///// The datetime of the last unsuccessful http status (non 200) was requested
        ///// </summary>
        //public DateTime LastUnsuccessfulHttpRequestDate { get; set; }

        /// <summary>
        /// Threadsafe collection of urls that have been crawled
        /// </summary>
        public ConcurrentBag<string> CrawledUrls { get; set; }
        
        /// <summary>
        /// Threadsafe dictionary of domains and how many pages were crawled in that domain
        /// </summary>
        public Dictionary<string, int> CrawlCountByDomain { get; set; }

        /// <summary>
        /// Configuration values used to determine crawl settings
        /// </summary>
        public CrawlConfiguration CrawlConfiguration { get; set; }
    }
}
