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
            Assert.AreEqual(false, unitUnderTest.Allow);
            Assert.AreEqual("", unitUnderTest.Reason);
        }
    }
}
