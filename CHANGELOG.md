# Changelog for CodeProject.ObjectPool #

### v2.0.6 (2016-09-??) ###

* Changed default min and max size for MemoryStreamPool: 4KB min, 512KB max.
* Changed default min and max size for StringBuilderPool: 4K char min, 512K char max.
* Created two ad-hoc interfaces for specialized pools.
* BREAKING CHANGE: Moved static properties which controlled specialized pool sizes to the new interfaces.

### v2.0.5 (2016-08-23) ###

* Fixed wrong name in an exception string.
* Added Id and CreatedAt properties to PooledMemoryStream and PooledStringBuilder.

### v2.0.4 (2016-08-20) ###

* Fixes for new MemoryStream pool.

### v2.0.3 (2016-08-20) ###

* Added a MemoryStream pool in the "Specialized" namespace.

### v2.0.2 (2016-08-20) ###

* Added LibLog to .NET 4.x projects.
* Added a DLL compiled for .NET 3.5.
* Added a DLL compiled for .NET Standard 1.3.
* Performance have been improved by 30%.
* Added a StringBuilder pool in the "Specialized" namespace.

### v2.0.1 (2016-08-07) ###

* Library for .NET Standard 1.1.
* Updated NUnit to 3.x branch.
