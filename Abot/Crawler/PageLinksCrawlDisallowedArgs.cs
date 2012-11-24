using Abot.Poco;
using System;

namespace Abot.Crawler
{
    public class PageLinksCrawlDisallowedArgs : PageCrawlCompletedArgs
    {
        public string DisallowedReason { get; private set; }

        public PageLinksCrawlDisallowedArgs(CrawledPage crawledPage, string disallowedReason)
            : base(crawledPage)
        {
            if (string.IsNullOrWhiteSpace(disallowedReason))
                throw new ArgumentNullException("disallowedReason");

            DisallowedReason = disallowedReason;
        }
    }
}
