using Abot.Core;
using Moq;
using NUnit.Framework;

namespace Abot.Tests
{
    [TestFixture]
    public class CrawlerBasicTest
    {
        CrawlerBasic _unitUnderTest;
        Mock<IThreadManager> _fakeThreadManager;
        Mock<IScheduler> _fakeScheduler; 
        Mock<IHyperLinkParser> _fakeHyperLinkParser;
        Mock<IHttpRequester> _fakeHttpRequester;

        [SetUp]
        public void SetUp()
        {
            _fakeThreadManager = new Mock<IThreadManager>();
            _fakeScheduler = new Mock<IScheduler>();
            _fakeHyperLinkParser = new Mock<IHyperLinkParser>();
            _fakeHttpRequester = new Mock<IHttpRequester>();

            _unitUnderTest = new CrawlerBasic(_fakeThreadManager.Object,
                _fakeScheduler.Object,
                _fakeHyperLinkParser.Object,
                _fakeHttpRequester.Object);
        }

        [Test]
        public void Crawl()
        {
            //_unitUnderTest.Crawl
        }
    }
}
