# Changelog for CodeProject.ObjectPool #

### v3.2.2 (2017-10-28)

* Added support for .NET Framework 4.7.1.

### v3.2.1 (2017-09-30)

* Added an adapter for Microsoft.Extensions.ObjectPool.

### v3.2.0 (2017-08-16)

* Added support for .NET Standard 2.0.
* System.Timer does not seem to be available on .NET Standard 1.0 anymore.
* Added support for .NET Standard 1.2, since it is the minimum version which implements System.Timer.
* Dropped support for .NET 3.5.

### v3.1.1 (2017-07-03)

* Object pool now supports an async eviction job (PR #6 by @uliian).
* Timed object pool is now backed by the new eviction system.
* Timed object pool is also available on .NET Standard 1.0 (PR #6 by @uliian).

### v3.1.0 (2017-06-24)

* Removed dependency on Thrower.
* Pooled objects can now specify a validation step (PR #4 by @uliian).
* Removed CannotResetStateException class, not needed with new validation step.

### v3.0.3 (2017-04-08)

* Added a timed object pool (issue #1).
* OnReleaseResources and OnResetState are now simple actions on PooledObject.

### v3.0.2 (2017-04-02)

* Moved core pool buffer into dedicated class: Core.PooledObjectBuffer. 

### v3.0.1 (2017-03-30)

* Breaking change - Pool does not handle minimum capacity anymore.
* Breaking change - Pooled object ID, state, handle have been moved to PooledObjectInfo property.
* Breaking change - Removed CreatedAt property from PooledMemoryStream and PooledStringBuilder.
* Breaking change - ID property on PooledMemoryStream and PooledStringBuilder is now an int instead of a GUID.
* Default maximum capacity is now 16.

### v2.2.4 (2017-03-05)

* Fixed a bug which could produce closed pooled memory streams.
* Converted the project to .NET Core format.

### v2.2.2 (2017-01-08)

* Updated Thrower to v4.0.6.
* Added unit tests for Portable and .NET Standard 1.1/1.3.

### v2.2.1 (2016-12-17)

* Updated Thrower to v4.
* Fixed some mistakes inside nuspec dependencies.

### v2.1.1 (2016-09-18)

* BREAKING CHANGE: Removed a feature added by mistake in v2.1.0.

### v2.1.0 (2016-09-18)

* Changed default min and max size for MemoryStreamPool: 4KB min, 512KB max.
* Changed default min and max size for StringBuilderPool: 4K char min, 512K char max.
* Created two ad-hoc interfaces for specialized pools.
* BREAKING CHANGE: Moved static properties which controlled specialized pool sizes to the new interfaces.
* Updated Thrower.
* ObjectPool did not respect minimum pool size bound. Now it does.
* When min or max capacity of specialized pools is changed, pool is cleared, if necessary.

### v2.0.5 (2016-08-23)

* Fixed wrong name in an exception string.
* Added Id and CreatedAt properties to PooledMemoryStream and PooledStringBuilder.

### v2.0.4 (2016-08-20)

* Fixes for new MemoryStream pool.

### v2.0.3 (2016-08-20)

* Added a MemoryStream pool in the "Specialized" namespace.

### v2.0.2 (2016-08-20)

* Added LibLog to .NET 4.x projects.
* Added a DLL compiled for .NET 3.5.
* Added a DLL compiled for .NET Standard 1.3.
* Performance have been improved by 30%.
* Added a StringBuilder pool in the "Specialized" namespace.

### v2.0.1 (2016-08-07)

* Library for .NET Standard 1.1.
* Updated NUnit to 3.x branch.
