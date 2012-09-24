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
        public void ShouldCrawl_AnyParam_ReturnsTrue()
        {
            Assert.IsTrue(_unitUnderTest.ShouldCrawl(null));
        }

        [Test]
        public void ShouldCrawlLinks_HasHtmlContent_ReturnsTrue()
        {
            Assert.IsTrue(_unitUnderTest.ShouldCrawlLinks(new CrawledPage(new Uri("http://a.com/")){ RawContent = "aaaa" }));
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
    }
}
