using Abot.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

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
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetLinks_NullUri()
        {
            _unitUnderTest.GetLinks(null, "");
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetLinks_NullHtml()
        {
            string nullString = null;
            _unitUnderTest.GetLinks(_uri, nullString);
        }

        [Test]
        public void GetLinks_AnchorTags_ReturnsLinks()
        {
            string html = "<a href=\"http://aaa.com/\" ></a><a href=\"/aaa/a.html\" /></a>";

            IEnumerable<Uri> result = _unitUnderTest.GetLinks(_uri, html);

            Assert.AreEqual(2, result.Count());
            Assert.AreEqual("http://aaa.com/", result.ElementAt(0).AbsoluteUri);
            Assert.AreEqual("http://a.com/aaa/a.html", result.ElementAt(1).AbsoluteUri);
        }

        [Test]
        public void GetLinks_AreaTags_ReturnsLinks()
        {
            string html = "<area href=\"http://bbb.com\" /><area href=\"bbb/b.html\" />";

            IEnumerable<Uri> result = _unitUnderTest.GetLinks(_uri, html);

            Assert.AreEqual(2, result.Count());
            Assert.AreEqual("http://bbb.com/", result.ElementAt(0).AbsoluteUri);
            Assert.AreEqual("http://a.com/bbb/b.html", result.ElementAt(1).AbsoluteUri);
        }

        [Test]
        public void GetLinks_NoLinks_NotReturned()
        {
            string html = "<html></html>";

            IEnumerable<Uri> result = _unitUnderTest.GetLinks(_uri, html);

            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void GetLinks_AnyScheme_Returned()
        {
            string html = "<a href=\"mailto:aaa@gmail.com\" /><a href=\"tel:+123456789\" /><a href=\"callto:+123456789\" /><a href=\"ftp://user@yourdomainname.com/\" /><a href=\"file:///C:/Users/\" />";

            IEnumerable<Uri> result = _unitUnderTest.GetLinks(_uri, html);

            Assert.AreEqual(5, result.Count());
            Assert.AreEqual("mailto:aaa@gmail.com", result.ElementAt(0).AbsoluteUri);
            Assert.AreEqual("tel:+123456789", result.ElementAt(1).AbsoluteUri);
            Assert.AreEqual("callto:+123456789", result.ElementAt(2).AbsoluteUri);
            Assert.AreEqual("ftp://user@yourdomainname.com/", result.ElementAt(3).AbsoluteUri);
            Assert.AreEqual("file:///C:/Users/", result.ElementAt(4).AbsoluteUri);
        }

        [Test]
        public void GetLinks_InvalidFormatUrl_NotReturned()
        {
            string html = "<a href=\"http://////\" />";

            IEnumerable<Uri> result = _unitUnderTest.GetLinks(_uri, html);

            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void GetLinks_LinksInComments_NotReturned()
        {
            string html = @"<html>
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
                </html";

            IEnumerable<Uri> result = _unitUnderTest.GetLinks(_uri, html);

            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void GetLinks_LinksInScript_NotReturned()
        {
            string html = @"<html>
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
                </html";

            IEnumerable<Uri> result = _unitUnderTest.GetLinks(_uri, html);

            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void GetLinks_LinksInStyleTag_NotReturned()
        {
            string html = @"<html>
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
                </html";

            IEnumerable<Uri> result = _unitUnderTest.GetLinks(_uri, html);

            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void GetLinks_DuplicateLinks_ReturnsOnlyOne()
        {
            string html = "<a href=\"/aaa/a.html\" ></a><a href=\"/aaa/a.html\" /></a>";
            
            IEnumerable<Uri> result = _unitUnderTest.GetLinks(_uri, html);

            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("http://a.com/aaa/a.html", result.ElementAt(0).AbsoluteUri);
        }

        [Test]
        public void GetLinks_NamedAnchors_Ignores()
        {
            string html = "<a href=\"/aaa/a.html\" ></a><a href=\"/aaa/a.html#top\" ></a><a href=\"/aaa/a.html#bottom\" /></a>";

            IEnumerable<Uri> result = _unitUnderTest.GetLinks(_uri, html);

            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("http://a.com/aaa/a.html", result.ElementAt(0).AbsoluteUri);
        }

        [Test]
        public void GetLinks_EmptyHtml()
        {
            IEnumerable<Uri> result = _unitUnderTest.GetLinks(_uri, "");

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void GetLinks_WhiteSpaceHtml()
        {
            IEnumerable<Uri> result = _unitUnderTest.GetLinks(_uri, "      ");

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void GetLinks_NoLinks_ReturnsEmptyCollection()
        {
            IEnumerable<Uri> result = _unitUnderTest.GetLinks(_uri, "<html></html>");

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void GetLinks_ValidBaseTagPresent_ReturnsRelativeLinksUsingBase()
        {
            string html = "<base href=\"http://bbb.com\"><a href=\"http://aaa.com/\" ></a><a href=\"/aaa/a.html\" /></a>";

            IEnumerable<Uri> result = _unitUnderTest.GetLinks(_uri, html);

            Assert.AreEqual(2, result.Count());
            Assert.AreEqual("http://aaa.com/", result.ElementAt(0).AbsoluteUri);
            Assert.AreEqual("http://bbb.com/aaa/a.html", result.ElementAt(1).AbsoluteUri);
        }

        [Test]
        public void GetLinks_RelativeBaseTagPresent_ReturnsRelativeLinksPageUri()
        {
            string html = "<base href=\"/images\"><a href=\"http://aaa.com/\" ></a><a href=\"/aaa/a.html\" /></a>";

            IEnumerable<Uri> result = _unitUnderTest.GetLinks(_uri, html);

            Assert.AreEqual(2, result.Count());
            Assert.AreEqual("http://aaa.com/", result.ElementAt(0).AbsoluteUri);
            Assert.AreEqual("http://a.com/aaa/a.html", result.ElementAt(1).AbsoluteUri);
        }

        [Test]
        public void GetLinks_InvalidBaseTagPresent_ReturnsRelativeLinksPageUri()
        {
            string html = "<base href=\"http:http://http:\"><a href=\"http://aaa.com/\" ></a><a href=\"/aaa/a.html\" /></a>";

            IEnumerable<Uri> result = _unitUnderTest.GetLinks(_uri, html);

            Assert.AreEqual(2, result.Count());
            Assert.AreEqual("http://aaa.com/", result.ElementAt(0).AbsoluteUri);
            Assert.AreEqual("http://a.com/aaa/a.html", result.ElementAt(1).AbsoluteUri);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetLinks_NullCrawledPage()
        {
            _unitUnderTest.GetLinks(null);
        }
    }
}
