using Abot.Core;
using Abot.Poco;
using NUnit.Framework;
using System;

namespace Abot.Tests.Unit.Core
{
    [TestFixture]
    public class HapHyperLinkParserTest : HyperLinkParserTest
    {
        protected override HyperLinkParser GetInstance()
        {
            return new HapHyperLinkParser();
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "CrawledPage.HtmlDocument is null. Be sure to set the config value ShouldLoadCsQueryForEachCrawledPage to true when using this HyperlinkParser.")]
        public void GetLinks_CsQueryDocumentIsNull()
        {
            CrawledPage page = new CrawledPage(new Uri("http://a.com")) { HtmlDocument = null };

            GetInstance().GetLinks(page);
        }
    }
}
