using Abot.Poco;
using System.Collections.Generic;
using System.Net;

namespace Abot.Core
{
    public interface ICrawlDecisionMaker
    {
        /// <summary>
        /// Decides whether the page param should be crawled
        /// </summary>
        CrawlDecision ShouldCrawlPage(PageToCrawl pageToCrawl);

        /// <summary>
        /// Decides whether the page's links should be crawled
        /// </summary>
        CrawlDecision ShouldCrawlPageLinks(CrawledPage crawledPage);

        /// <summary>
        /// Decides whether the page's content should be dowloaded
        /// </summary>
        CrawlDecision ShouldDownloadPageContent(CrawledPage crawledPage);
    }

    public class CrawlDecisionMaker : ICrawlDecisionMaker
    {
        List<string> crawledUrls = new List<string>();

        /// <summary>
        /// Will allow any page to be crawled that has not already been crawled
        /// </summary>
        public CrawlDecision ShouldCrawlPage(PageToCrawl pageToCrawl)
        {
            lock (crawledUrls)
            {
                if (crawledUrls.Contains(pageToCrawl.Uri.AbsoluteUri))
                    return new CrawlDecision { Allow = false, Reason = "Link already crawled" };
                else
                    crawledUrls.Add(pageToCrawl.Uri.AbsoluteUri);
            }

            if (!pageToCrawl.Uri.Scheme.StartsWith("http"))
                return new CrawlDecision { Allow = false, Reason = "Invalid scheme" };

            return new CrawlDecision { Allow = true }; ;
        }

        /// <summary>
        /// Will allow the crawling of all internal links only
        /// </summary>
        public CrawlDecision ShouldCrawlPageLinks(CrawledPage crawledPage)
        {
            if (crawledPage == null)
                return new CrawlDecision{Allow = false, Reason = "Null crawled page"};
            else if(string.IsNullOrEmpty(crawledPage.RawContent) || (crawledPage.RawContent.Trim().Length == 0))
                return new CrawlDecision { Allow = false, Reason = "Page has no links" };
            else if(crawledPage.RootUri == null || !crawledPage.RootUri.IsBaseOf(crawledPage.Uri))
                return new CrawlDecision { Allow = false, Reason = "Link is external" };
            else
                return new CrawlDecision{Allow = true};
        }

        /// <summary>
        /// Will allow the dowloading of a page's content if the page returned a 200 status and is text/html
        /// </summary>
        public CrawlDecision ShouldDownloadPageContent(CrawledPage crawledPage)
        {
            if (crawledPage == null)
                return new CrawlDecision { Allow = false, Reason = "Null crawled page" };
            else if (crawledPage.HttpWebResponse == null)
                return new CrawlDecision { Allow = false, Reason = "Null HttpWebResponse" };
            else if (crawledPage.HttpWebResponse.StatusCode != HttpStatusCode.OK)
                return new CrawlDecision { Allow = false, Reason = "HttpStatusCode is not 200" };
            else if (!crawledPage.HttpWebResponse.ContentType.ToLower().Contains("text/html"))
                return new CrawlDecision { Allow = false, Reason = "Content type is not text/html" };
            else
                return new CrawlDecision { Allow = true };            
        }
    }
}
