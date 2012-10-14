using Abot.Crawler;
using Abot.Poco;
using NUnit.Framework;
using System;

namespace Abot.Tests.Crawler
{
    public class PageCrawlDisallowedArgsTest
    {
        PageToCrawl _page = new CrawledPage(new Uri("http://aaa.com/"));

        [Test]
        public void Constructor_ValidReason_SetsPublicProperty()
        {
            string reason = "aaa";
            PageCrawlDisallowedArgs args = new PageCrawlDisallowedArgs(_page, reason);

            Assert.AreSame(reason, args.DisallowedReason);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullReason()
        {
            new PageCrawlDisallowedArgs(_page, null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_EmptyReason()
        {
            new PageCrawlDisallowedArgs(_page, "");
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WhitespaceReason()
        {
            new PageCrawlDisallowedArgs(_page, "   ");
        }
    }
}
