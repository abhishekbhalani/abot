using Abot.Core;
using System;

namespace Abot
{
    public interface ICrawler
    {
        int MaxPagesToCrawl { get; set; }
        string  UserAgentString { get; set; }
        void Crawl(Uri uri);
    }

    public abstract class CrawlerBase : ICrawler
    {
        public int MaxPagesToCrawl { get; set; }
        public string UserAgentString { get; set; }

        public CrawlerBase(IThreadManager threadManager, IScheduler scheduler, IHyperLinkParser hyperLinkParser, IHttpRequester httpRequester)
        {
            //set all default components
        }
        
        //TODO Where do rules go?
        //TODO Where do events go?

        public void Crawl(Uri uri)
        {

        }

        protected abstract void BeforeCall();
        protected abstract bool ShouldMakeCall(PageToCrawl pageToCrawl);
        protected abstract void AfterCall();

        protected abstract void BeforeDownloadPageContent();
        protected abstract bool ShouldDownloadPageContent(CrawledPage crawledPage);
        protected abstract void AfterDownloadPageContent();
    }
}
