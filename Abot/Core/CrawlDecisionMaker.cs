using Abot.Poco;
using System;
using System.Net;
using System.Linq;

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
        public virtual CrawlDecision ShouldCrawlPage(PageToCrawl pageToCrawl, CrawlContext crawlContext)
        {
            if(pageToCrawl == null)
                return new CrawlDecision { Allow = false, Reason = "Null page to crawl" };

            if (crawlContext == null)
                return new CrawlDecision { Allow = false, Reason = "Null crawl context" };            

            if (!pageToCrawl.Uri.Scheme.StartsWith("http"))
                return new CrawlDecision { Allow = false, Reason = "Scheme does not begin with http" };

            if (!crawlContext.CrawlConfiguration.IsUriRecrawlingEnabled && crawlContext.CrawledUrls.Contains(pageToCrawl.Uri.AbsoluteUri))
                return new CrawlDecision { Allow = false, Reason = "Link already crawled" };

            if (crawlContext.CrawledUrls.Count + 1 > crawlContext.CrawlConfiguration.MaxPagesToCrawl)
                return new CrawlDecision { Allow = false, Reason = string.Format("MaxPagesToCrawl limit of [{0}] has been reached", crawlContext.CrawlConfiguration.MaxPagesToCrawl) };

            if (crawlContext.CrawlConfiguration.CrawlTimeoutSeconds > 0)
            {
                double elapsedCrawlSeconds = (DateTime.Now - crawlContext.CrawlStartDate).TotalSeconds;
                if (elapsedCrawlSeconds > crawlContext.CrawlConfiguration.CrawlTimeoutSeconds)
                    return new CrawlDecision { Allow = false, Reason = string.Format("Crawl timeout of [{0}] seconds has been reached", crawlContext.CrawlConfiguration.CrawlTimeoutSeconds) };
            }

            if(!crawlContext.CrawlConfiguration.IsExternalPageCrawlingEnabled && !pageToCrawl.IsInternal)
                return new CrawlDecision { Allow = false, Reason = "Link is external" };

            return new CrawlDecision { Allow = true }; ;
        }

        public virtual CrawlDecision ShouldCrawlPageLinks(CrawledPage crawledPage, CrawlContext crawlContext)
        {
            if (crawledPage == null)
                return new CrawlDecision{Allow = false, Reason = "Null crawled page"};

            if (crawlContext == null)
                return new CrawlDecision { Allow = false, Reason = "Null crawl context" };            

            if(string.IsNullOrWhiteSpace(crawledPage.RawContent))
                return new CrawlDecision { Allow = false, Reason = "Page has no content" };

            if (!crawlContext.CrawlConfiguration.IsExternalPageLinksCrawlingEnabled && !crawledPage.IsInternal)
                return new CrawlDecision { Allow = false, Reason = "Link is external" };
            
            return new CrawlDecision{Allow = true};
        }

        public virtual CrawlDecision ShouldDownloadPageContent(CrawledPage crawledPage, CrawlContext crawlContext)
        {
            if (crawledPage == null)
                return new CrawlDecision { Allow = false, Reason = "Null crawled page" };

            if (crawlContext == null)
                return new CrawlDecision { Allow = false, Reason = "Null crawl context" };            

            if (crawledPage.HttpWebResponse == null)
                return new CrawlDecision { Allow = false, Reason = "Null HttpWebResponse" };
            
            if (crawledPage.HttpWebResponse.StatusCode != HttpStatusCode.OK)
                return new CrawlDecision { Allow = false, Reason = "HttpStatusCode is not 200" };
            
            string pageContentType = crawledPage.HttpWebResponse.ContentType.ToLower().Trim();
            bool isDownloadable = false;
            foreach (string downloadableContentType in crawlContext.CrawlConfiguration.DownloadableContentTypes.Split(','))
            {
                if (pageContentType.Contains(downloadableContentType.ToLower().Trim()))
                {
                    isDownloadable = true;
                    break;
                }
            }
            if (!isDownloadable)
                return new CrawlDecision { Allow = false, Reason = "Content type is not any of the following: " + crawlContext.CrawlConfiguration.DownloadableContentTypes };
            
            return new CrawlDecision { Allow = true };            
        }
    }
}
