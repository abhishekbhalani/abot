using System;

namespace Abot
{
    public class CrawlResult
    {
        public CrawlResult()
        {
            ErrorMessage = "";
        }

        public Uri RootUri { get; set; }

        public TimeSpan Elapsed { get; set; }

        public bool ErrorOccurred { get; set; }

        public string ErrorMessage { get; set; }
    }
}
