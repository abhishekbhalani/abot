using Abot.Poco;
using System;
using System.Net;

namespace Abot.Core
{
    public interface ICrawlDecisionMaker
    {
        /// <summary>
        /// Decides whether the page should be crawled
        /// </summary>
        CrawlDecision ShouldCrawlPage(PageToCrawl pageToCrawl, CrawlContext crawlContext);

        /// <summary>
        /// Decides whether the page's links should be crawled
        /// </summary>
        CrawlDecision ShouldCrawlPageLinks(CrawledPage crawledPage, CrawlContext crawlContext);

        /// <summary>
        /// Decides whether the page's content should be dowloaded
        /// </summary>
        CrawlDecision ShouldDownloadPageContent(CrawledPage crawledPage, CrawlContext crawlContext);
    }

    public class CrawlDecisionMaker : ICrawlDecisionMaker
    {
        /// <summary>
        /// Will allow any page to be crawled that has not already been crawled
        /// </summary>
        public CrawlDecision ShouldCrawlPage(PageToCrawl pageToCrawl, CrawlContext crawlContext)
        {
            if(pageToCrawl == null)
                return new CrawlDecision { Allow = false, Reason = "Null page to crawl" };

            if (crawlContext == null)
                return new CrawlDecision { Allow = false, Reason = "Null crawl context" };            

            if (!pageToCrawl.Uri.Scheme.StartsWith("http"))
                return new CrawlDecision { Allow = false, Reason = "Invalid scheme" };

            lock (crawlContext.CrawledUrls)
            {
                if (crawlContext.CrawledUrls.Contains(pageToCrawl.Uri.AbsoluteUri))
                    return new CrawlDecision { Allow = false, Reason = "Link already crawled" };

                if (crawlContext.CrawledUrls.Count + 1 > crawlContext.CrawlConfiguration.MaxPagesToCrawl)
                    return new CrawlDecision { Allow = false, Reason = string.Format("MaxPagesToCrawl limit of [{0}] has been reached", crawlContext.CrawlConfiguration.MaxPagesToCrawl) };
            }

            if (crawlContext.CrawlConfiguration.CrawlTimeoutSeconds > 0)
            {
                double elapsedCrawlSeconds = (DateTime.Now - crawlContext.CrawlStartDate).TotalSeconds;
                if (elapsedCrawlSeconds > crawlContext.CrawlConfiguration.CrawlTimeoutSeconds)
                    return new CrawlDecision { Allow = false, Reason = string.Format("Crawl timeout of [{0}] seconds has been reached", crawlContext.CrawlConfiguration.CrawlTimeoutSeconds) };
            }

            return new CrawlDecision { Allow = true }; ;
        }

        /// <summary>
        /// Will allow the crawling of all internal links only
        /// </summary>
        public CrawlDecision ShouldCrawlPageLinks(CrawledPage crawledPage, CrawlContext crawlContext)
        {
            if (crawledPage == null)
                return new CrawlDecision{Allow = false, Reason = "Null crawled page"};

            if (crawlContext == null)
                return new CrawlDecision { Allow = false, Reason = "Null crawl context" };            

            if(string.IsNullOrEmpty(crawledPage.RawContent) || (crawledPage.RawContent.Trim().Length == 0))
                return new CrawlDecision { Allow = false, Reason = "Page has no content" };

            if(crawlContext.RootUri == null || !crawlContext.RootUri.IsBaseOf(crawledPage.Uri))
                return new CrawlDecision { Allow = false, Reason = "Link is external" };
            
            return new CrawlDecision{Allow = true};
        }

        /// <summary>
        /// Will allow the dowloading of a page's content if the page returned a 200 status and is text/html
        /// </summary>
        public CrawlDecision ShouldDownloadPageContent(CrawledPage crawledPage, CrawlContext crawlContext)
        {
            if (crawledPage == null)
                return new CrawlDecision { Allow = false, Reason = "Null crawled page" };

            if (crawlContext == null)
                return new CrawlDecision { Allow = false, Reason = "Null crawl context" };            

            if (crawledPage.HttpWebResponse == null)
                return new CrawlDecision { Allow = false, Reason = "Null HttpWebResponse" };
            
            if (crawledPage.HttpWebResponse.StatusCode != HttpStatusCode.OK)
                return new CrawlDecision { Allow = false, Reason = "HttpStatusCode is not 200" };
            
            if (!crawledPage.HttpWebResponse.ContentType.ToLower().Contains("text/html"))
                return new CrawlDecision { Allow = false, Reason = "Content type is not text/html" };
            
            return new CrawlDecision { Allow = true };            
        }
    }
}
