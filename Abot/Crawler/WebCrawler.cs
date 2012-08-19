using Abot.Poco;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Abot.Crawler
{
    public interface IWebCrawler
    {
        CrawlResult Crawl(Uri uri);
    }

    public class WebCrawler : IWebCrawler
    {
        //TODO add licensing info to every page
        static ILog _logger = LogManager.GetLogger(typeof(WebCrawler).FullName);
        bool _crawlComplete = false;
        CrawlResult _crawlResult = null;

        IThreadManager _threadManager;
        IScheduler _scheduler;
        IPageRequester _httpRequester; 
        IHyperLinkParser _hyperLinkParser;

        public WebCrawler()
            :this(null, null, null, null)
        {
        }

        public WebCrawler(IThreadManager threadManager, IScheduler scheduler, IPageRequester httpRequester, IHyperLinkParser hyperLinkParser)
        {
            if(threadManager ==null)
                throw new ArgumentNullException("threadManager");

            if(scheduler == null)
                throw new ArgumentNullException("scheduler");

            if (httpRequester == null)
                throw new ArgumentNullException("httpRequester");

            if (hyperLinkParser == null)
                throw new ArgumentNullException("hyperLinkParser");

            _threadManager = threadManager ?? new ThreadManager(10);
            _scheduler = scheduler ?? new FifoScheduler();
            _httpRequester = httpRequester ?? new PageRequester("abot v1.0 http://code.google.com/p/abot");
            _hyperLinkParser = hyperLinkParser ?? null; //TODO Implement HyperLinkParser();
        }


        public CrawlResult Crawl(Uri uri)
        {
            _crawlResult = new CrawlResult();
            _crawlResult.RootUri = uri;
            _crawlComplete = false;

            if(uri == null)
                throw new ArgumentNullException("uri");

            BeforeSiteCrawl(uri);
            _scheduler.Add(new PageToCrawl(uri){ParentUri = uri});

            Stopwatch timer = Stopwatch.StartNew();
            CrawlSite();
            timer.Stop();

            _crawlResult.Elapsed = timer.Elapsed;
            AfterSiteCrawl(_crawlResult);

            return new CrawlResult { Elapsed = timer.Elapsed };
        }


        protected virtual void CrawlSite()
        {
            while (!_crawlComplete)
            {
                if (_scheduler.Count > 0)
                {
                    _threadManager.DoWork(() => CrawlPage(_scheduler.GetNext()));
                }
                else if (!_threadManager.HasRunningThreads())
                {
                    _crawlComplete = true;
                }
                else
                {
                    _logger.InfoFormat("Waiting for links to be scheduled...");
                    System.Threading.Thread.Sleep(2500);
                }
            }
        }

        protected virtual void CrawlPage(PageToCrawl pageToCrawl)
        {
            if (pageToCrawl == null)
                return;

            if (!ShouldCrawlPage(pageToCrawl))
                return;

            BeforePageCrawl(pageToCrawl);

            CrawledPage crawledPage = _httpRequester.MakeRequest(pageToCrawl.Uri);
            crawledPage.IsRetry = pageToCrawl.IsRetry;
            crawledPage.ParentUri = pageToCrawl.ParentUri;

            AfterPageCrawl(crawledPage);

            if (ShouldSchedulePageLinksToBeCrawled(crawledPage))
            {
                IEnumerable<Uri> crawledPageLinks = _hyperLinkParser.GetHyperLinks(crawledPage.Uri, crawledPage.RawContent);
                foreach (Uri uri in crawledPageLinks)
                {
                    _logger.DebugFormat("Found link [{0}] on page [{1}]", uri.AbsoluteUri, crawledPage.Uri.AbsoluteUri);
                    _scheduler.Add(new CrawledPage(uri) { ParentUri = crawledPage.Uri });
                }
            }
        }


        protected virtual void BeforeSiteCrawl(Uri uri)
        {
            _logger.DebugFormat("About to crawl site [{0}]", uri.AbsoluteUri);
        }

        protected virtual void AfterSiteCrawl(CrawlResult crawlResult)
        {
            _logger.DebugFormat("Crawl complete for site [{0}]: [{1}]", crawlResult.RootUri.AbsoluteUri, crawlResult.Elapsed);
        }

        protected virtual void BeforePageCrawl(PageToCrawl pageToCrawl)
        {
            _logger.DebugFormat("About to crawl page [{0}]", pageToCrawl.Uri.AbsoluteUri);
        }

        protected virtual void AfterPageCrawl(CrawledPage crawledPage)
        {
            if (crawledPage.HttpWebResponse == null)
                _logger.InfoFormat("Page crawl complete, Status:[NA] Url:[{0}] Parent:[{1}]", crawledPage.Uri.AbsoluteUri, crawledPage.ParentUri);
            else
                _logger.InfoFormat("Page crawl complete, Status:[{0}] Url:[{1}] Parent:[{2}]", Convert.ToInt32(crawledPage.HttpWebResponse.StatusCode), crawledPage.Uri.AbsoluteUri, crawledPage.ParentUri);
        }


        protected virtual bool ShouldCrawlPage(PageToCrawl pageToCrawl)
        {
            return (pageToCrawl != null);
        }

        protected virtual bool ShouldSchedulePageLinksToBeCrawled(CrawledPage crawledPage)
        {
            return (crawledPage != null && !string.IsNullOrWhiteSpace(crawledPage.RawContent));
        }
    }
}
