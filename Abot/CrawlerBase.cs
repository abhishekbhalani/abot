using log4net;
using System;
using System.Collections.Generic;

namespace Abot
{
    public interface ICrawler
    {
        void Crawl(Uri uri);
    }

    public abstract class CrawlerBase : ICrawler
    {
        static ILog _logger = LogManager.GetLogger(typeof(CrawlerBase).FullName);

        IThreadManager _threadManager;
        IScheduler _scheduler;
        
        //public IHyperLinkParser HyperLinkParser { get; protected set; }
        //public IHttpRequester HttpRequester { get; set; }

        public CrawlerBase()
        {
            _threadManager = GetThreadManager(10);
            _scheduler = GetScheduler();
        }

        public void Crawl(Uri uri)
        {
            
        }


        protected virtual void BeforeCall(PageToCrawl pageToCrawl)
        {
            _logger.DebugFormat("About to make http request to [{0}]", pageToCrawl.Uri.AbsoluteUri);
        }

        protected virtual void AfterCall(CrawledPage crawledPage)
        {
            if (crawledPage.HttpWebResponse == null)
                _logger.InfoFormat("Http request complete, Status:[NA] Url:[{0}] Parent:[{1}]", crawledPage.Uri.AbsoluteUri, crawledPage.ParentUri);
            else
                _logger.InfoFormat("Http request complete, Status:[{0}] Url:[{1}] Parent:[{2}]", Convert.ToInt32(crawledPage.HttpWebResponse.StatusCode), crawledPage.Uri.AbsoluteUri, crawledPage.ParentUri);
        }

        protected virtual bool ShouldMakeCall(PageToCrawl pageToCrawl)
        {
            return true;
        }

        protected virtual bool ShouldSchedulePageLinksToBeCrawled(CrawledPage crawledPage)
        {
            return true;
        }


        protected abstract IThreadManager GetThreadManager(int maxThreads);
        protected abstract IScheduler GetScheduler();
        protected abstract IHttpRequester GetHttpRequester(string userAgentString);
        protected abstract IHyperLinkParser GetHyperLinkParser(Uri pageUri, string pageHtml);


        private void CrawlPage(PageToCrawl pageToCrawl)
        {
            if (pageToCrawl == null)
                return;

            if (!ShouldMakeCall(pageToCrawl))
                return;

            BeforeCall(pageToCrawl);
            
            IHttpRequester httpRequester = GetHttpRequester(null);
            CrawledPage crawledPage = httpRequester.MakeHttpWebRequest(pageToCrawl.Uri);
            crawledPage.IsRetry = pageToCrawl.IsRetry;
            crawledPage.ParentUri = pageToCrawl.ParentUri;
            //crawledPage.HttpRequester = httpRequester;

            AfterCall(crawledPage);

            if (ShouldSchedulePageLinksToBeCrawled(crawledPage))
            {
                IHyperLinkParser linkParser = GetHyperLinkParser(crawledPage.Uri, crawledPage.Content);
                IEnumerable<Uri> crawledPageLinks = linkParser.GetHyperLinks();
                foreach (Uri uri in crawledPageLinks)
                {
                    _logger.DebugFormat("Found link [{0}] on page [{1}]", uri.AbsoluteUri, crawledPage.Uri.AbsoluteUri);
                    _scheduler.Add(new CrawledPage(uri) { ParentUri = crawledPage.Uri });
                }
            }

            FirePageCrawlCompletedEvent(crawledPage);
            RunPostPageCrawlActions(crawledPage);
        }
    }
}
