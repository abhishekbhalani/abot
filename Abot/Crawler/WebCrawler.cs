using Abot.Core;
using Abot.Poco;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Timers;

namespace Abot.Crawler
{
    public interface IWebCrawler
    {
        /// <summary>
        /// Synchronous event that is fired before a page is crawled.
        /// </summary>
        event EventHandler<PageCrawlStartingArgs> PageCrawlStarting;

        /// <summary>
        /// Synchronous event that is fired when an individual page has been crawled.
        /// </summary>
        event EventHandler<PageCrawlCompletedArgs> PageCrawlCompleted;

        /// <summary>
        /// Synchronous event that is fired when the ICrawlDecisionMaker.ShouldCrawl impl returned false. This means the page or its links were not crawled.
        /// </summary>
        event EventHandler<PageCrawlDisallowedArgs> PageCrawlDisallowed;

        /// <summary>
        /// Synchronous event that is fired when the ICrawlDecisionMaker.ShouldCrawlLinks impl returned false. This means the page's links were not crawled.
        /// </summary>
        event EventHandler<PageLinksCrawlDisallowedArgs> PageLinksCrawlDisallowed;

        /// <summary>
        /// Asynchronous event that is fired before a page is crawled.
        /// </summary>
        event EventHandler<PageCrawlStartingArgs> PageCrawlStartingAsync;

        /// <summary>
        /// Asynchronous event that is fired when an individual page has been crawled.
        /// </summary>
        event EventHandler<PageCrawlCompletedArgs> PageCrawlCompletedAsync;

        /// <summary>
        /// Asynchronous event that is fired when the ICrawlDecisionMaker.ShouldCrawl impl returned false. This means the page or its links were not crawled.
        /// </summary>
        event EventHandler<PageCrawlDisallowedArgs> PageCrawlDisallowedAsync;

        /// <summary>
        /// Asynchronous event that is fired when the ICrawlDecisionMaker.ShouldCrawlLinks impl returned false. This means the page's links were not crawled.
        /// </summary>
        event EventHandler<PageLinksCrawlDisallowedArgs> PageLinksCrawlDisallowedAsync;

        /// <summary>
        /// Synchronous method that registers a delegate to be called to determine whether a page should be crawled or not
        /// </summary>
        void ShouldCrawlPage(Func<PageToCrawl, CrawlContext, CrawlDecision> decisionMaker);

        /// <summary>
        /// Synchronous method that registers a delegate to be called to determine whether the page's content should be dowloaded
        /// </summary>
        /// <param name="shouldDownloadPageContent"></param>
        void ShouldDownloadPageContent(Func<CrawledPage, CrawlContext, CrawlDecision> decisionMaker);

        /// <summary>
        /// Synchronous method that registers a delegate to be called to determine whether a page's links should be crawled or not
        /// </summary>
        /// <param name="shouldCrawlPageLinksDelegate"></param>
        void ShouldCrawlPageLinks(Func<CrawledPage, CrawlContext, CrawlDecision> decisionMaker);

        /// <summary>
        /// Synchronous method that registers a delegate to be called to determine whether the 1st uri param is considered an internal uri to the second uri param
        /// </summary>
        /// <param name="decisionMaker delegate"></param>
        void IsInternalUri(Func<Uri, Uri, bool> decisionMaker);

        /// <summary>
        /// Begins a crawl using the uri param
        /// </summary>
        CrawlResult Crawl(Uri uri);

        /// <summary>
        /// Dynamic object that can hold any value that needs to be available in the crawl context
        /// </summary>
        dynamic CrawlBag { get; set; }
    }

