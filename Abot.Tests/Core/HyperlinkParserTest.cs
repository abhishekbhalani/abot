using Abot.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Abot.Tests.Core
{
    [TestFixture]
    public class HyperLinkParserTest
    {
        HyperLinkParser _unitUnderTest;
        Uri _uri = new Uri("http://a.com/");

        [SetUp]
        public void Setup()
        {
            _unitUnderTest = new HyperLinkParser();
        }

        [Test]
        public void GetHyperlinks_AnchorTags_ReturnsLinks()
        {
            string html = "<a href=\"http://aaa.com/\" ></a><a href=\"/aaa/a.html\" /></a>";

            IEnumerable<Uri> result = _unitUnderTest.GetHyperLinks(_uri, html);

            Assert.AreEqual(2, result.Count());
            Assert.AreEqual("http://aaa.com/", result.ElementAt(0).AbsoluteUri);
            Assert.AreEqual("http://a.com/aaa/a.html", result.ElementAt(1).AbsoluteUri);
        }

        [Test]
        public void GetHyperlinks_AreaTags_ReturnsLinks()
        {
            string html = "<area href=\"http://bbb.com\" /><area href=\"bbb/b.html\" />";

            IEnumerable<Uri> result = _unitUnderTest.GetHyperLinks(_uri, html);

            Assert.AreEqual(2, result.Count());
            Assert.AreEqual("http://bbb.com/", result.ElementAt(0).AbsoluteUri);
            Assert.AreEqual("http://a.com/bbb/b.html", result.ElementAt(1).AbsoluteUri);
        }

        [Test]
        public void GetHyperlinks_NoLinks_NotReturned()
        {
            string html = "<html></html>";

            IEnumerable<Uri> result = _unitUnderTest.GetHyperLinks(_uri, html);

            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void GetHyperlinks_BadScheme_NotReturned()
        {
            string html = "<a href=\"mailto:aaa@gmail.com\" />";

            IEnumerable<Uri> result = _unitUnderTest.GetHyperLinks(_uri, html);

            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void GetHyperlinks_InvalidFormatUrl_NotReturned()
        {
            string html = "<a href=\"http://////\" />";

            IEnumerable<Uri> result = _unitUnderTest.GetHyperLinks(_uri, html);

            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void GetHyperlinks_LinksInComments_NotReturned()
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

            IEnumerable<Uri> result = _unitUnderTest.GetHyperLinks(_uri, html);

            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void GetHyperlinks_LinksInScript_NotReturned()
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

            IEnumerable<Uri> result = _unitUnderTest.GetHyperLinks(_uri, html);

            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void GetHyperlinks_LinksInStyleTag_NotReturned()
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

            IEnumerable<Uri> result = _unitUnderTest.GetHyperLinks(_uri, html);

            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void GetHyperlinks_DuplicateLinks_ReturnsOnlyOne()
        {
            string html = "<a href=\"/aaa/a.html\" ></a><a href=\"/aaa/a.html\" /></a>";
            
            IEnumerable<Uri> result = _unitUnderTest.GetHyperLinks(_uri, html);

            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("http://a.com/aaa/a.html", result.ElementAt(0).AbsoluteUri);
        }

        [Test]
        public void GetHyperlinks_NamedAnchors_Ignores()
        {
            string html = "<a href=\"/aaa/a.html\" ></a><a href=\"/aaa/a.html#top\" ></a><a href=\"/aaa/a.html#bottom\" /></a>";

            IEnumerable<Uri> result = _unitUnderTest.GetHyperLinks(_uri, html);

            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("http://a.com/aaa/a.html", result.ElementAt(0).AbsoluteUri);
        }

        [Test]
        public void GetHyperlinks_EmptyHtml()
        {
            IEnumerable<Uri> result = _unitUnderTest.GetHyperLinks(_uri, "");

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void GetHyperlinks_WhiteSpaceHtml()
        {
            IEnumerable<Uri> result = _unitUnderTest.GetHyperLinks(_uri, "      ");

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void GetHyperlinks_NoLinks_ReturnsEmptyCollection()
        {
            IEnumerable<Uri> result = _unitUnderTest.GetHyperLinks(_uri, "<html></html>");

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }
    }
}
