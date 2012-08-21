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
        FifoScheduler _dummyScheduler;
        ThreadManager _dummyThreadManager;
        Uri _rootUri;

        [SetUp]
        public void SetUp()
        {
            //_fakeThreadManager = new Mock<IThreadManager>();
            //_fakeScheduler = new Mock<IScheduler>();
            _fakeHyperLinkParser = new Mock<IHyperLinkParser>();
            _fakeHttpRequester = new Mock<IPageRequester>();

            _dummyScheduler = new FifoScheduler();
            _dummyThreadManager = new ThreadManager(1);

            _unitUnderTest = new WebCrawler(_dummyThreadManager, _dummyScheduler, _fakeHttpRequester.Object, _fakeHyperLinkParser.Object);

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
            _fakeHyperLinkParser.Setup(f => f.GetHyperLinks(_rootUri, It.IsAny<string>())).Returns(links);

            _unitUnderTest.Crawl(_rootUri);

            _fakeHttpRequester.Verify(f => f.MakeRequest(_rootUri));
            _fakeHttpRequester.Verify(f => f.MakeRequest(uri1));
            _fakeHttpRequester.Verify(f => f.MakeRequest(uri2));
            _fakeHyperLinkParser.Verify(f => f.GetHyperLinks(_rootUri, It.IsAny<string>()));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Crawl_NullUri()
        {
            _unitUnderTest.Crawl(null);
        }


        [Test]
        public void Crawl_PageEventsFire()
        {
            _fakeHttpRequester.Setup(f => f.MakeRequest(It.IsAny<Uri>())).Returns(new CrawledPage(_rootUri));
            _fakeHyperLinkParser.Setup(f => f.GetHyperLinks(It.IsAny<Uri>(), It.IsAny<string>())).Returns(new List<Uri>());

            int _pageCrawlStartingCount = 0;
            int _pageCrawlCompletedCount = 0;
            _unitUnderTest.PageCrawlCompleted += (s, e) => ++_pageCrawlCompletedCount;
            _unitUnderTest.PageCrawlStarting += (s, e) => ++_pageCrawlStartingCount;
            
            _unitUnderTest.Crawl(_rootUri);
            System.Threading.Thread.Sleep(100);//sleep since the events are async and may not complete before returning

            Assert.AreEqual(1, _pageCrawlStartingCount);
            Assert.AreEqual(1, _pageCrawlCompletedCount);
        }

        [Test]
        public void Crawl_EventSubscriberThrowsExceptions_DoesNotCrash()
        {
            _fakeHttpRequester.Setup(f => f.MakeRequest(It.IsAny<Uri>())).Returns(new CrawledPage(_rootUri));
            _fakeHyperLinkParser.Setup(f => f.GetHyperLinks(It.IsAny<Uri>(), It.IsAny<string>())).Returns(new List<Uri>());

            FifoScheduler _dummyScheduler = new FifoScheduler();
            ThreadManager _dummyThreadManager = new ThreadManager(1);
            _unitUnderTest = new WebCrawler(_dummyThreadManager, _dummyScheduler, _fakeHttpRequester.Object, _fakeHyperLinkParser.Object);

            int _pageCrawlStartingCount = 0;
            int _pageCrawlCompletedCount = 0;
            _unitUnderTest.PageCrawlCompleted += (s, e) => ++_pageCrawlCompletedCount;
            _unitUnderTest.PageCrawlStarting += (s, e) => ++_pageCrawlStartingCount;
            _unitUnderTest.PageCrawlStarting += new EventHandler<PageCrawlStartingArgs>(ThrowExceptionWhen_PageCrawlStarting);
            _unitUnderTest.PageCrawlCompleted += new EventHandler<PageCrawlCompletedArgs>(ThrowExceptionWhen_PageCrawlCompleted);
            
            _unitUnderTest.Crawl(_rootUri);
            System.Threading.Thread.Sleep(1000);//sleep since the events are async and may not complete

            Assert.AreEqual(1, _pageCrawlStartingCount);
            Assert.AreEqual(1, _pageCrawlCompletedCount);
        }

        [Test]
        public void Crawl_PageCrawlCompletedEvent_IsAsynchronous()
        {
            int elapsedTimeForLongJob = 5000;

            _fakeHttpRequester.Setup(f => f.MakeRequest(It.IsAny<Uri>())).Returns(new CrawledPage(_rootUri));
            _fakeHyperLinkParser.Setup(f => f.GetHyperLinks(It.IsAny<Uri>(), It.IsAny<string>())).Returns(new List<Uri>());
            _unitUnderTest.PageCrawlCompleted += new EventHandler<PageCrawlCompletedArgs>((sender, args) => System.Threading.Thread.Sleep(elapsedTimeForLongJob));

            Stopwatch timer = Stopwatch.StartNew();
            _unitUnderTest.Crawl(_rootUri);
            timer.Stop();

            Assert.IsTrue(timer.ElapsedMilliseconds < elapsedTimeForLongJob);
        }

        [Test]
        public void Crawl_PageCrawlStartingEvent_IsAsynchronous()
        {
            int elapsedTimeForLongJob = 5000;

            _fakeHttpRequester.Setup(f => f.MakeRequest(It.IsAny<Uri>())).Returns(new CrawledPage(_rootUri));
            _fakeHyperLinkParser.Setup(f => f.GetHyperLinks(It.IsAny<Uri>(), It.IsAny<string>())).Returns(new List<Uri>());
            _unitUnderTest.PageCrawlStarting += new EventHandler<PageCrawlStartingArgs>((sender, args) => System.Threading.Thread.Sleep(elapsedTimeForLongJob));

            Stopwatch timer = Stopwatch.StartNew();
            _unitUnderTest.Crawl(_rootUri);
            timer.Stop();

            Assert.IsTrue(timer.ElapsedMilliseconds < elapsedTimeForLongJob);
        }


        [Test]
        public void Crawl_ShouldCrawlPageReturnsFalse_DoesNotFireEvents()
        {
            //Have to use a stub inheriting from WebCrawler and override
            WebCrawlerTestWrapper testWrapper = new WebCrawlerTestWrapper();

            int _pageCrawlStartingCount = 0;
            int _pageCrawlCompletedCount = 0;
            testWrapper.PageCrawlCompleted += (s, e) => ++_pageCrawlCompletedCount;
            testWrapper.PageCrawlStarting += (s, e) => ++_pageCrawlStartingCount;

            testWrapper.Crawl(_rootUri);
            System.Threading.Thread.Sleep(100);//sleep since the events are async and may not complete before returning

            Assert.AreEqual(0, _pageCrawlStartingCount);
            Assert.AreEqual(0, _pageCrawlCompletedCount);
        }


        private void ThrowExceptionWhen_PageCrawlStarting(object sender, PageCrawlStartingArgs e)
        {
            throw new Exception("no!!!");
        }

        private void ThrowExceptionWhen_PageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            throw new Exception("Oh No!");
        }

