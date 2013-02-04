using Abot.Core;
using Abot.Poco;
using NUnit.Framework;
using System;

namespace Abot.Tests.Unit.Core
{
    [TestFixture]
    public class CsQueryHyperLinkParserTest : HyperLinkParserTest
    {
        protected override HyperLinkParser GetInstance()
        {
            return new CSQueryHyperlinkParser();
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "CrawledPage.CsQueryDocument is null. Be sure to set the config value ShouldLoadHtmlAgilityPackForEachCrawledPage to true when using this HyperlinkParser.")]
        public void GetLinks_CsQueryDocumentIsNull()
        {
            CrawledPage page = new CrawledPage(new Uri("http://a.com")){ CsQueryDocument = null };

            GetInstance().GetLinks(page);
        }
    }
}
