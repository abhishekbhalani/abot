using System;
using System.Collections.Generic;

namespace Abot.Poco
{
    public class CrawlContext
    {
        public CrawlContext()
        {
            CrawledUrls = new List<string>();
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
        /// Urls that have been crawled. NOTE: Use lock when accessing this property since multiple threads are reading/writing to it
        /// </summary>
        public List<string> CrawledUrls { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public CrawlConfiguration CrawlConfiguration { get; set; }
    }
}
