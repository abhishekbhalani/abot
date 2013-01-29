using Abot.Core;
using Abot.Crawler;
using Abot.Poco;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Abot.Tests.Unit.Crawler
{
    [TestFixture]
    public class PoliteWebCrawlerTest
    {
        PoliteWebCrawler _unitUnderTest;
        Mock<IPageRequester> _fakeHttpRequester;
        Mock<IHyperLinkParser> _fakeHyperLinkParser;
        Mock<ICrawlDecisionMaker> _fakeCrawlDecisionMaker;
        Mock<IDomainRateLimiter> _fakeDomainRateLimiter;
        FifoScheduler _dummyScheduler;
        ThreadManager _dummyThreadManager;
        CrawlConfiguration _dummyConfiguration;
        Uri _rootUri;

        [SetUp]
        public void SetUp()
        {
            _fakeHyperLinkParser = new Mock<IHyperLinkParser>();
            _fakeHttpRequester = new Mock<IPageRequester>();
            _fakeCrawlDecisionMaker = new Mock<ICrawlDecisionMaker>();
            _fakeDomainRateLimiter = new Mock<IDomainRateLimiter>();

            _dummyScheduler = new FifoScheduler();
            _dummyThreadManager = new ThreadManager(1);
            _dummyConfiguration = new CrawlConfiguration();
            _dummyConfiguration.ConfigurationExtensions.Add("somekey", "someval");

            _rootUri = new Uri("http://a.com/");
        }

        [Test]
        public void Constructor_Empty()
        {
            Assert.IsNotNull(new PoliteWebCrawler());
        }

        [Test]
        public void Constructor_Empty2()
        {
            new PoliteWebCrawler(null, null, null, null, null, null, null);
        }

        [Test]
        public void Crawl_MinCrawlDelayDelayZero_DomainRateLimiterNotCalled()
        {
            Uri uri1 = new Uri(_rootUri.AbsoluteUri + "a.html");
            Uri uri2 = new Uri(_rootUri.AbsoluteUri + "b.html");

            CrawledPage homePage = new CrawledPage(_rootUri) { RawContent = "content here" };
            CrawledPage page1 = new CrawledPage(uri1);
            CrawledPage page2 = new CrawledPage(uri2);

            List<Uri> links = new List<Uri> { uri1, uri2 };
            
            _fakeHttpRequester.Setup(f => f.MakeRequest(_rootUri, It.IsAny<Func<CrawledPage, CrawlDecision>>())).Returns(homePage);
            _fakeHttpRequester.Setup(f => f.MakeRequest(uri1, It.IsAny<Func<CrawledPage, CrawlDecision>>())).Returns(page1);
            _fakeHttpRequester.Setup(f => f.MakeRequest(uri2, It.IsAny<Func<CrawledPage, CrawlDecision>>())).Returns(page2);
            _fakeHyperLinkParser.Setup(f => f.GetLinks(_rootUri, It.IsAny<string>())).Returns(links);
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = true });
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPageLinks(It.IsAny<CrawledPage>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = true });

            _unitUnderTest = new PoliteWebCrawler(_dummyConfiguration, _fakeCrawlDecisionMaker.Object, _dummyThreadManager, _dummyScheduler, _fakeHttpRequester.Object, _fakeHyperLinkParser.Object, _fakeDomainRateLimiter.Object);

            _unitUnderTest.Crawl(_rootUri);

            _fakeDomainRateLimiter.Verify(f => f.RateLimit(It.IsAny<Uri>()), Times.Never());
        }

        [Test]
        public void Crawl_MinCrawlDelayGreaterThanZero_CallsDomainRateLimiter()
        {
            Uri uri1 = new Uri(_rootUri.AbsoluteUri + "a.html");
            Uri uri2 = new Uri(_rootUri.AbsoluteUri + "b.html");

            CrawledPage homePage = new CrawledPage(_rootUri) { RawContent = "content here" };
            CrawledPage page1 = new CrawledPage(uri1);
            CrawledPage page2 = new CrawledPage(uri2);

            List<Uri> links = new List<Uri> { uri1, uri2 };

            _fakeHttpRequester.Setup(f => f.MakeRequest(_rootUri, It.IsAny<Func<CrawledPage, CrawlDecision>>())).Returns(homePage);
            _fakeHttpRequester.Setup(f => f.MakeRequest(uri1, It.IsAny<Func<CrawledPage, CrawlDecision>>())).Returns(page1);
            _fakeHttpRequester.Setup(f => f.MakeRequest(uri2, It.IsAny<Func<CrawledPage, CrawlDecision>>())).Returns(page2);
            _fakeHyperLinkParser.Setup(f => f.GetLinks(_rootUri, It.IsAny<string>())).Returns(links);
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = true });
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPageLinks(It.IsAny<CrawledPage>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = true });

            _dummyConfiguration.MinCrawlDelayPerDomainMilliSeconds = 1;//BY HAVING A CRAWL DELAY ABOVE ZERO WE EXPECT THE IDOMAINRATELIMITER TO BE CALLED
            _unitUnderTest = new PoliteWebCrawler(_dummyConfiguration, _fakeCrawlDecisionMaker.Object, _dummyThreadManager, _dummyScheduler, _fakeHttpRequester.Object, _fakeHyperLinkParser.Object, _fakeDomainRateLimiter.Object);

            _unitUnderTest.Crawl(_rootUri);

            _fakeDomainRateLimiter.Verify(f => f.RateLimit(It.IsAny<Uri>()), Times.Exactly(3));//BY HAVING A CRAWL DELAY ABOVE ZERO WE EXPECT THE IDOMAINRATELIMITER TO BE CALLED
        }
    }
}
