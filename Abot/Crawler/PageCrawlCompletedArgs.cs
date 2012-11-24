using Abot.Poco;
using System;

namespace Abot.Crawler
{
    public class PageCrawlCompletedArgs : EventArgs
    {
        public CrawledPage CrawledPage { get; private set; }

        public PageCrawlCompletedArgs(CrawledPage crawledPage)
        {
            if (crawledPage == null)
                throw new ArgumentNullException("crawledPage");

            CrawledPage = crawledPage;
        }
    }
}
