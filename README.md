# Dragonfly Umbraco 10 SiteTester #

Tools to test pages in an Umbraco 10 website created by [Heather Floyd](https://www.HeatherFloyd.com).

[![Dragonfly Website](https://img.shields.io/badge/Dragonfly-Website-A84492)](https://DragonflyLibraries.com/umbraco-packages/site-tester/) [![Umbraco Marketplace](https://img.shields.io/badge/Umbraco-Marketplace-3544B1?logo=Umbraco&logoColor=white)](https://marketplace.umbraco.com/package/Dragonfly.Umbraco10.SiteTester) [![Nuget Downloads](https://buildstats.info/nuget/Dragonfly.Umbraco10.SiteTester)](https://www.nuget.org/packages/Dragonfly.Umbraco10.SiteTester/) [![GitHub](https://img.shields.io/badge/GitHub-Sourcecode-blue?logo=github)](https://github.com/hfloyd/Dragonfly.Umbraco10.SiteTester)
## Versions ##
This package is designed to work with Umbraco 10-12. [View all available versions](https://DragonflyLibraries.com/umbraco-packages/site-tester/#Versions).

## Installation ##

[![Nuget Downloads](https://buildstats.info/nuget/Dragonfly.Umbraco10.SiteTester)](https://www.nuget.org/packages/Dragonfly.Umbraco10.SiteTester/)

```
PM>   Install-Package Dragonfly.Umbraco10.SiteTester


dotnet add package Dragonfly.Umbraco10.SiteTester

```

You need to add a section to the top-level of your appSettings. You can copy it from [here](https://github.com/hfloyd/Dragonfly.Umbraco10.SiteTester/blob/master/src/example_appSettings.json). 

## Features ##

- Loops through all your Content nodes, attempting to "view" the page via HTTP, compiles a list of all results for display
- Checks for "on-page" errors as well as 500 errors.
- "On-page" error strings are fully customizable
- Stores test results in JSON files for later review

## Usage ##

After NuGet installation and appSettings config, make sure you are logged-in as a User in the Umbraco back-office, and visit the Url:
http://YOURSITE.COM/umbraco/backoffice/Dragonfly/SiteTester/Start

Or look for the 'Site Tester' dashboard in the Content section.

*NOTE:* Testing all nodes can take awhile, especially for large sites. 

