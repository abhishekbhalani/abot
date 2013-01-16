using Abot.Poco;
using NUnit.Framework;
using System;

namespace Abot.Tests.Unit.Poco
{
    [TestFixture]
    public class CrawledResultTest
    {
        [Test]
        public void Constructor_ValidUri_CreatesInstance()
        {
            CrawlResult unitUnderTest = new CrawlResult();
            Assert.AreEqual(default(TimeSpan), unitUnderTest.Elapsed);
            Assert.AreEqual("", unitUnderTest.ErrorMessage);
            Assert.AreEqual(false, unitUnderTest.ErrorOccurred);
            Assert.AreEqual(null, unitUnderTest.RootUri);
            Assert.AreEqual(null, unitUnderTest.CrawlContext);
        }
    }
}
