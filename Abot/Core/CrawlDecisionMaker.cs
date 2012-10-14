using Abot.Poco;

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
            return (crawledPage != null 
                && !string.IsNullOrEmpty(crawledPage.RawContent) 
                && !(crawledPage.RawContent.Trim().Length == 0))
                && crawledPage.RootUri != null
                && crawledPage.RootUri.IsBaseOf(crawledPage.Uri);
        }
    }
}
