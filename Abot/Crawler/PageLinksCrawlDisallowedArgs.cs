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
            if (string.IsNullOrEmpty(disallowedReason) || disallowedReason.Trim().Length == 0)
                throw new ArgumentNullException("disallowedReason");

            DisallowedReason = disallowedReason;
        }
    }
}