    public class WebCrawler : IWebCrawler
    {
        static ILog _logger = LogManager.GetLogger(typeof(WebCrawler).FullName);
        protected bool _crawlComplete = false;
        protected bool _crawlStopReported = false;
        protected Timer _timeoutTimer;
        protected CrawlResult _crawlResult = null;
        protected CrawlContext _crawlContext;
        protected IWorkScheduler _workScheduler;
        protected ICrawlList _crawlList;
        protected IPageRequester _httpRequester;
        protected IHyperLinkParser _hyperLinkParser;
        protected ICrawlDecisionMaker _crawlDecisionMaker;
        protected Func<PageToCrawl, CrawlContext, CrawlDecision> _shouldCrawlPageDecisionMaker;
        protected Func<CrawledPage, CrawlContext, CrawlDecision> _shouldDownloadPageContentDecisionMaker;
        protected Func<CrawledPage, CrawlContext, CrawlDecision> _shouldCrawlPageLinksDecisionMaker;
        protected Func<Uri, Uri, bool> _isInternalDecisionMaker = (uriInQuestion, rootUri) => uriInQuestion.Authority == rootUri.Authority;

        /// <summary>
        /// Dynamic object that can hold any value that needs to be available in the crawl context
        /// </summary>
        public dynamic CrawlBag { get; set; }

        #region Constructors

        /// <summary>
        /// Creates a crawler instance with the default settings and implementations.
        /// </summary>
        public WebCrawler()
            : this(null, null, null, null, null, null)
        {
        }

        /// <summary>
        /// Creates a crawler instance with custom settings or implementation. Passing in null for all params is the equivalent of the empty constructor.
        /// </summary>
        /// <param name="workScheduler">Handles scheduling of and crawling tasks that need to be performed</param>
        /// <param name="crawlList">Decides what link should be crawled next</param>
        /// <param name="httpRequester">Makes the raw http requests</param>
        /// <param name="hyperLinkParser">Parses a crawled page for it's hyperlinks</param>
        /// <param name="crawlDecisionMaker">Decides whether or not to crawl a page or that page's links</param>
        /// <param name="crawlConfiguration">Configurable crawl values</param>
        public WebCrawler(
            CrawlConfiguration crawlConfiguration, 
            ICrawlDecisionMaker crawlDecisionMaker, 
            IWorkScheduler workScheduler, 
            ICrawlList crawlList, 
            IPageRequester httpRequester, 
            IHyperLinkParser hyperLinkParser)
        {
            _crawlContext = new CrawlContext();
            _crawlContext.CrawlConfiguration = crawlConfiguration ?? GetCrawlConfigurationFromConfigFile() ?? new CrawlConfiguration();
            CrawlBag = _crawlContext.CrawlBag;

            _workScheduler = workScheduler ?? new ManualThreadManager(_crawlContext.CrawlConfiguration.MaxConcurrentThreads);//new ProducerConsumerThreadManager(_crawlContext.CrawlConfiguration.MaxConcurrentThreads);
            _crawlList = crawlList ?? new FifoCrawlList(_crawlContext.CrawlConfiguration.IsUriRecrawlingEnabled);
            _httpRequester = httpRequester ?? new PageRequester(_crawlContext.CrawlConfiguration);
            _crawlDecisionMaker = crawlDecisionMaker ?? new CrawlDecisionMaker();

            _hyperLinkParser = hyperLinkParser ?? new HapHyperLinkParser();

            _crawlContext.CrawlList = _crawlList;
        }

        #endregion Constructors

