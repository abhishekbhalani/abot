using Abot.Poco;
using System.Collections.Generic;

namespace Abot.Core
{
    public interface ICrawlDecisionMaker
    {
        bool ShouldCrawl(PageToCrawl pageToCrawl);
        bool ShouldCrawlLinks(CrawledPage crawledPage);
    }

    public class CrawlDecisionMaker : ICrawlDecisionMaker
    {
        List<string> crawledUrls = new List<string>();

        public bool ShouldCrawl(PageToCrawl pageToCrawl)
        {
            lock (crawledUrls)
            {
                if (crawledUrls.Contains(pageToCrawl.Uri.AbsoluteUri))
                    return false;
                else
                    crawledUrls.Add(pageToCrawl.Uri.AbsoluteUri);
            }
            return true;
        }

        public bool ShouldCrawlLinks(CrawledPage crawledPage)
        {
            return (crawledPage != null 
                && !string.IsNullOrEmpty(crawledPage.RawContent) 
                && !(crawledPage.RawContent.Trim().Length == 0))
                && crawledPage.RootUri != null
                && crawledPage.RootUri.IsBaseOf(crawledPage.Uri);
        }
    }
}
