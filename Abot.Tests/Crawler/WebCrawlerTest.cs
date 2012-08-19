using Abot.Crawler;
using Moq;
using NUnit.Framework;
using System;

namespace Abot.Tests.Crawler
{
    [TestFixture]
    public class CrawlerBasicTest
    {
        WebCrawler _unitUnderTest;
        Mock<IThreadManager> _fakeThreadManager;
        Mock<IScheduler> _fakeScheduler; 
        Mock<IPageRequester> _fakeHttpRequester;
        Mock<IHyperLinkParser> _fakeHyperLinkParser;
        Uri _rootUri;

        [SetUp]
        public void SetUp()
        {
            _fakeThreadManager = new Mock<IThreadManager>();
            _fakeScheduler = new Mock<IScheduler>();
            _fakeHyperLinkParser = new Mock<IHyperLinkParser>();
            _fakeHttpRequester = new Mock<IPageRequester>();

            _unitUnderTest = new WebCrawler(_fakeThreadManager.Object, _fakeScheduler.Object, _fakeHttpRequester.Object, _fakeHyperLinkParser.Object);

            _rootUri = new Uri("http://a.com");
        }

        [Test]
        public void Crawl()
        {   
            //TODO setup mocks so that it returns links on the first request but not on the rest

            _unitUnderTest.Crawl(_rootUri);

            _fakeScheduler.Verify(f => f.Add(It.Is<PageToCrawl>(p => p.Uri == _rootUri)));
        }
    }
}
