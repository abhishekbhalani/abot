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
            Assert.IsTrue(_unitUnderTest.ShouldCrawl(new PageToCrawl(new Uri("http://a.com/"))));
        }

        [Test]
        public void ShouldCrawl_Duplicate_ReturnsFalse()
        {
            _unitUnderTest.ShouldCrawl(new PageToCrawl(new Uri("http://a.com/")));
            Assert.IsFalse(_unitUnderTest.ShouldCrawl(new PageToCrawl(new Uri("http://a.com/"))));
        }


        [Test]
        public void ShouldCrawlLinks_NullHtmlContent_ReturnsFalse()
        {
            Assert.IsFalse(_unitUnderTest.ShouldCrawlLinks(new CrawledPage(new Uri("http://a.com/")) { RawContent = null }));
        }

        [Test]
        public void ShouldCrawlLinks_WhitespaceHtmlContent_ReturnsFalse()
        {
            Assert.IsFalse(_unitUnderTest.ShouldCrawlLinks(new CrawledPage(new Uri("http://a.com/")) { RawContent = "     " }));
        }

        [Test]
        public void ShouldCrawlLinks_EmptyHtmlContent_ReturnsFalse()
        {
            Assert.IsFalse(_unitUnderTest.ShouldCrawlLinks(new CrawledPage(new Uri("http://a.com/")) { RawContent = "" }));
        }

        [Test]
        public void ShouldCrawlLinks_InternalLink_ReturnsTrue()
        {
            bool shouldCrawlLinks = _unitUnderTest.ShouldCrawlLinks(
                new CrawledPage(new Uri("http://a.com/a.html")) { 
                    RawContent = "aaaa", 
                    RootUri = new Uri("http://a.com/ ")
                });
            Assert.AreEqual(true, shouldCrawlLinks);
        }

        [Test]
        public void ShouldCrawlLinks_ExternalLink_ReturnsFalse()
        {
            bool shouldCrawlLinks = _unitUnderTest.ShouldCrawlLinks(
                new CrawledPage(new Uri("http://b.com/a.html"))
                {
                    RawContent = "aaaa",
                    RootUri = new Uri("http://a.com/ ")
                });
            Assert.AreEqual(false, shouldCrawlLinks);
        }
    }
}
