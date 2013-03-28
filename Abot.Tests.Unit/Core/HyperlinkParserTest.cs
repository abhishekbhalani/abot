using Abot.Core;
using Abot.Poco;
using Microsoft.QualityTools.Testing.Fakes;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Fakes;

namespace Abot.Tests.Unit.Core
{
    [TestFixture]
    public abstract class HyperLinkParserTest
    {
        HyperLinkParser _unitUnderTest;
        Uri _uri = new Uri("http://a.com/");

        protected abstract HyperLinkParser GetInstance();

        [SetUp]
        public void Setup()
        {
            _unitUnderTest = GetInstance();
        }

        [Test]
        public void GetLinks_AnchorTags_ReturnsLinks()
        {
            using (ShimsContext.Create())
            {
                CrawledPage crawledPage = new CrawledPage(_uri)
                { 
                    RawContent = "<a href=\"http://aaa.com/\" ></a><a href=\"/aaa/a.html\" /></a>", 
                    HttpWebRequest = new ShimHttpWebRequest { AddressGet = () => _uri }.Instance
                };

                IEnumerable<Uri> result = _unitUnderTest.GetLinks(crawledPage);
            
                Assert.AreEqual(2, result.Count());
                Assert.AreEqual("http://aaa.com/", result.ElementAt(0).AbsoluteUri);
                Assert.AreEqual("http://a.com/aaa/a.html", result.ElementAt(1).AbsoluteUri);
            }
        }

        [Test]
        public void GetLinks_AreaTags_ReturnsLinks()
        {
            using (ShimsContext.Create())
            {
                CrawledPage crawledPage = new CrawledPage(_uri)
                {
                    RawContent = "<area href=\"http://bbb.com\" /><area href=\"bbb/b.html\" />",
                    HttpWebRequest = new ShimHttpWebRequest { AddressGet = () => _uri }.Instance
                };

                IEnumerable<Uri> result = _unitUnderTest.GetLinks(crawledPage);

                Assert.AreEqual(2, result.Count());
                Assert.AreEqual("http://bbb.com/", result.ElementAt(0).AbsoluteUri);
                Assert.AreEqual("http://a.com/bbb/b.html", result.ElementAt(1).AbsoluteUri);
            }
        }

        [Test]
        public void GetLinks_NoLinks_NotReturned()
        {
            using (ShimsContext.Create())
            {
                CrawledPage crawledPage = new CrawledPage(_uri)
                {
                    RawContent = "<html></html>",
                    HttpWebRequest = new ShimHttpWebRequest { AddressGet = () => _uri }.Instance
                };

                IEnumerable<Uri> result = _unitUnderTest.GetLinks(crawledPage);

                Assert.AreEqual(0, result.Count());
            }
        }

        [Test]
        public void GetLinks_AnyScheme_Returned()
        {
            using (ShimsContext.Create())
            {
                CrawledPage crawledPage = new CrawledPage(_uri)
                {
                    RawContent = "<a href=\"mailto:aaa@gmail.com\" /><a href=\"tel:+123456789\" /><a href=\"callto:+123456789\" /><a href=\"ftp://user@yourdomainname.com/\" /><a href=\"file:///C:/Users/\" />",
                    HttpWebRequest = new ShimHttpWebRequest { AddressGet = () => _uri }.Instance
                };

                IEnumerable<Uri> result = _unitUnderTest.GetLinks(crawledPage);

                Assert.AreEqual(5, result.Count());
                Assert.AreEqual("mailto:aaa@gmail.com", result.ElementAt(0).AbsoluteUri);
                Assert.AreEqual("tel:+123456789", result.ElementAt(1).AbsoluteUri);
                Assert.AreEqual("callto:+123456789", result.ElementAt(2).AbsoluteUri);
                Assert.AreEqual("ftp://user@yourdomainname.com/", result.ElementAt(3).AbsoluteUri);
                Assert.AreEqual("file:///C:/Users/", result.ElementAt(4).AbsoluteUri);
            }
        }

        [Test]
		public void GetLinks_InvalidFormatUrl_NotReturned()
		{
            using (ShimsContext.Create())
            {
                CrawledPage crawledPage = new CrawledPage(_uri)
                {
                    RawContent = "<a href=\"http://////\" />",
                    HttpWebRequest = new ShimHttpWebRequest { AddressGet = () => _uri }.Instance
                };

                IEnumerable<Uri> result = _unitUnderTest.GetLinks(crawledPage);

                Assert.AreEqual(0, result.Count());
            }
        }

