using Abot.Core;
using Abot.Poco;
using log4net;

namespace Abot.Crawler
{
    ////TODO move this to the real implementation
    //public interface IThrottler
    //{

    //}

    public class PoliteWebCrawler : WebCrawler
    {
        private static ILog _logger = LogManager.GetLogger(typeof(PoliteWebCrawler).FullName);
        protected IDomainRateLimiter _domainRateLimiter;
        //protected IThrottler _throttler;

        public PoliteWebCrawler()
            : this(null, null, null, null, null, null, null, null)
        {
        }

        public PoliteWebCrawler(
            CrawlConfiguration crawlConfiguration,
            ICrawlDecisionMaker crawlDecisionMaker,
            IThreadManager threadManager,
            IScheduler scheduler,
            IPageRequester httpRequester,
            IHyperLinkParser hyperLinkParser,
            IDomainRateLimiter domainRateLimiter,
            IRobotsDotTextFinder robotsDotTextFinder)
            : base(threadManager, scheduler, httpRequester, hyperLinkParser, crawlDecisionMaker, crawlConfiguration)
        {
            bool isRespectRobotsDotTextCrawlDelayEnabled = false;//TODO add to config helper
            
            long robotsCrawlDelay = 0;
            if (isRespectRobotsDotTextCrawlDelayEnabled)
            {
                //get robots.txt file
                //get the crawl delay
            }


            long crawlDelayInMilliseconds = _crawlContext.CrawlConfiguration.MinCrawlDelayPerDomainMilliSeconds;
            if(robotsCrawlDelay > _crawlContext.CrawlConfiguration.MinCrawlDelayPerDomainMilliSeconds)
                crawlDelayInMilliseconds = robotsCrawlDelay;

            if (_crawlContext.CrawlConfiguration.MinCrawlDelayPerDomainMilliSeconds > 0)
            {
                _domainRateLimiter = domainRateLimiter ?? new DomainRateLimiter(crawlDelayInMilliseconds);
                PageCrawlStarting += (sender, e) => _domainRateLimiter.RateLimit(e.PageToCrawl.Uri);
            }

            //if (_crawlContext.CrawlConfiguration.IsThrottlingEnabled)
            //{
            //    PageCrawlCompleted += ProcessForThrottling;
            //}
        }
    }
}
