using Abot.Crawler;
using Abot.Poco;
using log4net.Config;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;

namespace Abot.Tests.Integration
{
    [TestFixture]
    public class CrawlSiteSimulator
    {
        List<CrawledPage> nonBrokenPages = new List<CrawledPage>();
        List<CrawledPage> brokenPages = new List<CrawledPage>();

        [Test]
        public void Crawl_VerifyCrawlResultIsAsExpected()
        {
            XmlConfigurator.Configure();

            Uri rootUri = new Uri("http://localhost:1111/");

            WebCrawler crawler = new WebCrawler();
            crawler.PageCrawlCompleted += crawler_PageCrawlCompleted;

            CrawlResult result = crawler.Crawl(rootUri);

            Assert.AreEqual("", result.ErrorMessage);
            Assert.IsFalse(result.ErrorOccurred);
            Assert.AreSame(rootUri, result.RootUri);
            Assert.IsTrue(result.Elapsed.TotalSeconds < 30);
        }

        void crawler_PageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            if (e.CrawledPage.WebException != null || 
                e.CrawledPage.HttpWebResponse == null || 
                e.CrawledPage.HttpWebResponse.StatusCode != HttpStatusCode.OK)
            {
                lock (nonBrokenPages)
                {
                    brokenPages.Add(e.CrawledPage);
                }
            }
            else
            {
                lock (nonBrokenPages)
                {
                    nonBrokenPages.Add(e.CrawledPage);
                }
            }
        }
    }
}