        /// <summary>
        /// Begins a synchronous crawl using the uri param, subscribe to events to process data as it becomes available
        /// </summary>
        public virtual CrawlResult Crawl(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");

            _crawlContext.RootUri = uri;

            _crawlResult = new CrawlResult();
            _crawlResult.RootUri = _crawlContext.RootUri;
            _crawlResult.CrawlContext = _crawlContext;
            _crawlComplete = false;

            _logger.InfoFormat("About to crawl site [{0}]", uri.AbsoluteUri);

            PrintConfigValues(_crawlContext.CrawlConfiguration);

            _crawlList.Add(new PageToCrawl(uri) { ParentUri = uri, IsInternal = true, IsRoot = true });

            _crawlContext.CrawlStartDate = DateTime.Now;
            Stopwatch timer = Stopwatch.StartNew();

            if (_crawlContext.CrawlConfiguration.CrawlTimeoutSeconds > 0)
            {
                _timeoutTimer = new Timer(_crawlContext.CrawlConfiguration.CrawlTimeoutSeconds * 1000);
                _timeoutTimer.Elapsed += HandleCrawlTimeout;
                _timeoutTimer.Start();
            }

            try
            {
                CrawlSite();
            }
            catch (Exception e)
            {
                _crawlResult.ErrorException = e;
                _logger.FatalFormat("An error occurred while crawling site [{0}]", uri);
                _logger.Fatal(e);
            }
            finally
            {
                if(_workScheduler != null)
                    _workScheduler.Dispose();
            }

            if(_timeoutTimer != null)
                _timeoutTimer.Stop();

            timer.Stop();

            _crawlResult.Elapsed = timer.Elapsed;
            _logger.InfoFormat("Crawl complete for site [{0}]: [{1}]", _crawlResult.RootUri.AbsoluteUri, _crawlResult.Elapsed);

            return _crawlResult;
        }

        #region Synchronous Events

        /// <summary>
        /// hronous event that is fired before a page is crawled.
        /// </summary>
        public event EventHandler<PageCrawlStartingArgs> PageCrawlStarting;

        /// <summary>
        /// hronous event that is fired when an individual page has been crawled.
        /// </summary>
        public event EventHandler<PageCrawlCompletedArgs> PageCrawlCompleted;

        /// <summary>
        /// hronous event that is fired when the ICrawlDecisionMaker.ShouldCrawl impl returned false. This means the page or its links were not crawled.
        /// </summary>
        public event EventHandler<PageCrawlDisallowedArgs> PageCrawlDisallowed;

        /// <summary>
        /// hronous event that is fired when the ICrawlDecisionMaker.ShouldCrawlLinks impl returned false. This means the page's links were not crawled.
        /// </summary>
        public event EventHandler<PageLinksCrawlDisallowedArgs> PageLinksCrawlDisallowed;

        protected virtual void FirePageCrawlStartingEvent(PageToCrawl pageToCrawl)
        {
            try
            {
                EventHandler<PageCrawlStartingArgs> threadSafeEvent = PageCrawlStarting;
                if (threadSafeEvent != null)
                    threadSafeEvent(this, new PageCrawlStartingArgs(_crawlContext, pageToCrawl));
            }
            catch (Exception e)
            {
                _logger.Error("An unhandled exception was thrown by a subscriber of the PageCrawlStarting event for url:" + pageToCrawl.Uri.AbsoluteUri);
                _logger.Error(e);
            }
        }

        protected virtual void FirePageCrawlCompletedEvent(CrawledPage crawledPage)
        {
            try
            {
                EventHandler<PageCrawlCompletedArgs> threadSafeEvent = PageCrawlCompleted;
                if (threadSafeEvent != null)
                    threadSafeEvent(this, new PageCrawlCompletedArgs(_crawlContext, crawledPage));
            }
            catch (Exception e)
            {
                _logger.Error("An unhandled exception was thrown by a subscriber of the PageCrawlCompleted event for url:" + crawledPage.Uri.AbsoluteUri);
                _logger.Error(e);
            }
        }

        protected virtual void FirePageCrawlDisallowedEvent(PageToCrawl pageToCrawl, string reason)
        {
            try
            {
                EventHandler<PageCrawlDisallowedArgs> threadSafeEvent = PageCrawlDisallowed;
                if (threadSafeEvent != null)
                    threadSafeEvent(this, new PageCrawlDisallowedArgs(_crawlContext, pageToCrawl, reason));
            }
            catch (Exception e)
            {
                _logger.Error("An unhandled exception was thrown by a subscriber of the PageCrawlDisallowed event for url:" + pageToCrawl.Uri.AbsoluteUri);
                _logger.Error(e);
            }
        }

