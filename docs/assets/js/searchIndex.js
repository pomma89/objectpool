
var camelCaseTokenizer = function (obj) {
    var previous = '';
    return obj.toString().trim().split(/[\s\-]+|(?=[A-Z])/).reduce(function(acc, cur) {
        var current = cur.toLowerCase();
        if(acc.length === 0) {
            previous = current;
            return acc.concat(current);
        }
        previous = previous.concat(current);
        return acc.concat([current, previous]);
    }, []);
}
lunr.tokenizer.registerFunction(camelCaseTokenizer, 'camelCaseTokenizer')
var searchModule = function() {
    var idMap = [];
    function y(e) { 
        idMap.push(e); 
    }
    var idx = lunr(function() {
        this.field('title', { boost: 10 });
        this.field('content');
        this.field('description', { boost: 5 });
        this.field('tags', { boost: 50 });
        this.ref('id');
        this.tokenizer(camelCaseTokenizer);

        this.pipeline.remove(lunr.stopWordFilter);
        this.pipeline.remove(lunr.stemmer);
    });
    function a(e) { 
        idx.add(e); 
    }

    a({
        id:0,
        title:"ObjectPoolAdapterForPooledObject",
        content:"ObjectPoolAdapterForPooledObject",
        description:'',
        tags:''
    });

    a({
        id:1,
        title:"EvictionTimer",
        content:"EvictionTimer",
        description:'',
        tags:''
    });

    a({
        id:2,
        title:"EvictionSettings",
        content:"EvictionSettings",
        description:'',
        tags:''
    });

    a({
        id:3,
        title:"ILogProvider",
        content:"ILogProvider",
        description:'',
        tags:''
    });

    a({
        id:4,
        title:"IParameterizedObjectPool",
        content:"IParameterizedObjectPool",
        description:'',
        tags:''
    });

    a({
        id:5,
        title:"Logger",
        content:"Logger",
        description:'',
        tags:''
    });

    a({
        id:6,
        title:"IObjectPool",
        content:"IObjectPool",
        description:'',
        tags:''
    });

    a({
        id:7,
        title:"LogProvider",
        content:"LogProvider",
        description:'',
        tags:''
    });

    a({
        id:8,
        title:"ITimedObjectPool",
        content:"ITimedObjectPool",
        description:'',
        tags:''
    });

    a({
        id:9,
        title:"PooledObjectDirection",
        content:"PooledObjectDirection",
        description:'',
        tags:''
    });

    a({
        id:10,
        title:"LogLevel",
        content:"LogLevel",
        description:'',
        tags:''
    });

    a({
        id:11,
        title:"PooledStringBuilder",
        content:"PooledStringBuilder",
        description:'',
        tags:''
    });

    a({
        id:12,
        title:"IEvictionTimer",
        content:"IEvictionTimer",
        description:'',
        tags:''
    });

    a({
        id:13,
        title:"MemoryStreamPool",
        content:"MemoryStreamPool",
        description:'',
        tags:''
    });

    a({
        id:14,
        title:"StringBuilderPool",
        content:"StringBuilderPool",
        description:'',
        tags:''
    });

    a({
        id:15,
        title:"PooledObjectState",
        content:"PooledObjectState",
        description:'',
        tags:''
    });

    a({
        id:16,
        title:"IStringBuilderPool",
        content:"IStringBuilderPool",
        description:'',
        tags:''
    });

    a({
        id:17,
        title:"ObjectPoolDiagnostics",
        content:"ObjectPoolDiagnostics",
        description:'',
        tags:''
    });

    a({
        id:18,
        title:"PooledMemoryStream",
        content:"PooledMemoryStream",
        description:'',
        tags:''
    });

    a({
        id:19,
        title:"TimedObjectPool",
        content:"TimedObjectPool",
        description:'',
        tags:''
    });

    a({
        id:20,
        title:"PooledObjectWrapper",
        content:"PooledObjectWrapper",
        description:'',
        tags:''
    });

    a({
        id:21,
        title:"ObjectPool",
        content:"ObjectPool",
        description:'',
        tags:''
    });

    a({
        id:22,
        title:"PooledObjectWrapper",
        content:"PooledObjectWrapper",
        description:'',
        tags:''
    });

    a({
        id:23,
        title:"PooledObjectInfo",
        content:"PooledObjectInfo",
        description:'',
        tags:''
    });

    a({
        id:24,
        title:"ObjectPoolAdapter",
        content:"ObjectPoolAdapter",
        description:'',
        tags:''
    });

    a({
        id:25,
        title:"ObjectPoolAdapter",
        content:"ObjectPoolAdapter",
        description:'',
        tags:''
    });

    a({
        id:26,
        title:"ObjectPool",
        content:"ObjectPool",
        description:'',
        tags:''
    });

    a({
        id:27,
        title:"PooledObjectValidationContext",
        content:"PooledObjectValidationContext",
        description:'',
        tags:''
    });

    a({
        id:28,
        title:"IMemoryStreamPool",
        content:"IMemoryStreamPool",
        description:'',
        tags:''
    });

    a({
        id:29,
        title:"ParameterizedObjectPool",
        content:"ParameterizedObjectPool",
        description:'',
        tags:''
    });

    a({
        id:30,
        title:"PooledObjectBuffer",
        content:"PooledObjectBuffer",
        description:'',
        tags:''
    });

    a({
        id:31,
        title:"PooledObject",
        content:"PooledObject",
        description:'',
        tags:''
    });

    y({
        url:'/ObjectPool/api/CodeProject.ObjectPool.MicrosoftExtensionsAdapter/ObjectPoolAdapterForPooledObject_1',
        title:"ObjectPoolAdapterForPooledObject<T>",
        description:""
    });

    y({
        url:'/ObjectPool/api/CodeProject.ObjectPool/EvictionTimer',
        title:"EvictionTimer",
        description:""
    });

    y({
        url:'/ObjectPool/api/CodeProject.ObjectPool/EvictionSettings',
        title:"EvictionSettings",
        description:""
    });

    y({
        url:'/ObjectPool/api/CodeProject.ObjectPool.Logging/ILogProvider',
        title:"ILogProvider",
        description:""
    });

    y({
        url:'/ObjectPool/api/CodeProject.ObjectPool/IParameterizedObjectPool_2',
        title:"IParameterizedObjectPool<TKey, TValue>",
        description:""
    });

    y({
        url:'/ObjectPool/api/CodeProject.ObjectPool.Logging/Logger',
        title:"Logger",
        description:""
    });

    y({
        url:'/ObjectPool/api/CodeProject.ObjectPool/IObjectPool_1',
        title:"IObjectPool<T>",
        description:""
    });

    y({
        url:'/ObjectPool/api/CodeProject.ObjectPool.Logging/LogProvider',
        title:"LogProvider",
        description:""
    });

    y({
        url:'/ObjectPool/api/CodeProject.ObjectPool/ITimedObjectPool_1',
        title:"ITimedObjectPool<T>",
        description:""
    });

    y({
        url:'/ObjectPool/api/CodeProject.ObjectPool.Core/PooledObjectDirection',
        title:"PooledObjectDirection",
        description:""
    });

    y({
        url:'/ObjectPool/api/CodeProject.ObjectPool.Logging/LogLevel',
        title:"LogLevel",
        description:""
    });

    y({
        url:'/ObjectPool/api/CodeProject.ObjectPool.Specialized/PooledStringBuilder',
        title:"PooledStringBuilder",
        description:""
    });

    y({
        url:'/ObjectPool/api/CodeProject.ObjectPool/IEvictionTimer',
        title:"IEvictionTimer",
        description:""
    });

    y({
        url:'/ObjectPool/api/CodeProject.ObjectPool.Specialized/MemoryStreamPool',
        title:"MemoryStreamPool",
        description:""
    });

    y({
        url:'/ObjectPool/api/CodeProject.ObjectPool.Specialized/StringBuilderPool',
        title:"StringBuilderPool",
        description:""
    });

    y({
        url:'/ObjectPool/api/CodeProject.ObjectPool.Core/PooledObjectState',
        title:"PooledObjectState",
        description:""
    });

    y({
        url:'/ObjectPool/api/CodeProject.ObjectPool.Specialized/IStringBuilderPool',
        title:"IStringBuilderPool",
        description:""
    });

    y({
        url:'/ObjectPool/api/CodeProject.ObjectPool.Core/ObjectPoolDiagnostics',
        title:"ObjectPoolDiagnostics",
        description:""
    });

    y({
        url:'/ObjectPool/api/CodeProject.ObjectPool.Specialized/PooledMemoryStream',
        title:"PooledMemoryStream",
        description:""
    });

    y({
        url:'/ObjectPool/api/CodeProject.ObjectPool/TimedObjectPool_1',
        title:"TimedObjectPool<T>",
        description:""
    });

    y({
        url:'/ObjectPool/api/CodeProject.ObjectPool/PooledObjectWrapper',
        title:"PooledObjectWrapper",
        description:""
    });

    y({
        url:'/ObjectPool/api/CodeProject.ObjectPool/ObjectPool',
        title:"ObjectPool",
        description:""
    });

    y({
        url:'/ObjectPool/api/CodeProject.ObjectPool/PooledObjectWrapper_1',
        title:"PooledObjectWrapper<T>",
        description:""
    });

    y({
        url:'/ObjectPool/api/CodeProject.ObjectPool.Core/PooledObjectInfo',
        title:"PooledObjectInfo",
        description:""
    });

    y({
        url:'/ObjectPool/api/CodeProject.ObjectPool.MicrosoftExtensionsAdapter/ObjectPoolAdapter',
        title:"ObjectPoolAdapter",
        description:""
    });

    y({
        url:'/ObjectPool/api/CodeProject.ObjectPool.MicrosoftExtensionsAdapter/ObjectPoolAdapter_1',
        title:"ObjectPoolAdapter<T>",
        description:""
    });

    y({
        url:'/ObjectPool/api/CodeProject.ObjectPool/ObjectPool_1',
        title:"ObjectPool<T>",
        description:""
    });

    y({
        url:'/ObjectPool/api/CodeProject.ObjectPool.Core/PooledObjectValidationContext',
        title:"PooledObjectValidationContext",
        description:""
    });

    y({
        url:'/ObjectPool/api/CodeProject.ObjectPool.Specialized/IMemoryStreamPool',
        title:"IMemoryStreamPool",
        description:""
    });

    y({
        url:'/ObjectPool/api/CodeProject.ObjectPool/ParameterizedObjectPool_2',
        title:"ParameterizedObjectPool<TKey, TValue>",
        description:""
    });

    y({
        url:'/ObjectPool/api/CodeProject.ObjectPool.Core/PooledObjectBuffer_1',
        title:"PooledObjectBuffer<T>",
        description:""
    });

    y({
        url:'/ObjectPool/api/CodeProject.ObjectPool/PooledObject',
        title:"PooledObject",
        description:""
    });

    return {
        search: function(q) {
            return idx.search(q).map(function(i) {
                return idMap[i.ref];
            });
        }
    };
}();
