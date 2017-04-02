![](http://pomma89.altervista.org/objectpool/logo-64.png "Object Pool Logo") Object Pool
=============================================================================================================================

A generic, concurrent, portable and flexible Object Pool for the .NET Framework, completely based on the [Code Project article of Ofir Makmal](http://www.codeproject.com/Articles/535735/Implementing-a-Generic-Object-Pool-in-NET).

## Summary ##

* Latest release version: `v3.0.2`
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

All benchmarks were implemented and run using the wonderful [BenchmarkDotNet](https://github.com/PerfDotNet/BenchmarkDotNet) library.

### [Retrieve one object](https://github.com/pomma89/ObjectPool/blob/master/ObjectPool.Benchmarks/RetrieveOneObject.cs) ###

In this benchmark we evaluate how long it takes to extract and return an object stored into the pool, using a single thread. We compare three implementations:

* [This project's ObjectPool](https://github.com/pomma89/ObjectPool/blob/master/ObjectPool/ObjectPool.cs)
* [This project's ParameterizedObjectPool](https://github.com/pomma89/ObjectPool/blob/master/ObjectPool/ParameterizedObjectPool.cs)
* [Microsoft's ObjectPool](http://www.nuget.org/packages/Microsoft.Extensions.ObjectPool/)

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
        SimpleObjectPool | 136.0373 ns | 3.4367 ns | 11.02 |  9.58 |  9.58 |               3,09 |
 ParameterizedObjectPool | 199.7968 ns | 4.4435 ns | 21.00 |     - |     - |               3,08 |
     MicrosoftObjectPool |  60.7935 ns | 1.8593 ns |     - |     - |     - |               0,00 |

### [Retrieve objects concurrently](https://github.com/pomma89/ObjectPool/blob/master/ObjectPool.Benchmarks/RetrieveObjectsConcurrently.cs) ###

In this benchmark we evaluate how long it takes to extract and return an object stored into the pool, using `Count` threads. We compare three implementations:

* [This project's ObjectPool](https://github.com/pomma89/ObjectPool/blob/master/ObjectPool/ObjectPool.cs)
* [This project's ParameterizedObjectPool](https://github.com/pomma89/ObjectPool/blob/master/ObjectPool/ParameterizedObjectPool.cs)
* [Microsoft's ObjectPool](http://www.nuget.org/packages/Microsoft.Extensions.ObjectPool/)

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
        **SimpleObjectPool** |    **10** |   **5.7390 us** | **0.3109 us** |  **1.72** |  **0.72** |  **0.07** |             **278,68** |
 ParameterizedObjectPool |    10 |   6.4189 us | 1.1792 us |  1.58 |  1.23 |     - |             284,70 |
     MicrosoftObjectPool |    10 |   4.2985 us | 0.0975 us |  1.30 |  1.11 |     - |             244,84 |
        **SimpleObjectPool** |   **100** |  **19.6048 us** | **1.2525 us** |  **3.04** |  **2.32** |  **0.73** |             **576,85** |
 ParameterizedObjectPool |   100 |  27.8705 us | 1.9570 us |  3.09 |  2.81 |     - |             560,35 |
     MicrosoftObjectPool |   100 |  10.5264 us | 1.5110 us |  1.78 |  0.97 |  0.03 |             233,25 |
        **SimpleObjectPool** |  **1000** | **157.9986 us** | **6.2393 us** | **12.67** | **12.12** |  **9.92** |           **3.277,64** |
 ParameterizedObjectPool |  1000 | 223.4054 us | 7.6698 us | 20.00 |     - |     - |           2.697,82 |
     MicrosoftObjectPool |  1000 |  76.3736 us | 0.8204 us |  5.17 |  0.29 |     - |             247,38 |

### [Memory stream pooling](https://github.com/pomma89/ObjectPool/blob/master/ObjectPool.Benchmarks/MemoryStreamPooling.cs) ###

In this benchmark we evaluate how long it takes to extract and return a memory stream stored into the pool, using a single thread. We compare three implementations:

* [This project's MemoryStreamPool](https://github.com/pomma89/ObjectPool/blob/master/ObjectPool/Specialized/MemoryStreamPool.cs)
* [Microsoft's RecyclableMemoryStreamManager](http://www.nuget.org/packages/Microsoft.IO.RecyclableMemoryStream/)

```ini

Host Process Environment Information:
BenchmarkDotNet.Core=v0.9.9.0
OS=Microsoft Windows NT 6.2.9200.0
Processor=Intel(R) Core(TM) i3-2330M CPU 2.20GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
CLR=MS.NET 4.0.30319.42000, Arch=32-bit RELEASE
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1586.0

Type=MemoryStreamPooling  Mode=Throughput  

```
                        Method |        Median |      StdDev |  Gen 0 | Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------------ |-------------- |------------ |------- |------ |------ |------------------- |
              MemoryStreamPool |   171.7594 ns |   4.7127 ns |   1.46 |  1.26 |  1.26 |               3,08 |
 RecyclableMemoryStreamManager | 2,924.0579 ns | 117.2652 ns | 341.00 |     - |     - |              88,74 |

## About this repository and its maintainer ##

Everything done on this repository is freely offered on the terms of the project license. You are free to do everything you want with the code and its related files, as long as you respect the license and use common sense while doing it :-)

I maintain this project during my spare time, so I can offer limited assistance and I can offer **no kind of warranty**.

Development of this project is sponsored by [Finsa SpA](https://www.finsa.it), my current employer.