        protected virtual void FirePageLinksCrawlDisallowedEvent(CrawledPage crawledPage, string reason)
        {
            try
            {
                EventHandler<PageLinksCrawlDisallowedArgs> threadSafeEvent = PageLinksCrawlDisallowed;
                if (threadSafeEvent != null)
                    threadSafeEvent(this, new PageLinksCrawlDisallowedArgs(_crawlContext, crawledPage, reason));
            }
            catch (Exception e)
            {
                _logger.Error("An unhandled exception was thrown by a subscriber of the PageLinksCrawlDisallowed event for url:" + crawledPage.Uri.AbsoluteUri);
                _logger.Error(e);
            }
        }

        #endregion

        #region Asynchronous Events

        /// <summary>
        /// Asynchronous event that is fired before a page is crawled.
        /// </summary>
        public event EventHandler<PageCrawlStartingArgs> PageCrawlStartingAsync;

        /// <summary>
        /// Asynchronous event that is fired when an individual page has been crawled.
        /// </summary>
        public event EventHandler<PageCrawlCompletedArgs> PageCrawlCompletedAsync;

        /// <summary>
        /// Asynchronous event that is fired when the ICrawlDecisionMaker.ShouldCrawl impl returned false. This means the page or its links were not crawled.
        /// </summary>
        public event EventHandler<PageCrawlDisallowedArgs> PageCrawlDisallowedAsync;

        /// <summary>
        /// Asynchronous event that is fired when the ICrawlDecisionMaker.ShouldCrawlLinks impl returned false. This means the page's links were not crawled.
        /// </summary>
        public event EventHandler<PageLinksCrawlDisallowedArgs> PageLinksCrawlDisallowedAsync;

        protected virtual void FirePageCrawlStartingEventAsync(PageToCrawl pageToCrawl)
        {
            EventHandler<PageCrawlStartingArgs> threadSafeEvent = PageCrawlStartingAsync;
            if (threadSafeEvent != null)
            {
                //Fire each subscribers delegate async
                foreach (EventHandler<PageCrawlStartingArgs> del in threadSafeEvent.GetInvocationList())
                {
                    del.BeginInvoke(this, new PageCrawlStartingArgs(_crawlContext, pageToCrawl), null, null);
                }
            }
        }

        protected virtual void FirePageCrawlCompletedEventAsync(CrawledPage crawledPage)
        {
            EventHandler<PageCrawlCompletedArgs> threadSafeEvent = PageCrawlCompletedAsync;
            if (threadSafeEvent != null)
            {
                //Fire each subscribers delegate async
                foreach (EventHandler<PageCrawlCompletedArgs> del in threadSafeEvent.GetInvocationList())
                {
                    del.BeginInvoke(this, new PageCrawlCompletedArgs(_crawlContext, crawledPage), null, null);
                }
            }
        }

        protected virtual void FirePageCrawlDisallowedEventAsync(PageToCrawl pageToCrawl, string reason)
        {
            EventHandler<PageCrawlDisallowedArgs> threadSafeEvent = PageCrawlDisallowedAsync;
            if (threadSafeEvent != null)
            {
                //Fire each subscribers delegate async
                foreach (EventHandler<PageCrawlDisallowedArgs> del in threadSafeEvent.GetInvocationList())
                {
                    del.BeginInvoke(this, new PageCrawlDisallowedArgs(_crawlContext, pageToCrawl, reason), null, null);
                }
            }
        }

        protected virtual void FirePageLinksCrawlDisallowedEventAsync(CrawledPage crawledPage, string reason)
        {
            EventHandler<PageLinksCrawlDisallowedArgs> threadSafeEvent = PageLinksCrawlDisallowedAsync;
            if (threadSafeEvent != null)
            {
                //Fire each subscribers delegate async
                foreach (EventHandler<PageLinksCrawlDisallowedArgs> del in threadSafeEvent.GetInvocationList())
                {
                    del.BeginInvoke(this, new PageLinksCrawlDisallowedArgs(_crawlContext, crawledPage, reason), null, null);
                }
            }
        }

