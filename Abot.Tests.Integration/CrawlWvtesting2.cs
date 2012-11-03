using Abot.Crawler;
using Abot.Poco;
using log4net;
using log4net.Config;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Abot.Tests.Integration
{
    [TestFixture]
    public class CrawlWvtesting2
    {
        List<CrawledPage> crawledPages = new List<CrawledPage>();
        ILog _logger = LogManager.GetLogger(typeof(CrawlWvtesting2).FullName);

        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            XmlConfigurator.Configure();
        }

        [Test]
        public void Crawl_VerifyCrawlResultIsAsExpected()
        {
            Uri rootUri = new Uri("http://wvtesting2.com/");

            WebCrawler crawler = new WebCrawler();
            crawler.PageCrawlCompleted += crawler_PageCrawlCompleted;

            CrawlResult result = crawler.Crawl(rootUri);

            Assert.AreEqual("", result.ErrorMessage);
            Assert.IsFalse(result.ErrorOccurred);
            Assert.AreSame(rootUri, result.RootUri);
            //PrintCrawlResult(crawledPages);
            Assert.AreEqual(297, crawledPages.Where(c => c.HttpWebResponse != null && c.HttpWebResponse.StatusCode == HttpStatusCode.OK).Count());
            Assert.AreEqual(440, crawledPages.Where(c => c.HttpWebResponse == null || c.HttpWebResponse.StatusCode != HttpStatusCode.OK).Count());
            Assert.IsTrue(result.Elapsed.TotalSeconds < 20, string.Format("Elapsed Time to crawl {0}, over 30 second threshold", result.Elapsed.TotalSeconds));
        }

        private void PrintCrawlResult(List<CrawledPage> crawledPages)
        {
            var workingPages = crawledPages.Where(c => c.HttpWebResponse != null && c.HttpWebResponse.StatusCode == HttpStatusCode.OK).OrderBy(o => o.Uri.AbsoluteUri);
            var brokenPages = crawledPages.Where(c => c.HttpWebResponse == null || c.HttpWebResponse.StatusCode != HttpStatusCode.OK).OrderBy(o => o.Uri.AbsoluteUri);

            _logger.DebugFormat("Total Crawled Pages: [{0}]", crawledPages.Count());
            PrintCollection("Working Pages", workingPages);
            PrintCollection("Broken Pages", brokenPages);
        }

        private void PrintCollection(string p, IOrderedEnumerable<CrawledPage> workingPages)
        {
            _logger.DebugFormat("Working Pages: [{0}]", workingPages.Count());
            foreach (CrawledPage page in workingPages)
            {
                _logger.DebugFormat("[{0}] [{1}]", page.Uri.AbsoluteUri, page.HttpWebResponse != null ? ((int)page.HttpWebResponse.StatusCode).ToString() : "NA");
            }
        }

        

        void crawler_PageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            lock (crawledPages)
            {
                crawledPages.Add(e.CrawledPage);
            }
        }

    }
}
