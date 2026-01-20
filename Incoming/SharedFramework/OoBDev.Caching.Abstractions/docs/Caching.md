# OoBDev - Caching Framework

## Configuration

| Key                                     | Notes                       | Options                                              | Default      |
| --------------------------------------- | --------------------------- | ---------------------------------------------------- | ------------ |
| OoBDev:Caching:Disabled              | Disable caching             | true, false                                          | false        |
| OoBDev:CachingProvider:Type          | Caching source provider     | RedisCachingProvider, MicrosoftMemoryCachingProvider |              |

Caching providers are based on possible providers registered with your application

## Setup

Ensure the IOC container has the `.AddOoBDevCachingServices()` and at least one of the caching provider.  

Example providers being `.AddMicrosoftCachingServices()` or `.AddRedisCachingServices()`.  Also ensure the chosen caching provider
is configured.

## Usage
 
For any interface that you want cached you will need to use this extensions pattern to register the class in the IOC container.

```csharp
using OoBDev.Caching.Abstractions;

// in your IOC registration
	.AddTransient(sp => sp.Cacheable<IExampleRepository, ExampleRepository>())
```

Methods that you want cached must be tagged with the `IsCacheable` attribute.  The caching key formatter can use parameters
to the method as part of the key value allowing multiple responses to be cached for the same method.  The life time is a timespan
formatted string to capture the maximum expected retention lifetime for the request.  The methods may return Task<T> or just 
regular objects.  

Note, if multiple methods are mapped to the same caching key they will try to return the same results.  

```csharp
    [IsCacheable("bucket1/set/{param1}/{param2}", "00:05:00")]
    public Task<ReturnModel[]> GetDataSet(string param1, string param2)

    [IsCacheable("bucket1/data/{param1}/{param2}", "01:00:00")]
    public Task<ReturnModel> GetData(string param1, string param2)
        
    [IsCacheable("bucket1/data/{model.Param1}/{model.Param2}", "00:00:30")]
    public Task GetByModel(ReturnModel model) 
```

If you need the ability to force clear cache you may tag a method with the `FlushCache` attribute.  It's caching key formatter works
the same as `IsCacheable`. Before the method is execute the system will automatically clear any cached value matching the key value.

```csharp
    [FlushCache("bucket1/data/{model.Param1}/{model.Param2}")]
    public Task UpdateData(ReturnModel model)
```