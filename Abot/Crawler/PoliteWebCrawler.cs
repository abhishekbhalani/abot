using Abot.Core;
using Abot.Poco;
using log4net;
using System;

namespace Abot.Crawler
{
    /// <summary>
    /// Extends the WebCrawler class and added politeness features like crawl delays and respecting robots.txt files. 
    /// </summary>
    public class PoliteWebCrawler : WebCrawler
    {
        private static ILog _logger = LogManager.GetLogger(typeof(PoliteWebCrawler).FullName);
        protected IDomainRateLimiter _domainRateLimiter;
        protected IRobotsDotTextFinder _robotsDotTextFinder;
        protected IRobotsDotText _robotsDotText;

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
            _domainRateLimiter = domainRateLimiter ?? new DomainRateLimiter(_crawlContext.CrawlConfiguration.MinCrawlDelayPerDomainMilliSeconds);
            _robotsDotTextFinder = robotsDotTextFinder ?? new RobotsDotTextFinder(new PageRequester(_crawlContext.CrawlConfiguration.UserAgentString));
        }

        public override CrawlResult Crawl(Uri uri)
        {
            int robotsDotTextCrawlDelay = 0;
            if (_crawlContext.CrawlConfiguration.IsRespectRobotsDotTextEnabled)
            {
                _robotsDotText = _robotsDotTextFinder.Find(uri);

                if (_robotsDotText != null)
                    robotsDotTextCrawlDelay = _robotsDotText.GetCrawlDelay(_crawlContext.CrawlConfiguration.UserAgentString) * 1000;
            }

            if (robotsDotTextCrawlDelay > 0 && robotsDotTextCrawlDelay > _crawlContext.CrawlConfiguration.MinCrawlDelayPerDomainMilliSeconds)
            {
                if (robotsDotTextCrawlDelay > _crawlContext.CrawlConfiguration.MaxRobotsDotTextCrawlDelayInSeconds)
                {
                    robotsDotTextCrawlDelay = _crawlContext.CrawlConfiguration.MaxRobotsDotTextCrawlDelayInSeconds;
                    _logger.WarnFormat("[{0}] robot.txt file directive [Crawl-delay: {1}] is above the value set in the config value MaxRobotsDotTextCrawlDelay, will use MaxRobotsDotTextCrawlDelay value instead.", uri, robotsDotTextCrawlDelay);
                }

                _logger.WarnFormat("[{0}] robot.txt file directive [Crawl-delay: {1}] will be respected.", uri, robotsDotTextCrawlDelay);
                _domainRateLimiter.AddDomain(uri, robotsDotTextCrawlDelay);
            }

            if(robotsDotTextCrawlDelay > 0 || _crawlContext.CrawlConfiguration.MinCrawlDelayPerDomainMilliSeconds > 0)
                PageCrawlStarting += (s, e) => _domainRateLimiter.RateLimit(e.PageToCrawl.Uri);

            return base.Crawl(uri);
        }
        
        protected override bool ShouldCrawlPage(PageToCrawl pageToCrawl)
        {
            bool allowedByRobots = true;
            if (_robotsDotText != null)
                allowedByRobots = _robotsDotText.IsUrlAllowed(pageToCrawl.Uri.AbsoluteUri, _crawlContext.CrawlConfiguration.UserAgentString);

            if (!allowedByRobots)
            {
                string message = string.Format("Page [{0}] not crawled, [Disallowed by robots.txt file], set IsRespectRobotsDotText=false in config file if you would like to ignore robots.txt files.", pageToCrawl.Uri.AbsoluteUri);
                _logger.DebugFormat(message);

                FirePageCrawlDisallowedEventAsync(pageToCrawl, message);
                FirePageCrawlDisallowedEvent(pageToCrawl, message);
            }
            return base.ShouldCrawlPage(pageToCrawl);
        }
    }
}
