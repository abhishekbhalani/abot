using Abot.Core;
using Abot.Poco;
using NUnit.Framework;
using System;

namespace Abot.Tests.Unit.Poco
{
    [TestFixture]
    public class CrawledPageTest
    {
        [Test]
        public void Constructor_ValidUri_CreatesInstance()
        {
            CrawledPage unitUnderTest = new CrawledPage(new Uri("http://a.com/"));
            Assert.AreEqual(null, unitUnderTest.HttpWebRequest);
            Assert.AreEqual(null, unitUnderTest.HttpWebResponse);
            Assert.AreEqual(false, unitUnderTest.IsRetry);
            Assert.AreEqual(null, unitUnderTest.ParentUri);
            Assert.AreEqual("", unitUnderTest.RawContent);
            Assert.IsNotNull(unitUnderTest.HtmlDocument);
            Assert.IsNotNull(unitUnderTest.CsQueryDocument);
            Assert.AreEqual("http://a.com/", unitUnderTest.Uri.AbsoluteUri);
            Assert.AreEqual(null, unitUnderTest.WebException);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_InvalidUri()
        {
            new CrawledPage(null);
        }

        [Test]
        public void HtmlDocument_RawContentIsNull_HtmlDocumentIsNotNull()
        {
            CrawledPage unitUnderTest = new CrawledPage(new Uri("http://a.com/")) { RawContent = null };

            Assert.IsNotNull(unitUnderTest.HtmlDocument);
        }

        [Test]
        public void CsQueryDocument_RawContentIsNull_CsQueryDocumentIsNotNull()
        {
            CrawledPage unitUnderTest = new CrawledPage(new Uri("http://a.com/")) { RawContent = null };

            Assert.IsNotNull(unitUnderTest.CsQueryDocument);
        }

        [Test]
        public void CsQuery_EncodingChangedTwice_DoesNotCrash()
        {
            CrawledPage unitUnderTest = new CrawledPage(new Uri("http://a.com/")) { RawContent = @"<meta http-equiv=""Content-Type"" content=""text/html; charset=iso-8859-1""><meta http-equiv=""content-type"" content=""text/html; charset=utf-8"" />" };

            Assert.IsNotNull(unitUnderTest.CsQueryDocument);
        }

        [Test]
        public void ToString_HttpResponseDoesNotExists_MessageHasUri()
        {
            Assert.AreEqual("http://localhost:1111/", new CrawledPage(new Uri("http://localhost:1111/")).ToString());
        }

        [Test]
        public void ToString_HttpResponseExists_MessageHasUriAndStatus()
        {
            Assert.AreEqual("http://localhost:1111/[200]", new PageRequester("someuseragent").MakeRequest(new Uri("http://localhost:1111/")).ToString());
        }
    }
}
