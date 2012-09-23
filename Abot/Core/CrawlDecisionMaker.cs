using Abot.Poco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Abot.Core
{
    public interface ICrawlDecisionMaker
    {
        bool ShouldCrawl(PageToCrawl pageToCrawl);
        bool ShouldCrawlLinks(CrawledPage crawledPage);
    }

    public class CrawlDecisionMaker : ICrawlDecisionMaker
    {
        public bool ShouldCrawl(PageToCrawl pageToCrawl)
        {
            return true;
        }

        public bool ShouldCrawlLinks(CrawledPage crawledPage)
        {
            return (crawledPage != null && !string.IsNullOrWhiteSpace(crawledPage.RawContent));
        }
    }
}
