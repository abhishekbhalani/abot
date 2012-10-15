using Abot.Poco;
using NUnit.Framework;
using System;

namespace Abot.Tests.Poco
{
    [TestFixture]
    public class CrawlDecisionTest
    {
        [Test]
        public void Constructor_ValidUri_CreatesInstance()
        {
            CrawlDecision unitUnderTest = new CrawlDecision();
            Assert.AreEqual(false, unitUnderTest.Should);
            Assert.AreEqual("", unitUnderTest.Reason);
        }
    }
}
