
namespace Abot.Crawler
{
    ////TODO move this to the real implementation
    //public interface IThrottler
    //{

    //}

    public class PoliteWebCrawler : WebCrawler
    {
        //private static ILog _logger = LogManager.GetLogger(typeof(PoliteWebCrawler).FullName);
        //protected IDomainRateLimiter _domainRateLimiter;
        //protected IThrottler _throttler;

        //public PoliteWebCrawler()
        //    : this(null, null, null, null, null, null, null)
        //{
        //}

        //public PoliteWebCrawler(
        //    CrawlConfiguration crawlConfiguration,
        //    ICrawlDecisionMaker crawlDecisionMaker,
        //    IThreadManager threadManager,
        //    IScheduler scheduler,
        //    IPageRequester httpRequester,
        //    IHyperLinkParser hyperLinkParser,
        //    IDomainRateLimiter domainRateLimiter)
        //    : base(threadManager, scheduler, httpRequester, hyperLinkParser, crawlDecisionMaker, crawlConfiguration)
        //{
        //    _domainRateLimiter = domainRateLimiter ?? new DomainRateLimiter(_crawlContext.CrawlConfiguration.MinCrawlDelayPerDomainMilliSeconds);

        //    if (_crawlContext.CrawlConfiguration.MinCrawlDelayPerDomainMilliSeconds > 0)
        //        PageCrawlStarting += RateLimitHttpRequests;
        //}

        //protected virtual void RateLimitHttpRequests(object sender, PageCrawlStartingArgs e)
        //{
        //    _domainRateLimiter.RateLimit(e.PageToCrawl.Uri);
        //}

        ////ToDo Add throttling
        ////ToDo Add manual crawl delay
        ////ToDo Add respect robots crawl delay
        ////ToDo Add respect robots disallow directive
        ////ToDo Add respect meta robots no index no follow
    }
}