        [Test]
        public void GetLinks_LinksInComments_NotReturned()
        {
            using (ShimsContext.Create())
            {
                CrawledPage crawledPage = new CrawledPage(_uri)
                {
                    RawContent = @"<html>
                    <head>
                        <!--
                            <a href='http://a1.com' />
                            <area href='http://a2.com' />
                        -->
                    </head>
                    <body>
                        <!--
                            <a href='http://b1.com' />
                            <area href='http://b2.com' />
                        -->
                    </body>
                    </html",
                    HttpWebRequest = new ShimHttpWebRequest { AddressGet = () => _uri }.Instance
                };

                IEnumerable<Uri> result = _unitUnderTest.GetLinks(crawledPage);

                Assert.AreEqual(0, result.Count());
            }
        }

        [Test]
        public void GetLinks_LinksInScript_NotReturned()
        {
            using (ShimsContext.Create())
            {
                CrawledPage crawledPage = new CrawledPage(_uri)
                {
                    RawContent = @"<html>
                    <head>
                        <script>
                            <a href='http://a1.com' />
                            <area href='http://a2.com' />
                        </script>
                    </head>
                    <body>
                        <script>
                            <a href='http://b1.com' />
                            <area href='http://b2.com' />
                        </script>
                    </body>
                    </html",
                    HttpWebRequest = new ShimHttpWebRequest { AddressGet = () => _uri }.Instance
                };

                IEnumerable<Uri> result = _unitUnderTest.GetLinks(crawledPage);

                Assert.AreEqual(0, result.Count());
            }
        }

        [Test]
        public void GetLinks_LinksInStyleTag_NotReturned()
        {
            using (ShimsContext.Create())
            {
                CrawledPage crawledPage = new CrawledPage(_uri)
                {
                    RawContent = @"<html>
                    <head>
                        <style>
                            <a href='http://a1.com' />
                            <area href='http://a2.com' />
                        </style>
                    </head>
                    <body>
                        <style>
                            <a href='http://b1.com' />
                            <area href='http://b2.com' />
                        </style>
                    </body>
                    </html",
                    HttpWebRequest = new ShimHttpWebRequest { AddressGet = () => _uri }.Instance
                };

                IEnumerable<Uri> result = _unitUnderTest.GetLinks(crawledPage);

                Assert.AreEqual(0, result.Count());
            }
        }

        [Test]
        public void GetLinks_DuplicateLinks_ReturnsOnlyOne()
        {
            using (ShimsContext.Create())
            {
                CrawledPage crawledPage = new CrawledPage(_uri)
                {
                    RawContent = "<a href=\"/aaa/a.html\" ></a><a href=\"/aaa/a.html\" /></a>",
                    HttpWebRequest = new ShimHttpWebRequest { AddressGet = () => _uri }.Instance
                };

                IEnumerable<Uri> result = _unitUnderTest.GetLinks(crawledPage);

                Assert.AreEqual(1, result.Count());
                Assert.AreEqual("http://a.com/aaa/a.html", result.ElementAt(0).AbsoluteUri);
            }
        }

        [Test]
        public void GetLinks_NamedAnchors_Ignores()
        {
            using (ShimsContext.Create())
            {
                CrawledPage crawledPage = new CrawledPage(_uri)
                {
                    RawContent = "<a href=\"/aaa/a.html\" ></a><a href=\"/aaa/a.html#top\" ></a><a href=\"/aaa/a.html#bottom\" /></a>",
                    HttpWebRequest = new ShimHttpWebRequest { AddressGet = () => _uri }.Instance
                };

                IEnumerable<Uri> result = _unitUnderTest.GetLinks(crawledPage);

                Assert.AreEqual(1, result.Count());
                Assert.AreEqual("http://a.com/aaa/a.html", result.ElementAt(0).AbsoluteUri);
            }
        }

        [Test]
        public void GetLinks_EmptyHtml()
        {
            using (ShimsContext.Create())
            {
                CrawledPage crawledPage = new CrawledPage(_uri)
                {
                    RawContent = "",
                    HttpWebRequest = new ShimHttpWebRequest { AddressGet = () => _uri }.Instance
                };

                IEnumerable<Uri> result = _unitUnderTest.GetLinks(crawledPage);

                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Count());
            }
        }

