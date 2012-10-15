using Abot.Core;
using Abot.Poco;
using NUnit.Framework;
using System;

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
        public void ShouldCrawl_NonDuplicate_ReturnsTrue()
        {
            CrawlDecision result = _unitUnderTest.ShouldCrawl(new PageToCrawl(new Uri("http://a.com/")));
            Assert.IsTrue(result.Should);
            Assert.AreEqual("", result.Reason);
        }

        [Test]
        public void ShouldCrawl_Duplicate_ReturnsFalse()
        {
            _unitUnderTest.ShouldCrawl(new PageToCrawl(new Uri("http://a.com/")));

            CrawlDecision result = _unitUnderTest.ShouldCrawl(new PageToCrawl(new Uri("http://a.com/")));
            Assert.IsFalse(result.Should);
            Assert.AreEqual("Link already crawled", result.Reason);
        }


        [Test]
        public void ShouldCrawlLinks_NullHtmlContent_ReturnsFalse()
        {
            CrawlDecision result = _unitUnderTest.ShouldCrawlLinks(new CrawledPage(new Uri("http://a.com/")) { RawContent = null });
            Assert.IsFalse(result.Should);
            Assert.AreEqual("Page has no links", result.Reason);
        }

        [Test]
        public void ShouldCrawlLinks_WhitespaceHtmlContent_ReturnsFalse()
        {
            CrawlDecision result = _unitUnderTest.ShouldCrawlLinks(new CrawledPage(new Uri("http://a.com/")) { RawContent = "     " });
            Assert.IsFalse(result.Should);
            Assert.AreEqual("Page has no links", result.Reason);
        }

        [Test]
        public void ShouldCrawlLinks_EmptyHtmlContent_ReturnsFalse()
        {
            CrawlDecision result = _unitUnderTest.ShouldCrawlLinks(new CrawledPage(new Uri("http://a.com/")) { RawContent = "" });
            Assert.IsFalse(result.Should);
            Assert.AreEqual("Page has no links", result.Reason);            
        }

        [Test]
        public void ShouldCrawlLinks_InternalLink_ReturnsTrue()
        {
            CrawlDecision result = _unitUnderTest.ShouldCrawlLinks(
                new CrawledPage(new Uri("http://a.com/a.html")) { 
                    RawContent = "aaaa", 
                    RootUri = new Uri("http://a.com/ ")
                });
            Assert.AreEqual(true, result.Should);
            Assert.AreEqual("", result.Reason);
        }

        [Test]
        public void ShouldCrawlLinks_ExternalLink_ReturnsFalse()
        {
            CrawlDecision result = _unitUnderTest.ShouldCrawlLinks(
                new CrawledPage(new Uri("http://b.com/a.html"))
                {
                    RawContent = "aaaa",
                    RootUri = new Uri("http://a.com/ ")
                });
            Assert.AreEqual(false, result.Should);
            Assert.AreEqual("Link is external", result.Reason);
        }
    }
}