        #endregion

        /// <summary>
        /// Synchronous method that registers a delegate to be called to determine whether a page should be crawled or not
        /// </summary>
        public void ShouldCrawlPage(Func<PageToCrawl, CrawlContext, CrawlDecision> decisionMaker)
        {
            _shouldCrawlPageDecisionMaker = decisionMaker;
        }

        /// <summary>
        /// Synchronous method that registers a delegate to be called to determine whether the page's content should be dowloaded
        /// </summary>
        /// <param name="shouldDownloadPageContent"></param>
        public void ShouldDownloadPageContent(Func<CrawledPage, CrawlContext, CrawlDecision> decisionMaker)
        {
            _shouldDownloadPageContentDecisionMaker = decisionMaker;
        }

        /// <summary>
        /// Synchronous method that registers a delegate to be called to determine whether a page's links should be crawled or not
        /// </summary>
        /// <param name="shouldCrawlPageLinksDelegate"></param>
        public void ShouldCrawlPageLinks(Func<CrawledPage, CrawlContext, CrawlDecision> decisionMaker)
        {
            _shouldCrawlPageLinksDecisionMaker = decisionMaker;
        }

        /// <summary>
        /// Synchronous method that registers a delegate to be called to determine whether the 1st uri param is considered an internal uri to the second uri param
        /// </summary>
        /// <param name="decisionMaker delegate"></param>     
        public void IsInternalUri(Func<Uri, Uri, bool> decisionMaker)
        {
            _isInternalDecisionMaker = decisionMaker;
        }

        private CrawlConfiguration GetCrawlConfigurationFromConfigFile()
        {
            AbotConfigurationSectionHandler configFromFile = null;
            try{ configFromFile = AbotConfigurationSectionHandler.LoadFromXml(); } catch {}

            if (configFromFile == null)
            {
                _logger.DebugFormat("abot config section was NOT found");
                return null;
            }

            _logger.DebugFormat("abot config section was found");
            return configFromFile.Convert();
        }

        protected virtual void CrawlSite()
        {
            while (!_crawlComplete)
            {
                if (_crawlList.Count > 0)
                {
                    _workScheduler.DoWork(() => ProcessPage(_crawlList.GetNext()));
                }
                else if (!_workScheduler.HasRunningJobs())
                {
                    _crawlComplete = true;
                }
                else
                {
                    _logger.DebugFormat("Waiting for links to be scheduled...");
                    System.Threading.Thread.Sleep(50);
                }
            }
        }

        protected virtual void CheckForStopRequest()
        {
            if (_crawlContext.IsCrawlStopRequested || _crawlContext.IsCrawlHardStopRequested)
            {
                if (!_crawlStopReported)
                {
                    if(_crawlContext.IsCrawlHardStopRequested)
                        _logger.InfoFormat("Hard crawl stop requested for site [{0}]!", _crawlContext.RootUri);
                    else
                        _logger.InfoFormat("Crawl stop requested for site [{0}]!", _crawlContext.RootUri);

                    _crawlStopReported = true;
                }
            }

            if (_crawlContext.IsCrawlHardStopRequested)
            {
                _workScheduler.AbortAll();

                //Set all events to null so no more events are fired
                PageCrawlStarting = null;
                PageCrawlCompleted = null;
                PageCrawlDisallowed = null;
                PageLinksCrawlDisallowed = null;
                PageCrawlStartingAsync = null;
                PageCrawlCompletedAsync = null;
                PageCrawlDisallowedAsync = null;
                PageLinksCrawlDisallowedAsync = null;
            }
        }

        protected virtual void HandleCrawlTimeout(object sender, ElapsedEventArgs e)
        {
            Timer elapsedTimer = sender as Timer;
            if (elapsedTimer != null)
                elapsedTimer.Stop();

            _logger.InfoFormat("Crawl timeout of [{0}] seconds has been reached for [{1}]", _crawlContext.CrawlConfiguration.CrawlTimeoutSeconds, _crawlContext.RootUri);
            _crawlContext.IsCrawlHardStopRequested = true;
        }

