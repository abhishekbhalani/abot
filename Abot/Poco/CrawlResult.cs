using System;

namespace Abot.Poco
{
    public class CrawlResult
    {
        public CrawlResult()
        {
            ErrorMessage = "";
        }

        /// <summary>
        /// The root of the crawl
        /// </summary>
        public Uri RootUri { get; set; }

        /// <summary>
        /// The amount of time that elapsed before the crawl completed
        /// </summary>
        public TimeSpan Elapsed { get; set; }

        /// <summary>
        /// Whether or not an error occurred during the crawl that caused it to end prematurely
        /// </summary>
        public bool ErrorOccurred { get; set; }

        /// <summary>
        /// The error message which describes the condition that prematurely ended the crawl
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
