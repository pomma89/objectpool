![](http://pomma89.altervista.org/objectpool/logo-64.png "Object Pool Logo") Object Pool
=============================================================================================================================

*A generic, concurrent, portable and flexible Object Pool for the .NET Framework, completely based on the [Code Project article of Ofir Makmal](http://www.codeproject.com/Articles/535735/Implementing-a-Generic-Object-Pool-in-NET).*

## Summary ##

* Latest release version: `v3.1.1`
* Build status on [AppVeyor](https://ci.appveyor.com): [![Build status](https://ci.appveyor.com/api/projects/status/r4qnqaqj9ri6cicn?svg=true)](https://ci.appveyor.com/project/pomma89/objectpool)
* [Doxygen](http://www.stack.nl/~dimitri/doxygen/index.html) documentation: 
    + [HTML](http://pomma89.altervista.org/objectpool/doc/html/index.html)
    + [CHM](http://pomma89.altervista.org/objectpool/doc/refman.chm)
    + [PDF](http://pomma89.altervista.org/objectpool/doc/refman.pdf)
* [NuGet](https://www.nuget.org) package(s):
    + [CodeProject.ObjectPool](https://nuget.org/packages/CodeProject.ObjectPool/)

## Introduction ##

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

## Benchmarks ##

All benchmarks were implemented and run using the wonderful [BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet) library.

### [Retrieve one object](https://github.com/pomma89/ObjectPool/blob/master/test/CodeProject.ObjectPool.Benchmarks/RetrieveOneObject.cs) ###

In this benchmark we evaluate how long it takes to extract and return an object stored into the pool, using a single thread. We compare four implementations:

* [This project's ObjectPool](https://github.com/pomma89/ObjectPool/blob/master/src/CodeProject.ObjectPool/ObjectPool.cs)
* [This project's ParameterizedObjectPool](https://github.com/pomma89/ObjectPool/blob/master/src/CodeProject.ObjectPool/ParameterizedObjectPool.cs)
* [Microsoft's ObjectPool](http://www.nuget.org/packages/Microsoft.Extensions.ObjectPool/)
* [Original ObjectPool](http://www.codeproject.com/Articles/535735/Implementing-a-Generic-Object-Pool-in-NET)

``` ini

BenchmarkDotNet=v0.10.3.0, OS=Microsoft Windows NT 6.2.9200.0
Processor=AMD A10 Extreme Edition Radeon R8, 4C+8G, ProcessorCount=4
Frequency=1949466 Hz, Resolution=512.9610 ns, Timer=TSC
  [Host]    : Clr 4.0.30319.42000, 64bit RyuJIT-v4.6.1637.0
  RyuJitX64 : Clr 4.0.30319.42000, 64bit RyuJIT-v4.6.1637.0

Job=RyuJitX64  Jit=RyuJit  Platform=X64  

```
 |        Method |          Mean |    StdErr |     StdDev |        Median | Scaled | Scaled-StdDev |  Gen 0 | Allocated |
 |-------------- |-------------- |---------- |----------- |-------------- |------- |-------------- |------- |---------- |
 |        Simple |   121.7587 ns | 2.6138 ns |  9.7801 ns |   117.3474 ns |   1.00 |          0.00 |      - |       0 B |
 | Parameterized |   180.3051 ns | 0.4339 ns |  1.5644 ns |   180.0446 ns |   1.49 |          0.10 | 0.0331 |      24 B |
 |     Microsoft |    60.0949 ns | 0.7015 ns |  5.0583 ns |    57.2039 ns |   0.50 |          0.05 |      - |       0 B |
 |      Original | 2,015.6827 ns | 8.4539 ns | 32.7417 ns | 2,023.0768 ns |  16.64 |          1.18 |      - |     239 B |

![](http://pomma89.altervista.org/objectpool/perf/RetrieveOneObject-barplot.png "Retrieve one object barplot")

### [Retrieve objects concurrently](https://github.com/pomma89/ObjectPool/blob/master/test/CodeProject.ObjectPool.Benchmarks/RetrieveObjectsConcurrently.cs) ###

In this benchmark we evaluate how long it takes to extract and return an object stored into the pool, using `Count` threads. We compare four implementations:

* [This project's ObjectPool](https://github.com/pomma89/ObjectPool/blob/master/src/CodeProject.ObjectPool/ObjectPool.cs)
* [This project's ParameterizedObjectPool](https://github.com/pomma89/ObjectPool/blob/master/src/CodeProject.ObjectPool/ParameterizedObjectPool.cs)
* [Microsoft's ObjectPool](http://www.nuget.org/packages/Microsoft.Extensions.ObjectPool/)
* [Original ObjectPool](http://www.codeproject.com/Articles/535735/Implementing-a-Generic-Object-Pool-in-NET)

``` ini

BenchmarkDotNet=v0.10.3.0, OS=Microsoft Windows NT 6.2.9200.0
Processor=AMD A10 Extreme Edition Radeon R8, 4C+8G, ProcessorCount=4
Frequency=1949466 Hz, Resolution=512.9610 ns, Timer=TSC
  [Host]    : Clr 4.0.30319.42000, 64bit RyuJIT-v4.6.1637.0
  RyuJitX64 : Clr 4.0.30319.42000, 64bit RyuJIT-v4.6.1637.0

Job=RyuJitX64  Jit=RyuJit  Platform=X64  

```
 |        Method | Count |          Mean |     StdErr |     StdDev | Scaled | Scaled-StdDev |   Gen 0 |   Gen 1 | Allocated |
 |-------------- |------ |-------------- |----------- |----------- |------- |-------------- |-------- |-------- |---------- |
 |        **Simple** |    **10** |    **12.5464 us** |  **0.0161 us** |  **0.0559 us** |   **1.00** |          **0.00** |  **2.0091** |       **-** |   **1.15 kB** |
 | Parameterized |    10 |    14.6937 us |  0.1460 us |  0.8758 us |   1.17 |          0.07 |  2.4578 |       - |   1.43 kB |
 |     Microsoft |    10 |     9.9922 us |  0.0994 us |  0.9325 us |   0.80 |          0.07 |  1.8722 |       - |   1.12 kB |
 |      Original |    10 |    27.0116 us |  0.1161 us |  0.4184 us |   2.15 |          0.03 |       - |       - |   3.72 kB |
 |        **Simple** |   **100** |    **69.6033 us** |  **0.4120 us** |  **1.4271 us** |   **1.00** |          **0.00** |  **0.0163** |       **-** |   **1.47 kB** |
 | Parameterized |   100 |    78.6620 us |  0.6899 us |  2.3897 us |   1.13 |          0.04 |  6.0963 |       - |   3.92 kB |
 |     Microsoft |   100 |    33.5158 us |  0.3347 us |  2.5710 us |   0.48 |          0.04 |  4.1996 |  0.7410 |   2.42 kB |
 |      Original |   100 |   177.8085 us |  0.8120 us |  3.1450 us |   2.56 |          0.07 |  3.6719 |       - |  27.01 kB |
 |        **Simple** |  **1000** |   **800.4478 us** |  **1.4427 us** |  **5.2017 us** |   **1.00** |          **0.00** |       **-** |       **-** |   **4.75 kB** |
 | Parameterized |  1000 |   847.4692 us |  2.5917 us | 10.0377 us |   1.06 |          0.01 | 31.3802 |       - |  27.91 kB |
 |     Microsoft |  1000 |   367.3571 us |  3.6502 us | 28.0380 us |   0.46 |          0.03 | 53.3482 | 10.9933 |  33.35 kB |
 |      Original |  1000 | 1,447.6767 us | 22.5522 us | 84.3827 us |   1.81 |          0.10 |       - |       - | 267.77 kB |

![](http://pomma89.altervista.org/objectpool/perf/RetrieveObjectsConcurrently-barplot.png "Retrieve objects concurrently barplot")

### [Memory stream pooling](https://github.com/pomma89/ObjectPool/blob/master/test/CodeProject.ObjectPool.Benchmarks/MemoryStreamPooling.cs) ###

In this benchmark we evaluate how long it takes to extract and return a memory stream stored into the pool, using a single thread. We compare two implementations:

* [This project's MemoryStreamPool](https://github.com/pomma89/ObjectPool/blob/master/src/CodeProject.ObjectPool/Specialized/MemoryStreamPool.cs)
* [Microsoft's RecyclableMemoryStreamManager](http://www.nuget.org/packages/Microsoft.IO.RecyclableMemoryStream/)

``` ini

BenchmarkDotNet=v0.10.3.0, OS=Microsoft Windows NT 6.2.9200.0
Processor=AMD A10 Extreme Edition Radeon R8, 4C+8G, ProcessorCount=4
Frequency=1949466 Hz, Resolution=512.9610 ns, Timer=TSC
  [Host]    : Clr 4.0.30319.42000, 64bit RyuJIT-v4.6.1637.0
  RyuJitX64 : Clr 4.0.30319.42000, 64bit RyuJIT-v4.6.1637.0

Job=RyuJitX64  Jit=RyuJit  Platform=X64  

```
 |                        Method |          Mean |     StdDev | Scaled | Scaled-StdDev |  Gen 0 | Allocated |
 |------------------------------ |-------------- |----------- |------- |-------------- |------- |---------- |
 |              MemoryStreamPool |   173.8324 ns |  5.5788 ns |   1.00 |          0.00 |      - |       0 B |
 | RecyclableMemoryStreamManager | 3,406.0877 ns | 88.6058 ns |  19.61 |          0.76 | 0.7796 |     448 B |

![](http://pomma89.altervista.org/objectpool/perf/MemoryStreamPooling-barplot.png "Memory stream pooling barplot")

## About this repository and its maintainer ##

Everything done on this repository is freely offered on the terms of the project license. You are free to do everything you want with the code and its related files, as long as you respect the license and use common sense while doing it :-)

I maintain this project during my spare time, so I can offer limited assistance and I can offer **no kind of warranty**.

Development of this project is sponsored by [Finsa SpA](https://www.finsa.it), my current employer.