        protected virtual void ProcessPage(PageToCrawl pageToCrawl)
        {
            try
            {
                if (pageToCrawl == null)
                    return;

                if (!ShouldCrawlPage(pageToCrawl))
                    return;

                CrawledPage crawledPage = CrawlThePage(pageToCrawl);

                if (PageSizeIsAboveMax(crawledPage))
                    return;

                FirePageCrawlCompletedEventAsync(crawledPage);
                FirePageCrawlCompletedEvent(crawledPage);

                if (ShouldCrawlPageLinks(crawledPage))
                    EnqueueChildLinksForCrawling(crawledPage);
            }
            catch(Exception e)
            {
                _crawlResult.ErrorException = e;
                _logger.FatalFormat("Error occurred during processing of page [{0}]", pageToCrawl.Uri);
                _logger.Fatal(e);

                _crawlContext.IsCrawlHardStopRequested = true;
            }
        }

        protected virtual bool PageSizeIsAboveMax(CrawledPage crawledPage)
        {
            bool isAboveMax = false;
            if (_crawlContext.CrawlConfiguration.MaxPageSizeInBytes > 0 &&
                crawledPage.PageSizeInBytes > _crawlContext.CrawlConfiguration.MaxPageSizeInBytes)
            {
                isAboveMax = true;
                _logger.DebugFormat("Page [{0}] has a page size of [{1}] bytes which is above the [{2}] byte max", crawledPage.Uri, crawledPage.PageSizeInBytes, _crawlContext.CrawlConfiguration.MaxPageSizeInBytes);
            }
            return isAboveMax;
        }

        protected virtual bool ShouldCrawlPageLinks(CrawledPage crawledPage)
        {
            CrawlDecision shouldCrawlPageLinksDecision = _crawlDecisionMaker.ShouldCrawlPageLinks(crawledPage, _crawlContext);
            if (shouldCrawlPageLinksDecision.Allow)
                shouldCrawlPageLinksDecision = (_shouldCrawlPageLinksDecisionMaker != null) ? _shouldCrawlPageLinksDecisionMaker.Invoke(crawledPage, _crawlContext) : new CrawlDecision { Allow = true };

            if (!shouldCrawlPageLinksDecision.Allow)
            {
                _logger.DebugFormat("Links on page [{0}] not crawled, [{1}]", crawledPage.Uri.AbsoluteUri, shouldCrawlPageLinksDecision.Reason);
                FirePageLinksCrawlDisallowedEventAsync(crawledPage, shouldCrawlPageLinksDecision.Reason);
                FirePageLinksCrawlDisallowedEvent(crawledPage, shouldCrawlPageLinksDecision.Reason);
            }

            return shouldCrawlPageLinksDecision.Allow;
        }

        protected virtual bool ShouldCrawlPage(PageToCrawl pageToCrawl)
        {
            CrawlDecision shouldCrawlPageDecision = _crawlDecisionMaker.ShouldCrawlPage(pageToCrawl, _crawlContext);
            if (shouldCrawlPageDecision.Allow)
                shouldCrawlPageDecision = (_shouldCrawlPageDecisionMaker != null) ? _shouldCrawlPageDecisionMaker.Invoke(pageToCrawl, _crawlContext) : new CrawlDecision { Allow = true };

            if (shouldCrawlPageDecision.Allow)
            {
                AddPageToContext(pageToCrawl);
            }
            else
            {
                _logger.DebugFormat("Page [{0}] not crawled, [{1}]", pageToCrawl.Uri.AbsoluteUri, shouldCrawlPageDecision.Reason);
                FirePageCrawlDisallowedEventAsync(pageToCrawl, shouldCrawlPageDecision.Reason);
                FirePageCrawlDisallowedEvent(pageToCrawl, shouldCrawlPageDecision.Reason);
            }

            return shouldCrawlPageDecision.Allow;
        }

