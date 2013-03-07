using Abot.Core;
using Abot.Crawler;
using Abot.Poco;
using HtmlAgilityPack;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Abot.Tests.Unit.Crawler
{
    [TestFixture]
    public class WebCrawlerTest
    {
        WebCrawler _unitUnderTest;
        Mock<IPageRequester> _fakeHttpRequester;
        Mock<IHyperLinkParser> _fakeHyperLinkParser;
        Mock<ICrawlDecisionMaker> _fakeCrawlDecisionMaker;
        FifoScheduler _dummyScheduler;
        ManualThreadManager _dummyThreadManager;
        CrawlConfiguration _dummyConfiguration;
        Uri _rootUri;

        [SetUp]
        public void SetUp()
        {
            _fakeHyperLinkParser = new Mock<IHyperLinkParser>();
            _fakeHttpRequester = new Mock<IPageRequester>();
            _fakeCrawlDecisionMaker = new Mock<ICrawlDecisionMaker>();

            _dummyScheduler = new FifoScheduler();
            _dummyThreadManager = new ManualThreadManager(1);
            _dummyConfiguration = new CrawlConfiguration();
            _dummyConfiguration.ConfigurationExtensions.Add("somekey", "someval");

            _unitUnderTest = new WebCrawler(_dummyThreadManager, _dummyScheduler, _fakeHttpRequester.Object, _fakeHyperLinkParser.Object, _fakeCrawlDecisionMaker.Object, _dummyConfiguration);
            _unitUnderTest.CrawlBag.SomeVal = "SomeVal";
            _unitUnderTest.CrawlBag.SomeList = new List<string>() { "a", "b" };
            _rootUri = new Uri("http://a.com/");
        }

        [Test]
        public void Constructor_Empty()
        {
            Assert.IsNotNull(new WebCrawler());
        }

        [Test]
        public void Constructor_WithConfiguration()
        {
            Assert.IsNotNull(new WebCrawler(null, null, null, null, null, new CrawlConfiguration()));
        }

        [Test]
        public void Constructor_WithDecisionMaker()
        {
            Assert.IsNotNull(new WebCrawler(null, null, null, null, new CrawlDecisionMaker(), null));
        }

        [Test]
        public void Constructor_WithDecisionMakerAndConfiguration()
        {
            Assert.IsNotNull(new WebCrawler(null, null, null, null, new CrawlDecisionMaker(), new CrawlConfiguration()));
        }


        [Test]
        public void Crawl_CallsDependencies()
        {
            Uri uri1 = new Uri(_rootUri.AbsoluteUri + "a.html");
            Uri uri2 = new Uri(_rootUri.AbsoluteUri + "b.html");

            CrawledPage homePage = new CrawledPage(_rootUri) { RawContent = "content here" };
            CrawledPage page1 = new CrawledPage(uri1);
            CrawledPage page2 = new CrawledPage(uri2);

            List<Uri> links = new List<Uri>{uri1, uri2};

            _fakeHttpRequester.Setup(f => f.MakeRequest(_rootUri, It.IsAny<Func<CrawledPage, CrawlDecision>>())).Returns(homePage);
            _fakeHttpRequester.Setup(f => f.MakeRequest(uri1, It.IsAny<Func<CrawledPage, CrawlDecision>>())).Returns(page1);
            _fakeHttpRequester.Setup(f => f.MakeRequest(uri2, It.IsAny<Func<CrawledPage, CrawlDecision>>())).Returns(page2);
            _fakeHyperLinkParser.Setup(f => f.GetLinks(It.Is<CrawledPage>(p => p.Uri == homePage.Uri))).Returns(links);
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision{Allow = true});
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPageLinks(It.IsAny<CrawledPage>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = true });
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPageLinks(It.Is<CrawledPage>(p => p.Uri == homePage.Uri), It.IsAny<CrawlContext>())).Returns(new CrawlDecision{ Allow = true });

            _unitUnderTest.Crawl(_rootUri);

            _fakeHttpRequester.Verify(f => f.MakeRequest(_rootUri, It.IsAny<Func<CrawledPage, CrawlDecision>>()), Times.Once());
            _fakeHttpRequester.Verify(f => f.MakeRequest(uri1, It.IsAny<Func<CrawledPage, CrawlDecision>>()), Times.Once());
            _fakeHttpRequester.Verify(f => f.MakeRequest(uri2, It.IsAny<Func<CrawledPage, CrawlDecision>>()), Times.Once());
            _fakeHyperLinkParser.Verify(f => f.GetLinks(It.Is<CrawledPage>(p => p.Uri == homePage.Uri)), Times.Exactly(1));
            _fakeHyperLinkParser.Verify(f => f.GetLinks(It.Is<CrawledPage>(p => p.Uri == uri1)), Times.Exactly(1));
            _fakeHyperLinkParser.Verify(f => f.GetLinks(It.Is<CrawledPage>(p => p.Uri == uri2)), Times.Exactly(1));
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>()), Times.Exactly(3));
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPageLinks(It.IsAny<CrawledPage>(), It.IsAny<CrawlContext>()), Times.Exactly(3));
        }


        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Crawl_NullUri()
        {
            _unitUnderTest.Crawl(null);
        }

        [Test]
        public void Crawl_ExceptionThrownByScheduler_SetsCrawlResultError()
        {
            Mock<IScheduler> fakeScheduler = new Mock<IScheduler>();
            Exception ex = new Exception("oh no");
            fakeScheduler.Setup(f => f.Count).Throws(ex);
            _unitUnderTest = new WebCrawler(_dummyThreadManager, fakeScheduler.Object, _fakeHttpRequester.Object, _fakeHyperLinkParser.Object, _fakeCrawlDecisionMaker.Object, _dummyConfiguration);

            CrawlResult result = _unitUnderTest.Crawl(_rootUri);

            fakeScheduler.Verify(f => f.Count, Times.Exactly(1));
            Assert.IsTrue(result.ErrorOccurred);
            Assert.AreSame(ex, result.ErrorException);
        }

        [Test]
        public void Crawl_ExceptionThrownByCrawlDecisionMaker_SetsCrawlResultError()
        {
            Exception ex = new Exception("oh no");
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>())).Throws(ex);

            CrawlResult result = _unitUnderTest.Crawl(_rootUri);

            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>()), Times.Exactly(1));
            Assert.IsTrue(result.ErrorOccurred);
            Assert.AreSame(ex, result.ErrorException);
        }

        #region Synchronous Event Tests

        [Test]
        public void Crawl_CrawlDecisionMakerMethodsReturnTrue_PageCrawlStartingAndCompletedEventsFires()
        {
            _fakeHttpRequester.Setup(f => f.MakeRequest(It.IsAny<Uri>(), It.IsAny<Func<CrawledPage, CrawlDecision>>())).Returns(new CrawledPage(_rootUri));
            _fakeHyperLinkParser.Setup(f => f.GetLinks(It.IsAny<Uri>(), It.IsAny<string>())).Returns(new List<Uri>());
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = true });
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPageLinks(It.IsAny<CrawledPage>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = true });

            int _pageCrawlStartingCount = 0;
            int _pageCrawlCompletedCount = 0;
            int _pageCrawlDisallowedCount = 0;
            int _pageLinksCrawlDisallowedCount = 0;
            _unitUnderTest.PageCrawlCompleted += (s, e) => ++_pageCrawlCompletedCount;
            _unitUnderTest.PageCrawlStarting += (s, e) => ++_pageCrawlStartingCount;
            _unitUnderTest.PageCrawlDisallowed += (s, e) => ++_pageCrawlDisallowedCount;
            _unitUnderTest.PageLinksCrawlDisallowed += (s, e) => ++_pageLinksCrawlDisallowedCount;

            _unitUnderTest.Crawl(_rootUri);

            _fakeHttpRequester.Verify(f => f.MakeRequest(It.IsAny<Uri>(), It.IsAny<Func<CrawledPage, CrawlDecision>>()), Times.Once());
            _fakeHyperLinkParser.Verify(f => f.GetLinks(It.IsAny<CrawledPage>()), Times.Once());
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>()), Times.Once());
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPageLinks(It.IsAny<CrawledPage>(), It.IsAny<CrawlContext>()), Times.Once());

            Assert.AreEqual(1, _pageCrawlStartingCount);
            Assert.AreEqual(1, _pageCrawlCompletedCount);
            Assert.AreEqual(0, _pageCrawlDisallowedCount);
            Assert.AreEqual(0, _pageLinksCrawlDisallowedCount);
        }

        [Test]
        public void Crawl_CrawlDecisionMakerShouldCrawlLinksMethodReturnsFalse_PageLinksCrawlDisallowedEventFires()
        {
            _fakeHttpRequester.Setup(f => f.MakeRequest(It.IsAny<Uri>(), It.IsAny<Func<CrawledPage, CrawlDecision>>())).Returns(new CrawledPage(_rootUri));
            _fakeHyperLinkParser.Setup(f => f.GetLinks(It.IsAny<Uri>(), It.IsAny<string>())).Returns(new List<Uri>());
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = true });
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPageLinks(It.IsAny<CrawledPage>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = false, Reason = "aaa" });

            int _pageCrawlStartingCount = 0;
            int _pageCrawlCompletedCount = 0;
            int _pageCrawlDisallowedCount = 0;
            int _pageLinksCrawlDisallowedCount = 0;
            _unitUnderTest.PageCrawlCompleted += (s, e) => ++_pageCrawlCompletedCount;
            _unitUnderTest.PageCrawlStarting += (s, e) => ++_pageCrawlStartingCount;
            _unitUnderTest.PageCrawlDisallowed += (s, e) => ++_pageCrawlDisallowedCount;
            _unitUnderTest.PageLinksCrawlDisallowed += (s, e) => ++_pageLinksCrawlDisallowedCount;

            _unitUnderTest.Crawl(_rootUri);

            _fakeHttpRequester.Verify(f => f.MakeRequest(It.IsAny<Uri>(), It.IsAny<Func<CrawledPage, CrawlDecision>>()), Times.Once());
            _fakeHyperLinkParser.Verify(f => f.GetLinks(It.IsAny<Uri>(), It.IsAny<string>()), Times.Never());
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>()), Times.Once());
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPageLinks(It.IsAny<CrawledPage>(), It.IsAny<CrawlContext>()), Times.Once());

            Assert.AreEqual(1, _pageCrawlStartingCount);
            Assert.AreEqual(1, _pageCrawlCompletedCount);
            Assert.AreEqual(0, _pageCrawlDisallowedCount);
            Assert.AreEqual(1, _pageLinksCrawlDisallowedCount);
        }

        [Test]
        public void Crawl_CrawlDecisionMakerShouldCrawlMethodReturnsFalse_PageCrawlDisallowedEventFires()
        {
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = false, Reason = "aaa" });

            int _pageCrawlStartingCount = 0;
            int _pageCrawlCompletedCount = 0;
            int _pageCrawlDisallowedCount = 0;
            int _pageLinksCrawlDisallowedCount = 0;
            _unitUnderTest.PageCrawlCompleted += (s, e) => ++_pageCrawlCompletedCount;
            _unitUnderTest.PageCrawlStarting += (s, e) => ++_pageCrawlStartingCount;
            _unitUnderTest.PageCrawlDisallowed += (s, e) => ++_pageCrawlDisallowedCount;
            _unitUnderTest.PageLinksCrawlDisallowed += (s, e) => ++_pageLinksCrawlDisallowedCount;

            _unitUnderTest.Crawl(_rootUri);

            _fakeHttpRequester.Verify(f => f.MakeRequest(It.IsAny<Uri>()), Times.Never());
            _fakeHyperLinkParser.Verify(f => f.GetLinks(It.IsAny<Uri>(), It.IsAny<string>()), Times.Never());
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>()), Times.Once());
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPageLinks(It.IsAny<CrawledPage>(), It.IsAny<CrawlContext>()), Times.Never());

            Assert.AreEqual(0, _pageCrawlStartingCount);
            Assert.AreEqual(0, _pageCrawlCompletedCount);
            Assert.AreEqual(1, _pageCrawlDisallowedCount);
            Assert.AreEqual(0, _pageLinksCrawlDisallowedCount);
        }


        [Test]
        public void Crawl_PageCrawlStartingAndCompletedEventSubscriberThrowsExceptions_DoesNotCrash()
        {
            _fakeHttpRequester.Setup(f => f.MakeRequest(It.IsAny<Uri>(), It.IsAny<Func<CrawledPage, CrawlDecision>>())).Returns(new CrawledPage(_rootUri));
            _fakeHyperLinkParser.Setup(f => f.GetLinks(It.IsAny<Uri>(), It.IsAny<string>())).Returns(new List<Uri>());
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = true });
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPageLinks(It.IsAny<CrawledPage>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = true });

            FifoScheduler _dummyScheduler = new FifoScheduler();
            ManualThreadManager _dummyThreadManager = new ManualThreadManager(1);
            _unitUnderTest = new WebCrawler(_dummyThreadManager, _dummyScheduler, _fakeHttpRequester.Object, _fakeHyperLinkParser.Object, _fakeCrawlDecisionMaker.Object, new CrawlConfiguration());

            int _pageCrawlStartingCount = 0;
            int _pageCrawlCompletedCount = 0;
            int _pageCrawlDisallowedCount = 0;
            int _pageLinksCrawlDisallowedCount = 0;
            _unitUnderTest.PageCrawlCompleted += (s, e) => ++_pageCrawlCompletedCount;
            _unitUnderTest.PageCrawlStarting += (s, e) => ++_pageCrawlStartingCount;
            _unitUnderTest.PageCrawlStarting += new EventHandler<PageCrawlStartingArgs>(ThrowExceptionWhen_PageCrawlStarting);
            _unitUnderTest.PageCrawlCompleted += new EventHandler<PageCrawlCompletedArgs>(ThrowExceptionWhen_PageCrawlCompleted);
            _unitUnderTest.PageCrawlDisallowed += (s, e) => ++_pageCrawlDisallowedCount;
            _unitUnderTest.PageLinksCrawlDisallowed += (s, e) => ++_pageLinksCrawlDisallowedCount;

            _unitUnderTest.Crawl(_rootUri);

            _fakeHttpRequester.Verify(f => f.MakeRequest(It.IsAny<Uri>(), It.IsAny<Func<CrawledPage, CrawlDecision>>()), Times.Once());
            _fakeHyperLinkParser.Verify(f => f.GetLinks(It.IsAny<CrawledPage>()), Times.Once());
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>()), Times.Once());
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPageLinks(It.IsAny<CrawledPage>(), It.IsAny<CrawlContext>()), Times.Once());

            Assert.AreEqual(1, _pageCrawlStartingCount);
            Assert.AreEqual(1, _pageCrawlCompletedCount);
            Assert.AreEqual(0, _pageCrawlDisallowedCount);
            Assert.AreEqual(0, _pageLinksCrawlDisallowedCount);
        }

        [Test]
        public void Crawl_PageCrawlDisallowedSubscriberThrowsExceptions_DoesNotCrash()
        {
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = false, Reason = "aaa" });

            int _pageCrawlStartingCount = 0;
            int _pageCrawlCompletedCount = 0;
            int _pageCrawlDisallowedCount = 0;
            int _pageLinksCrawlDisallowedCount = 0;
            _unitUnderTest.PageCrawlCompleted += (s, e) => ++_pageCrawlCompletedCount;
            _unitUnderTest.PageCrawlStarting += (s, e) => ++_pageCrawlStartingCount;
            _unitUnderTest.PageCrawlDisallowed += (s, e) => ++_pageCrawlDisallowedCount;
            _unitUnderTest.PageLinksCrawlDisallowed += (s, e) => ++_pageLinksCrawlDisallowedCount;
            _unitUnderTest.PageCrawlDisallowed += new EventHandler<PageCrawlDisallowedArgs>(ThrowExceptionWhen_PageCrawlDisallowed);
            _unitUnderTest.PageLinksCrawlDisallowed += new EventHandler<PageLinksCrawlDisallowedArgs>(ThrowExceptionWhen_PageLinksCrawlDisallowed);

            _unitUnderTest.Crawl(_rootUri);

            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>()), Times.Once());

            Assert.AreEqual(0, _pageCrawlStartingCount);
            Assert.AreEqual(0, _pageCrawlCompletedCount);
            Assert.AreEqual(1, _pageCrawlDisallowedCount);
            Assert.AreEqual(0, _pageLinksCrawlDisallowedCount);
        }

        [Test]
        public void Crawl_PageLinksCrawlDisallowedSubscriberThrowsExceptions_DoesNotCrash()
        {
            _fakeHttpRequester.Setup(f => f.MakeRequest(It.IsAny<Uri>(), It.IsAny<Func<CrawledPage, CrawlDecision>>())).Returns(new CrawledPage(_rootUri));
            _fakeHyperLinkParser.Setup(f => f.GetLinks(It.IsAny<Uri>(), It.IsAny<string>())).Returns(new List<Uri>());
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = true });
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPageLinks(It.IsAny<CrawledPage>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = false, Reason = "aaa" });

            FifoScheduler _dummyScheduler = new FifoScheduler();
            ManualThreadManager _dummyThreadManager = new ManualThreadManager(1);
            _unitUnderTest = new WebCrawler(_dummyThreadManager, _dummyScheduler, _fakeHttpRequester.Object, _fakeHyperLinkParser.Object, _fakeCrawlDecisionMaker.Object, new CrawlConfiguration());

            int _pageCrawlStartingCount = 0;
            int _pageCrawlCompletedCount = 0;
            int _pageCrawlDisallowedCount = 0;
            int _pageLinksCrawlDisallowedCount = 0;
            _unitUnderTest.PageCrawlCompleted += (s, e) => ++_pageCrawlCompletedCount;
            _unitUnderTest.PageCrawlStarting += (s, e) => ++_pageCrawlStartingCount;
            _unitUnderTest.PageCrawlDisallowed += (s, e) => ++_pageCrawlDisallowedCount;
            _unitUnderTest.PageLinksCrawlDisallowed += (s, e) => ++_pageLinksCrawlDisallowedCount;
            _unitUnderTest.PageLinksCrawlDisallowed += new EventHandler<PageLinksCrawlDisallowedArgs>(ThrowExceptionWhen_PageLinksCrawlDisallowed);

            _unitUnderTest.Crawl(_rootUri);

            _fakeHttpRequester.Verify(f => f.MakeRequest(It.IsAny<Uri>(), It.IsAny<Func<CrawledPage, CrawlDecision>>()), Times.Once());
            _fakeHyperLinkParser.Verify(f => f.GetLinks(It.IsAny<Uri>(), It.IsAny<string>()), Times.Never());
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>()), Times.Once());
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPageLinks(It.IsAny<CrawledPage>(), It.IsAny<CrawlContext>()), Times.Once());

            Assert.AreEqual(1, _pageCrawlStartingCount);
            Assert.AreEqual(1, _pageCrawlCompletedCount);
            Assert.AreEqual(0, _pageCrawlDisallowedCount);
            Assert.AreEqual(1, _pageLinksCrawlDisallowedCount);
        }


        [Test]
        public void Crawl_PageCrawlStartingEvent_IsSynchronous()
        {
            int elapsedTimeForLongJob = 1000;

            _fakeHttpRequester.Setup(f => f.MakeRequest(It.IsAny<Uri>(), It.IsAny<Func<CrawledPage, CrawlDecision>>())).Returns(new CrawledPage(_rootUri));
            _fakeHyperLinkParser.Setup(f => f.GetLinks(It.IsAny<Uri>(), It.IsAny<string>())).Returns(new List<Uri>());
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = true });
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPageLinks(It.IsAny<CrawledPage>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = false, Reason = "aaaa" });

            _unitUnderTest.PageCrawlStarting += new EventHandler<PageCrawlStartingArgs>((sender, args) => System.Threading.Thread.Sleep(elapsedTimeForLongJob));

            Stopwatch timer = Stopwatch.StartNew();
            _unitUnderTest.Crawl(_rootUri);
            timer.Stop();

            Assert.IsTrue(timer.ElapsedMilliseconds > 800);
        }

        [Test]
        public void Crawl_PageCrawlCompletedEvent_IsSynchronous()
        {
            int elapsedTimeForLongJob = 1000;

            _fakeHttpRequester.Setup(f => f.MakeRequest(It.IsAny<Uri>(), It.IsAny<Func<CrawledPage, CrawlDecision>>())).Returns(new CrawledPage(_rootUri));
            _fakeHyperLinkParser.Setup(f => f.GetLinks(It.IsAny<Uri>(), It.IsAny<string>())).Returns(new List<Uri>());
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = true });
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPageLinks(It.IsAny<CrawledPage>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = false, Reason = "aaaa" });

            _unitUnderTest.PageCrawlCompleted += new EventHandler<PageCrawlCompletedArgs>((sender, args) => System.Threading.Thread.Sleep(elapsedTimeForLongJob));

            Stopwatch timer = Stopwatch.StartNew();
            _unitUnderTest.Crawl(_rootUri);
            timer.Stop();

            Assert.IsTrue(timer.ElapsedMilliseconds > 800);
        }

        [Test]
        public void Crawl_PageCrawlDisallowedEvent_IsSynchronous()
        {
            int elapsedTimeForLongJob = 1000;

            _fakeHttpRequester.Setup(f => f.MakeRequest(It.IsAny<Uri>())).Returns(new CrawledPage(_rootUri));
            _fakeHyperLinkParser.Setup(f => f.GetLinks(It.IsAny<Uri>(), It.IsAny<string>())).Returns(new List<Uri>());
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = false, Reason = "aaa" });

            _unitUnderTest.PageCrawlDisallowed += new EventHandler<PageCrawlDisallowedArgs>((sender, args) => System.Threading.Thread.Sleep(elapsedTimeForLongJob));

            Stopwatch timer = Stopwatch.StartNew();
            _unitUnderTest.Crawl(_rootUri);
            timer.Stop();

            Assert.IsTrue(timer.ElapsedMilliseconds > 800);
        }

        [Test]
        public void Crawl_PageLinksCrawlDisallowedEvent_IsSynchronous()
        {
            int elapsedTimeForLongJob = 1000;

            _fakeHttpRequester.Setup(f => f.MakeRequest(It.IsAny<Uri>(), It.IsAny<Func<CrawledPage, CrawlDecision>>())).Returns(new CrawledPage(_rootUri));
            _fakeHyperLinkParser.Setup(f => f.GetLinks(It.IsAny<Uri>(), It.IsAny<string>())).Returns(new List<Uri>());
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = true });
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPageLinks(It.IsAny<CrawledPage>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = false, Reason = "aaa" });

            _unitUnderTest.PageLinksCrawlDisallowed += new EventHandler<PageLinksCrawlDisallowedArgs>((sender, args) => System.Threading.Thread.Sleep(elapsedTimeForLongJob));

            Stopwatch timer = Stopwatch.StartNew();
            _unitUnderTest.Crawl(_rootUri);
            timer.Stop();

            Assert.IsTrue(timer.ElapsedMilliseconds > 800);
        }

        #endregion

        #region Async Event Tests

        [Test]
        public void Crawl_CrawlDecisionMakerMethodsReturnTrue_PageCrawlStartingAndCompletedAsyncEventsFires()
        {
            _fakeHttpRequester.Setup(f => f.MakeRequest(It.IsAny<Uri>(), It.IsAny<Func<CrawledPage, CrawlDecision>>())).Returns(new CrawledPage(_rootUri));
            _fakeHyperLinkParser.Setup(f => f.GetLinks(It.IsAny<Uri>(), It.IsAny<string>())).Returns(new List<Uri>());
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = true });
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPageLinks(It.IsAny<CrawledPage>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = true });

            int _pageCrawlStartingCount = 0;
            int _pageCrawlCompletedCount = 0;
            int _pageCrawlDisallowedCount = 0;
            int _pageLinksCrawlDisallowedCount = 0;
            _unitUnderTest.PageCrawlCompletedAsync += (s, e) => ++_pageCrawlCompletedCount;
            _unitUnderTest.PageCrawlStartingAsync += (s, e) => ++_pageCrawlStartingCount;
            _unitUnderTest.PageCrawlDisallowedAsync += (s, e) => ++_pageCrawlDisallowedCount;
            _unitUnderTest.PageLinksCrawlDisallowedAsync += (s, e) => ++_pageLinksCrawlDisallowedCount;

            _unitUnderTest.Crawl(_rootUri);
            System.Threading.Thread.Sleep(100);//sleep since the events are async and may not complete before returning

            _fakeHttpRequester.Verify(f => f.MakeRequest(It.IsAny<Uri>(), It.IsAny<Func<CrawledPage, CrawlDecision>>()), Times.Once());
            _fakeHyperLinkParser.Verify(f => f.GetLinks(It.IsAny<CrawledPage>()), Times.Once());
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>()), Times.Once());
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPageLinks(It.IsAny<CrawledPage>(), It.IsAny<CrawlContext>()), Times.Once());

            Assert.AreEqual(1, _pageCrawlStartingCount);
            Assert.AreEqual(1, _pageCrawlCompletedCount);
            Assert.AreEqual(0, _pageCrawlDisallowedCount);
            Assert.AreEqual(0, _pageLinksCrawlDisallowedCount);
        }

        [Test]
        public void Crawl_CrawlDecisionMakerShouldCrawlLinksMethodReturnsFalse_PageLinksCrawlDisallowedAsyncEventFires()
        {
            _fakeHttpRequester.Setup(f => f.MakeRequest(It.IsAny<Uri>(), It.IsAny<Func<CrawledPage, CrawlDecision>>())).Returns(new CrawledPage(_rootUri));
            _fakeHyperLinkParser.Setup(f => f.GetLinks(It.IsAny<Uri>(), It.IsAny<string>())).Returns(new List<Uri>());
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = true });
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPageLinks(It.IsAny<CrawledPage>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = false, Reason = "aaa" });

            int _pageCrawlStartingCount = 0;
            int _pageCrawlCompletedCount = 0;
            int _pageCrawlDisallowedCount = 0;
            int _pageLinksCrawlDisallowedCount = 0;
            _unitUnderTest.PageCrawlCompletedAsync += (s, e) => ++_pageCrawlCompletedCount;
            _unitUnderTest.PageCrawlStartingAsync += (s, e) => ++_pageCrawlStartingCount;
            _unitUnderTest.PageCrawlDisallowedAsync += (s, e) => ++_pageCrawlDisallowedCount;
            _unitUnderTest.PageLinksCrawlDisallowedAsync += (s, e) => ++_pageLinksCrawlDisallowedCount;

            _unitUnderTest.Crawl(_rootUri);
            System.Threading.Thread.Sleep(100);//sleep since the events are async and may not complete before returning

            _fakeHttpRequester.Verify(f => f.MakeRequest(It.IsAny<Uri>(), It.IsAny<Func<CrawledPage, CrawlDecision>>()), Times.Once());
            _fakeHyperLinkParser.Verify(f => f.GetLinks(It.IsAny<Uri>(), It.IsAny<string>()), Times.Never());
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>()), Times.Once());
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPageLinks(It.IsAny<CrawledPage>(), It.IsAny<CrawlContext>()), Times.Once());

            Assert.AreEqual(1, _pageCrawlStartingCount);
            Assert.AreEqual(1, _pageCrawlCompletedCount);
            Assert.AreEqual(0, _pageCrawlDisallowedCount);
            Assert.AreEqual(1, _pageLinksCrawlDisallowedCount);
        }

        [Test]
        public void Crawl_CrawlDecisionMakerShouldCrawlMethodReturnsFalse_PageCrawlDisallowedAsyncEventFires()
        {
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = false, Reason = "aaa" });

            int _pageCrawlStartingCount = 0;
            int _pageCrawlCompletedCount = 0;
            int _pageCrawlDisallowedCount = 0;
            int _pageLinksCrawlDisallowedCount = 0;
            _unitUnderTest.PageCrawlCompletedAsync += (s, e) => ++_pageCrawlCompletedCount;
            _unitUnderTest.PageCrawlStartingAsync += (s, e) => ++_pageCrawlStartingCount;
            _unitUnderTest.PageCrawlDisallowedAsync += (s, e) => ++_pageCrawlDisallowedCount;
            _unitUnderTest.PageLinksCrawlDisallowedAsync += (s, e) => ++_pageLinksCrawlDisallowedCount;

            _unitUnderTest.Crawl(_rootUri);
            System.Threading.Thread.Sleep(100);//sleep since the events are async and may not complete before returning

            _fakeHttpRequester.Verify(f => f.MakeRequest(It.IsAny<Uri>()), Times.Never());
            _fakeHyperLinkParser.Verify(f => f.GetLinks(It.IsAny<Uri>(), It.IsAny<string>()), Times.Never());
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>()), Times.Once());
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPageLinks(It.IsAny<CrawledPage>(), It.IsAny<CrawlContext>()), Times.Never());

            Assert.AreEqual(0, _pageCrawlStartingCount);
            Assert.AreEqual(0, _pageCrawlCompletedCount);
            Assert.AreEqual(1, _pageCrawlDisallowedCount);
            Assert.AreEqual(0, _pageLinksCrawlDisallowedCount);
        }


        [Test]
        public void Crawl_PageCrawlStartingAndCompletedAsyncEventSubscriberThrowsExceptions_DoesNotCrash()
        {
            _fakeHttpRequester.Setup(f => f.MakeRequest(It.IsAny<Uri>(), It.IsAny<Func<CrawledPage, CrawlDecision>>())).Returns(new CrawledPage(_rootUri));
            _fakeHyperLinkParser.Setup(f => f.GetLinks(It.IsAny<Uri>(), It.IsAny<string>())).Returns(new List<Uri>());
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = true });
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPageLinks(It.IsAny<CrawledPage>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = true });

            FifoScheduler _dummyScheduler = new FifoScheduler();
            ManualThreadManager _dummyThreadManager = new ManualThreadManager(1);
            _unitUnderTest = new WebCrawler(_dummyThreadManager, _dummyScheduler, _fakeHttpRequester.Object, _fakeHyperLinkParser.Object, _fakeCrawlDecisionMaker.Object, new CrawlConfiguration());

            int _pageCrawlStartingCount = 0;
            int _pageCrawlCompletedCount = 0;
            int _pageCrawlDisallowedCount = 0;
            int _pageLinksCrawlDisallowedCount = 0;
            _unitUnderTest.PageCrawlCompletedAsync += (s, e) => ++_pageCrawlCompletedCount;
            _unitUnderTest.PageCrawlStartingAsync += (s, e) => ++_pageCrawlStartingCount;
            _unitUnderTest.PageCrawlStartingAsync += new EventHandler<PageCrawlStartingArgs>(ThrowExceptionWhen_PageCrawlStarting);
            _unitUnderTest.PageCrawlCompletedAsync += new EventHandler<PageCrawlCompletedArgs>(ThrowExceptionWhen_PageCrawlCompleted);
            _unitUnderTest.PageCrawlDisallowedAsync += (s, e) => ++_pageCrawlDisallowedCount;
            _unitUnderTest.PageLinksCrawlDisallowedAsync += (s, e) => ++_pageLinksCrawlDisallowedCount;

            _unitUnderTest.Crawl(_rootUri);
            System.Threading.Thread.Sleep(1000);//sleep since the events are async and may not complete

            _fakeHttpRequester.Verify(f => f.MakeRequest(It.IsAny<Uri>(), It.IsAny<Func<CrawledPage, CrawlDecision>>()), Times.Once());
            _fakeHyperLinkParser.Verify(f => f.GetLinks(It.IsAny<CrawledPage>()), Times.Once());
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>()), Times.Once());
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPageLinks(It.IsAny<CrawledPage>(), It.IsAny<CrawlContext>()), Times.Once());

            Assert.AreEqual(1, _pageCrawlStartingCount);
            Assert.AreEqual(1, _pageCrawlCompletedCount);
            Assert.AreEqual(0, _pageCrawlDisallowedCount);
            Assert.AreEqual(0, _pageLinksCrawlDisallowedCount);
        }

        [Test]
        public void Crawl_PageCrawlDisallowedAsyncSubscriberThrowsExceptions_DoesNotCrash()
        {
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = false, Reason = "aaa" });

            int _pageCrawlStartingCount = 0;
            int _pageCrawlCompletedCount = 0;
            int _pageCrawlDisallowedCount = 0;
            int _pageLinksCrawlDisallowedCount = 0;
            _unitUnderTest.PageCrawlCompletedAsync += (s, e) => ++_pageCrawlCompletedCount;
            _unitUnderTest.PageCrawlStartingAsync += (s, e) => ++_pageCrawlStartingCount;
            _unitUnderTest.PageCrawlDisallowedAsync += (s, e) => ++_pageCrawlDisallowedCount;
            _unitUnderTest.PageLinksCrawlDisallowedAsync += (s, e) => ++_pageLinksCrawlDisallowedCount;
            _unitUnderTest.PageCrawlDisallowedAsync += new EventHandler<PageCrawlDisallowedArgs>(ThrowExceptionWhen_PageCrawlDisallowed);
            _unitUnderTest.PageLinksCrawlDisallowedAsync += new EventHandler<PageLinksCrawlDisallowedArgs>(ThrowExceptionWhen_PageLinksCrawlDisallowed);

            _unitUnderTest.Crawl(_rootUri);
            System.Threading.Thread.Sleep(1000);//sleep since the events are async and may not complete

            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>()), Times.Once());

            Assert.AreEqual(0, _pageCrawlStartingCount);
            Assert.AreEqual(0, _pageCrawlCompletedCount);
            Assert.AreEqual(1, _pageCrawlDisallowedCount);
            Assert.AreEqual(0, _pageLinksCrawlDisallowedCount);
        }

        [Test]
        public void Crawl_PageLinksCrawlDisallowedAsyncSubscriberThrowsExceptions_DoesNotCrash()
        {
            _fakeHttpRequester.Setup(f => f.MakeRequest(It.IsAny<Uri>(), It.IsAny<Func<CrawledPage, CrawlDecision>>())).Returns(new CrawledPage(_rootUri));
            _fakeHyperLinkParser.Setup(f => f.GetLinks(It.IsAny<Uri>(), It.IsAny<string>())).Returns(new List<Uri>());
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = true });
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPageLinks(It.IsAny<CrawledPage>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = false, Reason = "aaa" });

            FifoScheduler _dummyScheduler = new FifoScheduler();
            ManualThreadManager _dummyThreadManager = new ManualThreadManager(1);
            _unitUnderTest = new WebCrawler(_dummyThreadManager, _dummyScheduler, _fakeHttpRequester.Object, _fakeHyperLinkParser.Object, _fakeCrawlDecisionMaker.Object, new CrawlConfiguration());

            int _pageCrawlStartingCount = 0;
            int _pageCrawlCompletedCount = 0;
            int _pageCrawlDisallowedCount = 0;
            int _pageLinksCrawlDisallowedCount = 0;
            _unitUnderTest.PageCrawlCompletedAsync += (s, e) => ++_pageCrawlCompletedCount;
            _unitUnderTest.PageCrawlStartingAsync += (s, e) => ++_pageCrawlStartingCount;
            _unitUnderTest.PageCrawlDisallowedAsync += (s, e) => ++_pageCrawlDisallowedCount;
            _unitUnderTest.PageLinksCrawlDisallowedAsync += (s, e) => ++_pageLinksCrawlDisallowedCount;
            //_unitUnderTest.PageLinksCrawlDisallowed += new EventHandler<PageLinksCrawlDisallowedArgs>(ThrowExceptionWhen_PageLinksCrawlDisallowed);

            _unitUnderTest.Crawl(_rootUri);
            System.Threading.Thread.Sleep(2000);//sleep since the events are async and may not complete, set to 2 seconds since this test was mysteriously failing only when run with code coverage

            _fakeHttpRequester.Verify(f => f.MakeRequest(It.IsAny<Uri>(), It.IsAny<Func<CrawledPage, CrawlDecision>>()), Times.Once());
            _fakeHyperLinkParser.Verify(f => f.GetLinks(It.IsAny<Uri>(), It.IsAny<string>()), Times.Never());
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>()), Times.Once());
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPageLinks(It.IsAny<CrawledPage>(), It.IsAny<CrawlContext>()), Times.Once());

            Assert.AreEqual(1, _pageCrawlStartingCount);
            Assert.AreEqual(1, _pageCrawlCompletedCount);
            Assert.AreEqual(0, _pageCrawlDisallowedCount);
            Assert.AreEqual(1, _pageLinksCrawlDisallowedCount);
        }


        [Test]
        public void Crawl_PageCrawlStartingAsyncEvent_IsAsynchronous()
        {
            int elapsedTimeForLongJob = 5000;

            _fakeHttpRequester.Setup(f => f.MakeRequest(It.IsAny<Uri>(), It.IsAny<Func<CrawledPage, CrawlDecision>>())).Returns(new CrawledPage(_rootUri));
            _fakeHyperLinkParser.Setup(f => f.GetLinks(It.IsAny<Uri>(), It.IsAny<string>())).Returns(new List<Uri>());
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = true });
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPageLinks(It.IsAny<CrawledPage>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = false, Reason = "aaaa" });

            _unitUnderTest.PageCrawlStartingAsync += new EventHandler<PageCrawlStartingArgs>((sender, args) => System.Threading.Thread.Sleep(elapsedTimeForLongJob));

            Stopwatch timer = Stopwatch.StartNew();
            _unitUnderTest.Crawl(_rootUri);
            timer.Stop();

            Assert.IsTrue(timer.ElapsedMilliseconds < elapsedTimeForLongJob);
        }

        [Test]
        public void Crawl_PageCrawlCompletedAsyncEvent_IsAsynchronous()
        {
            int elapsedTimeForLongJob = 5000;

            _fakeHttpRequester.Setup(f => f.MakeRequest(It.IsAny<Uri>(), It.IsAny<Func<CrawledPage, CrawlDecision>>())).Returns(new CrawledPage(_rootUri));
            _fakeHyperLinkParser.Setup(f => f.GetLinks(It.IsAny<Uri>(), It.IsAny<string>())).Returns(new List<Uri>());
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = true });
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPageLinks(It.IsAny<CrawledPage>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = false, Reason = "aaaa" });

            _unitUnderTest.PageCrawlCompletedAsync += new EventHandler<PageCrawlCompletedArgs>((sender, args) => System.Threading.Thread.Sleep(elapsedTimeForLongJob));

            Stopwatch timer = Stopwatch.StartNew();
            _unitUnderTest.Crawl(_rootUri);
            timer.Stop();

            Assert.IsTrue(timer.ElapsedMilliseconds < elapsedTimeForLongJob);
        }

        [Test]
        public void Crawl_PageCrawlDisallowedAsyncEvent_IsAsynchronous()
        {
            int elapsedTimeForLongJob = 5000;

            _fakeHttpRequester.Setup(f => f.MakeRequest(It.IsAny<Uri>())).Returns(new CrawledPage(_rootUri));
            _fakeHyperLinkParser.Setup(f => f.GetLinks(It.IsAny<Uri>(), It.IsAny<string>())).Returns(new List<Uri>());
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = false, Reason = "aaa" });

            _unitUnderTest.PageCrawlDisallowedAsync += new EventHandler<PageCrawlDisallowedArgs>((sender, args) => System.Threading.Thread.Sleep(elapsedTimeForLongJob));

            Stopwatch timer = Stopwatch.StartNew();
            _unitUnderTest.Crawl(_rootUri);
            timer.Stop();

            Assert.IsTrue(timer.ElapsedMilliseconds < elapsedTimeForLongJob);
        }

        [Test]
        public void Crawl_PageLinksCrawlDisallowedAsyncEvent_IsAsynchronous()
        {
            int elapsedTimeForLongJob = 5000;

            _fakeHttpRequester.Setup(f => f.MakeRequest(It.IsAny<Uri>(), It.IsAny<Func<CrawledPage, CrawlDecision>>())).Returns(new CrawledPage(_rootUri));
            _fakeHyperLinkParser.Setup(f => f.GetLinks(It.IsAny<Uri>(), It.IsAny<string>())).Returns(new List<Uri>());
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = true });
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPageLinks(It.IsAny<CrawledPage>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = false, Reason = "aaa" });

            _unitUnderTest.PageLinksCrawlDisallowedAsync += new EventHandler<PageLinksCrawlDisallowedArgs>((sender, args) => System.Threading.Thread.Sleep(elapsedTimeForLongJob));

            Stopwatch timer = Stopwatch.StartNew();
            _unitUnderTest.Crawl(_rootUri);
            timer.Stop();

            Assert.IsTrue(timer.ElapsedMilliseconds < elapsedTimeForLongJob);
        }

        #endregion

        [Test]
        public void Crawl_CrawlDecisionDelegatesReturnTrue_EventsFired()
        {
            //Arrange
            _fakeHttpRequester.Setup(f => f.MakeRequest(It.IsAny<Uri>(), It.IsAny<Func<CrawledPage, CrawlDecision>>())).Returns(new CrawledPage(_rootUri));
            _fakeHyperLinkParser.Setup(f => f.GetLinks(It.IsAny<CrawledPage>())).Returns(new List<Uri> { new Uri("http://a.com/a"), new Uri("http://a.com/b") });
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = true });
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPageLinks(It.IsAny<CrawledPage>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = true });

            int pageCrawlStartingCount = 0;
            int pageCrawlCompletedCount = 0;
            _unitUnderTest.PageCrawlCompleted += (s, e) => ++pageCrawlCompletedCount;
            _unitUnderTest.PageCrawlStarting += (s, e) => ++pageCrawlStartingCount;

            bool shouldCrawlPageDelegateCalled = false;
            _unitUnderTest.ShouldCrawlPage((x, y) =>
            {
                if (shouldCrawlPageDelegateCalled)
                {
                    return new CrawlDecision { Allow = false };
                }
                else
                {
                    //only return true on the first call to avoid an infinite loop
                    shouldCrawlPageDelegateCalled = true;
                    return new CrawlDecision { Allow = true };
                }
            });

            bool shouldCrawlPageLinksDelegateCalled = false;
            _unitUnderTest.ShouldCrawlPageLinks((x, y) =>
            {
                shouldCrawlPageLinksDelegateCalled = true;
                return new CrawlDecision { Allow = true };
            });

            int isInternalUriDelegateCalledCount = 0;
            _unitUnderTest.IsInternalUri((x, y) =>
            {
                isInternalUriDelegateCalledCount++;
                return false;
            });

            //Act
            _unitUnderTest.Crawl(_rootUri);

            //Assert
            System.Threading.Thread.Sleep(100);//sleep since the events are async and may not complete before returning

            _fakeHttpRequester.Verify(f => f.MakeRequest(It.IsAny<Uri>(), It.IsAny<Func<CrawledPage, CrawlDecision>>()), Times.Once());
            _fakeHyperLinkParser.Verify(f => f.GetLinks(It.IsAny<CrawledPage>()), Times.Once());
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>()), Times.Exactly(3));//1 for _rootUri, 2 for the returned links
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPageLinks(It.IsAny<CrawledPage>(), It.IsAny<CrawlContext>()), Times.Once());

            Assert.IsTrue(shouldCrawlPageDelegateCalled);
            //Assert.IsTrue(shouldCrawlDownloadPageContentDelegateCalled);
            Assert.IsTrue(shouldCrawlPageLinksDelegateCalled);
            Assert.AreEqual(2, isInternalUriDelegateCalledCount);
            Assert.AreEqual(1, pageCrawlStartingCount);
            Assert.AreEqual(1, pageCrawlCompletedCount);
        }

        [Test]
        public void Crawl_ShouldCrawlPageDelegateReturnsFalse_PageIsNotCrawled()
        {
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = true });

            int pageCrawlStartingCount = 0;
            int pageCrawlCompletedCount = 0;
            int pageCrawlDisallowedCount = 0;
            int pageLinksCrawlDisallowedCount = 0;
            _unitUnderTest.PageCrawlCompleted += (s, e) => ++pageCrawlCompletedCount;
            _unitUnderTest.PageCrawlStarting += (s, e) => ++pageCrawlStartingCount;
            _unitUnderTest.PageCrawlDisallowed += (s, e) => ++pageCrawlDisallowedCount;
            _unitUnderTest.PageLinksCrawlDisallowed += (s, e) => ++pageLinksCrawlDisallowedCount;

            bool shouldCrawlPageDelegateCalled = false;
            _unitUnderTest.ShouldCrawlPage((x, y) =>
            {
                shouldCrawlPageDelegateCalled = true;
                return new CrawlDecision { Allow = false, Reason = "aaa" };
            });

            _unitUnderTest.Crawl(_rootUri);

            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>()), Times.Once());

            Assert.IsTrue(shouldCrawlPageDelegateCalled);
            Assert.AreEqual(0, pageCrawlStartingCount);
            Assert.AreEqual(0, pageCrawlCompletedCount);
            Assert.AreEqual(1, pageCrawlDisallowedCount);
            Assert.AreEqual(0, pageLinksCrawlDisallowedCount);
        }

        [Test]
        public void Crawl_ShouldCrawlLinksDelegateReturnsFalse_PageLinksNotCrawled()
        {
            _fakeHttpRequester.Setup(f => f.MakeRequest(It.IsAny<Uri>(), It.IsAny<Func<CrawledPage, CrawlDecision>>())).Returns(new CrawledPage(_rootUri));
            _fakeHyperLinkParser.Setup(f => f.GetLinks(It.IsAny<Uri>(), It.IsAny<string>())).Returns(new List<Uri>());
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = true });
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPageLinks(It.IsAny<CrawledPage>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = true});

            int _pageCrawlStartingCount = 0;
            int _pageCrawlCompletedCount = 0;
            int _pageCrawlDisallowedCount = 0;
            int _pageLinksCrawlDisallowedCount = 0;
            _unitUnderTest.PageCrawlCompleted += (s, e) => ++_pageCrawlCompletedCount;
            _unitUnderTest.PageCrawlStarting += (s, e) => ++_pageCrawlStartingCount;
            _unitUnderTest.PageCrawlDisallowed += (s, e) => ++_pageCrawlDisallowedCount;
            _unitUnderTest.PageLinksCrawlDisallowed += (s, e) => ++_pageLinksCrawlDisallowedCount;

            _unitUnderTest.ShouldCrawlPageLinks((x, y) =>
            {
                return new CrawlDecision { Allow = false, Reason = "aaa" };
            });

            _unitUnderTest.Crawl(_rootUri);

            _fakeHttpRequester.Verify(f => f.MakeRequest(It.IsAny<Uri>(), It.IsAny<Func<CrawledPage, CrawlDecision>>()), Times.Once());
            _fakeHyperLinkParser.Verify(f => f.GetLinks(It.IsAny<Uri>(), It.IsAny<string>()), Times.Never());
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>()), Times.Once());
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPageLinks(It.IsAny<CrawledPage>(), It.IsAny<CrawlContext>()), Times.Once());

            Assert.AreEqual(1, _pageCrawlStartingCount);
            Assert.AreEqual(1, _pageCrawlCompletedCount);
            Assert.AreEqual(0, _pageCrawlDisallowedCount);
            Assert.AreEqual(1, _pageLinksCrawlDisallowedCount);
        }

        [Test]
        public void Crawl_CrawlResult_CrawlContextIsSet()
        {
            _fakeHttpRequester.Setup(f => f.MakeRequest(It.IsAny<Uri>(), It.IsAny<Func<CrawledPage, CrawlDecision>>())).Returns(new CrawledPage(_rootUri));
            _fakeHyperLinkParser.Setup(f => f.GetLinks(It.IsAny<Uri>(), It.IsAny<string>())).Returns(new List<Uri>());
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = true });
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPageLinks(It.IsAny<CrawledPage>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = true });

            CrawlResult result = _unitUnderTest.Crawl(_rootUri);

            _fakeHttpRequester.Verify(f => f.MakeRequest(It.IsAny<Uri>(), It.IsAny<Func<CrawledPage, CrawlDecision>>()), Times.Once());
            _fakeHyperLinkParser.Verify(f => f.GetLinks(It.IsAny<CrawledPage>()), Times.Once());
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>()), Times.Once());
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPageLinks(It.IsAny<CrawledPage>(), It.IsAny<CrawlContext>()), Times.Once());
            Assert.IsNotNull(result.CrawlContext);
            Assert.AreSame(_dummyScheduler, result.CrawlContext.Scheduler);
        }

        [Test]
        public void Crawl_StopRequested_CrawlIsStoppedBeforeCompletion()
        {
            PageToCrawl pageToReturn = new PageToCrawl(_rootUri);
            for (int i = 0; i < 100; i++)
                _dummyScheduler.Add(pageToReturn);
            
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>()))
                .Callback<PageToCrawl, CrawlContext>((p, c) =>
                {
                    c.IsCrawlStopRequested = true;
                    System.Threading.Thread.Sleep(500);
                })
                .Returns(new CrawlDecision { Allow = false, Reason = "Should have timed out so this crawl decision doesn't matter." });

            CrawlResult result = _unitUnderTest.Crawl(_rootUri);

            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPage(pageToReturn, It.IsAny<CrawlContext>()), Times.Exactly(1));
            Assert.AreEqual(0, _dummyScheduler.Count);
            Assert.IsTrue(result.CrawlContext.IsCrawlStopRequested);
            Assert.IsFalse(result.CrawlContext.IsCrawlHardStopRequested);
        }

        [Test, Ignore]
        public void Crawl_HardStopRequested_CrawlIsStoppedBeforeCompletion()
        {
            //Nothing calls hard stop due to thread aborting is a bad way to stop threads
            //Waiting for tpl impl so we can use a cancellation token
        }

        [Test]
        public void Crawl_OverCrawlTimeoutSeconds_CrawlIsStoppedBeforeCompletion()
        {
            _dummyConfiguration.CrawlTimeoutSeconds = 1;

            PageToCrawl pageToReturn = new PageToCrawl(_rootUri);
            CrawledPage crawledPage = new CrawledPage(_rootUri) { ParentUri = _rootUri};

            for (int i = 0; i < 100; i++)
                _dummyScheduler.Add(pageToReturn);

            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>()))
                .Callback(() => System.Threading.Thread.Sleep(2000))
                .Returns(new CrawlDecision { Allow = true });
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPageLinks(It.IsAny<CrawledPage>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = false });
            _fakeHttpRequester.Setup(f => f.MakeRequest(It.IsAny<Uri>(), It.IsAny<Func<CrawledPage, CrawlDecision>>())).Returns(crawledPage);

            CrawlResult result = _unitUnderTest.Crawl(_rootUri);

            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPage(pageToReturn, It.IsAny<CrawlContext>()), Times.Exactly(1));
            Assert.AreEqual(0, _dummyScheduler.Count);
            Assert.IsTrue(result.CrawlContext.IsCrawlStopRequested, "should have been false");
            Assert.IsFalse(result.CrawlContext.IsCrawlHardStopRequested);
        }

        [Test]
        public void CrawlBag_IsSetOnCrawlContext()
        {
            _fakeHttpRequester.Setup(f => f.MakeRequest(It.IsAny<Uri>(), It.IsAny<Func<CrawledPage, CrawlDecision>>())).Returns(new CrawledPage(_rootUri));
            _fakeHyperLinkParser.Setup(f => f.GetLinks(It.IsAny<Uri>(), It.IsAny<string>())).Returns(new List<Uri>());
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = true });
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlPageLinks(It.IsAny<CrawledPage>(), It.IsAny<CrawlContext>())).Returns(new CrawlDecision { Allow = true });

            CrawlContext actualCrawlContext = null;

            _unitUnderTest.PageCrawlCompleted += (s, e) => actualCrawlContext = e.CrawlContext;

            _unitUnderTest.Crawl(_rootUri);

            _fakeHttpRequester.Verify(f => f.MakeRequest(It.IsAny<Uri>(), It.IsAny<Func<CrawledPage, CrawlDecision>>()), Times.Once());
            _fakeHyperLinkParser.Verify(f => f.GetLinks(It.IsAny<CrawledPage>()), Times.Once());
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPage(It.IsAny<PageToCrawl>(), It.IsAny<CrawlContext>()), Times.Once());
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlPageLinks(It.IsAny<CrawledPage>(), It.IsAny<CrawlContext>()), Times.Once());

            Assert.AreEqual("SomeVal", actualCrawlContext.CrawlBag.SomeVal);
            Assert.AreEqual(2, actualCrawlContext.CrawlBag.SomeList.Count);
        }

        private void ThrowExceptionWhen_PageCrawlStarting(object sender, PageCrawlStartingArgs e)
        {
            throw new Exception("no!!!");
        }

        private void ThrowExceptionWhen_PageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            throw new Exception("Oh No!");
        }

        private void ThrowExceptionWhen_PageCrawlDisallowed(object sender, PageCrawlDisallowedArgs e)
        {
            throw new Exception("no!!!");
        }

        private void ThrowExceptionWhen_PageLinksCrawlDisallowed(object sender, PageLinksCrawlDisallowedArgs e)
        {
            throw new Exception("Oh No!");
        }

        private HtmlDocument GetHtmlDocument(string html)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            return doc;
        }
    }
}