/*

        [SetUp]
        public void SetUp()
        {
            _realThreadManager = new ThreadManager(10);
            _realThrottler = new SingleDomainThrottler(_validUri);

            //Create mock objects
            _fakeHttpRequester = new Mock<IHttpRequester>();
            _fakeHtmlParser = new Mock<IHtmlParser>();
            _fakeScheduler = new Mock<IScheduler>();

            _fakePreCrawlRule1 = new Mock<IPreCrawlRule>();
            _fakePreCrawlRule2 = new Mock<IPreCrawlRule>();
            _fakePreScheduleLinksRule1 = new Mock<IPreScheduleLinksRule>();
            _fakePreScheduleLinksRule2 = new Mock<IPreScheduleLinksRule>();

            //Create crawl CrawlComponents wrapper
            Factory.Container.RegisterInstance<IHttpRequester>(_fakeHttpRequester.Object);
            Factory.Container.RegisterInstance<IHtmlParser>(_fakeHtmlParser.Object);
            Factory.Container.RegisterInstance<IScheduler>(_fakeScheduler.Object);
            Factory.Container.RegisterInstance<IThreadManager>(_realThreadManager);
            Factory.Container.RegisterInstance<IThrottler>(_realThrottler);

            InitializeUnitUnderTest();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructer_NullUriToCrawl()
        {
            new CrawlEngine(null);
        }

        [Test]
        public void Crawl_SinglePageWithoutHyperlinks_SingleThreaded_CrawlComponentsAndPluginsAreCalledAsExpected()
        {
            Factory.Container.RegisterInstance<IThreadManager>(new ThreadManager(1));
            MockSinglePageWithoutHyperlinksScenario();

            _unitUnderTest.Crawl();

            VerifySingePageWithoutHyperlinksScenario();
        }

        [Test]
        public void Crawl_SinglePageWithoutHyperlinks_MultiThreaded_CrawlComponentsAndPluginsAreCalledAsExpected()
        {
            MockSinglePageWithoutHyperlinksScenario();

            _unitUnderTest.Crawl();

            VerifySingePageWithoutHyperlinksScenario();
        }

        [Test]
        public void Crawl_SinglePageWithHyperlinks_SingleThreaded_CrawlComponentsAndPluginsAreCalledAsExpected()
        {
            Uri secondUri = new Uri("http://a.com/a");
            Factory.Container.RegisterInstance<IThreadManager>(new ThreadManager(1));
            MockSinglePageWithHyperlinksScenario(_validUri, secondUri);

            _unitUnderTest.Crawl();

            VerifySinglePageWithHyperlinksScenario(_validUri, secondUri);
        }

        [Test]
        public void Crawl_SinglePageWithHyperlinks_MultiThreaded_CrawlComponentsAndPluginsAreCalledAsExpected()
        {
            Uri secondUri = new Uri("http://a.com/a");
            MockSinglePageWithHyperlinksScenario(_validUri, secondUri);

            _unitUnderTest.Crawl();

            VerifySinglePageWithHyperlinksScenario(_validUri, secondUri);
        }

        [Test]
        public void Crawl_FirstPreCrawlRuleFails_CrawlComponentsAndPluginsAreCalledAsExpected()
        {
            Factory.Container.RegisterInstance<IScheduler>(new FifoScheduler());
            InitializeUnitUnderTest();

            _unitUnderTest.Crawl();

            _fakeScheduler.Verify(f => f.Add(It.IsAny<PageToCrawl>()), Times.Never());//We added a real scheduler so the count loop would correctly decrement
            _fakeHttpRequester.Verify(f => f.MakeHttpWebRequest(It.IsAny<Uri>()), Times.Never());
            _fakeHtmlParser.Verify(f => f.GetHyperLinks(), Times.Never());
            _fakePreCrawlRule1.Verify(f => f.IsObeyed(It.IsAny<PageToCrawl>()), Times.Once());//only thing called
            _fakePreCrawlRule2.Verify(f => f.IsObeyed(It.IsAny<PageToCrawl>()), Times.Never());
            _fakePreScheduleLinksRule1.Verify(f => f.IsObeyed(It.IsAny<CrawledPage>()), Times.Never());
            _fakePreScheduleLinksRule2.Verify(f => f.IsObeyed(It.IsAny<CrawledPage>()), Times.Never());
            Assert.AreEqual(1, _pageCrawlStartingCount);
            Assert.AreEqual(0, _pageCrawlCompletedCount);
        }

        [Test]
        public void Crawl_FirstPreCrawlRuleThrowsException_CrawlComponentsAndPluginsAreCalledAsExpected()
        {
            Factory.Container.RegisterInstance<IScheduler>(new FifoScheduler());
            InitializeUnitUnderTest();
            _fakePreCrawlRule1.Setup(f => f.IsObeyed(It.IsAny<PageToCrawl>())).Throws(new Exception("oh no"));

            _unitUnderTest.Crawl();

            _fakeScheduler.Verify(f => f.Add(It.IsAny<PageToCrawl>()), Times.Never());//We added a real scheduler so the count loop would correctly decrement
            _fakeHttpRequester.Verify(f => f.MakeHttpWebRequest(It.IsAny<Uri>()), Times.Never());
            _fakeHtmlParser.Verify(f => f.GetHyperLinks(), Times.Never());
            _fakePreCrawlRule1.Verify(f => f.IsObeyed(It.IsAny<PageToCrawl>()), Times.Once());//only thing called
            _fakePreCrawlRule2.Verify(f => f.IsObeyed(It.IsAny<PageToCrawl>()), Times.Never());//only thing called
            _fakePreScheduleLinksRule1.Verify(f => f.IsObeyed(It.IsAny<CrawledPage>()), Times.Never());
            _fakePreScheduleLinksRule2.Verify(f => f.IsObeyed(It.IsAny<CrawledPage>()), Times.Never());
            Assert.AreEqual(1, _pageCrawlStartingCount);
            Assert.AreEqual(0, _pageCrawlCompletedCount);
        }

        [Test]
        public void Crawl_SecondPreCrawlRuleFails_CrawlComponentsAndPluginsAreCalledAsExpected()
        {
            Factory.Container.RegisterInstance<IScheduler>(new FifoScheduler());
            InitializeUnitUnderTest();
            _fakePreCrawlRule1.Setup(f => f.IsObeyed(It.IsAny<PageToCrawl>())).Returns(true);
            _fakePreCrawlRule2.Setup(f => f.IsObeyed(It.IsAny<PageToCrawl>())).Returns(false);

            _unitUnderTest.Crawl();

            _fakeScheduler.Verify(f => f.Add(It.IsAny<PageToCrawl>()), Times.Never());//We added a real scheduler so the count loop would correctly decrement
            _fakeHttpRequester.Verify(f => f.MakeHttpWebRequest(It.IsAny<Uri>()), Times.Never());
            _fakeHtmlParser.Verify(f => f.GetHyperLinks(), Times.Never());
            _fakePreCrawlRule1.Verify(f => f.IsObeyed(It.IsAny<PageToCrawl>()), Times.Once());
            _fakePreCrawlRule2.Verify(f => f.IsObeyed(It.IsAny<PageToCrawl>()), Times.Once());
            _fakePreScheduleLinksRule1.Verify(f => f.IsObeyed(It.IsAny<CrawledPage>()), Times.Never());
            _fakePreScheduleLinksRule2.Verify(f => f.IsObeyed(It.IsAny<CrawledPage>()), Times.Never());
            Assert.AreEqual(1, _pageCrawlStartingCount);
            Assert.AreEqual(0, _pageCrawlCompletedCount);
        }

        [Test]
        public void Crawl_SecondPreCrawlRuleThrowsException_CrawlComponentsAndPluginsAreCalledAsExpected()
        {
            Factory.Container.RegisterInstance<IScheduler>(new FifoScheduler());
            InitializeUnitUnderTest();
            _fakePreCrawlRule1.Setup(f => f.IsObeyed(It.IsAny<PageToCrawl>())).Returns(true);
            _fakePreCrawlRule2.Setup(f => f.IsObeyed(It.IsAny<PageToCrawl>())).Throws(new Exception("oh no"));

            _unitUnderTest.Crawl();

            _fakeScheduler.Verify(f => f.Add(It.IsAny<PageToCrawl>()), Times.Never());//We added a real scheduler so the count loop would correctly decrement
            _fakeHttpRequester.Verify(f => f.MakeHttpWebRequest(It.IsAny<Uri>()), Times.Never());
            _fakeHtmlParser.Verify(f => f.GetHyperLinks(), Times.Never());
            _fakePreCrawlRule1.Verify(f => f.IsObeyed(It.IsAny<PageToCrawl>()), Times.Once());//only thing called
            _fakePreCrawlRule2.Verify(f => f.IsObeyed(It.IsAny<PageToCrawl>()), Times.Once());//only thing called
            _fakePreScheduleLinksRule1.Verify(f => f.IsObeyed(It.IsAny<CrawledPage>()), Times.Never());
            _fakePreScheduleLinksRule2.Verify(f => f.IsObeyed(It.IsAny<CrawledPage>()), Times.Never());
            Assert.AreEqual(1, _pageCrawlStartingCount);
            Assert.AreEqual(0, _pageCrawlCompletedCount);
        }

        [Test]
        public void Crawl_FirstPreScheduleLinksRuleFails_CrawlComponentsAndPluginsAreCalledAsExpected()
        {
            Factory.Container.RegisterInstance<IScheduler>(new FifoScheduler());
            InitializeUnitUnderTest();
            _fakeHttpRequester.Setup(f => f.MakeHttpWebRequest(It.IsAny<Uri>())).Returns(new CrawledPage(_validUri));
            _fakePreCrawlRule1.Setup(f => f.IsObeyed(It.IsAny<PageToCrawl>())).Returns(true);//must have this pass or we will never get to the pre schedule rules
            _fakePreCrawlRule2.Setup(f => f.IsObeyed(It.IsAny<PageToCrawl>())).Returns(true);//must have this pass or we will never get to the pre schedule rules

            _unitUnderTest.Crawl();

            _fakeScheduler.Verify(f => f.Add(It.IsAny<PageToCrawl>()), Times.Never());//We added a real scheduler so the count loop would correctly decrement
            _fakeHttpRequester.Verify(f => f.MakeHttpWebRequest(It.IsAny<Uri>()), Times.Once());
            _fakeHtmlParser.Verify(f => f.GetHyperLinks(), Times.Never());//Never called since rule failed
            _fakePreCrawlRule1.Verify(f => f.IsObeyed(It.IsAny<PageToCrawl>()), Times.Once());
            _fakePreCrawlRule2.Verify(f => f.IsObeyed(It.IsAny<PageToCrawl>()), Times.Once());
            _fakePreScheduleLinksRule1.Verify(f => f.IsObeyed(It.IsAny<CrawledPage>()), Times.Once());
            _fakePreScheduleLinksRule2.Verify(f => f.IsObeyed(It.IsAny<CrawledPage>()), Times.Never());
            Assert.AreEqual(1, _pageCrawlStartingCount);
            Assert.AreEqual(1, _pageCrawlCompletedCount);
        }

        [Test]
        public void Crawl_FistPreScheduleLinksRuleThrowsException_CrawlComponentsAndPluginsAreCalledAsExpected()
        {
            Factory.Container.RegisterInstance<IScheduler>(new FifoScheduler());
            InitializeUnitUnderTest();
            _fakeHttpRequester.Setup(f => f.MakeHttpWebRequest(It.IsAny<Uri>())).Returns(new CrawledPage(_validUri));
            _fakePreCrawlRule1.Setup(f => f.IsObeyed(It.IsAny<PageToCrawl>())).Returns(true);//must have this pass or we will never get to the pre schedule rules
            _fakePreCrawlRule2.Setup(f => f.IsObeyed(It.IsAny<PageToCrawl>())).Returns(true);//must have this pass or we will never get to the pre schedule rules
            _fakePreScheduleLinksRule1.Setup(f => f.IsObeyed(It.IsAny<CrawledPage>())).Throws(new Exception("oh no"));

            _unitUnderTest.Crawl();

            _fakeScheduler.Verify(f => f.Add(It.IsAny<PageToCrawl>()), Times.Never());//We added a real scheduler so the count loop would correctly decrement
            _fakeHttpRequester.Verify(f => f.MakeHttpWebRequest(It.IsAny<Uri>()), Times.Once());
            _fakeHtmlParser.Verify(f => f.GetHyperLinks(), Times.Never());//Never called since rule failed because of exception
            _fakePreCrawlRule1.Verify(f => f.IsObeyed(It.IsAny<PageToCrawl>()), Times.Once());
            _fakePreCrawlRule2.Verify(f => f.IsObeyed(It.IsAny<PageToCrawl>()), Times.Once());
            _fakePreScheduleLinksRule1.Verify(f => f.IsObeyed(It.IsAny<CrawledPage>()), Times.Once());
            _fakePreScheduleLinksRule2.Verify(f => f.IsObeyed(It.IsAny<CrawledPage>()), Times.Never());
            Assert.AreEqual(1, _pageCrawlStartingCount);
            Assert.AreEqual(1, _pageCrawlCompletedCount);
        }

        [Test]
        public void Crawl_SecondPreScheduleLinksRuleFails_CrawlComponentsAndPluginsAreCalledAsExpected()
        {
            Factory.Container.RegisterInstance<IScheduler>(new FifoScheduler());
            InitializeUnitUnderTest();
            _fakeHttpRequester.Setup(f => f.MakeHttpWebRequest(It.IsAny<Uri>())).Returns(new CrawledPage(_validUri));
            _fakePreCrawlRule1.Setup(f => f.IsObeyed(It.IsAny<PageToCrawl>())).Returns(true);//must have this pass or we will never get to the pre schedule rules
            _fakePreCrawlRule2.Setup(f => f.IsObeyed(It.IsAny<PageToCrawl>())).Returns(true);//must have this pass or we will never get to the pre schedule rules
            _fakePreScheduleLinksRule1.Setup(f => f.IsObeyed(It.IsAny<CrawledPage>())).Returns(true);
            _fakePreScheduleLinksRule2.Setup(f => f.IsObeyed(It.IsAny<CrawledPage>())).Returns(false);

            _unitUnderTest.Crawl();

            _fakeScheduler.Verify(f => f.Add(It.IsAny<PageToCrawl>()), Times.Never());//We added a real scheduler so the count loop would correctly decrement
            _fakeHttpRequester.Verify(f => f.MakeHttpWebRequest(It.IsAny<Uri>()), Times.Once());
            _fakeHtmlParser.Verify(f => f.GetHyperLinks(), Times.Never());//Never called since rule failed
            _fakePreCrawlRule1.Verify(f => f.IsObeyed(It.IsAny<PageToCrawl>()), Times.Once());
            _fakePreCrawlRule2.Verify(f => f.IsObeyed(It.IsAny<PageToCrawl>()), Times.Once());
            _fakePreScheduleLinksRule1.Verify(f => f.IsObeyed(It.IsAny<CrawledPage>()), Times.Once());
            _fakePreScheduleLinksRule2.Verify(f => f.IsObeyed(It.IsAny<CrawledPage>()), Times.Once());
            Assert.AreEqual(1, _pageCrawlStartingCount);
            Assert.AreEqual(1, _pageCrawlCompletedCount);
        }

        [Test]
        public void Crawl_SecondPreScheduleLinksRuleThrowsException_CrawlComponentsAndPluginsAreCalledAsExpected()
        {
            Factory.Container.RegisterInstance<IScheduler>(new FifoScheduler());
            InitializeUnitUnderTest();
            _fakeHttpRequester.Setup(f => f.MakeHttpWebRequest(It.IsAny<Uri>())).Returns(new CrawledPage(_validUri));
            _fakePreCrawlRule1.Setup(f => f.IsObeyed(It.IsAny<PageToCrawl>())).Returns(true);//must have this pass or we will never get to the pre schedule rules
            _fakePreCrawlRule2.Setup(f => f.IsObeyed(It.IsAny<PageToCrawl>())).Returns(true);//must have this pass or we will never get to the pre schedule rules
            _fakePreScheduleLinksRule1.Setup(f => f.IsObeyed(It.IsAny<CrawledPage>())).Returns(true);
            _fakePreScheduleLinksRule2.Setup(f => f.IsObeyed(It.IsAny<CrawledPage>())).Throws(new Exception("oh no"));

            _unitUnderTest.Crawl();

            _fakeScheduler.Verify(f => f.Add(It.IsAny<PageToCrawl>()), Times.Never());//We added a real scheduler so the count loop would correctly decrement
            _fakeHttpRequester.Verify(f => f.MakeHttpWebRequest(It.IsAny<Uri>()), Times.Once());
            _fakeHtmlParser.Verify(f => f.GetHyperLinks(), Times.Never());//Never called since rule failed because of exception
            _fakePreCrawlRule1.Verify(f => f.IsObeyed(It.IsAny<PageToCrawl>()), Times.Once());
            _fakePreCrawlRule2.Verify(f => f.IsObeyed(It.IsAny<PageToCrawl>()), Times.Once());
            _fakePreScheduleLinksRule1.Verify(f => f.IsObeyed(It.IsAny<CrawledPage>()), Times.Once());
            _fakePreScheduleLinksRule1.Verify(f => f.IsObeyed(It.IsAny<CrawledPage>()), Times.Once());
            Assert.AreEqual(1, _pageCrawlStartingCount);
            Assert.AreEqual(1, _pageCrawlCompletedCount);
        }

        [Test]
        public void Crawl_PageCrawlStartingEventSubscriberThrowsException_DoesNotCrash()
        {
            Factory.Container.RegisterInstance<IScheduler>(new FifoScheduler());
            InitializeUnitUnderTest();
            _fakeHttpRequester.Setup(f => f.MakeHttpWebRequest(It.IsAny<Uri>())).Returns(new CrawledPage(_validUri));
            _fakeHtmlParser.Setup(f => f.GetHyperLinks()).Returns(new List<Uri>());
            _fakePreCrawlRule1.Setup(f => f.IsObeyed(It.IsAny<PageToCrawl>())).Returns(true);
            _fakePreCrawlRule2.Setup(f => f.IsObeyed(It.IsAny<PageToCrawl>())).Returns(true);
            _fakePreScheduleLinksRule1.Setup(f => f.IsObeyed(It.IsAny<CrawledPage>())).Returns(true);
            _fakePreScheduleLinksRule2.Setup(f => f.IsObeyed(It.IsAny<CrawledPage>())).Returns(true);
            _unitUnderTest.PageCrawlStarting += new EventHandler<PageCrawlStartingArgs>(ThrowExceptionWhen_PageCrawlStarting);

            _unitUnderTest.Crawl();

            Assert.AreEqual(1, _pageCrawlStartingCount);
            Assert.AreEqual(1, _pageCrawlCompletedCount);
        }

        [Test]
        public void Crawl_PageCrawlStartingEvent_IsAsynchronous()
        {
            int elapsedTimeForLongJob = 5000;
            Factory.Container.RegisterInstance<IScheduler>(new FifoScheduler());
            InitializeUnitUnderTest();
            _fakeHttpRequester.Setup(f => f.MakeHttpWebRequest(It.IsAny<Uri>())).Returns(new CrawledPage(_validUri));
            _fakeHtmlParser.Setup(f => f.GetHyperLinks()).Returns(new List<Uri>());
            _fakePreCrawlRule1.Setup(f => f.IsObeyed(It.IsAny<PageToCrawl>())).Returns(true);
            _fakePreCrawlRule2.Setup(f => f.IsObeyed(It.IsAny<PageToCrawl>())).Returns(true);
            _fakePreScheduleLinksRule1.Setup(f => f.IsObeyed(It.IsAny<CrawledPage>())).Returns(true);
            _fakePreScheduleLinksRule2.Setup(f => f.IsObeyed(It.IsAny<CrawledPage>())).Returns(true);
            _unitUnderTest.PageCrawlStarting += new EventHandler<PageCrawlStartingArgs>((sender, args) => System.Threading.Thread.Sleep(elapsedTimeForLongJob));

            Stopwatch timer = Stopwatch.StartNew();
            _unitUnderTest.Crawl();
            timer.Stop();
            Assert.IsTrue(timer.ElapsedMilliseconds < elapsedTimeForLongJob);
        }

        [Test]
        public void Crawl_PageCrawlCompletedEventSubscriberThrowsException_DoesNotCrash()
        {
            Factory.Container.RegisterInstance<IScheduler>(new FifoScheduler());
            InitializeUnitUnderTest();
            _fakeHttpRequester.Setup(f => f.MakeHttpWebRequest(It.IsAny<Uri>())).Returns(new CrawledPage(_validUri));
            _fakeHtmlParser.Setup(f => f.GetHyperLinks()).Returns(new List<Uri>());
            _fakePreCrawlRule1.Setup(f => f.IsObeyed(It.IsAny<PageToCrawl>())).Returns(true);
            _fakePreCrawlRule2.Setup(f => f.IsObeyed(It.IsAny<PageToCrawl>())).Returns(true);
            _fakePreScheduleLinksRule1.Setup(f => f.IsObeyed(It.IsAny<CrawledPage>())).Returns(true);
            _fakePreScheduleLinksRule2.Setup(f => f.IsObeyed(It.IsAny<CrawledPage>())).Returns(true);
            _unitUnderTest.PageCrawlCompleted += new EventHandler<PageCrawlCompletedArgs>(ThrowExceptionWhen_PageCrawlCompleted);

            _unitUnderTest.Crawl();

            Assert.AreEqual(1, _pageCrawlStartingCount);
            Assert.AreEqual(1, _pageCrawlCompletedCount);
        }

        [Test]
        public void Crawl_PageCrawlCompletedEvent_IsAsynchronous()
        {
            int elapsedTimeForLongJob = 5000;
            Factory.Container.RegisterInstance<IScheduler>(new FifoScheduler());
            InitializeUnitUnderTest();
            _fakeHttpRequester.Setup(f => f.MakeHttpWebRequest(It.IsAny<Uri>())).Returns(new CrawledPage(_validUri));
            _fakeHtmlParser.Setup(f => f.GetHyperLinks()).Returns(new List<Uri>());
            _fakePreCrawlRule1.Setup(f => f.IsObeyed(It.IsAny<PageToCrawl>())).Returns(true);
            _fakePreCrawlRule2.Setup(f => f.IsObeyed(It.IsAny<PageToCrawl>())).Returns(true);
            _fakePreScheduleLinksRule1.Setup(f => f.IsObeyed(It.IsAny<CrawledPage>())).Returns(true);
            _fakePreScheduleLinksRule2.Setup(f => f.IsObeyed(It.IsAny<CrawledPage>())).Returns(true);
            _unitUnderTest.PageCrawlCompleted += new EventHandler<PageCrawlCompletedArgs>((sender, args) => System.Threading.Thread.Sleep(elapsedTimeForLongJob));

            Stopwatch timer = Stopwatch.StartNew();
            _unitUnderTest.Crawl();
            timer.Stop();
            Assert.IsTrue(timer.ElapsedMilliseconds < elapsedTimeForLongJob);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Crawl_CalledTwiceOnSameInstance()
        {
            Factory.Container.RegisterInstance<IScheduler>(new FifoScheduler());
            InitializeUnitUnderTest();
            _unitUnderTest.Crawl();
            _unitUnderTest.Crawl();
        }

        [Test]
        public void Crawl_ThrottlingEnabled_CallsThrottler()
        {
            Mock<IThrottler> fakeThrottler = new Mock<IThrottler>();
            fakeThrottler.Setup(f => f.GetThrottledPages()).Returns(new List<CrawledPage>() { new CrawledPage(_validUri) });
            Factory.Container.RegisterInstance<IThrottler>(fakeThrottler.Object);
            InitializeUnitUnderTest();
            _unitUnderTest.IsThrottlingEnabled = true;

            _unitUnderTest.Crawl();

            fakeThrottler.Verify(f => f.HasThrottledPages());
        }

        [Test]
        public void Crawl_CrawlDelaySet_DisablesMultiThreadingAndThrottling()
        {
            Factory.Container.RegisterType<IThreadManager, ThreadManager>(new InjectionConstructor(10));
            Factory.Container.RegisterInstance<IScheduler>(new FifoScheduler());
            InitializeUnitUnderTest();
            _unitUnderTest.CrawlDelay = 2000;
            _unitUnderTest.IsThrottlingEnabled = true;
            _fakeHttpRequester.Setup(f => f.MakeHttpWebRequest(It.IsAny<Uri>())).Returns(new CrawledPage(_validUri));
            _fakeHtmlParser.Setup(f => f.GetHyperLinks()).Returns(new List<Uri>());

            _unitUnderTest.Crawl();

            Assert.IsFalse(_unitUnderTest.IsThrottlingEnabled);
            //Assert.AreEqual(1, realThreadManager.MaxThreads);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Crawl_ManualCrawlDelaySetBelowZero()
        {
            _unitUnderTest.CrawlDelay = -1;
            _unitUnderTest.Crawl();
        }

        private void MockSinglePageWithoutHyperlinksScenario()
        {
            Factory.Container.RegisterInstance<IScheduler>(new FifoScheduler());
            InitializeUnitUnderTest();

            _fakeHttpRequester.Setup(f => f.MakeHttpWebRequest(It.IsAny<Uri>())).Returns(new CrawledPage(_validUri));
            _fakeHtmlParser.Setup(f => f.GetHyperLinks()).Returns(new List<Uri>());
            _fakePreCrawlRule1.Setup(f => f.IsObeyed(It.IsAny<PageToCrawl>())).Returns(true);
            _fakePreCrawlRule2.Setup(f => f.IsObeyed(It.IsAny<PageToCrawl>())).Returns(true);
            _fakePreScheduleLinksRule1.Setup(f => f.IsObeyed(It.IsAny<CrawledPage>())).Returns(true);
            _fakePreScheduleLinksRule2.Setup(f => f.IsObeyed(It.IsAny<CrawledPage>())).Returns(true);
        }

        private void MockSinglePageWithHyperlinksScenario(Uri firstUri, Uri secondUri)
        {
            Factory.Container.RegisterInstance<IScheduler>(new FifoScheduler());
            InitializeUnitUnderTest();

            Queue<List<Uri>> queueOfLinksToReturn = new Queue<List<Uri>>();
            queueOfLinksToReturn.Enqueue(new List<Uri> { secondUri });
            queueOfLinksToReturn.Enqueue(new List<Uri>());

            _fakeHttpRequester.Setup(f => f.MakeHttpWebRequest(firstUri)).Returns(new CrawledPage(firstUri));
            _fakeHttpRequester.Setup(f => f.MakeHttpWebRequest(secondUri)).Returns(new CrawledPage(secondUri));
            _fakeHtmlParser.Setup(f => f.GetHyperLinks()).Returns(() => queueOfLinksToReturn.Dequeue());
            _fakePreCrawlRule1.Setup(f => f.IsObeyed(It.IsAny<PageToCrawl>())).Returns(true);
            _fakePreCrawlRule2.Setup(f => f.IsObeyed(It.IsAny<PageToCrawl>())).Returns(true);
            _fakePreScheduleLinksRule1.Setup(f => f.IsObeyed(It.IsAny<CrawledPage>())).Returns(true);
            _fakePreScheduleLinksRule2.Setup(f => f.IsObeyed(It.IsAny<CrawledPage>())).Returns(true);
        }

        private void VerifySingePageWithoutHyperlinksScenario()
        {
            _fakeScheduler.Verify(f => f.Add(It.IsAny<PageToCrawl>()), Times.Never());//We added a real scheduler so the count loop would correctly decrement
            _fakeHttpRequester.Verify(f => f.MakeHttpWebRequest(It.IsAny<Uri>()), Times.Once());
            _fakeHtmlParser.Verify(f => f.GetHyperLinks(), Times.Once());
            _fakePreCrawlRule1.Verify(f => f.IsObeyed(It.IsAny<PageToCrawl>()), Times.Once());
            _fakePreCrawlRule2.Verify(f => f.IsObeyed(It.IsAny<PageToCrawl>()), Times.Once());
            _fakePreScheduleLinksRule1.Verify(f => f.IsObeyed(It.IsAny<CrawledPage>()), Times.Once());
            _fakePreScheduleLinksRule2.Verify(f => f.IsObeyed(It.IsAny<CrawledPage>()), Times.Once());
            Assert.AreEqual(1, _pageCrawlCompletedCount);
            Assert.AreEqual(1, _pageCrawlStartingCount);
        }

        private void VerifySinglePageWithHyperlinksScenario(Uri firstUri, Uri secondUri)
        {
            _fakeScheduler.Verify(f => f.Add(It.IsAny<PageToCrawl>()), Times.Never());//We added a real scheduler so the count loop would correctly decrement
            _fakeHttpRequester.Verify(f => f.MakeHttpWebRequest(firstUri), Times.Once());
            _fakeHttpRequester.Verify(f => f.MakeHttpWebRequest(secondUri), Times.Once());
            _fakeHtmlParser.Verify(f => f.GetHyperLinks(), Times.Exactly(2));
            _fakePreCrawlRule1.Verify(f => f.IsObeyed(It.IsAny<PageToCrawl>()), Times.Exactly(2));
            _fakePreScheduleLinksRule1.Verify(f => f.IsObeyed(It.IsAny<CrawledPage>()), Times.Exactly(2));
            Assert.AreEqual(2, _pageCrawlStartingCount);
            Assert.AreEqual(2, _pageCrawlCompletedCount);
        }

        private void ThrowExceptionWhen_PageCrawlStarting(object sender, PageCrawlStartingArgs e)
        {
            throw new Exception("no!!!");
        }

        private void ThrowExceptionWhen_PageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            throw new Exception("Oh No!");
        }

*/
    }

    internal class WebCrawlerTestWrapper : WebCrawler
    {
        public WebCrawlerTestWrapper()
        {
        }

        protected override bool ShouldCrawlPage(PageToCrawl pageToCrawl)
        {
            return false;
        }
    }
}
