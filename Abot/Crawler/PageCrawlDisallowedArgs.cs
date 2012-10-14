using Abot.Poco;
using System;

namespace Abot.Crawler
{
    public class PageCrawlDisallowedArgs: PageCrawlStartingArgs
    {
        public string DisallowedReason { get; private set; }

        public PageCrawlDisallowedArgs(PageToCrawl pageToCrawl, string disallowedReason)
            : base(pageToCrawl)
        {
            if (string.IsNullOrEmpty(disallowedReason) || disallowedReason.Trim().Length == 0)
                throw new ArgumentNullException("disallowedReason");

            DisallowedReason = disallowedReason;
        }
    }
}