        protected virtual CrawledPage CrawlThePage(PageToCrawl pageToCrawl)
        {
            _logger.DebugFormat("About to crawl page [{0}]", pageToCrawl.Uri.AbsoluteUri);
            FirePageCrawlStartingEventAsync(pageToCrawl);
            FirePageCrawlStartingEvent(pageToCrawl);

            CrawledPage crawledPage = _httpRequester.MakeRequest(pageToCrawl.Uri, (x) => ShouldDownloadPageContentWrapper(x));
            AutoMapper.Mapper.CreateMap<PageToCrawl, CrawledPage>();
            AutoMapper.Mapper.Map(pageToCrawl, crawledPage);

            if (crawledPage.HttpWebResponse == null)
                _logger.InfoFormat("Page crawl complete, Status:[NA] Url:[{0}] Parent:[{1}]", crawledPage.Uri.AbsoluteUri, crawledPage.ParentUri);
            else
                _logger.InfoFormat("Page crawl complete, Status:[{0}] Url:[{1}] Parent:[{2}]", Convert.ToInt32(crawledPage.HttpWebResponse.StatusCode), crawledPage.Uri.AbsoluteUri, crawledPage.ParentUri);

            return crawledPage;

        }

        protected virtual void AddPageToContext(PageToCrawl pageToCrawl)
        {
            _crawlContext.CrawledUrls.Add(pageToCrawl.Uri.AbsoluteUri);

            int domainCount = 0;
            lock (_crawlContext.CrawlCountByDomain)
            {
                if (_crawlContext.CrawlCountByDomain.TryGetValue(pageToCrawl.Uri.Authority, out domainCount))
                    _crawlContext.CrawlCountByDomain[pageToCrawl.Uri.Authority] = domainCount + 1;
                else
                    _crawlContext.CrawlCountByDomain.TryAdd(pageToCrawl.Uri.Authority, 1);
            }
        }

        protected virtual void EnqueueChildLinksForCrawling(CrawledPage crawledPage)
        {
            IEnumerable<Uri> crawledPageLinks = _hyperLinkParser.GetLinks(crawledPage);
            foreach (Uri uri in crawledPageLinks)
            {
                //Added due to a bug in the Uri class related to this (http://stackoverflow.com/questions/2814951/system-uriformatexception-invalid-uri-the-hostname-could-not-be-parsed)
                try
                {
                    PageToCrawl page = new CrawledPage(uri);
                    page.ParentUri = crawledPage.Uri;
                    page.CrawlDepth = crawledPage.CrawlDepth + 1;
                    page.IsInternal = _isInternalDecisionMaker(uri, _crawlContext.RootUri);
                    page.IsRoot = false;
                    _crawlList.Add(page);
                }
                catch{}
            }
        }

        protected virtual CrawlDecision ShouldDownloadPageContentWrapper(CrawledPage crawledPage)
        {
            CrawlDecision decision = _crawlDecisionMaker.ShouldDownloadPageContent(crawledPage, _crawlContext);
            if (decision.Allow)
                decision = (_shouldDownloadPageContentDecisionMaker != null) ? _shouldDownloadPageContentDecisionMaker.Invoke(crawledPage, _crawlContext) : new CrawlDecision { Allow = true };

            return decision;
        }

        protected virtual void PrintConfigValues(CrawlConfiguration config)
        {
            _logger.Info("Configuration Values:");

            string indentString = new string(' ', 2);
            foreach (PropertyInfo property in config.GetType().GetProperties())
            {
                if (property.Name != "ConfigurationExtensions")
                    _logger.InfoFormat("{0}{1}: {2}", indentString, property.Name, property.GetValue(config, null));
            }

            foreach (string key in config.ConfigurationExtensions.Keys)
            {
                _logger.InfoFormat("{0}{1}: {2}", indentString, key, config.ConfigurationExtensions[key]);
            }
        }
    }
}