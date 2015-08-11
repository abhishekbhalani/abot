# Customizing Crawl Behavior #

Abot was designed to be as pluggable as possible. This allows you to easily alter the way it works to suite your needs.

The easiest way to change Abot's behavior for common features is to change the config values that control them. See the QuickStart page for examples on the different ways Abot can be configured.

## CrawlDecision Callbacks ##
Sometimes you don't want to create a class and go through the ceremony of extending a base class or implementing the interface directly. For all you lazy developers out there Abot provides a shorthand method to easily add your custom crawl decision logic. NOTE: The ICrawlDecisionMaker is called first and if it does not "allow" a decision, these callbacks will not be called.

```
PoliteWebCrawler crawler = new PoliteWebCrawler();

crawler.ShouldCrawlPage((pageToCrawl, crawlContext) => 
{
	CrawlDecision decision = new CrawlDecision();
	if(pageToCrawl.Uri.Authority == "google.com")
		return new CrawlDecision{ Allow = false, Reason = "Dont want to crawl google pages" };
	
	return decision;
});

crawler.ShouldDownloadPageContent((crawledPage, crawlContext) =>
{
	CrawlDecision decision = new CrawlDecision();
	if (!crawledPage.Uri.AbsoluteUri.Contains(".com"))
		return new CrawlDecision { Allow = false, Reason = "Only download raw page content for .com tlds" };

	return decision;
});

crawler.ShouldCrawlPageLinks((crawledPage, crawlContext) =>
{
	CrawlDecision decision = new CrawlDecision();
	if (crawledPage.PageSizeInBytes < 100)
		return new CrawlDecision { Allow = false, Reason = "Just crawl links in pages that have at least 100 bytes" };

	return decision;
});
```


## Custom Implementations ##
PoliteWebCrawler is the master of orchestrating the crawl. It's job is to coordinate all the utility classes to "crawl" a site. PoliteWebCrawler accepts an alternate implementation for all its dependencies through it's constructor.

```
PoliteWebCrawler crawler = new PoliteWebCrawler(
        new CrawlConfiguration(),
	new YourCrawlDecisionMaker(),
	new YourThreadMgr(), 
	new YourScheduler(), 
	new YourHttpRequester(), 
	new YourHyperLinkParser(), 
	new YourMemoryManager(), 
        new YourDomainRateLimiter,
	new YourRobotsDotTextFinder());
```

Passing null for any implementation will use the default. The example below will use your custom implementation for the IPageRequester and IHyperLinkParser but will use the default for all others.

```
PoliteWebCrawler crawler = new PoliteWebCrawler(
	null, 
	null, 
        null,
        null,
	new YourPageRequester(), 
	new YourHyperLinkParser(), 
	null,
        null, 
	null);
```

The following are explanations of each interface that PoliteWebCrawler relies on to do the real work.

### ICrawlDecisionMaker ###
The delegate shortcuts are great to add a small amount of logic but if you are doing anything more heavy you will want to pass in your custom implementation of ICrawlDecisionMaker. The crawler calls this implementation to see whether a page should be crawled, whether the page's content should be downloaded and whether a crawled page's links should be crawled.

