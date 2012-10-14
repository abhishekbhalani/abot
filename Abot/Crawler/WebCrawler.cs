using Abot.Core;
using Abot.Poco;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Abot.Crawler
{
    public interface IWebCrawler
    {
        /// <summary>
        /// Asynchronous event that is fired before a page is crawled.
        /// </summary>
        event EventHandler<PageCrawlStartingArgs> PageCrawlStarting;

        /// <summary>
        /// Asynchronous event that is fired when an individual page has been crawled.
        /// </summary>
        event EventHandler<PageCrawlCompletedArgs> PageCrawlCompleted;

        /// <summary>
        /// Asynchronous event that is fired when the ICrawlDecisionMaker.ShouldCrawl impl returned false. This means the page or its links were not crawled.
        /// </summary>
        event EventHandler<PageCrawlDisallowedArgs> PageCrawlDisallowed;

        /// <summary>
        /// Asynchronous event that is fired when the ICrawlDecisionMaker.ShouldCrawlLinks impl returned false. This means the page's links were not crawled.
        /// </summary>
        event EventHandler<PageLinksCrawlDisallowedArgs> PageLinksCrawlDisallowed;  

        /// <summary>
        /// Begins a crawl using the uri param
        /// </summary>
        CrawlResult Crawl(Uri uri);
    }

    public class WebCrawler : IWebCrawler
    {
        static ILog _logger = LogManager.GetLogger(typeof(WebCrawler).FullName);
        bool _crawlComplete = false;
        CrawlResult _crawlResult = null;
        Uri _rootUri = null;

        IThreadManager _threadManager;
        IScheduler _scheduler;
        IPageRequester _httpRequester;
        IHyperLinkParser _hyperLinkParser;
        ICrawlDecisionMaker _crawlDecisionMaker;


        /// <summary>
        /// Asynchronous event that is fired before a page is crawled.
        /// </summary>
        public event EventHandler<PageCrawlStartingArgs> PageCrawlStarting;

        /// <summary>
        /// Asynchronous event that is fired when an individual page has been crawled.
        /// </summary>
        public event EventHandler<PageCrawlCompletedArgs> PageCrawlCompleted;

        /// <summary>
        /// Asynchronous event that is fired when the ICrawlDecisionMaker.ShouldCrawl impl returned false. This means the page or its links were not crawled.
        /// </summary>
        public event EventHandler<PageCrawlDisallowedArgs> PageCrawlDisallowed;

        /// <summary>
        /// Asynchronous event that is fired when the ICrawlDecisionMaker.ShouldCrawlLinks impl returned false. This means the page's links were not crawled.
        /// </summary>
        public event EventHandler<PageLinksCrawlDisallowedArgs> PageLinksCrawlDisallowed;


        public WebCrawler()
            :this(null, null, null, null, null)
        {
        }

        public WebCrawler(IThreadManager threadManager, 
            IScheduler scheduler, 
            IPageRequester httpRequester, 
            IHyperLinkParser hyperLinkParser, 
            ICrawlDecisionMaker crawlDecisionMaker)
        {
            _threadManager = threadManager ?? new ThreadManager(10);
            _scheduler = scheduler ?? new FifoScheduler();
            _httpRequester = httpRequester ?? new PageRequester("abot v1.0 http://code.google.com/p/abot");
            _hyperLinkParser = hyperLinkParser ?? new HyperLinkParser();
            _crawlDecisionMaker = crawlDecisionMaker ?? new CrawlDecisionMaker();
        }


        public CrawlResult Crawl(Uri uri)
        {
            _crawlResult = new CrawlResult();
            _crawlResult.RootUri = uri;
            _crawlComplete = false;

            if(uri == null)
                throw new ArgumentNullException("uri");

            _rootUri = uri;
            _logger.DebugFormat("About to crawl site [{0}]", uri.AbsoluteUri);
            _scheduler.Add(new PageToCrawl(uri){ParentUri = uri, RootUri = _rootUri});

            Stopwatch timer = Stopwatch.StartNew();
            CrawlSite();
            timer.Stop();

            _crawlResult.Elapsed = timer.Elapsed;
            _logger.DebugFormat("Crawl complete for site [{0}]: [{1}]", _crawlResult.RootUri.AbsoluteUri, _crawlResult.Elapsed);

            return new CrawlResult { Elapsed = timer.Elapsed };
        }


        private void CrawlSite()
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

            if (!_crawlDecisionMaker.ShouldCrawl(pageToCrawl))
            {
                FirePageCrawlDisallowedEvent(pageToCrawl, "Need to add reason here");//TODO GET REASON HERE, add _crawlDescisionMake.Reason????
                return;
            }

            _logger.DebugFormat("About to crawl page [{0}]", pageToCrawl.Uri.AbsoluteUri);
            FirePageCrawlStartingEvent(pageToCrawl);

            //Crawl page
            CrawledPage crawledPage = _httpRequester.MakeRequest(pageToCrawl.Uri);
            crawledPage.RootUri = _rootUri;
            crawledPage.IsRetry = pageToCrawl.IsRetry;
            crawledPage.ParentUri = pageToCrawl.ParentUri;

            if (crawledPage.HttpWebResponse == null)
                _logger.InfoFormat("Page crawl complete, Status:[NA] Url:[{0}] Parent:[{1}]", crawledPage.Uri.AbsoluteUri, crawledPage.ParentUri);
            else
                _logger.InfoFormat("Page crawl complete, Status:[{0}] Url:[{1}] Parent:[{2}]", Convert.ToInt32(crawledPage.HttpWebResponse.StatusCode), crawledPage.Uri.AbsoluteUri, crawledPage.ParentUri);
            
            FirePageCrawlCompletedEvent(crawledPage);

            //Crawl page's links
            if (_crawlDecisionMaker.ShouldCrawlLinks(crawledPage))
            {
                IEnumerable<Uri> crawledPageLinks = _hyperLinkParser.GetLinks(crawledPage.Uri, crawledPage.RawContent);
                foreach (Uri uri in crawledPageLinks)
                {
                    _logger.DebugFormat("Found link [{0}] on page [{1}]", uri.AbsoluteUri, crawledPage.Uri.AbsoluteUri);
                    _scheduler.Add(new CrawledPage(uri) { ParentUri = crawledPage.Uri, RootUri = _rootUri});
                }
            }
            else
            {
                FirePageLinksCrawlDisallowedEvent(crawledPage, "Need to add reason here");//TODO GET REASON HERE, add _crawlDescisionMake.Reason????
            }
        }


        private void FirePageCrawlStartingEvent(PageToCrawl pageToCrawl)
        {
            try
            {
                OnPageCrawlStarting(new PageCrawlStartingArgs(pageToCrawl));
            }
            catch (Exception e)
            {
                //Since the implementation of OnPageCrawlStarting() is async this should never happen, however leaving this try catch in case the impl changes
                _logger.Error("An unhandled exception was thrown by a subscriber of the PageCrawlStarting event for url:" + pageToCrawl.Uri.AbsoluteUri, e);
            }
        }

        private void FirePageCrawlCompletedEvent(CrawledPage crawledPage)
        {
            try
            {
                OnPageCrawlCompleted(new PageCrawlCompletedArgs(crawledPage));
            }
            catch (Exception e)
            {
                //Since the implementation of OnPageCrawlStarting() is async this should never happen, however leaving this try catch in case the impl changes
                _logger.Error("An unhandled exception was thrown by a subscriber of the PageCrawlCompleted event for url:" + crawledPage.Uri.AbsoluteUri, e);
            }
        }

        private void FirePageCrawlDisallowedEvent(PageToCrawl pageToCrawl, string reason)
        {
            try
            {
                OnPageCrawlDisallowed(new PageCrawlDisallowedArgs(pageToCrawl, reason));
            }
            catch (Exception e)
            {
                //Since the implementation of OnPageCrawlStarting() is async this should never happen, however leaving this try catch in case the impl changes
                _logger.Error("An unhandled exception was thrown by a subscriber of the PageCrawlDisallowed event for url:" + pageToCrawl.Uri.AbsoluteUri, e);
            }
        }

        private void FirePageLinksCrawlDisallowedEvent(CrawledPage crawledPage, string reason)
        {
            try
            {
                OnPageLinksCrawlDisallowed(new PageLinksCrawlDisallowedArgs(crawledPage, reason));
            }
            catch (Exception e)
            {
                //Since the implementation of OnPageCrawlStarting() is async this should never happen, however leaving this try catch in case the impl changes
                _logger.Error("An unhandled exception was thrown by a subscriber of the PageLinksCrawlDisallowed event for url:" + crawledPage.Uri.AbsoluteUri, e);
            }
        }

        private void OnPageCrawlStarting(PageCrawlStartingArgs e)
        {
            EventHandler<PageCrawlStartingArgs> threadSafeEvent = PageCrawlStarting;
            if (threadSafeEvent != null)
            {
                //Fire each subscribers delegate async
                foreach (EventHandler<PageCrawlStartingArgs> del in threadSafeEvent.GetInvocationList())
                {
                    del.BeginInvoke(this, e, null, null);
                }
            }
        }

        private void OnPageCrawlCompleted(PageCrawlCompletedArgs e)
        {
            EventHandler<PageCrawlCompletedArgs> threadSafeEvent = PageCrawlCompleted;
            if (threadSafeEvent != null)
            {
                //Fire each subscribers delegate async
                foreach (EventHandler<PageCrawlCompletedArgs> del in threadSafeEvent.GetInvocationList())
                {
                    del.BeginInvoke(this, e, null, null);
                }
            }
        }

        private void OnPageCrawlDisallowed(PageCrawlDisallowedArgs e)
        {
            EventHandler<PageCrawlDisallowedArgs> threadSafeEvent = PageCrawlDisallowed;
            if (threadSafeEvent != null)
            {
                //Fire each subscribers delegate async
                foreach (EventHandler<PageCrawlDisallowedArgs> del in threadSafeEvent.GetInvocationList())
                {
                    del.BeginInvoke(this, e, null, null);
                }
            }
        }

        private void OnPageLinksCrawlDisallowed(PageLinksCrawlDisallowedArgs e)
        {
            EventHandler<PageLinksCrawlDisallowedArgs> threadSafeEvent = PageLinksCrawlDisallowed;
            if (threadSafeEvent != null)
            {
                //Fire each subscribers delegate async
                foreach (EventHandler<PageLinksCrawlDisallowedArgs> del in threadSafeEvent.GetInvocationList())
                {
                    del.BeginInvoke(this, e, null, null);
                }
            }
        }
    }
}
