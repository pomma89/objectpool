A generic, concurrent, portable and flexible Object Pool for the .NET Framework, completely based on the Code Project article of Ofir Makmal ([http://goo.gl/4qig6T](http://goo.gl/4qig6T)).
Library is production ready and it is successfully working in real life systems.

Original source code has been modified, in order to introduce a Parameterized Object Pool, already drafted by Ofir Makmal in the comments of the article. 
Moreover, a few unit tests have been added, in order to improve code reliability, and a lot of other small changes have also been applied. 
Of course, all modified source code is freely available in this repository.

Many thanks to Ofir Makmal for his great work.

Build status: [![Build status](https://ci.appveyor.com/api/projects/status/xaon6fgwal0vjbhl)](https://ci.appveyor.com/project/pomma89/objectpool)

Quick example:


```
#!c#

internal static class Program
{
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
        // External Resource reset state code
    }

    public static void ExternalResourceReleaseResource(ExternalExpensiveResource resource)
    {
        // External Resource release code
    }
}

public class ExpensiveResource : PooledObject
{
    public void DoStuff()
    {
        // Do some work here, for example
    }

    protected override void OnReleaseResources()
    {
        // Override if the resource needs to be manually cleaned before the memory is reclaimed
    }

    protected override void OnResetState()
    {
        // Override if the resource needs resetting before it is getting back into the pool
    }
}

public class ExternalExpensiveResource
{
    public void DoOtherStuff()
    {
        // Do some work here, for example
    }
}
```