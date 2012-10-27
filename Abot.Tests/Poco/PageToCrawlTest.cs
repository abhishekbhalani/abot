using Abot.Poco;
using NUnit.Framework;
using System;

namespace Abot.Tests.Poco
{
    [TestFixture]
    public class PageToCrawlTest
    {
        [Test]
        public void Constructor_ValidUri_CreatesInstance()
        {
            PageToCrawl unitUnderTest = new PageToCrawl(new Uri("http://a.com/"));
            Assert.AreEqual(false, unitUnderTest.IsRetry);
            Assert.AreEqual(null, unitUnderTest.ParentUri);
            Assert.AreEqual("http://a.com/", unitUnderTest.Uri.AbsoluteUri);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_InvalidUri()
        {
            new PageToCrawl(null);
        }
    }
}
