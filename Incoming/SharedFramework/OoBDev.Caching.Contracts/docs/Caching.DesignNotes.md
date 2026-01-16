# OoBDev - Caching Framework

### Design Notes

I will write an operation that will be used at the IOC Registration for classes to build out a proxy class that will support caching.  
                something like services.AddTransiant<TInterface> (sp => CacheFactory<TInterface, TImplementation>.Build(sp))
Caching will be controlled first by the object registration noted above then with an Attribute declaration on the method to be cached.  
                This attribute will have a formatter string to build out the caching key and will receive the parameters from the method to build out the key
                This attribute will also have a parameter to set the lifetime for the caching instance (assuming lifetime is supported in the caching store)  
                This attribute will be put on the implementation class not the 
 
First planned implementation will be the User/Roles and other claim data backing the IUserSession interface.
We will have a global configuration value to bypass 

Caching will have synchronous persistence 
                if caching enabled
                                if value cached 
                                                return cached value
                                If cache miss
                                                Query data, persist to cache, return value
                                If cache timeout
                                                Query and return 
                Else
                                Query and return

Intention is to use Azure Redis cache for deployed services
                will plan to use docker container for local development

This will all follow my normal level of crazy so I’ll be adding at least a OoBDev.Caching.Contracts 
and a OoBDev.Api.Redis.Caching to the OoBDev.SharedFramework


in theory this will be able to cache anything in the IOC container as I am not going to add any 
artificial limitations but the intention would be only deterministic queries on the repository classes.
 
Bonus round would be to create some short of caching flushing support somewhere along the lines if 
either a operation that takes an expression tree of the matched lookup query and/or an attribute to 
identity the matched query/keys to purge.      