        [Test]
        public void GetLinks_WhiteSpaceHtml()
        {
            using (ShimsContext.Create())
            {
                CrawledPage crawledPage = new CrawledPage(_uri)
                {
                    RawContent = "         ",
                    HttpWebRequest = new ShimHttpWebRequest { AddressGet = () => _uri }.Instance
                };

                IEnumerable<Uri> result = _unitUnderTest.GetLinks(crawledPage);

                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Count());
            }
        }

        [Test]
        public void GetLinks_ValidBaseTagPresent_ReturnsRelativeLinksUsingBase()
        {
            using (ShimsContext.Create())
            {
                CrawledPage crawledPage = new CrawledPage(_uri)
                {
                    RawContent = "<base href=\"http://bbb.com\"><a href=\"http://aaa.com/\" ></a><a href=\"/aaa/a.html\" /></a>",
                    HttpWebRequest = new ShimHttpWebRequest { AddressGet = () => _uri }.Instance
                };

                IEnumerable<Uri> result = _unitUnderTest.GetLinks(crawledPage);

                Assert.AreEqual(2, result.Count());
                Assert.AreEqual("http://aaa.com/", result.ElementAt(0).AbsoluteUri);
                Assert.AreEqual("http://bbb.com/aaa/a.html", result.ElementAt(1).AbsoluteUri);

            }
        }

        [Test]
		public void GetLinks_RelativeBaseTagPresent_ReturnsRelativeLinksPageUri ()
		{
            using (ShimsContext.Create())
            {
                CrawledPage crawledPage = new CrawledPage(_uri)
                {
                    RawContent = "<base href=\"/images\"><a href=\"http://aaa.com/\" ></a><a href=\"/aaa/a.html\" /></a>",
                    HttpWebRequest = new ShimHttpWebRequest { AddressGet = () => _uri }.Instance
                };

                IEnumerable<Uri> result = _unitUnderTest.GetLinks(crawledPage);

                Assert.AreEqual(2, result.Count());
                Assert.AreEqual("http://aaa.com/", result.ElementAt(0).AbsoluteUri);
                Assert.AreEqual("http://a.com/aaa/a.html", result.ElementAt(1).AbsoluteUri);
            }
        }

        [Test]
        public void GetLinks_InvalidBaseTagPresent_ReturnsRelativeLinksPageUri()
        {
            using (ShimsContext.Create())
            {
                CrawledPage crawledPage = new CrawledPage(_uri)
                {
                    RawContent = "<base href=\"http:http://http:\"><a href=\"http://aaa.com/\" ></a><a href=\"/aaa/a.html\" /></a>",
                    HttpWebRequest = new ShimHttpWebRequest { AddressGet = () => _uri }.Instance
                };

                IEnumerable<Uri> result = _unitUnderTest.GetLinks(crawledPage);

                Assert.AreEqual(2, result.Count());
                Assert.AreEqual("http://aaa.com/", result.ElementAt(0).AbsoluteUri);
                Assert.AreEqual("http://a.com/aaa/a.html", result.ElementAt(1).AbsoluteUri);
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetLinks_NullCrawledPage()
        {
            _unitUnderTest.GetLinks(null);
        }

        [Test]
        public void GetLinks_ResponseUriDiffFromRequestUri_UsesResponseUri()
        {
            using (ShimsContext.Create())
            {
                CrawledPage crawledPage = new CrawledPage(_uri)
                {
                    RawContent = "<a href=\"/aaa/a.html\" ></a><a href=\"/bbb/b.html\" /></a>",
                    HttpWebRequest = new ShimHttpWebRequest { AddressGet = () => new Uri("http://zzz.com/") }.Instance
                };

                IEnumerable<Uri> result = _unitUnderTest.GetLinks(crawledPage);

                Assert.AreEqual(2, result.Count());
                Assert.AreEqual("http://zzz.com/aaa/a.html", result.ElementAt(0).AbsoluteUri);
                Assert.AreEqual("http://zzz.com/bbb/b.html", result.ElementAt(1).AbsoluteUri);
            }
        }
    }
}