[CrawlDecisionMaker.cs](https://code.google.com/p/abot/source/browse/branches/1.1/Abot/Core/CrawlDecisionMaker.cs) is the default ICrawlDecisionMaker used by Abot. This class takes care of common checks like making sure the config value MaxPagesToCrawl is not exceeded. Most users will only need to create a class that extends CrawlDecision maker and just add their custom logic. However, you are completely free to create a class that implements ICrawlDecisionMaker and pass it into PoliteWebCrawlers constructor.

```
/// <summary>
/// Determines what pages should be crawled, whether the raw content should be downloaded and if the links on a page should be crawled
/// </summary>
public interface ICrawlDecisionMaker
{
	/// <summary>
	/// Decides whether the page should be crawled
	/// </summary>
	CrawlDecision ShouldCrawlPage(PageToCrawl pageToCrawl, CrawlContext crawlContext);

	/// <summary>
	/// Decides whether the page's links should be crawled
	/// </summary>
	CrawlDecision ShouldCrawlPageLinks(CrawledPage crawledPage, CrawlContext crawlContext);

	/// <summary>
	/// Decides whether the page's content should be dowloaded
	/// </summary>
	CrawlDecision ShouldDownloadPageContent(CrawledPage crawledPage, CrawlContext crawlContext);
}
```


### IThreadManager ###
The IThreadManager interface deals with the multithreading details. It is used by the crawler to manage concurrent http requests.

[ManualThreadManager.cs](https://code.google.com/p/abot/source/browse/branches/1.1/Abot/Core/ManualThreadManager.cs) is the default IThreadManager used by Abot. There are currently two other implementations, [ProducerConsumerThreadManager.cs](https://code.google.com/p/abot/source/browse/branches/1.1/Abot/Core/ProducerConsumerThreadManager.cs) and [TaskThreadManager.cs](https://code.google.com/p/abot/source/browse/branches/1.1/Abot/Core/TaskThreadManager.cs) which are available. They are still being tested to determine their best/worst use cases.

So far here are the observations:

  * ManualThreadManager
    * Seems to be the fastest all around thread manager.
    * Fastest startup since threads are only created when work has to be done on them
    * Creates real thread objects directly. A bit crude but outperforms others.
  * TaskThreadManager
    * Initial tests show that it is only a little slower than the ManualThreadManager impl.
    * Uses tpl/threadpool to execute work so this implementation is the most "Correct" way to implement this interface.
    * Will likely become the default implementation in the next major release.
  * ProducerConsumerThreadManager
    * Seems to be the slowest so far.
    * Slowest startup time since it creates MaxConcurrentThreads threads for every instance even if that instance only crawls a single page




```
/// <summary>
/// Handles the multithreading implementation details
/// </summary>
public interface IThreadManager : IDisposable
{
	/// <summary>
	/// Max number of threads to use.
	/// </summary>
	int MaxThreads { get; }

	/// <summary>
	/// Will perform the action asynchrously on a seperate thread
	/// </summary>
	/// <param name="action">The action to perform</param>
	void DoWork(Action action);

	/// <summary>
	/// Whether there are running threads
	/// </summary>
	bool HasRunningThreads();

	/// <summary>
	/// Abort all running threads
	/// </summary>
	void AbortAll();
}
```


### IScheduler ###
The IScheduler interface deals with managing what pages need to be crawled. The crawler gives the links it finds to and gets the pages to crawl from the IScheduler implementation. A common use cases for writing your own implementation might be to distribute crawls across multiple machines which could be managed by a DistributedScheduler.

[FifoScheduler.cs](https://code.google.com/p/abot/source/browse/branches/1.1/Abot/Core/FifoScheduler.cs) is the default IScheduler used by the crawler. It uses a simple queue to crawl found links in a first found first crawled basis.

```
/// <summary>
/// Handles managing the priority of what pages need to be crawled
/// </summary>
public interface IScheduler
{
	/// <summary>
	/// Count of remaining items that are currently scheduled
	/// </summary>
	int Count { get; }

	/// <summary>
	/// Schedules the param to be crawled
	/// </summary>
	void Add(PageToCrawl page);

	/// <summary>
	/// Schedules the param to be crawled
	/// </summary>
	void Add(IEnumerable<PageToCrawl> pages);

	/// <summary>
	/// Gets the next page to crawl
	/// </summary>
	PageToCrawl GetNext();

	/// <summary>
	/// Clear all currently scheduled pages
	/// </summary>
	void Clear();
}
```


### IPageRequester ###
The IPageRequester interface deals with making the raw http requests.

[PageRequester.cs](https://code.google.com/p/abot/source/browse/branches/1.1/Abot/Core/PageRequester.cs) is the default IPageRequester used by the crawler.

```
public interface IPageRequester
{
	/// <summary>
	/// Make an http web request to the url and download its content
	/// </summary>
	CrawledPage MakeRequest(Uri uri);

	/// <summary>
	/// Make an http web request to the url and download its content based on the param func decision
	/// </summary>
	CrawledPage MakeRequest(Uri uri, Func<CrawledPage, CrawlDecision> shouldDownloadContent);
}
```

### IHyperLinkParser ###
The IHyperLinkParser interface deals with parsing the links out of raw html.

[HapHyperlinkParser.cs](https://code.google.com/p/abot/source/browse/branches/1.1/Abot/Core/HapHyperLinkParser.cs) is the default IHyperLinkParser used by the crawler. It uses the well known html parsing library [Html Agility Pack](http://htmlagilitypack.codeplex.com/). There is also an alternative implementation [CsQueryHyperLinkParser.cs](https://code.google.com/p/abot/source/browse/branches/1.1/Abot/Core/CsQueryHyperLinkParser.cs) which uses [CsQuery](https://github.com/jamietre/CsQuery) to do the parsing. CsQuery uses a css style selector like jquery but all in c#.

```
/// <summary>
/// Handles parsing hyperlikns out of the raw html
/// </summary>
public interface IHyperLinkParser
{
	/// <summary>
	/// Parses html to extract hyperlinks, converts each into an absolute url
	/// </summary>
	IEnumerable<Uri> GetLinks(CrawledPage crawledPage);
}
```

### IMemoryManager ###
The IMemoryManager handles memory monitoring. This feature is still experimental and could be removed in a future release if found to be unreliable.

[MemoryManager.cs](https://code.google.com/p/abot/source/browse/branches/1.1/Abot/Core/MemoryManager.cs) is the default implementation used by the crawler.

```
/// <summary>
/// Handles memory monitoring/usage
/// </summary>
public interface IMemoryManager : IMemoryMonitor, IDisposable
{
	/// <summary>
	/// Whether the current process that is hosting this instance is allocated/using above the param value of memory in mb
	/// </summary>
	bool IsCurrentUsageAbove(int sizeInMb);

	/// <summary>
	/// Whether there is at least the param value of available memory in mb
	/// </summary>
	bool IsSpaceAvailable(int sizeInMb);
}
```

### IDomainRateLimiter ###
The IDomainRateLimiter handles domain rate limiting. It will handle determining how much time needs to elapse before it is ok to make another http request to the domain.

[DomainRateLimiter.cs](https://code.google.com/p/abot/source/browse/branches/1.1/Abot/Core/DomainRateLimiter.cs) is the default implementation used by the crawler.

```
/// <summary>
/// Rate limits or throttles on a per domain basis
/// </summary>
public interface IDomainRateLimiter
{
	/// <summary>
	/// If the domain of the param has been flagged for rate limiting, it will be rate limited according to the configured minimum crawl delay
	/// </summary>
	void RateLimit(Uri uri);

	/// <summary>
	/// Add a domain entry so that domain may be rate limited according the the param minumum crawl delay
	/// </summary>
	void AddDomain(Uri uri, long minCrawlDelayInMillisecs);
}
```


### IRobotsDotTextFinder ###
The IRobotsDotTextFinder is responsible for retrieving the robots.txt file for every domain (if isRespectRobotsDotTextEnabled="true") and building the robots.txt abstraction which implements the IRobotsDotText interface.

[RobotsDotTextFinder.cs](https://code.google.com/p/abot/source/browse/branches/1.1/Abot/Core/RobotsDotTextFinder.cs) is the default implementation used by the crawler.

```
/// <summary>
/// Finds and builds the robots.txt file abstraction
/// </summary>
public interface IRobotsDotTextFinder
{
	/// <summary>
	/// Finds the robots.txt file using the rootUri. 
        /// 
	IRobotsDotText Find(Uri rootUri);
}
```