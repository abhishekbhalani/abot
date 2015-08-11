# QuickStart #


### Installing Abot ###
  * Install Abot using Nuget https://www.nuget.org/packages/Abot/
  * If you prefer to build from source yourself see the [Working With The Source Code](v1d5SourceCode.md) tutorial

### Using Abot ###
  1. Add the following using statements..
```
  using Abot.Crawler;
  using Abot.Poco;
```
  1. Configure Abot using **ONE** of the following methods. Look at https://github.com/sjdirect/abot/blob/master/Abot/Poco/CrawlConfiguration.cs comments to see what effect each config value has on the crawl.
    1. Add the following to the app.config or web.config file of the assembly using the library. Nuget will NOT add this for you. **NOTE: The gcServer or gcConcurrent entry may help memory usage in your specific use of abot.**
```
<configuration>
  <configSections>
    <section name="log4net" type="log4net.Config.Log4NetConfigurationSectionHandler, log4net"/>
    <section name="abot" type="Abot.Core.AbotConfigurationSectionHandler, Abot"/>
  </configSections>
  <runtime>
    <!-- Experiment with these to see if it helps your memory usage, USE ONLY ONE OF THE FOLLOWING -->
    <!--<gcServer enabled="true"/>-->
    <!--<gcConcurrent enabled="true"/>-->
  </runtime>
  <log4net>
    <appender name="ConsoleAppender" type="log4net.Appender.ConsoleAppender">
      <layout type="log4net.Layout.PatternLayout">
        <conversionPattern value="[%date] [%thread] [%-5level] - %message%newline"/>
      </layout>
    </appender>
    <appender name="RollingFileAppender" type="log4net.Appender.RollingFileAppender">
      <file value="abotlog.txt"/>
      <appendToFile value="true"/>
      <rollingStyle value="Size"/>
      <maxSizeRollBackups value="10"/>
      <maximumFileSize value="10240KB"/>
      <staticLogFileName value="true"/>
      <layout type="log4net.Layout.PatternLayout">
        <conversionPattern value="[%date] [%-3thread] [%-5level] - %message%newline"/>
      </layout>
    </appender>
    <root>
      <level value="INFO"/>
      <appender-ref ref="ConsoleAppender"/>
      <appender-ref ref="RollingFileAppender"/>
    </root>
  </log4net>

  <abot>
    <crawlBehavior 
      maxConcurrentThreads="10" 
      maxPagesToCrawl="1000" 
      maxPagesToCrawlPerDomain="0" 
      maxPageSizeInBytes="0"
      userAgentString="Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko" 
      crawlTimeoutSeconds="0" 
      downloadableContentTypes="text/html, text/plain" 
      isUriRecrawlingEnabled="false" 
      isExternalPageCrawlingEnabled="false" 
      isExternalPageLinksCrawlingEnabled="false"
      httpServicePointConnectionLimit="200"  
      httpRequestTimeoutInSeconds="15" 
      httpRequestMaxAutoRedirects="7" 
      isHttpRequestAutoRedirectsEnabled="true" 
      isHttpRequestAutomaticDecompressionEnabled="false"
      isSendingCookiesEnabled="false"
      minAvailableMemoryRequiredInMb="0"
      maxMemoryUsageInMb="0"
      maxMemoryUsageCacheTimeInSeconds="0"
      maxCrawlDepth="1000"
      isForcedLinkParsingEnabled="false"
      maxRetryCount="0"
      minRetryDelayInMilliseconds="0"
      />
    <politeness 
      isRespectRobotsDotTextEnabled="false"
      isRespectMetaRobotsNoFollowEnabled="false"
      isRespectAnchorRelNoFollowEnabled="false"
      robotsDotTextUserAgentString="abot"
      maxRobotsDotTextCrawlDelayInSeconds="5" 
      minCrawlDelayPerDomainMilliSeconds="0"/>
    <extensionValues>
      <add key="key1" value="value1"/>
      <add key="key2" value="value2"/>
    </extensionValues>
  </abot>  
</configuration>    
```
    1. Create an instance of Abot.Poco.CrawlConfiguration. This approach ignores the app.config values completely. Every app.config value has a property with the same name on the CrawlConfiguration object.
```
CrawlConfiguration crawlConfig = new CrawlConfiguration();
crawlConfig.CrawlTimeoutSeconds = 100;
crawlConfig.MaxConcurrentThreads = 10;
crawlConfig.MaxPagesToCrawl = 1000;
crawlConfig.UserAgentString = "abot v1.0 http://code.google.com/p/abot";
crawlConfig.ConfigurationExtensions.Add("SomeCustomConfigValue1", "1111");
crawlConfig.ConfigurationExtensions.Add("SomeCustomConfigValue2", "2222");
etc...
```
    1. Load from app.config then tweek
