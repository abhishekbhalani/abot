# Working With The Source Code #
Most common changes/extensions can be done just by extending classes and overriding methods or creating a custom implementation of a core interface. All this can be done outside of Abot's source code. However, if the changes that you are going to make are out of the ordinary or you want to contribute a bug fix or feature then you will want to work directly with Abot's source code. Below you will find what you need to get the solution building/running on your local machine.

### External Tools Needed ###
**Visual Studio 2012**: All code was created using Visual Studio 2012 and targeting the .NET framework 4.5. This means to open the solution/project source code you will need to have Visual Studio 2012.

Future releases may target version 4.0 instead so the solution/project source files can be opened in Visual Studio 2010 sp1. Visual Studio 2008 will not be supported.

<font color='red'><b>NOTE:</b> This only applies if you need to work with Abot's source code. Your Visual Studio 2010 projects that target .NET framework 4.0 or above can reference Abot.dll with no issues.</font>

**ILMerge**: A free console app that that merges multiple assemblies into a single assembly. This tool takes Abot.dll and combines it with all the 3rd party libs that it relies on (log4net.dll, Automapper.dll, etc...). This results in a single Abot.dll file which makes Abot's end users only have to reference a single dll file instead of 5. Abot invokes ILMerge through a Visual Studio post build command on the Abot.csproj project.

  1. Download the installer from http://www.microsoft.com/en-us/download/details.aspx?id=17630.
  1. Run the installer and TAKE NOTE OF THE INSTALLATION PATH, you may need it in the steps of "Your First build" section below.

**Fiddler**: A free http proxy application that allows you to see the request and response of http traffic on your local machine. It also allows you to capture all the http requests during a crawl, save them, then play back that exact crawl again (AutoResponding) using the saved .saz file.

  1. Download the installer from http://www.fiddler2.com/fiddler2/version.asp
  1. Run the installer.
  1. Download wvtesting2.saz file from http://abot.googlecode.com/files/wvtesting2.saz
  1. Open fiddler
  1. Select the "AutoResponder" tab and import the wvtesting2.saz file and check all checkbox options. This will playback the recorded crawl when fiddler is running.


### Your First Build ###
  1. Checkout the latest from http://code.google.com/p/abot/source/checkout
  1. Open solution in Visual Studio
  1. If ILMerge installation path is not "C:\Program Files (x86)\Microsoft\ILMerge\ILMerge.exe", you must...
    1. Right click the Abot project and select properties
    1. Select Build Events section
    1. In the "Post-build event command line" textbox replace "C:\Program Files (x86)\Microsoft\ILMerge\ilmerge.exe" with your ILMerge installation path
  1. Build the solution normally

### Solution Overview ###
**Abot**: Main library for all crawling and utility code.<br />
**Abot.Demo**: Simple console app that demonstrates how to use abot.<br />
**Abot.SiteSimulator**: An asp.net mvc application that can simulate any number of pages and several http responses that are encountered during a crawl. This site is used to produce a predictable site crawl for abot.
Both Abot.Demo, Abot.Tests.Unit and Abot.Tests.Integration rely on this project to be running on http://localhost:1111/. <br />
**Abot.Tests.Unit**: Unit tests that test all components under isolation. Abot.SiteSimulator site must be running for tests to pass since mocking http web requests is more trouble then its worth.<br />
**Abot.Test.Integration**: Tests the end to end crawl behavior. These are real crawls, no mock/stub/etc.. Abot.SiteSimulator site must be running for tests to pass.

### How to run Abot.Demo ###
  1. Right click on the Abot.SiteSimulator project and set it as the "startup project".
  1. Then hit ctrl + F5 to run it, You should see a simple webpage with a few links on http://localhost:1111/
  1. Right click on the Abot.Demo project and set it as the "startup project"
  1. Then hit ctrl + F5 to see the console app run.
  1. View the file output at Abot.Demo/bin/debug/log.txt file for all the output.

### How to run Abot.Tests.Unit ###
  1. Right click on the Abot.SiteSimulator project and set it as the "startup project".
  1. Then hit ctrl + F5 to run it, You should see a simple webpage with a few links on http://localhost:1111/
  1. Run Abot.Tests.Unit using NUnit app or a tool like TestDriven

### How to run Abot.Tests.Integration ###
  1. Verify "External Tools" defined above are installed and working
  1. Right clicking on the Abot.SiteSimulator project and set it as the "startup project".
  1. Then hit ctrl + F5 to run it, You should see a simple webpage with a few links on http://localhost:1111/
  1. Run fiddler with AutoResponder feature enabled
  1. Run Abot.Tests.Integration using NUnit app or a tool like TestDriven
  1. View the file output at Abot.Tests.Integration/bin/debug/log.txt file for all the output.