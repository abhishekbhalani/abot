using Abot.Poco;
using System.Collections.Generic;
using System.Net;

namespace Abot.Core
{
    public interface ICrawlDecisionMaker
    {
        CrawlDecision ShouldCrawlPage(PageToCrawl pageToCrawl);
        CrawlDecision ShouldCrawlPageLinks(CrawledPage crawledPage);
        CrawlDecision ShouldDownloadPageContent(CrawledPage crawledPage);
    }

    public class CrawlDecisionMaker : ICrawlDecisionMaker
    {
        List<string> crawledUrls = new List<string>();

        public CrawlDecision ShouldCrawlPage(PageToCrawl pageToCrawl)
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

        public CrawlDecision ShouldCrawlPageLinks(CrawledPage crawledPage)
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

        public CrawlDecision ShouldDownloadPageContent(CrawledPage crawledPage)
        {
            if (crawledPage == null)
                return new CrawlDecision { Should = false, Reason = "Null crawled page" };
            else if (crawledPage.HttpWebResponse == null)
                return new CrawlDecision { Should = false, Reason = "Null HttpWebResponse" };
            else if (crawledPage.HttpWebResponse.StatusCode != HttpStatusCode.OK)
                return new CrawlDecision { Should = false, Reason = "HttpStatusCode is not 200" };
            else if (!crawledPage.HttpWebResponse.ContentType.ToLower().Contains("text/html"))
                return new CrawlDecision { Should = false, Reason = "Content type is not text/html" };
            else
                return new CrawlDecision { Should = true };            
        }
    }
}
