
using System;
using Abot.Crawler;
using Abot.Poco;

namespace Abot.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            //TODO: Decide how to demo use of event data without logging since its already logged from log4net
            string userInput = "";
            if (args.Length < 1)
            {
                ConsoleColor originalColor = System.Console.ForegroundColor;
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine("The demo by default is configured to only crawl a total of 10 pages and will wait 1 second in between http requests. This is to avoid getting you blocked by your isp or the sites you are trying to crawl. You can change these values in the app.config file.");
                System.Console.ForegroundColor = originalColor;
                System.Console.WriteLine("Please enter ABSOLUTE url to crawl:");
                userInput = System.Console.ReadLine();
            }
            else
            {
                userInput = args[0];
            }

            if(string.IsNullOrWhiteSpace(userInput))
                throw new ApplicationException("Site url to crawl is as a required parameter");

            Uri uriToCrawl = new Uri(userInput);

            //Initialize the crawler
            WebCrawler crawler = new WebCrawler();

            //Subscribe to any of these asynchronous events, there are also sychronous versions of each
            crawler.PageCrawlStartingAsync += crawler_ProcessPageCrawlStarting;
            crawler.PageCrawlCompletedAsync += crawler_ProcessPageCrawlCompleted;
            crawler.PageCrawlDisallowedAsync += crawler_PageCrawlDisallowed;
            crawler.PageLinksCrawlDisallowedAsync += crawler_PageLinksCrawlDisallowed;

            //Start the crawl
            CrawlResult result = crawler.Crawl(uriToCrawl);

            ////Print some result data
            //if (result.ErrorOccurred)
            //    _logger.ErrorFormat("Crawl of [{0}] completed with error: [{1}]", result.RootUri.AbsoluteUri, result.ErrorMessage);
            //else
            //    _logger.InfoFormat("Crawl of [{0}] completed without error.", result.RootUri.AbsoluteUri);

            //_logger.InfoFormat("Completed in [{0}]", result.Elapsed);
        }

        static void crawler_ProcessPageCrawlStarting(object sender, PageCrawlStartingArgs e)
        {
            //PageToCrawl pageToCrawl = e.PageToCrawl;
            //_logger.InfoFormat("About to crawl link [{0}] which was found on page [{1}]", pageToCrawl.Uri.AbsoluteUri, pageToCrawl.ParentUri.AbsoluteUri);
        }

        static void crawler_ProcessPageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            //CrawledPage crawledPage = e.CrawledPage;

            //if (crawledPage.WebException != null || crawledPage.HttpWebResponse.StatusCode != HttpStatusCode.OK)
            //    _logger.WarnFormat("Crawl of page failed [{0}]", crawledPage.Uri.AbsoluteUri);
            //else
            //    _logger.InfoFormat("Crawl of page succeeded [{0}]", crawledPage.Uri.AbsoluteUri);

            //if (string.IsNullOrEmpty(crawledPage.RawContent))
            //    _logger.WarnFormat("Page had no content [{0}]", crawledPage.Uri.AbsoluteUri);
        }

        static void crawler_PageLinksCrawlDisallowed(object sender, PageLinksCrawlDisallowedArgs e)
        {
            //CrawledPage crawledPage = e.CrawledPage;
            //_logger.WarnFormat("Did not crawl the links on page [{0}] [{1}]", crawledPage.Uri.AbsoluteUri, e.DisallowedReason);
        }

        static void crawler_PageCrawlDisallowed(object sender, PageCrawlDisallowedArgs e)
        {
            //PageToCrawl pageToCrawl = e.PageToCrawl;
            //_logger.WarnFormat("Did not crawl page [{0}] due to [{1}]", pageToCrawl.Uri.AbsoluteUri, e.DisallowedReason);
        }
    }
}
