using Abot.Core;
using Abot.Crawler;
using Abot.Poco;
using log4net.Config;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Abot.Tests.Integration
{
    [TestFixture]
    public class CrawlSiteSimulator
    {
        List<CrawledPage> crawledPages = new List<CrawledPage>();

        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            XmlConfigurator.Configure();
        }

        [SetUp]
        public void SetUp()
        {
            new PageRequester("someagentstring").MakeRequest(new Uri("http://localhost:1111/PageGenerator/ClearCounters"));
        }

        [Test]
        public void Crawl_VerifyCrawlResultIsAsExpected()
        {
            Uri rootUri = new Uri("http://localhost:1111/");

            WebCrawler crawler = new WebCrawler();
            crawler.PageCrawlCompleted += crawler_PageCrawlCompleted;

            CrawlResult result = crawler.Crawl(rootUri);

            Assert.AreEqual("", result.ErrorMessage);
            Assert.IsFalse(result.ErrorOccurred);
            Assert.AreSame(rootUri, result.RootUri);
            Assert.AreEqual(27, crawledPages.Where(c => c.HttpWebResponse.StatusCode == HttpStatusCode.OK).Count());
            Assert.AreEqual(40, crawledPages.Where(c => c.HttpWebResponse.StatusCode != HttpStatusCode.OK).Count());
            Assert.IsTrue(result.Elapsed.TotalSeconds < 10, string.Format("Elapsed Time to crawl {0}, over 10 second threshold", result.Elapsed.TotalSeconds));
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