```
CrawlConfiguration crawlConfig = AbotConfigurationSectionHandler.LoadFromXml().Convert();
crawlConfig.CrawlTimeoutSeconds = 100;
crawlConfig.MaxConcurrentThreads = 10;
crawlConfig.MaxPagesToCrawl = 1000;
crawlConfig.UserAgentString = "abot v1.0 http://code.google.com/p/abot";
crawlConfig.ConfigurationExtensions.Add("SomeCustomConfigValue1", "1111");
crawlConfig.ConfigurationExtensions.Add("SomeCustomConfigValue2", "2222");
etc...
```
  1. Create an instance of Abot.Crawler.PoliteWebCrawler
```
//Will use app.config for confguration
PoliteWebCrawler crawler = new PoliteWebCrawler();
```
```
//Will use the manually created crawlConfig object created above
PoliteWebCrawler crawler = new PoliteWebCrawler(crawlConfig, null, null, null, null, null, null, null, null);
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

	if (string.IsNullOrEmpty(crawledPage.Content.Text))
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
  1. Add any number of custom objects to the dynamic crawl bag or page bag. These objects will be available in the CrawlContext.CrawlBag object, PageToCrawl.PageBag object or CrawledPage.PageBag object.
```
PoliteWebCrawler crawler = new PoliteWebCrawler();
crawler.CrawlBag.MyFoo1 = new Foo();
crawler.CrawlBag.MyFoo2 = new Foo();
crawler.PageCrawlStartingAsync += crawler_ProcessPageCrawlStarting;
...
```
```
void crawler_ProcessPageCrawlStarting(object sender, PageCrawlStartingArgs e)
{
        //Get your Foo instances from the CrawlContext object
	CrawlContext context = e.CrawlContext;
        context.CrawlBag.MyFoo1.Bar();
        context.CrawlBag.MyFoo2.Bar();

        //Also add a dynamic value to the PageToCrawl or CrawledPage
        e.PageToCrawl.PageBag.Bar = new Bar();
}
```
  1. Run the crawl
```
CrawlResult result = crawler.Crawl(new Uri("http://localhost:1111/"));

if (result.ErrorOccurred)
	Console.WriteLine("Crawl of {0} completed with error: {1}", result.RootUri.AbsoluteUri, result.ErrorException.Message);
else
	Console.WriteLine("Crawl of {0} completed without error.", result.RootUri.AbsoluteUri);

```
  1. OR run the crawl with a cancellation token to stop the crawl early
```
CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

PoliteWebCrawler crawler = new PoliteWebCrawler();
CrawlResult result = crawler.Crawl(new Uri("addurihere"), cancellationTokenSource);
```


## Logging (Optional) ##

Abot uses Log4Net to log messages. These log statements are a great way to see whats going on during a crawl. However, if you dont want to use log4net you can skip this section.

Below is an example log4net configuration. Read more abot log4net at http://logging.apache.org/log4net/release/manual/introduction.html.

Add using statement for log4net.
```
using log4net.Config;
```

Be sure to call the following method to tell log4net to read in the config file. This call must happen before Abot's Crawl(Uri) method, otherwise you wont see any output.
```
XmlConfigurator.Configure();
```

The following configuration data should be added to the app.config file of the application that will be running Abot.
```
  <configSections>
    <section name="log4net" type="log4net.Config.Log4NetConfigurationSectionHandler, log4net" />
  </configSections>
  
  <log4net>
    <appender name="ConsoleAppender" type="log4net.Appender.ConsoleAppender">
      <layout type="log4net.Layout.PatternLayout">
        <conversionPattern value="[%date] [%thread] [%-5level] - %message - [%logger]%newline"/>
      </layout>
    </appender>
    <appender name="AbotAppender" type="log4net.Appender.RollingFileAppender">
      <file value="abotlog.txt"/>
      <appendToFile value="true"/>
      <rollingStyle value="Size"/>
      <maxSizeRollBackups value="10"/>
      <maximumFileSize value="10240KB"/>
      <staticLogFileName value="true"/>
      <preserveLogFileNameExtension value="true" />
      <layout type="log4net.Layout.PatternLayout">
        <conversionPattern value="[%date] [%thread] [%-5level] - %message - [%logger]%newline"/>
      </layout>
    </appender>
    <logger name="AbotLogger">
      <level value="INFO"/>
      <appender-ref ref="ConsoleAppender"/>
      <appender-ref ref="AbotAppender"/>
    </logger>
  </log4net>
```