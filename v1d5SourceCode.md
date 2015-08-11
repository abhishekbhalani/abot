# Working With The Source Code #
The most common way to customize crawl behavior is by extending classes and overriding methods. You can also create a custom implementation of a core interface. All this can be done outside of Abot's source code.

However, if the changes that you are going to make are out of the ordinary or you want to contribute a bug fix or feature then you will want to work directly with Abot's source code. Below you will find what you need to get the solution building/running on your local machine.

### Your First Build ###
  1. Clone the latest using the following commands
    * git clone git@github.com:sjdirect/abot.git
    * cd abot
    * git submodule init
    * git submodule update
  1. Open the Abot.sln file in Visual Studio (all dev done in vs 2013 premium)
  1. Build the solution normally


### External Tools Needed ###
**NUnit Test Runner**: The unit tests for Abot are using NUnit which is not supported right out of the box in visual studio. You must either install a NUnit test adapter or a product like TestDriven or Resharper.

  1. Download the [NUnit test adapter](http://visualstudiogallery.msdn.microsoft.com/6ab922d0-21c0-4f06-ab5f-4ecd1fe7175d) or install it through visual studio extension manager.


### Solution Project/Assembly Overview ###
**Abot**: Main library for all crawling and utility code.<br />
**Abot.Demo**: Simple console app that demonstrates how to use abot.<br />
**Abot.SiteSimulator**: An asp.net mvc application that can simulate any number of pages and several http responses that are encountered during a crawl. This site is used to produce a predictable site crawl for abot.
Both Abot.Tests.Unit and Abot.Tests.Integration make calls to this site. However a sample of those calls were saved in a fiddler session and are not automatically used by FiddlerCore everytime the unit or integration tests are run. <br />
**Abot.Tests.Unit**: Unit tests for all Abot assemblies. Abot.SiteSimulator site must be running for tests to pass since mocking http web requests is more trouble then its worth.<br />
**Abot.Tests.Integration**: Tests the end to end crawl behavior. These are real crawls, no mocks/stubs/etc.. Abot.SiteSimulator site must be running for tests to pass.<br />

### How to run Abot.Demo ###
The demo project has a few config values set that greatly limit Abot's speed.  This is to make sure you don't get banned by your isp provider or get blocked by the sites you are crawling. These setting are..

```
<abot>
    <crawlBehavior 
      ...(excluded)
      maxConcurrentThreads="1" 
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

This will tell Abot to use 1 thread, to only crawl 10 pages and that it must wait 1 second between each http request. If you want to get a feel for the real speed of Abot then change those settings to the following...

```
<abot>
    <crawlBehavior 
      ...(excluded)
      maxConcurrentThreads="10" 
      maxPagesToCrawl="10000" 
      ...(excluded)
      />
    <politeness 
      ...(excluded)
      minCrawlDelayPerDomainMilliSeconds="0"
      ...(excluded)
      />
  </abot>  
```

  1. Right click on the Abot.Demo project and set it as the "startup project"
  1. Then hit ctrl + F5 to see the console app run.
  1. When prompted for a url enter whatever site you want to crawl (must begin with "http://" or "https://")
  1. Press enter
  1. View the Abot.Demo/bin/debug/abotlog.txt file for all the output.

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
  1. View the Abot.Demo/bin/debug/abotlog.txt file for all the output.


### How to run Abot.Tests.Unit ###
  1. Verify "External Tools" defined above are installed and working
  1. Run Abot.Tests.Unit tests.

### How to run Abot.Tests.Integration ###
  1. Verify "External Tools" defined above are installed and working
  1. Run Abot.Tests.Integration tests.
  1. View the file output at Abot.Tests.Integration/bin/debug/abotlog.txt file for all the output.