![](http://pomma89.altervista.org/objectpool/logo-64.png "Object Pool Logo") Object Pool
=============================================================================================================================

A generic, concurrent, portable and flexible Object Pool for the .NET Framework, completely based on the [Code Project article of Ofir Makmal](http://www.codeproject.com/Articles/535735/Implementing-a-Generic-Object-Pool-in-NET).

## Summary ##

* Latest release version: `v3.0.3`
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

All benchmarks were implemented and run using the wonderful [BenchmarkDotNet](https://github.com/PerfDotNet/BenchmarkDotNet) library.

### [Retrieve one object](https://github.com/pomma89/ObjectPool/blob/master/ObjectPool.Benchmarks/RetrieveOneObject.cs) ###

In this benchmark we evaluate how long it takes to extract and return an object stored into the pool, using a single thread. We compare four implementations:

* [This project's ObjectPool](https://github.com/pomma89/ObjectPool/blob/master/ObjectPool/ObjectPool.cs)
* [This project's ParameterizedObjectPool](https://github.com/pomma89/ObjectPool/blob/master/ObjectPool/ParameterizedObjectPool.cs)
* [Microsoft's ObjectPool](http://www.nuget.org/packages/Microsoft.Extensions.ObjectPool/)
* [Original ObjectPool](http://www.codeproject.com/Articles/535735/Implementing-a-Generic-Object-Pool-in-NET)

``` ini

BenchmarkDotNet=v0.10.3.0, OS=Microsoft Windows NT 6.2.9200.0
Processor=AMD A10 Extreme Edition Radeon R8, 4C+8G, ProcessorCount=4
Frequency=1949470 Hz, Resolution=512.9599 ns, Timer=TSC
  [Host]    : Clr 4.0.30319.42000, 64bit RyuJIT-v4.6.1637.0
  RyuJitX64 : Clr 4.0.30319.42000, 64bit RyuJIT-v4.6.1637.0

Job=RyuJitX64  Jit=RyuJit  Platform=X64  

```
 |                  Method |          Mean |     StdDev |  Gen 0 | Allocated |
 |------------------------ |-------------- |----------- |------- |---------- |
 |        SimpleObjectPool |   106.3367 ns |  1.9033 ns |      - |       0 B |
 | ParameterizedObjectPool |   174.2507 ns |  1.9017 ns | 0.0391 |      24 B |
 |     MicrosoftObjectPool |    59.3673 ns |  1.2349 ns |      - |       0 B |
 |      OriginalObjectPool | 1,773.9186 ns | 96.7615 ns | 0.0238 |     240 B |

### [Retrieve objects concurrently](https://github.com/pomma89/ObjectPool/blob/master/ObjectPool.Benchmarks/RetrieveObjectsConcurrently.cs) ###

In this benchmark we evaluate how long it takes to extract and return an object stored into the pool, using `Count` threads. We compare four implementations:

* [This project's ObjectPool](https://github.com/pomma89/ObjectPool/blob/master/ObjectPool/ObjectPool.cs)
* [This project's ParameterizedObjectPool](https://github.com/pomma89/ObjectPool/blob/master/ObjectPool/ParameterizedObjectPool.cs)
* [Microsoft's ObjectPool](http://www.nuget.org/packages/Microsoft.Extensions.ObjectPool/)
* [Original ObjectPool](http://www.codeproject.com/Articles/535735/Implementing-a-Generic-Object-Pool-in-NET)

``` ini

BenchmarkDotNet=v0.10.3.0, OS=Microsoft Windows NT 6.2.9200.0
Processor=AMD A10 Extreme Edition Radeon R8, 4C+8G, ProcessorCount=4
Frequency=1949470 Hz, Resolution=512.9599 ns, Timer=TSC
  [Host]    : Clr 4.0.30319.42000, 64bit RyuJIT-v4.6.1637.0
  RyuJitX64 : Clr 4.0.30319.42000, 64bit RyuJIT-v4.6.1637.0

Job=RyuJitX64  Jit=RyuJit  Platform=X64  

```
 |                  Method | Count |          Mean |     StdDev |   Gen 0 |   Gen 1 | Allocated |
 |------------------------ |------ |-------------- |----------- |-------- |-------- |---------- |
 |        **SimpleObjectPool** |    **10** |    **10.1346 us** |  **0.3799 us** |  **1.7548** |       **-** |   **1.12 kB** |
 | ParameterizedObjectPool |    10 |    13.6740 us |  0.3560 us |  1.9409 |       - |   1.42 kB |
 |     MicrosoftObjectPool |    10 |     9.3135 us |  0.1154 us |  1.7008 |       - |   1.12 kB |
 |      OriginalObjectPool |    10 |    26.4688 us |  0.7511 us |       - |       - |    3.7 kB |
 |        **SimpleObjectPool** |   **100** |    **54.1560 us** |  **1.7507 us** |       **-** |       **-** |   **1.31 kB** |
 | ParameterizedObjectPool |   100 |    72.3400 us |  0.8960 us |  4.8177 |       - |   3.93 kB |
 |     MicrosoftObjectPool |   100 |    30.9284 us |  1.1752 us |  2.9975 |       - |   2.16 kB |
 |      OriginalObjectPool |   100 |   177.0052 us |  3.7795 us |  1.7904 |       - |  27.04 kB |
 |        **SimpleObjectPool** |  **1000** |   **689.5726 us** | **21.9610 us** |       **-** |       **-** |   **4.07 kB** |
 | ParameterizedObjectPool |  1000 |   844.7284 us | 19.1851 us | 10.9863 |       - |  28.18 kB |
 |     MicrosoftObjectPool |  1000 |   362.2347 us | 21.3030 us | 51.1351 | 15.1438 |   28.3 kB |
 |      OriginalObjectPool |  1000 | 1,458.5456 us | 25.8381 us | 29.1667 |       - | 268.99 kB |


### [Memory stream pooling](https://github.com/pomma89/ObjectPool/blob/master/ObjectPool.Benchmarks/MemoryStreamPooling.cs) ###

In this benchmark we evaluate how long it takes to extract and return a memory stream stored into the pool, using a single thread. We compare two implementations:

* [This project's MemoryStreamPool](https://github.com/pomma89/ObjectPool/blob/master/ObjectPool/Specialized/MemoryStreamPool.cs)
* [Microsoft's RecyclableMemoryStreamManager](http://www.nuget.org/packages/Microsoft.IO.RecyclableMemoryStream/)

``` ini

BenchmarkDotNet=v0.10.3.0, OS=Microsoft Windows NT 6.2.9200.0
Processor=AMD A10 Extreme Edition Radeon R8, 4C+8G, ProcessorCount=4
Frequency=1949470 Hz, Resolution=512.9599 ns, Timer=TSC
  [Host]    : Clr 4.0.30319.42000, 64bit RyuJIT-v4.6.1637.0
  RyuJitX64 : Clr 4.0.30319.42000, 64bit RyuJIT-v4.6.1637.0

Job=RyuJitX64  Jit=RyuJit  Platform=X64  

```
 |                        Method |          Mean |     StdErr |      StdDev |  Gen 0 | Allocated |
 |------------------------------ |-------------- |----------- |------------ |------- |---------- |
 |              MemoryStreamPool |   180.0207 ns |  1.6126 ns |   6.0337 ns |      - |       0 B |
 | RecyclableMemoryStreamManager | 4,213.5708 ns | 44.6009 ns | 446.0087 ns | 0.8134 |     448 B |

## About this repository and its maintainer ##

Everything done on this repository is freely offered on the terms of the project license. You are free to do everything you want with the code and its related files, as long as you respect the license and use common sense while doing it :-)

I maintain this project during my spare time, so I can offer limited assistance and I can offer **no kind of warranty**.

Development of this project is sponsored by [Finsa SpA](https://www.finsa.it), my current employer.
