# ![](http://pomma89.altervista.org/objectpool/logo-64.png "Object Pool Logo") Object Pool

*A generic, concurrent, portable and flexible Object Pool for the .NET Framework, completely based on the [Code Project article of Ofir Makmal](http://www.codeproject.com/Articles/535735/Implementing-a-Generic-Object-Pool-in-NET).*

[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=ELJWKEYS9QGKA)

## Summary

* Latest release version: `v3.2.2`
* Build status on [Travis CI](https://travis-ci.org): [![Build Status](https://travis-ci.org/pomma89/ObjectPool.svg?branch=master)](https://travis-ci.org/pomma89/ObjectPool)
* Build status on [AppVeyor](https://www.appveyor.com/): [![Build status](https://ci.appveyor.com/api/projects/status/cgldtxe3p9v7nf0i/branch/master?svg=true)](https://ci.appveyor.com/project/pomma89/objectpool/branch/master)
* [Wyam](https://wyam.io/) generated API documentation: [https://pomma89.github.io/ObjectPool/api/](https://pomma89.github.io/ObjectPool/api/)
* [NuGet](https://www.nuget.org) package(s):
    + [CodeProject.ObjectPool](https://nuget.org/packages/CodeProject.ObjectPool/)
    + [CodeProject.ObjectPool.MicrosoftExtensionsAdapter](https://nuget.org/packages/CodeProject.ObjectPool.MicrosoftExtensionsAdapter/)

### How to build

#### Windows

Clone the project, go to the root and run PowerShell script `build.ps1`. In order for it to work, you need:

* At least Windows 10 Fall Creators Update
* At least Visual Studio 2017 Update 4
* .NET Framework 4.7.1 Developer Pack
* .NET Core 2.0 SDK

#### Linux

Clone the project, go to the root and run Bash script `build.sh`. In order for it to work, you need:

* TODO, still need to make it building reliably.

## Introduction

Library is production ready and it is successfully working in real life systems.

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
        // Creating a pool with a maximum size of 25, using custom Factory method to create and
        // instance of ExpensiveResource.
        var pool = new ObjectPool<ExpensiveResource>(25, () => new ExpensiveResource(/* resource specific initialization */));

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
                OnReleaseResources = ExternalResourceReleaseResource,
                OnResetState = ExternalResourceResetState
            });

        using (var wrapper = newPool.GetObject())
        {
            // wrapper.InternalResource contains the object that you pooled.
            wrapper.InternalResource.DoOtherStuff();
        } // Exiting the using scope will return the object back to the pool.

        // Creates a pool where objects which have not been used for over 2 seconds will be
        // cleaned up by a dedicated thread.
        var timedPool = new TimedObjectPool<ExpensiveResource>(TimeSpan.FromSeconds(2));

        using (var resource = timedPool.GetObject())
        {
            // Using the resource...
            resource.DoStuff();
        } // Exiting the using scope will return the object back to the pool and record last usage.

        Console.WriteLine($"Timed pool size after 0 seconds: {timedPool.ObjectsInPoolCount}"); // Should be 1
        Thread.Sleep(TimeSpan.FromSeconds(4));
        Console.WriteLine($"Timed pool size after 4 seconds: {timedPool.ObjectsInPoolCount}"); // Should be 0

        // Adapts a timed pool to Microsoft Extensions abstraction.
        var mPool = ObjectPoolAdapter.CreateForPooledObject(timedPool);

        // Sample usage of Microsoft pool.
        var mResource = mPool.Get();
        Debug.Assert(mResource is ExpensiveResource);
        mPool.Return(mResource);

        // Adapts a new pool to Microsoft Extensions abstraction. This example shows how to adapt
        // when object type does not extend PooledObject.
        var mPool2 = ObjectPoolAdapter.Create(new ObjectPool<PooledObjectWrapper<MemoryStream>>(() => PooledObjectWrapper.Create(new MemoryStream())));

        // Sample usage of second Microsoft pool.
        var mResource2 = mPool2.Get();
        Debug.Assert(mResource2 is MemoryStream);
        mPool2.Return(mResource2);

        Console.Read();
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
    public ExpensiveResource()
    {
        OnReleaseResources = () =>
        {
            // Called if the resource needs to be manually cleaned before the memory is reclaimed.
        };

        OnResetState = () =>
        {
            // Called if the resource needs resetting before it is getting back into the pool.
        };
    }

    public void DoStuff()
    {
        // Do some work here, for example.
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

## Benchmarks

All benchmarks were implemented and run using the wonderful [BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet) library.

### [Retrieve one object](https://github.com/pomma89/ObjectPool/blob/master/test/CodeProject.ObjectPool.Benchmarks/RetrieveOneObject.cs)

In this benchmark we evaluate how long it takes to extract and return an object stored into the pool, using a single thread. We compare four implementations:

* [This project's ObjectPool](https://github.com/pomma89/ObjectPool/blob/master/src/CodeProject.ObjectPool/ObjectPool.cs)
* [This project's ParameterizedObjectPool](https://github.com/pomma89/ObjectPool/blob/master/src/CodeProject.ObjectPool/ParameterizedObjectPool.cs)
* [Original ObjectPool](http://www.codeproject.com/Articles/535735/Implementing-a-Generic-Object-Pool-in-NET)
* [Microsoft's ObjectPool](http://www.nuget.org/packages/Microsoft.Extensions.ObjectPool/)
* [This project's adapter for Microsoft's ObjectPool](https://github.com/pomma89/ObjectPool/blob/master/src/CodeProject.ObjectPool.MicrosoftExtensionsAdapter/ObjectPoolAdapter.cs)

``` ini

BenchmarkDotNet=v0.10.9, OS=Windows 10 Redstone 1 (10.0.14393)
Processor=AMD A10 Extreme Edition Radeon R8, 4C+8G, ProcessorCount=4
Frequency=1949467 Hz, Resolution=512.9607 ns, Timer=TSC
.NET Core SDK=2.0.0
  [Host]    : .NET Core 2.0.0 (Framework 4.6.00001.0), 64bit RyuJIT
  RyuJitX64 : .NET Core 2.0.0 (Framework 4.6.00001.0), 64bit RyuJIT

Job=RyuJitX64  Jit=RyuJit  Platform=X64  

```
 |           Method |        Mean |     Error |    StdDev |      Median | Scaled | ScaledSD |  Gen 0 |  Gen 1 | Allocated |
 |----------------- |------------:|----------:|----------:|------------:|-------:|---------:|-------:|-------:|----------:|
 |           Simple |   235.58 ns |  4.935 ns | 10.729 ns |   237.90 ns |   1.00 |     0.00 | 0.1216 |      - |      64 B |
 |    Parameterized |   334.42 ns |  6.966 ns | 20.211 ns |   340.66 ns |   1.42 |     0.11 | 0.1674 |      - |      88 B |
 |         Original | 1,100.13 ns | 12.239 ns | 10.850 ns | 1,099.87 ns |   4.68 |     0.22 | 0.3399 | 0.0046 |     196 B |
 |        Microsoft |    79.35 ns |  1.791 ns |  2.329 ns |    79.92 ns |   0.34 |     0.02 |      - |      - |       0 B |
 | AdaptedMicrosoft |   243.91 ns |  5.221 ns | 15.394 ns |   246.22 ns |   1.04 |     0.08 | 0.1216 |      - |      64 B |

![](http://pomma89.altervista.org/objectpool/perf/RetrieveOneObject-barplot.png "Retrieve one object barplot")

### [Retrieve objects concurrently](https://github.com/pomma89/ObjectPool/blob/master/test/CodeProject.ObjectPool.Benchmarks/RetrieveObjectsConcurrently.cs)

In this benchmark we evaluate how long it takes to extract and return an object stored into the pool, using `Count` threads. We compare four implementations:

* [This project's ObjectPool](https://github.com/pomma89/ObjectPool/blob/master/src/CodeProject.ObjectPool/ObjectPool.cs)
* [This project's ParameterizedObjectPool](https://github.com/pomma89/ObjectPool/blob/master/src/CodeProject.ObjectPool/ParameterizedObjectPool.cs)
* [Original ObjectPool](http://www.codeproject.com/Articles/535735/Implementing-a-Generic-Object-Pool-in-NET)
* [Microsoft's ObjectPool](http://www.nuget.org/packages/Microsoft.Extensions.ObjectPool/)
* [This project's adapter for Microsoft's ObjectPool](https://github.com/pomma89/ObjectPool/blob/master/src/CodeProject.ObjectPool.MicrosoftExtensionsAdapter/ObjectPoolAdapter.cs)

``` ini

BenchmarkDotNet=v0.10.9, OS=Windows 10 Redstone 1 (10.0.14393)
Processor=AMD A10 Extreme Edition Radeon R8, 4C+8G, ProcessorCount=4
Frequency=1949467 Hz, Resolution=512.9607 ns, Timer=TSC
.NET Core SDK=2.0.0
  [Host]    : .NET Core 2.0.0 (Framework 4.6.00001.0), 64bit RyuJIT
  RyuJitX64 : .NET Core 2.0.0 (Framework 4.6.00001.0), 64bit RyuJIT

Job=RyuJitX64  Jit=RyuJit  Platform=X64  

```
 |           Method | Count |        Mean |      Error |     StdDev | Scaled | ScaledSD |    Gen 0 |   Gen 1 | Allocated |
 |----------------- |------ |------------:|-----------:|-----------:|-------:|---------:|---------:|--------:|----------:|
 |           **Simple** |    **10** |    **16.20 us** |  **0.3121 us** |  **0.4166 us** |   **1.00** |     **0.00** |   **4.9215** |       **-** |   **2.35 KB** |
 |    Parameterized |    10 |    16.04 us |  0.2204 us |  0.2062 us |   0.99 |     0.03 |   5.6030 |       - |   2.56 KB |
 |         Original |    10 |    18.60 us |  0.1700 us |  0.1420 us |   1.15 |     0.03 |   7.6222 |       - |      3 KB |
 |        Microsoft |    10 |    14.04 us |  0.2648 us |  0.2211 us |   0.87 |     0.03 |   3.6167 |       - |   1.74 KB |
 | AdaptedMicrosoft |    10 |    15.80 us |  0.2689 us |  0.2099 us |   0.98 |     0.03 |   5.0049 |       - |   2.35 KB |
 |           **Simple** |   **100** |    **66.00 us** |  **1.2006 us** |  **1.0643 us** |   **1.00** |     **0.00** |  **16.6273** |       **-** |   **4.75 KB** |
 |    Parameterized |   100 |    84.03 us |  1.6033 us |  1.5746 us |   1.27 |     0.03 |  21.5088 |       - |   5.09 KB |
 |         Original |   100 |   133.08 us |  1.0699 us |  1.0008 us |   2.02 |     0.03 |  43.2617 |       - |  14.15 KB |
 |        Microsoft |   100 |    32.72 us |  0.6511 us |  0.6395 us |   0.50 |     0.01 |   5.3711 |  0.9359 |   1.87 KB |
 | AdaptedMicrosoft |   100 |    74.85 us |  1.4805 us |  2.3908 us |   1.13 |     0.04 |  16.5876 |       - |   4.66 KB |
 |           **Simple** |  **1000** |   **756.57 us** |  **4.9338 us** |  **4.6151 us** |   **1.00** |     **0.00** | **128.0599** |       **-** |   **19.8 KB** |
 |    Parameterized |  1000 |   499.51 us |  3.4858 us |  2.7215 us |   0.66 |     0.01 | 174.9349 |       - |  25.92 KB |
 |         Original |  1000 | 1,222.56 us | 12.8086 us | 11.9812 us |   1.62 |     0.02 | 337.2396 |  9.3750 | 101.38 KB |
 |        Microsoft |  1000 |   428.70 us |  8.3805 us |  8.6061 us |   0.57 |     0.01 |  48.0572 | 15.8991 |   4.12 KB |
 | AdaptedMicrosoft |  1000 |   824.49 us |  6.4987 us |  6.0789 us |   1.09 |     0.01 | 128.0599 |       - |  19.67 KB |

![](http://pomma89.altervista.org/objectpool/perf/RetrieveObjectsConcurrently-barplot.png "Retrieve objects concurrently barplot")

### [Memory stream pooling](https://github.com/pomma89/ObjectPool/blob/master/test/CodeProject.ObjectPool.Benchmarks/MemoryStreamPooling.cs)

In this benchmark we evaluate how long it takes to extract and return a memory stream stored into the pool, using a single thread. We compare two implementations:

* [This project's MemoryStreamPool](https://github.com/pomma89/ObjectPool/blob/master/src/CodeProject.ObjectPool/Specialized/MemoryStreamPool.cs)
* [Microsoft's RecyclableMemoryStreamManager](http://www.nuget.org/packages/Microsoft.IO.RecyclableMemoryStream/)

``` ini

BenchmarkDotNet=v0.10.9, OS=Windows 10 Redstone 1 (10.0.14393)
Processor=AMD A10 Extreme Edition Radeon R8, 4C+8G, ProcessorCount=4
Frequency=1949467 Hz, Resolution=512.9607 ns, Timer=TSC
.NET Core SDK=2.0.0
  [Host]    : .NET Core 2.0.0 (Framework 4.6.00001.0), 64bit RyuJIT
  RyuJitX64 : .NET Core 2.0.0 (Framework 4.6.00001.0), 64bit RyuJIT

Job=RyuJitX64  Jit=RyuJit  Platform=X64  

```
 |                        Method |       Mean |     Error |    StdDev | Scaled | ScaledSD |  Gen 0 | Allocated |
 |------------------------------ |-----------:|----------:|----------:|-------:|---------:|-------:|----------:|
 |              MemoryStreamPool |   360.4 ns |  7.187 ns |  14.68 ns |   1.00 |     0.00 | 0.1216 |      64 B |
 | RecyclableMemoryStreamManager | 4,207.3 ns | 85.152 ns | 251.07 ns |  11.69 |     0.84 | 0.8469 |     448 B |

![](http://pomma89.altervista.org/objectpool/perf/MemoryStreamPooling-barplot.png "Memory stream pooling barplot")

## About this repository and its maintainer

Everything done on this repository is freely offered on the terms of the project license. You are free to do everything you want with the code and its related files, as long as you respect the license and use common sense while doing it :-)

I maintain this project during my spare time, so I can offer limited assistance and I can offer **no kind of warranty**.

However, if this project helps you, then you might offer me an hot cup of coffee:

[![Donate](http://pomma89.altervista.org/buy-me-a-coffee.png)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=ELJWKEYS9QGKA)
