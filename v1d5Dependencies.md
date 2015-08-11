Abot utilizes several external libraries. If you are using the most simple implementation of Abot (like the code in the quickstart demo) you will not need to reference anything other than the Abot.dll.

### What Do Each Of These Dependencies Do? ###
  1. **Abot.dll:** All of Abots functionality. The crawler, components, utilities and pocos.
  1. **AutoMapper.dll:** Used to move values from one object to another. Specifically used to convert from PageToCrawl to CrawledPage, config values and a few other conversions.
  1. **Commoner.Core.dll:** A set of very common utilities. Specifically used in the unit tests to set private variables through reflection.
  1. **CsQuery.dll:** By default not used by Abot. The main purpose of this library is to expose a lazy loaded property on CrawlPage.CsQueryDocument that anyone can use to do some heavy duty html parsing in a jquery-like interface.
  1. **HtmlAgilityPack.dll:** Well known html parsing library. Used by the default IHyperlinkParser to parse out the links to crawl on each page. Also a lazy loaded property on the CrawlPage.HtmlDocument that anyone can use to do some heavy duty html parsing. NOTE: This binary is a patched version to avoid a common StackOverflowException issue with Html Agility Pack. Read more about it [here](http://code.google.com/p/abot/issues/detail?id=77).
  1. **log4net.dll:** Provides logging functionality that is configurable through an xml file.
  1. **Moq.dll:** Mocking framework used in the unit tests.
  1. **nunit.dll:** Unit testing framework used in the unit tests.
  1. **Robots.dll:** Handles parsing robots.txt files.