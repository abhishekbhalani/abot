using Abot;
using Moq;
using NUnit.Framework;

namespace Abot.Tests
{
    [TestFixture]
    public class CrawlerBasicTest
    {
        WebCrawler _unitUnderTest;
        //Mock<IThreadManager> _fakeThreadManager;
        //Mock<IScheduler>() _fakeScheduler; 
        //Mock<IHtmlParser>() _fakeHtmlParser;
        //Mock<IHttpRequester>() _fakeHttpRequester;

        [Test]
        public void Crawl()
        {
            _unitUnderTest = new WebCrawler(new Mock<IThreadManager>(), new Mock<IScheduler>(), new Mock<IHtmlParser>(), new Mock<IHttpRequester>());
        }
    }
}
