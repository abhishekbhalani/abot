using Abot.Core;
using Abot.Poco;
using NUnit.Framework;
using System;
using System.Net;

namespace Abot.Tests.Core
{
    [TestFixture]
    public class CrawlDecisionMakerTest
    {
        CrawlDecisionMaker _unitUnderTest;

        [SetUp]
        public void SetUp()
        {
            _unitUnderTest = new CrawlDecisionMaker();
        }

        [Test]
        public void ShouldCrawlPage_NonDuplicate_ReturnsTrue()
        {
            CrawlDecision result = _unitUnderTest.ShouldCrawlPage(new PageToCrawl(new Uri("http://a.com/")));
            Assert.IsTrue(result.Allow);
            Assert.AreEqual("", result.Reason);
        }

        [Test]
        public void ShouldCrawlPage_Duplicate_ReturnsFalse()
        {
            _unitUnderTest.ShouldCrawlPage(new PageToCrawl(new Uri("http://a.com/")));

            CrawlDecision result = _unitUnderTest.ShouldCrawlPage(new PageToCrawl(new Uri("http://a.com/")));
            Assert.IsFalse(result.Allow);
            Assert.AreEqual("Link already crawled", result.Reason);
        }


        [Test]
        public void ShouldCrawlPageLinks_NullCrawledPage_ReturnsFalse()
        {
            CrawlDecision result = _unitUnderTest.ShouldCrawlPageLinks(null);
            Assert.IsFalse(result.Allow);
            Assert.AreEqual("Null crawled page", result.Reason);
        }

        [Test]
        public void ShouldCrawlPageLinks_NullHtmlContent_ReturnsFalse()
        {
            CrawlDecision result = _unitUnderTest.ShouldCrawlPageLinks(new CrawledPage(new Uri("http://a.com/")) { RawContent = null });
            Assert.IsFalse(result.Allow);
            Assert.AreEqual("Page has no links", result.Reason);
        }

        [Test]
        public void ShouldCrawlPageLinks_WhitespaceHtmlContent_ReturnsFalse()
        {
            CrawlDecision result = _unitUnderTest.ShouldCrawlPageLinks(new CrawledPage(new Uri("http://a.com/")) { RawContent = "     " });
            Assert.IsFalse(result.Allow);
            Assert.AreEqual("Page has no links", result.Reason);
        }

        [Test]
        public void ShouldCrawlPageLinks_EmptyHtmlContent_ReturnsFalse()
        {
            CrawlDecision result = _unitUnderTest.ShouldCrawlPageLinks(new CrawledPage(new Uri("http://a.com/")) { RawContent = "" });
            Assert.IsFalse(result.Allow);
            Assert.AreEqual("Page has no links", result.Reason);            
        }

        [Test]
        public void ShouldCrawlPageLinks_InternalLink_ReturnsTrue()
        {
            CrawlDecision result = _unitUnderTest.ShouldCrawlPageLinks(
                new CrawledPage(new Uri("http://a.com/a.html")) { 
                    RawContent = "aaaa", 
                    RootUri = new Uri("http://a.com/ ")
                });
            Assert.AreEqual(true, result.Allow);
            Assert.AreEqual("", result.Reason);
        }

        [Test]
        public void ShouldCrawlPageLinks_ExternalLink_ReturnsFalse()
        {
            CrawlDecision result = _unitUnderTest.ShouldCrawlPageLinks(
                new CrawledPage(new Uri("http://b.com/a.html"))
                {
                    RawContent = "aaaa",
                    RootUri = new Uri("http://a.com/ ")
                });
            Assert.AreEqual(false, result.Allow);
            Assert.AreEqual("Link is external", result.Reason);
        }


        [Test]
        public void ShouldDownloadPageContent_DownloadablePage_ReturnsTrue()
        {
            Uri valid200StatusUri = new Uri("http://localhost:1111/");

            CrawlDecision result = _unitUnderTest.ShouldDownloadPageContent(new PageRequester("someuseragentstring").MakeRequest(valid200StatusUri));

            Assert.AreEqual(true, result.Allow);
            Assert.AreEqual("", result.Reason);
        }

        [Test]
        public void ShouldDownloadPageContent_NullCrawledPage_ReturnsFalse()
        {
            CrawlDecision result = _unitUnderTest.ShouldDownloadPageContent(null);
            Assert.AreEqual(false, result.Allow);
            Assert.AreEqual("Null crawled page", result.Reason);
        }

        [Test]
        public void ShouldDownloadPageContent_NullHttpWebResponse_ReturnsFalse()
        {
            CrawlDecision result = _unitUnderTest.ShouldDownloadPageContent(
                new CrawledPage(new Uri("http://b.com/a.html"))
                {
                    HttpWebResponse = null
                });
            Assert.AreEqual(false, result.Allow);
            Assert.AreEqual("Null HttpWebResponse", result.Reason);
        }

        [Test]
        public void ShouldDownloadPageContent_HttpStatusNon200_ReturnsFalse()
        {
            Uri non200Uri = new Uri("http://localhost:1111/HttpResponse/Status403");

            CrawlDecision result = _unitUnderTest.ShouldDownloadPageContent(new PageRequester("someuseragentstring").MakeRequest(non200Uri));

            Assert.AreEqual(false, result.Allow);
            Assert.AreEqual("HttpStatusCode is not 200", result.Reason);
        }

        [Test]
        public void ShouldDownloadPageContent_NonHtmlPage_ReturnsFalse()
        {
            Uri imageUrl = new Uri("http://localhost:1111/Content/themes/base/images/ui-bg_flat_0_aaaaaa_40x100.png");

            CrawlDecision result = _unitUnderTest.ShouldDownloadPageContent(new PageRequester("someuseragentstring").MakeRequest(imageUrl));

            Assert.AreEqual(false, result.Allow);
            Assert.AreEqual("Content type is not text/html", result.Reason);
        }
    }
}
