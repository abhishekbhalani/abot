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
