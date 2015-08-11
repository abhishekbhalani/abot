# QuickStart #


### Installing Abot ###
  1. Download the latest from the [Downloads](http://code.google.com/p/abot/downloads/list) page
  1. Verify the project that will use Abot targets .NET framework version 4.0 or greater
  1. Extract the Abot.dll file and add a reference from your project
  1. Nuget package coming soon

### Using Abot ###
  1. Add the following using statements..
```
  using Abot.Crawler;
  using Abot.Core;
  using Abot.Poco;
```
  1. Configure Abot using one of the following
    1. Add the following to the app.config or web.cofig of the assembly using the library
```
<?xml version="1.0"?>
<configuration>
  <configSections>
    <section name="abot" type="Abot.Core.ConfigurationSectionHandler, Abot"/>
  </configSections>

  <abot>
    <crawlBehavior
      maxConcurrentThreads="10"
      maxPagesToCrawl="1000"
      userAgentString="abot v1.0 http://code.google.com/p/abot"
      crawlTimeoutSeconds="0"
      downloadableContentTypes="text/html"
      isUriRecrawlingEnabled="false"
      isExternalPageCrawlingEnabled="true"
      isExternalPageLinksCrawlingEnabled="false"
        />
    <politeness
      isThrottlingEnabled="false"
      minCrawlDelayPerDomainMilliSeconds="0"
      />
    <extensionValues>
      <add key="SomeCustomConfigValue1" value="1111" />
      <add key="SomeCustomConfigValue2" value="2222" />
    </extensionValues>
  </abot>  
</configuration>
```
    1. Create an instance of Abot.Poco.CrawlConfiguration (this ignores app.config values)
```
CrawlConfiguration crawlConfig = new CrawlConfiguration();
crawlConfig.CrawlTimeoutSeconds = 3600;//default is 0 which means it will not timeout
crawlConfig.MaxConcurrentThreads = 10;//default
crawlConfig.MaxPagesToCrawl = 1000;//default
crawlConfig.UserAgentString = "abot v1.0 http://code.google.com/p/abot";//default
crawlConfig.ConfigurationExtensions.Add("SomeCustomConfigValue1", "1111");//Add custom config values
crawlConfig.ConfigurationExtensions.Add("SomeCustomConfigValue2", "2222");//Add custom config values
```
  1. Create an instance of Abot.Crawler.WebCrawler
```
//Will use app.config for confguration
WebCrawler crawler = new WebCrawler();
```
```
//Will use the manually created crawlConfig object created above
WebCrawler crawler = new WebCrawler(crawlConfig);
```

  1. Register for events and create processing methods (both synchronous and asynchronous versions available)
```
  crawler.PageCrawlStartingAsync += crawler_ProcessPageCrawlStarting;
  crawler.PageCrawlCompletedAsync += crawler_ProcessPageCrawlCompleted;
  crawler.PageCrawlDisallowedAsync += crawler_PageCrawlDisallowed;
  crawler.PageLinksCrawlDisallowedAsync += crawler_PageLinksCrawlDisallowed;
```
```
void crawler_ProcessPageCrawlStarting(object sender, PageCrawlStartingArgs e)
{
	PageToCrawl pageToCrawl = e.PageToCrawl;
	Console.WriteLine("About to crawl link {0} which was found on page {1}", pageToCrawl.Uri.AbsoluteUri, pageToCrawl.ParentUri.AbsoluteUri);
}

void crawler_ProcessPageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
{
	CrawledPage crawledPage = e.CrawledPage;

	if (crawledPage.WebException != null || crawledPage.HttpWebResponse.StatusCode != HttpStatusCode.OK)
		Console.WriteLine("Crawl of page failed {0}", crawledPage.Uri.AbsoluteUri);
	else
		Console.WriteLine("Crawl of page succeeded {0}", crawledPage.Uri.AbsoluteUri);

	if (string.IsNullOrEmpty(crawledPage.RawContent))
		Console.WriteLine("Page had no content {0}", crawledPage.Uri.AbsoluteUri);
}

void crawler_PageLinksCrawlDisallowed(object sender, PageLinksCrawlDisallowedArgs e)
{
	CrawledPage crawledPage = e.CrawledPage;
	Console.WriteLine("Did not crawl the links on page {0} due to {1}", crawledPage.Uri.AbsoluteUri, e.DisallowedReason);
}

void crawler_PageCrawlDisallowed(object sender, PageCrawlDisallowedArgs e)
{
	PageToCrawl pageToCrawl = e.PageToCrawl;
	Console.WriteLine("Did not crawl page {0} due to {1}", pageToCrawl.Uri.AbsoluteUri, e.DisallowedReason);
}
```
  1. Run the crawl and check the crawl result
```
CrawlResult result = crawler.Crawl(new Uri("http://localhost:1111/"));

if (result.ErrorOccurred)
	Console.WriteLine("Crawl of {0} completed with error: {1}", result.RootUri.AbsoluteUri, result.ErrorMessage);
else
	Console.WriteLine("Crawl of {0} completed without error.", result.RootUri.AbsoluteUri);

```
> Notice that the CrawlResult object does not have a collection of pages that were crawled. Keeping all the crawled pages in memory by default can cause a system out of memory exception if enough pages are crawled. So if you need to keep all the pages that were crawled in memory you must use the events above.

### Changing Crawl Behavior ###
Plug in your own implementations of key interfaces.

```
WebCrawler crawler = new WebCrawler(
	new YourThreadMgr(), 
	new YourScheduler(), 
	new YourHttpRequester(), 
	new YourHyperLinkParser(), 
        new YourDomainRateLimiter,
	new YourCrawlDecisionMaker(), //You will most likely write an impl for this
	crawlConfig);
```

Passing null for any implementation will use the default. The example below will use your custom implementation for the IHttpRequester and IHyperLinkParser but will use the default for all others.

```
WebCrawler crawler = new WebCrawler(
	null, 
	null, 
	new YourHttpRequester(), 
	new YourHyperLinkParser(), 
	null,
        null, 
	null);
```

You will likely need to write your own implementation for ICrawlDecisionMaker since this is what will determine what pages get crawled, what pages will have their links crawled and whether the page's raw content should be downloaded. Also consider overriding CrawlDecisionMaker.cs methods of interest to take advantage of some of its common behaviors like making sure a url is only crawled once.

```
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
	/// Decides whether the page's content should be downloaded
	/// </summary>
	CrawlDecision ShouldDownloadPageContent(CrawledPage crawledPage, CrawlContext crawlContext);
}
```

## Logging ##

Abot uses Log4Net to log messages. See the app.config files in projects projects Abot.Demo or Abot.Tests.Integration to see how Log4Net is configured. These log statements are a great way to see whats going on during a crawl. Below is from Abot.Demo project, app.config file. If this prints out to much data for you change the level from "DEBUG" to "INFO".

```
  <configSections>
    <section name="log4net" type="log4net.Config.Log4NetConfigurationSectionHandler, log4net" />
  </configSections>

  <log4net>
    <appender name="ConsoleAppender" type="log4net.Appender.ConsoleAppender">
      <layout type="log4net.Layout.PatternLayout">
        <conversionPattern value="[%date] [%thread] [%-5level] - %message%newline" />
      </layout>
    </appender>
    <appender name="RollingFileAppender" type="log4net.Appender.RollingFileAppender">
      <file value="log.txt" />
      <appendToFile value="true" />
      <rollingStyle value="Size" />
      <maxSizeRollBackups value="10" />
      <maximumFileSize value="10240KB" />
      <staticLogFileName value="true" />
      <layout type="log4net.Layout.PatternLayout">
        <conversionPattern value="[%date] [%-3thread] [%-5level] - %message%newline" />
      </layout>
    </appender>
    <root>
      <level value="DEBUG" />
      <appender-ref ref="ConsoleAppender" />
      <appender-ref ref="RollingFileAppender" />
    </root>
  </log4net>
```