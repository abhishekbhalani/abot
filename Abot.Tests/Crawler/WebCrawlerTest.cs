using Abot.Core;
using Abot.Crawler;
using Abot.Poco;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Abot.Tests.Crawler
{
    [TestFixture]
    public class CrawlerBasicTest
    {
        WebCrawler _unitUnderTest;
        Mock<IPageRequester> _fakeHttpRequester;
        Mock<IHyperLinkParser> _fakeHyperLinkParser;
        Mock<ICrawlDecisionMaker> _fakeCrawlDecisionMaker;
        FifoScheduler _dummyScheduler;
        ThreadManager _dummyThreadManager;
        Uri _rootUri;

        [SetUp]
        public void SetUp()
        {
            _fakeHyperLinkParser = new Mock<IHyperLinkParser>();
            _fakeHttpRequester = new Mock<IPageRequester>();
            _fakeCrawlDecisionMaker = new Mock<ICrawlDecisionMaker>();

            _dummyScheduler = new FifoScheduler();
            _dummyThreadManager = new ThreadManager(1);

            _unitUnderTest = new WebCrawler(_dummyThreadManager, _dummyScheduler, _fakeHttpRequester.Object, _fakeHyperLinkParser.Object, _fakeCrawlDecisionMaker.Object);

            _rootUri = new Uri("http://a.com/");
        }

        [Test]
        public void Constructor_Empty()
        {
            Assert.IsNotNull(new WebCrawler());
        }

        [Test]
        public void Crawl_CallsDependencies()
        {
            Uri uri1 = new Uri(_rootUri.AbsoluteUri + "a.html");
            Uri uri2 = new Uri(_rootUri.AbsoluteUri + "b.html");

            CrawledPage homePage = new CrawledPage(_rootUri) { RawContent = "content here"};
            CrawledPage page1 = new CrawledPage(uri1);
            CrawledPage page2 = new CrawledPage(uri2);

            List<Uri> links = new List<Uri>{uri1, uri2};

            _fakeHttpRequester.Setup(f => f.MakeRequest(_rootUri)).Returns(homePage);
            _fakeHttpRequester.Setup(f => f.MakeRequest(uri1)).Returns(page1);
            _fakeHttpRequester.Setup(f => f.MakeRequest(uri2)).Returns(page2);
            _fakeHyperLinkParser.Setup(f => f.GetLinks(_rootUri, It.IsAny<string>())).Returns(links);
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawl(It.IsAny<PageToCrawl>())).Returns(new CrawlDecision{Should = true});
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlLinks(It.IsAny<CrawledPage>())).Returns(new CrawlDecision{ Should = true });

            _unitUnderTest.Crawl(_rootUri);

            _fakeHttpRequester.Verify(f => f.MakeRequest(_rootUri), Times.Once());
            _fakeHttpRequester.Verify(f => f.MakeRequest(uri1), Times.Once());
            _fakeHttpRequester.Verify(f => f.MakeRequest(uri2), Times.Once());
            _fakeHyperLinkParser.Verify(f => f.GetLinks(_rootUri, It.IsAny<string>()), Times.Once());
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawl(It.IsAny<PageToCrawl>()), Times.Exactly(3));
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlLinks(It.IsAny<CrawledPage>()), Times.Exactly(3));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Crawl_NullUri()
        {
            _unitUnderTest.Crawl(null);
        }


        [Test]
        public void Crawl_CrawlDecisionMakerMethodsReturnTrue_PageCrawlStartingAndCompletedEventsFire()
        {
            _fakeHttpRequester.Setup(f => f.MakeRequest(It.IsAny<Uri>())).Returns(new CrawledPage(_rootUri));
            _fakeHyperLinkParser.Setup(f => f.GetLinks(It.IsAny<Uri>(), It.IsAny<string>())).Returns(new List<Uri>());
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawl(It.IsAny<PageToCrawl>())).Returns(new CrawlDecision{Should = true});
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlLinks(It.IsAny<CrawledPage>())).Returns(new CrawlDecision{ Should = true });

            int _pageCrawlStartingCount = 0;
            int _pageCrawlCompletedCount = 0;
            int _pageCrawlDisallowedCount = 0;
            int _pageLinksCrawlDisallowedCount = 0;
            _unitUnderTest.PageCrawlCompleted += (s, e) => ++_pageCrawlCompletedCount;
            _unitUnderTest.PageCrawlStarting += (s, e) => ++_pageCrawlStartingCount;
            _unitUnderTest.PageCrawlDisallowed += (s, e) => ++_pageCrawlDisallowedCount;
            _unitUnderTest.PageLinksCrawlDisallowed += (s, e) => ++_pageLinksCrawlDisallowedCount;
            
            _unitUnderTest.Crawl(_rootUri);
            System.Threading.Thread.Sleep(100);//sleep since the events are async and may not complete before returning

            _fakeHttpRequester.Verify(f => f.MakeRequest(It.IsAny<Uri>()), Times.Once());
            _fakeHyperLinkParser.Verify(f => f.GetLinks(It.IsAny<Uri>(), It.IsAny<string>()), Times.Once());
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawl(It.IsAny<PageToCrawl>()), Times.Once());
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlLinks(It.IsAny<CrawledPage>()), Times.Once());

            Assert.AreEqual(1, _pageCrawlStartingCount);
            Assert.AreEqual(1, _pageCrawlCompletedCount);
            Assert.AreEqual(0, _pageCrawlDisallowedCount);
            Assert.AreEqual(0, _pageLinksCrawlDisallowedCount);
        }

        [Test]
        public void Crawl_CrawlDecisionMakerShouldCrawlLinksMethodReturnsFalse_PageLinksCrawlDisallowedEventFires()
        {
            _fakeHttpRequester.Setup(f => f.MakeRequest(It.IsAny<Uri>())).Returns(new CrawledPage(_rootUri));
            _fakeHyperLinkParser.Setup(f => f.GetLinks(It.IsAny<Uri>(), It.IsAny<string>())).Returns(new List<Uri>());
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawl(It.IsAny<PageToCrawl>())).Returns(new CrawlDecision{Should = true});
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlLinks(It.IsAny<CrawledPage>())).Returns(new CrawlDecision{ Should = false, Reason = "aaa" });

            int _pageCrawlStartingCount = 0;
            int _pageCrawlCompletedCount = 0;
            int _pageCrawlDisallowedCount = 0;
            int _pageLinksCrawlDisallowedCount = 0;
            _unitUnderTest.PageCrawlCompleted += (s, e) => ++_pageCrawlCompletedCount;
            _unitUnderTest.PageCrawlStarting += (s, e) => ++_pageCrawlStartingCount;
            _unitUnderTest.PageCrawlDisallowed += (s, e) => ++_pageCrawlDisallowedCount;
            _unitUnderTest.PageLinksCrawlDisallowed += (s, e) => ++_pageLinksCrawlDisallowedCount;

            _unitUnderTest.Crawl(_rootUri);
            System.Threading.Thread.Sleep(100);//sleep since the events are async and may not complete before returning

            _fakeHttpRequester.Verify(f => f.MakeRequest(It.IsAny<Uri>()), Times.Once());
            _fakeHyperLinkParser.Verify(f => f.GetLinks(It.IsAny<Uri>(), It.IsAny<string>()), Times.Never());
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawl(It.IsAny<PageToCrawl>()), Times.Once());
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlLinks(It.IsAny<CrawledPage>()), Times.Once());

            Assert.AreEqual(1, _pageCrawlStartingCount);
            Assert.AreEqual(1, _pageCrawlCompletedCount);
            Assert.AreEqual(0, _pageCrawlDisallowedCount);
            Assert.AreEqual(1, _pageLinksCrawlDisallowedCount);
        }

        [Test]
        public void Crawl_CrawlDecisionMakerShouldCrawlMethodReturnsFalse_PageCrawlDisallowedEventFires()
        {
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawl(It.IsAny<PageToCrawl>())).Returns(new CrawlDecision { Should = false, Reason = "aaa" });

            int _pageCrawlStartingCount = 0;
            int _pageCrawlCompletedCount = 0;
            int _pageCrawlDisallowedCount = 0;
            int _pageLinksCrawlDisallowedCount = 0;
            _unitUnderTest.PageCrawlCompleted += (s, e) => ++_pageCrawlCompletedCount;
            _unitUnderTest.PageCrawlStarting += (s, e) => ++_pageCrawlStartingCount;
            _unitUnderTest.PageCrawlDisallowed += (s, e) => ++_pageCrawlDisallowedCount;
            _unitUnderTest.PageLinksCrawlDisallowed += (s, e) => ++_pageLinksCrawlDisallowedCount;

            _unitUnderTest.Crawl(_rootUri);
            System.Threading.Thread.Sleep(100);//sleep since the events are async and may not complete before returning

            _fakeHttpRequester.Verify(f => f.MakeRequest(It.IsAny<Uri>()), Times.Never());
            _fakeHyperLinkParser.Verify(f => f.GetLinks(It.IsAny<Uri>(), It.IsAny<string>()), Times.Never());
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawl(It.IsAny<PageToCrawl>()), Times.Once());
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlLinks(It.IsAny<CrawledPage>()), Times.Never());

            Assert.AreEqual(0, _pageCrawlStartingCount);
            Assert.AreEqual(0, _pageCrawlCompletedCount);
            Assert.AreEqual(1, _pageCrawlDisallowedCount);
            Assert.AreEqual(0, _pageLinksCrawlDisallowedCount);
        }


        [Test]
        public void Crawl_PageCrawlStartingAndCompletedEventSubscriberThrowsExceptions_DoesNotCrash()
        {
            _fakeHttpRequester.Setup(f => f.MakeRequest(It.IsAny<Uri>())).Returns(new CrawledPage(_rootUri));
            _fakeHyperLinkParser.Setup(f => f.GetLinks(It.IsAny<Uri>(), It.IsAny<string>())).Returns(new List<Uri>());
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawl(It.IsAny<PageToCrawl>())).Returns(new CrawlDecision{Should = true});
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlLinks(It.IsAny<CrawledPage>())).Returns(new CrawlDecision{ Should = true });

            FifoScheduler _dummyScheduler = new FifoScheduler();
            ThreadManager _dummyThreadManager = new ThreadManager(1);
            _unitUnderTest = new WebCrawler(_dummyThreadManager, _dummyScheduler, _fakeHttpRequester.Object, _fakeHyperLinkParser.Object, _fakeCrawlDecisionMaker.Object);

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
            System.Threading.Thread.Sleep(1000);//sleep since the events are async and may not complete

            _fakeHttpRequester.Verify(f => f.MakeRequest(It.IsAny<Uri>()), Times.Once());
            _fakeHyperLinkParser.Verify(f => f.GetLinks(It.IsAny<Uri>(), It.IsAny<string>()), Times.Once());
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawl(It.IsAny<PageToCrawl>()), Times.Once());
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlLinks(It.IsAny<CrawledPage>()), Times.Once());

            Assert.AreEqual(1, _pageCrawlStartingCount);
            Assert.AreEqual(1, _pageCrawlCompletedCount);
            Assert.AreEqual(0, _pageCrawlDisallowedCount);
            Assert.AreEqual(0, _pageLinksCrawlDisallowedCount);
        }

        [Test]
        public void Crawl_PageCrawlDisallowedSubscriberThrowsExceptions_DoesNotCrash()
        {
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawl(It.IsAny<PageToCrawl>())).Returns(new CrawlDecision { Should = false, Reason = "aaa" });

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
            System.Threading.Thread.Sleep(1000);//sleep since the events are async and may not complete

            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawl(It.IsAny<PageToCrawl>()), Times.Once());

            Assert.AreEqual(0, _pageCrawlStartingCount);
            Assert.AreEqual(0, _pageCrawlCompletedCount);
            Assert.AreEqual(1, _pageCrawlDisallowedCount);
            Assert.AreEqual(0, _pageLinksCrawlDisallowedCount);
        }

        [Test, Ignore]//TODO This test only fails when run under NCOVER
        public void Crawl_PageLinksCrawlDisallowedSubscriberThrowsExceptions_DoesNotCrash()
        {
            _fakeHttpRequester.Setup(f => f.MakeRequest(It.IsAny<Uri>())).Returns(new CrawledPage(_rootUri));
            _fakeHyperLinkParser.Setup(f => f.GetLinks(It.IsAny<Uri>(), It.IsAny<string>())).Returns(new List<Uri>());
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawl(It.IsAny<PageToCrawl>())).Returns(new CrawlDecision{Should = true});
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlLinks(It.IsAny<CrawledPage>())).Returns(new CrawlDecision { Should = false, Reason = "aaa" });

            FifoScheduler _dummyScheduler = new FifoScheduler();
            ThreadManager _dummyThreadManager = new ThreadManager(1);
            _unitUnderTest = new WebCrawler(_dummyThreadManager, _dummyScheduler, _fakeHttpRequester.Object, _fakeHyperLinkParser.Object, _fakeCrawlDecisionMaker.Object);

            int _pageCrawlStartingCount = 0;
            int _pageCrawlCompletedCount = 0;
            int _pageCrawlDisallowedCount = 0;
            int _pageLinksCrawlDisallowedCount = 0;
            _unitUnderTest.PageCrawlCompleted += (s, e) => ++_pageCrawlCompletedCount;
            _unitUnderTest.PageCrawlStarting += (s, e) => ++_pageCrawlStartingCount;
            _unitUnderTest.PageCrawlDisallowed += (s, e) => ++_pageCrawlDisallowedCount;
            _unitUnderTest.PageLinksCrawlDisallowed += (s, e) => ++_pageLinksCrawlDisallowedCount;
            //_unitUnderTest.PageLinksCrawlDisallowed += new EventHandler<PageLinksCrawlDisallowedArgs>(ThrowExceptionWhen_PageLinksCrawlDisallowed);

            _unitUnderTest.Crawl(_rootUri);
            System.Threading.Thread.Sleep(1000);//sleep since the events are async and may not complete

            _fakeHttpRequester.Verify(f => f.MakeRequest(It.IsAny<Uri>()), Times.Once());
            _fakeHyperLinkParser.Verify(f => f.GetLinks(It.IsAny<Uri>(), It.IsAny<string>()), Times.Never());
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawl(It.IsAny<PageToCrawl>()), Times.Once());
            _fakeCrawlDecisionMaker.Verify(f => f.ShouldCrawlLinks(It.IsAny<CrawledPage>()), Times.Once());

            Assert.AreEqual(1, _pageCrawlStartingCount);
            Assert.AreEqual(1, _pageCrawlCompletedCount);
            Assert.AreEqual(0, _pageCrawlDisallowedCount);
            Assert.AreEqual(1, _pageLinksCrawlDisallowedCount);
        }


        [Test]
        public void Crawl_PageCrawlStartingEvent_IsAsynchronous()
        {
            int elapsedTimeForLongJob = 5000;

            _fakeHttpRequester.Setup(f => f.MakeRequest(It.IsAny<Uri>())).Returns(new CrawledPage(_rootUri));
            _fakeHyperLinkParser.Setup(f => f.GetLinks(It.IsAny<Uri>(), It.IsAny<string>())).Returns(new List<Uri>());
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawl(It.IsAny<PageToCrawl>())).Returns(new CrawlDecision{Should = true});
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlLinks(It.IsAny<CrawledPage>())).Returns(new CrawlDecision { Should = false, Reason = "aaaa" });

            _unitUnderTest.PageCrawlStarting += new EventHandler<PageCrawlStartingArgs>((sender, args) => System.Threading.Thread.Sleep(elapsedTimeForLongJob));

            Stopwatch timer = Stopwatch.StartNew();
            _unitUnderTest.Crawl(_rootUri);
            timer.Stop();

            Assert.IsTrue(timer.ElapsedMilliseconds < elapsedTimeForLongJob);
        }

        [Test]
        public void Crawl_PageCrawlCompletedEvent_IsAsynchronous()
        {
            int elapsedTimeForLongJob = 5000;

            _fakeHttpRequester.Setup(f => f.MakeRequest(It.IsAny<Uri>())).Returns(new CrawledPage(_rootUri));
            _fakeHyperLinkParser.Setup(f => f.GetLinks(It.IsAny<Uri>(), It.IsAny<string>())).Returns(new List<Uri>());
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawl(It.IsAny<PageToCrawl>())).Returns(new CrawlDecision{Should = true});
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlLinks(It.IsAny<CrawledPage>())).Returns(new CrawlDecision { Should = false, Reason = "aaaa" });

            _unitUnderTest.PageCrawlCompleted += new EventHandler<PageCrawlCompletedArgs>((sender, args) => System.Threading.Thread.Sleep(elapsedTimeForLongJob));

            Stopwatch timer = Stopwatch.StartNew();
            _unitUnderTest.Crawl(_rootUri);
            timer.Stop();

            Assert.IsTrue(timer.ElapsedMilliseconds < elapsedTimeForLongJob);
        }

        [Test]
        public void Crawl_PageCrawlDisallowedEvent_IsAsynchronous()
        {
            int elapsedTimeForLongJob = 5000;

            _fakeHttpRequester.Setup(f => f.MakeRequest(It.IsAny<Uri>())).Returns(new CrawledPage(_rootUri));
            _fakeHyperLinkParser.Setup(f => f.GetLinks(It.IsAny<Uri>(), It.IsAny<string>())).Returns(new List<Uri>());
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawl(It.IsAny<PageToCrawl>())).Returns(new CrawlDecision { Should = false, Reason = "aaa" });

            _unitUnderTest.PageCrawlDisallowed += new EventHandler<PageCrawlDisallowedArgs>((sender, args) => System.Threading.Thread.Sleep(elapsedTimeForLongJob));

            Stopwatch timer = Stopwatch.StartNew();
            _unitUnderTest.Crawl(_rootUri);
            timer.Stop();

            Assert.IsTrue(timer.ElapsedMilliseconds < elapsedTimeForLongJob);
        }

        [Test]
        public void Crawl_PageLinksCrawlDisallowedEvent_IsAsynchronous()
        {
            int elapsedTimeForLongJob = 5000;

            _fakeHttpRequester.Setup(f => f.MakeRequest(It.IsAny<Uri>())).Returns(new CrawledPage(_rootUri));
            _fakeHyperLinkParser.Setup(f => f.GetLinks(It.IsAny<Uri>(), It.IsAny<string>())).Returns(new List<Uri>());
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawl(It.IsAny<PageToCrawl>())).Returns(new CrawlDecision{Should = true});
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawlLinks(It.IsAny<CrawledPage>())).Returns(new CrawlDecision { Should = false, Reason = "aaa" });

            _unitUnderTest.PageLinksCrawlDisallowed += new EventHandler<PageLinksCrawlDisallowedArgs>((sender, args) => System.Threading.Thread.Sleep(elapsedTimeForLongJob));

            Stopwatch timer = Stopwatch.StartNew();
            _unitUnderTest.Crawl(_rootUri);
            timer.Stop();

            Assert.IsTrue(timer.ElapsedMilliseconds < elapsedTimeForLongJob);
        }


        [Test]
        [ExpectedException(typeof(Exception))]
        public void Crawl_FatalExceptionOccurrs()
        {
            Exception fakeException = new Exception("oh no");
            _fakeCrawlDecisionMaker.Setup(f => f.ShouldCrawl(It.IsAny<PageToCrawl>())).Throws(fakeException);

            _unitUnderTest.Crawl(_rootUri);
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
    }
}
