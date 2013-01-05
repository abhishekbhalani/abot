
using System;
using System.Net;
using Abot.Crawler;
using Abot.Poco;

namespace Abot.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            PrintDisclaimer();

            Uri uriToCrawl = GetSiteToCrawl(args);

            WebCrawler crawler;

            //Uncomment only one of the following to see that instance in action
            crawler = GetDefaultWebCrawler(uriToCrawl);
            //crawler = GetManuallyConfiguredWebCrawler(uriToCrawl)
            //crawler = CrawlUsingCustomCrawlDecesionMakerImplementation(uriToCrawl);

            //Subscribe to any of these asynchronous events, there are also sychronous versions of each.
            //This is where you process data about specific events of the crawl
            crawler.PageCrawlStartingAsync += crawler_ProcessPageCrawlStarting;
            crawler.PageCrawlCompletedAsync += crawler_ProcessPageCrawlCompleted;
            crawler.PageCrawlDisallowedAsync += crawler_PageCrawlDisallowed;
            crawler.PageLinksCrawlDisallowedAsync += crawler_PageLinksCrawlDisallowed;

            //Start the crawl
            //This is a synchronous call
            CrawlResult result = crawler.Crawl(uriToCrawl);

            //Now go view the log.txt file that is in the same directory as this executable. It has
            //all the statements that you were trying to read in the console window :)

            PrintDisclaimer();
        }

        private static WebCrawler GetDefaultWebCrawler()
        {
            return new WebCrawler();
        }

        private static WebCrawler GetManuallyConfiguredWebCrawler()
        {
            //Create a config object manually
            CrawlConfiguration config = new CrawlConfiguration();
            config.CrawlTimeoutSeconds = 0; 
            config.DownloadableContentTypes = "text/html, text/plain";
            config.IsExternalPageCrawlingEnabled = false;
            config.IsExternalPageLinksCrawlingEnabled = false;
            config.IsThrottlingEnabled = false;
            config.IsUriRecrawlingEnabled = false;
            config.MaxConcurrentThreads = 10;
            config.MaxPagesToCrawl = 10;
            config.MaxPagesToCrawlPerDomain = 0;
            config.MinCrawlDelayPerDomainMilliSeconds = 1000;
            config.UserAgentString = "abot v@ABOTASSEMBLYVERSION@ http://code.google.com/p/abot";

            //Add you own values without modifying Abot's source code.
            //these are accessible in CrawlContext.CrawlConfuration.ConfigurationException object throughout the crawl
            config.ConfigurationExtensions.Add("Somekey1", "SomeValue1");
            config.ConfigurationExtensions.Add("Somekey2", "SomeValue2");

            //Initialize the crawler with custom configuration created above.
            //This override the app.config file values
            return new WebCrawler(config);
        }

        private static WebCrawler GetCustomBehaviorUsingLambdaWebCrawler()
        {
            WebCrawler crawler = GetDefaultWebCrawler();

            //Register a lambda expression that will make Abot not crawl any url that has the word "ghost" in it.
            //For example http://a.com/ghost, would not get crawled if the link were found during the crawl.
            //If you set the log4net log level to "DEBUG" you will see a log message when any page is not allowed to be crawled.
            //NOTE: This is lambda is run after the regular ICrawlDecsionMaker.ShouldCrawlPage method is run.
            crawler.ShouldCrawlPage((pageToCrawl, crawlContext) =>
            {
                if (pageToCrawl.Uri.AbsoluteUri.Contains("ghost"))
                    return new CrawlDecision { Allow = false, Reason = "Scared of ghosts" };

                return new CrawlDecision { Allow = true };
            });

            //Register a lambda expression that will tell Abot to not download the page content for any page after 5.
            //Abot will still make the http request but will not read the raw content from the stream
            //NOTE: This lambda is run after the regular ICrawlDecsionMaker.ShouldDownloadPageContent method is run
            crawler.ShouldDownloadPageContent((crawledPage, crawlContext) =>
            {
                if (crawlContext.CrawledUrls.Count >= 5)
                    return new CrawlDecision { Allow = false, Reason = "We already downloaded the raw page content for 5 pages" };

                return new CrawlDecision { Allow = true };
            });

            //Register a lambda expression that will tell Abot to not crawl links on any page that is not internal to the root uri.
            //NOTE: This lambda is run after the regular ICrawlDecsionMaker.ShouldCrawlPageLinks method is run
            crawler.ShouldCrawlPageLinks((crawledPage, crawlContext) =>
            {
                if (!crawledPage.IsInternal)
                    return new CrawlDecision { Allow = false, Reason = "We dont crawl links of external pages" };

                return new CrawlDecision { Allow = true };
            });

            return crawler;
        }

        private static Uri GetSiteToCrawl(string[] args)
        {
            string userInput = "";
            if (args.Length < 1)
            {
                System.Console.WriteLine("Please enter ABSOLUTE url to crawl:");
                userInput = System.Console.ReadLine();
            }
            else
            {
                userInput = args[0];
            }

            if (string.IsNullOrWhiteSpace(userInput))
                throw new ApplicationException("Site url to crawl is as a required parameter");

            return new Uri(userInput);
        }

        static void crawler_ProcessPageCrawlStarting(object sender, PageCrawlStartingArgs e)
        {
            PageToCrawl pageToCrawl = e.PageToCrawl;
            //PageToCrawl object has information about the page/link being crawled along with the page it was found on
        }

        static void crawler_ProcessPageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            CrawledPage crawledPage = e.CrawledPage;
            //CrawlPage object, which is a child of PageToCrawl
            //It has information about the http request/response along with the raw page content

            if (crawledPage.WebException != null || crawledPage.HttpWebResponse.StatusCode != HttpStatusCode.OK)
                //do something
            else
                Console.WriteLine("Crawl of page succeeded [{0}]", crawledPage.Uri.AbsoluteUri);

            if (string.IsNullOrEmpty(crawledPage.RawContent))
                Console.WriteLine("Page had no content [{0}]", crawledPage.Uri.AbsoluteUri);
        }

        static void crawler_PageLinksCrawlDisallowed(object sender, PageLinksCrawlDisallowedArgs e)
        {
            CrawledPage crawledPage = e.CrawledPage;
            Console.WriteLine("Did not crawl the links on page [{0}] [{1}]", crawledPage.Uri.AbsoluteUri, e.DisallowedReason);
        }

        static void crawler_PageCrawlDisallowed(object sender, PageCrawlDisallowedArgs e)
        {
            PageToCrawl pageToCrawl = e.PageToCrawl;
            Console.WriteLine("Did not crawl page [{0}] due to [{1}]", pageToCrawl.Uri.AbsoluteUri, e.DisallowedReason);
        }

        private static void PrintDisclaimer()
        {
            PrintAttentionText("The demo is configured to only crawl a total of 10 pages and will wait 1 second in between http requests. This is to avoid getting you blocked by your isp or the sites you are trying to crawl. You can change these values in the app.config or Abot.Console.exe.config file.");
        }

        private static void PrintAttentionText(string text)
        {
            ConsoleColor originalColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.WriteLine(text);
            System.Console.ForegroundColor = originalColor;
        }
    }
}
