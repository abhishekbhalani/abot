using System;

namespace CheetahBot.Crawler
{
    public class CrawlResult
    {
        public CrawlResult()
        {
            ErrorMessage = "";
        }

        public TimeSpan Elapsed { get; set; }

        public bool ErrorOccurred { get; set; }

        public string ErrorMessage { get; set; }
    }
}
