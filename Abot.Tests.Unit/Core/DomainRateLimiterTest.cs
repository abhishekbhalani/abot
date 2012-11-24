using Abot.Core;
using NUnit.Framework;
using System;
using System.Diagnostics;

namespace Abot.Tests.Unit.Core
{
    [TestFixture]
    public class DomainRateLimiterTest
    {
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_ZeroCrawlDelay()
        {
            new DomainRateLimiter(0);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_NegativeCrawlDelay()
        {
            new DomainRateLimiter(-1);
        }

        [Test]
        public void RateLimit_SameDomain_WaitsBetweenRequests()
        {
            Uri uri = new Uri("http://a.com/");
            Stopwatch timer = Stopwatch.StartNew();
            DomainRateLimiter unitUnderTest = new DomainRateLimiter(100);
            unitUnderTest.RateLimit(uri);
            unitUnderTest.RateLimit(uri);
            unitUnderTest.RateLimit(uri);
            timer.Stop();

            Assert.IsTrue(timer.ElapsedMilliseconds > 200);
        }

        [Test]
        public void RateLimit_DifferentDomain_DoesNotWaitsBetweenRequests()
        {
            Uri uri1 = new Uri("http://a.com/");
            Uri uri2 = new Uri("http://b.com/");
            Uri uri3 = new Uri("http://c.com/");
            Uri uri4 = new Uri("http://d.com/");

            Stopwatch timer = Stopwatch.StartNew();
            DomainRateLimiter unitUnderTest = new DomainRateLimiter(1000);
            unitUnderTest.RateLimit(uri1);
            unitUnderTest.RateLimit(uri2);
            unitUnderTest.RateLimit(uri3);
            unitUnderTest.RateLimit(uri4);
            timer.Stop();

            Assert.IsTrue(timer.ElapsedMilliseconds < 100);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RateLimit_NullUri()
        {
            new DomainRateLimiter(1000).RateLimit(null);
        }
    }
}
