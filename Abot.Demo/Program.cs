
using Abot.Crawler;
using Abot.Poco;
using log4net.Config;
using System;
using System.Net;

namespace Abot.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlConfigurator.Configure();

            //Initialize the crawler
            WebCrawler crawler = new WebCrawler();

            //Subscribe to page events
            crawler.PageCrawlStarting += new EventHandler<PageCrawlStartingArgs>(ProcessPageCrawlStarting);
            crawler.PageCrawlCompleted += new EventHandler<PageCrawlCompletedArgs>(ProcessPageCrawlCompleted);

            //Start the crawl
            CrawlResult result = crawler.Crawl(new Uri("http://wvtesting2.com/"));

            //Print some result data
            if (result.ErrorOccurred)
                Console.WriteLine("Crawl of {0} completed with error: {1}", result.RootUri.AbsoluteUri, result.ErrorMessage);
            else
                Console.WriteLine("Crawl of {0} completed without error.", result.RootUri.AbsoluteUri);

            Console.WriteLine("Completed in {0}", result.Elapsed);
        }

        //Event handler for when the Async PageCrawlStarting event fires
        private static void ProcessPageCrawlStarting(object sender, PageCrawlStartingArgs e)
        {
            PageToCrawl pageToCrawl = e.PageToCrawl;
            Console.WriteLine("About to crawl link {0} which was found on page {1}", pageToCrawl.Uri.AbsoluteUri, pageToCrawl.ParentUri.AbsoluteUri);
        }

        //Event handler for when the Async PageCrawlCompleted event fires
        private static void ProcessPageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            CrawledPage crawledPage = e.CrawledPage;

            if (crawledPage.WebException != null || crawledPage.HttpWebResponse.StatusCode != HttpStatusCode.OK)
                Console.WriteLine("Crawl of page failed!!");
            else
                Console.WriteLine("Crawl of page succeeded!!");

            if (string.IsNullOrEmpty(crawledPage.RawContent))
                Console.WriteLine("Page returned no html content");
        }
    }
}
