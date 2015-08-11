# Working With The Source Code #
The most common way to customize crawl behavior is by extending classes and overriding methods. You can also create a custom implementation of a core interface. All this can be done outside of Abot's source code.

However, if the changes that you are going to make are out of the ordinary or you want to contribute a bug fix or feature then you will want to work directly with Abot's source code. Below you will find what you need to get the solution building/running on your local machine.

### External Tools Needed ###
**Visual Studio 2010 or greater**: All code was created using Visual Studio 2012 and targeting the .NET framework 4.0.

**Fiddler**: A free http proxy application that allows you to see the request and response of http traffic on your local machine. It also allows you to capture all the http requests during a crawl, save them, then play back that exact crawl again (AutoResponding) using the saved .saz file.

  1. Download the installer http://www.fiddler2.com/fiddler2/version.asp
  1. Run the installer.
  1. Download the RecordedCrawls.saz file http://abot.googlecode.com/files/RecordedCrawls.saz
  1. Open fiddler
  1. Select the "AutoResponder" tab and import the RecordedCrawls.saz file and check all checkbox options. This will playback the recorded crawl when fiddler is running.

**NUnit Test Runner**: The unit tests for Abot are using NUnit which is not supported right out of the box in visual studio. You must either install a NUnit test adapter or a product like TestDriven

  1. Download the NUnit test adapter http://visualstudiogallery.msdn.microsoft.com/6ab922d0-21c0-4f06-ab5f-4ecd1fe7175d
  1. Or download Testdriven.net plugin (my favorite) http://testdriven.net/

### Your First Build ###
  1. Clone the latest from https://github.com/sjdirect/abot/ master branch
  1. Open the Abot.sln file in Visual Studio
  1. Build the solution normally

### Solution Project/Assembly Overview ###
**Abot**: Main library for all crawling and utility code.<br />
**Abot.Demo**: Simple console app that demonstrates how to use abot.<br />
**Abot.SiteSimulator**: An asp.net mvc application that can simulate any number of pages and several http responses that are encountered during a crawl. This site is used to produce a predictable site crawl for abot.
Both Abot.Tests.Unit and Abot.Tests.Integration rely on this project to be running on http://localhost:1111/. <br />
**Abot.Tests.Unit**: Unit tests for all Abot assemblies. Abot.SiteSimulator site must be running for tests to pass since mocking http web requests is more trouble then its worth.<br />
**Abot.Tests.Integration**: Tests the end to end crawl behavior. These are real crawls, no mocks/stubs/etc.. Abot.SiteSimulator site must be running for tests to pass.

### How to run Abot.Demo ###
The demo project has a few config values set that greatly limit Abot's speed.  This is to make sure you don't get banned by your isp provider or get blocked by the sites you are crawling. These setting are..

```
<abot>
    <crawlBehavior 
      ...(excluded)
      maxPagesToCrawl="10" 
      ...(excluded)
      />
    <politeness 
      ...(excluded)
      minCrawlDelayPerDomainMilliSeconds="1000"
      ...(excluded)
      />
  </abot>  
```

This will tell Abot to only crawl up to 10 pages and that it must wait 1 second between each http request.

  1. Right click on the Abot.Demo project and set it as the "startup project"
  1. Then hit ctrl + F5 to see the console app run.
  1. When prompted for a url enter whatever site you want to crawl (must begin with "http://" or "https://")
  1. Press enter
  1. View the Abot.Demo/bin/debug/log.txt file for all the output.

If you want to get a real feel for Abot's speed you can safely set it up to crawl the SiteSimulator test site. This site is hosted on your machine and will not generate any http traffic beyond your local network.  To do so change the config values to...

```
<abot>
    <crawlBehavior 
      ...(excluded)
      maxPagesToCrawl="1000" 
      ...(excluded)
      />
    <politeness 
      ...(excluded)
      minCrawlDelayPerDomainMilliSeconds="0"
      ...(excluded)
      />
  </abot>  
```

  1. Right click on the Abot.SiteSimulator project and set it as the "startup project".
  1. Then hit ctrl + F5 to run it, You should see a simple webpage with a few links on http://localhost:1111/
  1. Right click on the Abot.!Demo project and set it as the "startup project".
  1. Then hit ctrl + F5 to see the console app run.
  1. When prompted for a url enter: http://localhost:1111/
  1. Press enter
  1. View the Abot.Demo/bin/debug/log.txt file for all the output.


### How to run Abot.Tests.Unit ###
  1. Right click on the Abot.SiteSimulator project and set it as the "startup project".
  1. Then hit ctrl + F5 to run it, You should see a simple webpage with a few links on http://localhost:1111/
  1. Run Abot.Tests.Unit tests.

### How to run Abot.Tests.Integration ###
  1. Verify "External Tools" defined above are installed and working
  1. Right clicking on the Abot.SiteSimulator project and set it as the "startup project".
  1. Then hit ctrl + F5 to run it, You should see a simple webpage with a few links on http://localhost:1111/
  1. Run fiddler with AutoResponder feature enabled
  1. Run Abot.Tests.Integration tests.
  1. View the file output at Abot.Tests.Integration/bin/debug/log.txt file for all the output.