using Abot.Poco;
using System;

namespace Abot.Crawler
{
    public class PageCrawlStartingArgs : EventArgs
    {
        public PageToCrawl PageToCrawl { get; private set; }

        public PageCrawlStartingArgs(PageToCrawl pageToCrawl)
        {
            if (pageToCrawl == null)
                throw new ArgumentNullException("pageToCrawl");

            PageToCrawl = pageToCrawl;
        }
    }
}
