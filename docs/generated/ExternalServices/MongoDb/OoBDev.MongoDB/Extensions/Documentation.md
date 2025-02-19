Here is the documentation for the source code files, including class diagrams in PlantUML:

**BsonObjectIdConvention.cs**

* Class: `BsonObjectIdConvention` (`IMemberMapConvention`)
* Summary: Represents a convention for configuring BSON serialization of object IDs.
* Apply method: Modifies the member map to set the type of the object ID property to `ObjectId`.

[PlantUML class diagram]
```plantuml
@startuml
class BsonObjectIdConvention implements IMemberMapConvention {
  - apply(BsonMemberMap memberMap)
}
@enduml
```

**IAtlasVectorSearch.cs**

* Interface: `IAtlasVectorSearch`
* Summary: Provides a centralized means to create the index for Atlas Vector Search.
* Properties: `Type`, `Path`, `NumDimensions`, and `Similarity`.

[PlantUML interface diagram]
```plantuml
@startuml
interface IAtlasVectorSearch {
  - string Type
  - string Path
  - int NumDimensions
  - int Similarity
}
@enduml
```

**IMongoDatabaseFactory.cs**

* Interface: `IMongoDatabaseFactory`
* Summary: Provides a centralized means to create MongoDB instances.
* Methods: `Create<TSettings>()` and `Create<TDatabase, TSettings>()`.

[PlantUML interface diagram]
```plantuml
@startuml
interface IMongoDatabaseFactory {
  - <TSettings> IMongoDatabase Create<TSettings>()
  - <TDatabase, TSettings> TDatabase Create<TDatabase, TSettings>()
}
@enduml
```

**IMongoDatabaseRegistration.cs**

* Interface: `IMongoDatabaseRegistration`
* Summary: Internal registry for MongoDatabase connections.
* Property: `Types` (IReadOnlyCollection of Type).

[PlantUML interface diagram]
```plantuml
@startuml
interface IMongoDatabaseRegistration {
  - IReadOnlyCollection<Type> Types
}
@enduml
```

**IMongoSettings.cs**

* Interface: `IMongoSettings`
* Summary: Common pattern for declaring MongoDB settings.
* Properties: `ConnectionString` and `DatabaseName`.

[PlantUML interface diagram]
```plantuml
@startuml
interface IMongoSettings {
  - string ConnectionString
  - string DatabaseName
}
@enduml
```

**IMongoVectorSearch.cs**

* Interface: `IMongoVectorSearch`
* Summary: Provides a centralized means to create Atlas Vector Search.
* Method: `CreateVectorSearch<TSettings>()`.

[PlantUML interface diagram]
```plantuml
@startuml
interface IMongoVectorSearch {
  - <TSettings> IMongoDatabase CreateVectorSearch<TSettings>()
}
@enduml
```

**MongoDatabaseFactory.cs**

* Class: `MongoDatabaseFactory` (`IMongoDatabaseFactory`)
* Summary: Factory for creating MongoDB database instances.
* Methods: `Create<TSettings>()` and `Create<TDatabase, TSettings>()`.
* Static constructor: Registers serializers and conventions.

[PlantUML class diagram]
```plantuml
@startuml
class MongoDatabaseFactory implements IMongoDatabaseFactory {
  - Create<TSettings>()
  - Create<TDatabase, TSettings>()
  static constructor: Register serializers and conventions
}
@enduml
```

**MongoDatabaseOptions.cs**

* Class: `MongoDatabaseOptions` (`IMongoSettings`)
* Summary: Default connection information for MongoDB databases.
* Properties: `ConnectionString` and `DatabaseName`.

[PlantUML class diagram]
```plantuml
@startuml
class MongoDatabaseOptions implements IMongoSettings {
  - string ConnectionString
  - string DatabaseName
}
@enduml
```

**MongoDatabaseRegistration.cs**

* Class: `MongoDatabaseRegistration` (`IMongoDatabaseRegistration`)
* Summary: Represents a registration of types for MongoDB databases.
* Property: `InternalTypes` (HashSet of Type).

[PlantUML class diagram]
```plantuml
@startuml
class MongoDatabaseRegistration implements IMongoDatabaseRegistration {
  - internal HashSet<Type> InternalTypes
}
@enduml
```

**MongoDispatchProxy.cs**

* Class: `MongoDispatchProxy` (`DispatchProxy`)
* Summary: This proxy allows for dynamic creation of wrapper classes to expose MongoDatabase instances.
* Fields: `_database`, `_settings`, `_jsonSerializer`, and `_collections`.
* Method: `Invoke(MethodInfo, object[])`.

[PlantUML class diagram]
```plantuml
@startuml
class MongoDispatchProxy extends DispatchProxy {
  - _database: IMongoDatabase
  - _settings: IMongoSettings
  - _jsonSerializer: IJsonSerializer
  - _collections: Dictionary<MethodInfo, object>
  - Invoke(MethodInfo, object[])

  static method Create<T>(IMongoDatabase database, IMongoSettings settings, IJsonSerializer jsonSerializer)
}
@enduml
```

I hope this helps! Let me know if you have any further requests.