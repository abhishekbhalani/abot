using Abot.Poco;
using System.Collections.Generic;

namespace Abot.Core
{
    public interface ICrawlDecisionMaker
    {
        CrawlDecision ShouldCrawl(PageToCrawl pageToCrawl);
        CrawlDecision ShouldCrawlLinks(CrawledPage crawledPage);
    }

    public class CrawlDecisionMaker : ICrawlDecisionMaker
    {
        List<string> crawledUrls = new List<string>();

        public CrawlDecision ShouldCrawl(PageToCrawl pageToCrawl)
        {
            lock (crawledUrls)
            {
                if (crawledUrls.Contains(pageToCrawl.Uri.AbsoluteUri))
                    return new CrawlDecision { Should = false, Reason = "Link already crawled" };
                else
                    crawledUrls.Add(pageToCrawl.Uri.AbsoluteUri);
            }
            return new CrawlDecision { Should = true }; ;
        }

        public CrawlDecision ShouldCrawlLinks(CrawledPage crawledPage)
        {
            if (crawledPage == null)
                return new CrawlDecision{Should = false, Reason = "Null crawled page"};
            else if(string.IsNullOrEmpty(crawledPage.RawContent) || (crawledPage.RawContent.Trim().Length == 0))
                return new CrawlDecision { Should = false, Reason = "Page has no links" };
            else if(crawledPage.RootUri == null || !crawledPage.RootUri.IsBaseOf(crawledPage.Uri))
                return new CrawlDecision { Should = false, Reason = "Link is external" };
            else
                return new CrawlDecision{Should = true};
        }
    }
}
