![](https://googledrive.com/host/0B8v0ikF4z2BiR29YQmxfSlE1Sms/Progetti/ObjectPool/logo-64.png "Object Pool Logo") Object Pool
=============================================================================================================================

A generic, concurrent, portable and flexible Object Pool for the .NET Framework, completely based on the [Code Project article of Ofir Makmal](http://www.codeproject.com/Articles/535735/Implementing-a-Generic-Object-Pool-in-NET).

## Summary ##

* Latest release version: `v2.0.3`
* Build status on [AppVeyor](https://ci.appveyor.com): [![Build status](https://ci.appveyor.com/api/projects/status/r4qnqaqj9ri6cicn?svg=true)](https://ci.appveyor.com/project/pomma89/objectpool)
* [Doxygen](http://www.stack.nl/~dimitri/doxygen/index.html) documentation: 
    + [HTML](https://goo.gl/RVA7mV)
    + [PDF](https://goo.gl/U6dNkt)
* [NuGet](https://www.nuget.org) package(s):
    + [CodeProject.ObjectPool](https://nuget.org/packages/CodeProject.ObjectPool/)

## Introduction ##

Library is production ready and it is successfully working in real life systems. [Here](https://4538d366a46bbb00d202aaaa7b99c4e50320a061.googledrive.com/host/0B8v0ikF4z2BiR29YQmxfSlE1Sms/Progetti/ObjectPool/doc/index.html) you can find the Doxygen-generated API documentation.

Original source code has been modified, in order to introduce a Parameterized Object Pool, already drafted by Ofir Makmal in the comments of the article. 
Moreover, a few unit tests have been added, in order to improve code reliability, and a lot of other small changes have also been applied. 
Of course, all modified source code is freely available in this repository.

Many thanks to Ofir Makmal for his great work.

Quick and dirty example:


```cs
/// <summary>
///   Example usages of ObjectPool.
/// </summary>
internal static class Program
{
    /// <summary>
    ///   Example usages of ObjectPool.
    /// </summary>
    private static void Main()
    {
        // Creating a pool with minimum size of 5 and maximum size of 25, using custom Factory
        // method to create and instance of ExpensiveResource.
        var pool = new ObjectPool<ExpensiveResource>(5, 25, () => new ExpensiveResource(/* resource specific initialization */));

        using (var resource = pool.GetObject())
        {
            // Using the resource...
            resource.DoStuff();
        } // Exiting the using scope will return the object back to the pool.

        // Creating a pool with wrapper object for managing external resources, that is, classes
        // which cannot inherit from PooledObject.
        var newPool = new ObjectPool<PooledObjectWrapper<ExternalExpensiveResource>>(() =>
            new PooledObjectWrapper<ExternalExpensiveResource>(CreateNewResource())
            {
                WrapperReleaseResourcesAction = r => ExternalResourceReleaseResource(r),
                WrapperResetStateAction = r => ExternalResourceResetState(r)
            });

        using (var wrapper = newPool.GetObject())
        {
            // wrapper.InternalResource contains the object that you pooled.
            wrapper.InternalResource.DoOtherStuff();
        } // Exiting the using scope will return the object back to the pool.
    }

    private static ExternalExpensiveResource CreateNewResource()
    {
        return new ExternalExpensiveResource();
    }

    public static void ExternalResourceResetState(ExternalExpensiveResource resource)
    {
        // External Resource reset state code.
    }

    public static void ExternalResourceReleaseResource(ExternalExpensiveResource resource)
    {
        // External Resource release code.
    }
}

internal sealed class ExpensiveResource : PooledObject
{
    public void DoStuff()
    {
        // Do some work here, for example.
    }

    protected override void OnReleaseResources()
    {
        // Override if the resource needs to be manually cleaned before the memory is reclaimed.
    }

    protected override void OnResetState()
    {
        // Override if the resource needs resetting before it is getting back into the pool.
    }
}

internal sealed class ExternalExpensiveResource
{
    public void DoOtherStuff()
    {
        // Do some work here, for example.
    }
}
```

## Benchmarks ##

```ini

Host Process Environment Information:
BenchmarkDotNet.Core=v0.9.9.0
OS=Microsoft Windows NT 6.2.9200.0
Processor=Intel(R) Core(TM) i3-2330M CPU 2.20GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
CLR=MS.NET 4.0.30319.42000, Arch=32-bit RELEASE
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1586.0

Type=RetrieveOneObject  Mode=Throughput  

```
                  Method |      Median |    StdDev | Gen 0 | Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------ |------------ |---------- |------ |------ |------ |------------------- |
        SimpleObjectPool | 137.7914 ns | 5.7873 ns | 21.00 | 18.00 | 18.00 |               2,99 |
 ParameterizedObjectPool | 202.5664 ns | 2.1918 ns | 43.80 |     - |     - |               3,40 |
     MicrosoftObjectPool |  60.5100 ns | 1.5422 ns |     - |     - |     - |               0,00 |

```ini

Host Process Environment Information:
BenchmarkDotNet.Core=v0.9.9.0
OS=Microsoft Windows NT 6.2.9200.0
Processor=Intel(R) Core(TM) i3-2330M CPU 2.20GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
CLR=MS.NET 4.0.30319.42000, Arch=32-bit RELEASE
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1586.0

Type=RetrieveObjectsConcurrently  Mode=Throughput  Affinity=2  

```
                  Method | Count |      Median |    StdDev | Gen 0 | Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------ |------ |------------ |---------- |------ |------ |------ |------------------- |
        **SimpleObjectPool** |    **10** |   **5.8370 us** | **0.1055 us** |  **1.27** |  **1.15** |     **-** |             **275,84** |
 ParameterizedObjectPool |    10 |   6.5972 us | 0.4267 us |  1.46 |  1.17 |     - |             299,46 |
     MicrosoftObjectPool |    10 |   4.3874 us | 0.2575 us |  1.33 |  1.06 |     - |             267,88 |
        **SimpleObjectPool** |   **100** |  **20.2330 us** | **0.8434 us** |  **2.64** |  **1.98** |  **0.66** |             **562,87** |
 ParameterizedObjectPool |   100 |  27.6820 us | 1.1765 us |  2.43 |  2.17 |     - |             490,65 |
     MicrosoftObjectPool |   100 |  10.9720 us | 1.3934 us |  1.34 |  0.97 |     - |             226,68 |
        **SimpleObjectPool** |  **1000** | **165.2282 us** | **5.9998 us** | **10.50** | **12.75** |  **6.00** |           **3.107,34** |
 ParameterizedObjectPool |  1000 | 221.2623 us | 1.8009 us | 19.00 |     - |     - |           2.872,10 |
     MicrosoftObjectPool |  1000 |  77.0221 us | 5.2310 us |  4.71 |  0.24 |     - |             251,05 |

## About this repository and its maintainer ##

Everything done on this repository is freely offered on the terms of the project license. You are free to do everything you want with the code and its related files, as long as you respect the license and use common sense while doing it :-)
