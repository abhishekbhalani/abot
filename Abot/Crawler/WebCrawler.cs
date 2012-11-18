using Abot.Core;
using Abot.Poco;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

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
        CrawlContext _crawlContext;
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

        /// <summary>
        /// Creates a crawler instance with the default settings and implementations.
        /// </summary>
        public WebCrawler()
            :this(null, null, null, null, null, null)
        {
        }

        /// <summary>
        /// Creates a crawler instance with the default settings and implementations.
        /// </summary>
        public WebCrawler(CrawlConfiguration crawlConfiguration)
            : this(null, null, null, null, null, crawlConfiguration)
        {
        }

        /// <summary>
        /// Creates a crawler instance with custom settings or implementation. Passing in null for all params is the equivalent of the empty constructor.
        /// </summary>
        /// <param name="threadManager">Distributes http requests over multiple threads</param>
        /// <param name="scheduler">Decides what link should be crawled next</param>
        /// <param name="httpRequester">Makes the raw http requests</param>
        /// <param name="hyperLinkParser">Parses a crawled page for it's hyperlinks</param>
        /// <param name="crawlDecisionMaker">Decides whether or not to crawl a page or that page's links</param>
        /// <param name="crawlConfiguration">Configurable crawl values</param>
        public WebCrawler(IThreadManager threadManager, 
            IScheduler scheduler, 
            IPageRequester httpRequester, 
            IHyperLinkParser hyperLinkParser, 
            ICrawlDecisionMaker crawlDecisionMaker,
            CrawlConfiguration crawlConfiguration)
        {
            _crawlContext = new CrawlContext();
            _crawlContext.CrawlConfiguration = crawlConfiguration ?? new CrawlConfiguration();

            _threadManager = threadManager ?? new ThreadManager(_crawlContext.CrawlConfiguration.MaxConcurrentThreads);
            _scheduler = scheduler ?? new FifoScheduler();
            _httpRequester = httpRequester ?? new PageRequester(_crawlContext.CrawlConfiguration.UserAgentString);
            _hyperLinkParser = hyperLinkParser ?? new HyperLinkParser();
            _crawlDecisionMaker = crawlDecisionMaker ?? new CrawlDecisionMaker();
        }


        /// <summary>
        /// Begins a synchronous crawl using the uri param, subscribe to events to process data as it becomes available
        /// </summary>
        public CrawlResult Crawl(Uri uri)
        {
            if(uri == null)
                throw new ArgumentNullException("uri");

            _crawlContext.RootUri = uri;
            
            _crawlResult = new CrawlResult();
            _crawlResult.RootUri = _crawlContext.RootUri;
            _crawlComplete = false;

            _logger.InfoFormat("About to crawl site [{0}]", uri.AbsoluteUri);
            PrintConfigValues(_crawlContext.CrawlConfiguration);

            _scheduler.Add(new PageToCrawl(uri) { ParentUri = uri });

            _crawlContext.CrawlStartDate = DateTime.Now;
            Stopwatch timer = Stopwatch.StartNew();
            CrawlSite();
            timer.Stop();

            _crawlResult.Elapsed = timer.Elapsed;
            _logger.InfoFormat("Crawl complete for site [{0}]: [{1}]", _crawlResult.RootUri.AbsoluteUri, _crawlResult.Elapsed);

            return _crawlResult;
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
                    _logger.DebugFormat("Waiting for links to be scheduled...");
                    System.Threading.Thread.Sleep(2500);
                }
            }
        }

        private void CrawlPage(PageToCrawl pageToCrawl)
        {
            if (pageToCrawl == null)
                return;

            //Crawl the page
            CrawlDecision shouldCrawlPageDecision = _crawlDecisionMaker.ShouldCrawlPage(pageToCrawl, _crawlContext);
            if (!shouldCrawlPageDecision.Allow)
            {
                _logger.DebugFormat("Page [{0}] not crawled, [{1}]", pageToCrawl.Uri.AbsoluteUri, shouldCrawlPageDecision.Reason);
                FirePageCrawlDisallowedEvent(pageToCrawl, shouldCrawlPageDecision.Reason);
                return;
            }

            _logger.DebugFormat("About to crawl page [{0}]", pageToCrawl.Uri.AbsoluteUri);
            FirePageCrawlStartingEvent(pageToCrawl);

            lock (_crawlContext.CrawledUrls)
            {
                _crawlContext.CrawledUrls.Add(pageToCrawl.Uri.AbsoluteUri);
            }

            CrawledPage crawledPage = _httpRequester.MakeRequest(pageToCrawl.Uri, (x) => _crawlDecisionMaker.ShouldDownloadPageContent(x, _crawlContext));
            crawledPage.IsRetry = pageToCrawl.IsRetry;
            crawledPage.ParentUri = pageToCrawl.ParentUri;

            if (crawledPage.HttpWebResponse == null)
                _logger.InfoFormat("Page crawl complete, Status:[NA] Url:[{0}] Parent:[{1}]", crawledPage.Uri.AbsoluteUri, crawledPage.ParentUri);
            else
                _logger.InfoFormat("Page crawl complete, Status:[{0}] Url:[{1}] Parent:[{2}]", Convert.ToInt32(crawledPage.HttpWebResponse.StatusCode), crawledPage.Uri.AbsoluteUri, crawledPage.ParentUri);
            
            FirePageCrawlCompletedEvent(crawledPage);


            //Crawl the page's links
            CrawlDecision shouldCrawlPageLinksDecision = _crawlDecisionMaker.ShouldCrawlPageLinks(crawledPage, _crawlContext);
            if (shouldCrawlPageLinksDecision.Allow)
            {
                IEnumerable<Uri> crawledPageLinks = _hyperLinkParser.GetLinks(crawledPage.Uri, crawledPage.RawContent);
                foreach (Uri uri in crawledPageLinks)
                {
                    _logger.DebugFormat("Found link [{0}] on page [{1}]", uri.AbsoluteUri, crawledPage.Uri.AbsoluteUri);
                    _scheduler.Add(new CrawledPage(uri) { ParentUri = crawledPage.Uri });
                }
            }
            else
            {
                _logger.DebugFormat("Links on page [{0}] not crawled, [{1}]", pageToCrawl.Uri.AbsoluteUri, shouldCrawlPageLinksDecision.Reason);
                FirePageLinksCrawlDisallowedEvent(crawledPage, shouldCrawlPageLinksDecision.Reason);
            }
        }

        private void PrintConfigValues(CrawlConfiguration config)
        {
            _logger.Info("Configuration Values:");

            string indentString = new string(' ', 2);
            foreach (PropertyInfo property in config.GetType().GetProperties())
            {
                if (property.Name != "Data")
                    _logger.InfoFormat("{0}{1}: {2}", indentString, property.Name, property.GetValue(config, null));
            }

            foreach (string key in config.Data.Keys)
            {
                _logger.InfoFormat("{0}{1}: {2}", indentString, key, config.Data[key]);
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
