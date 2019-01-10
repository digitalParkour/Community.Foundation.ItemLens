﻿Sitecore - Item Lens
==============

Summary
--------------
Quickly identify Sitecore data discrepancies between states of cache, indexes, and publishing.

* Requires developer login
* View data from API calls (Sitecore cache)
* View data from SQL records
* View data from Solr indexes
* Compare values between sources
* Color group highlighting of value matches
* Cookie key to secure in CD instances

Example
--------------
![alt text](https://github.com/digitalParkour/Community.Foundation.ItemLens/raw/master/screenshots/ItemLensExample.png "Screensot Example")

Installation
--------------
Either:
* Install Sitecore package. [See Releases](https://github.com/digitalParkour/Community.Foundation.ItemLens/releases).
	> To use on CD instances, copy files from package to CD server.
    > See config file for setting a unique cookie to access tool without login

Or:
1. Include this project in your Helix style solution
2. Update NuGet references to match target sitecore version
