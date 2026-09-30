# OoBDev.MongoDB

## BsonObjectIdConvention

Represents a convention for configuring BSON serialization of object IDs.

### Methods

#### Apply(MongoDB.Bson.Serialization.BsonMemberMap)

Applies a modification to the member map.

## IMongoDatabaseFactory

Provides a centralized means to create MongoDB instances.

### Methods

#### Create<T>

Factory method to create a MongoDB instance.

#### Create<TDocument, TSettings>

Factory method to create a MongoDB Database abstraction.

## IMongoDatabaseRegistration

Internal registry for MongoDB database connections.

### Properties

- **Types**: List of registered interfaces for MongoDB instances.

## IMongoSettings

Common pattern for declaring MongoDB settings.

### Properties

- **ConnectionString**: MongoDB Connection String
- **DatabaseName**: Name of the database to map for MongoDB

## MongoDatabaseFactory

Factory for creating MongoDB database instances.

### Methods

#### Constructor

Initializes a new instance of the class.

#### Create<T>

Creates a MongoDB database instance based on the provided settings.

#### Create<TDocument, TSettings>

Creates a proxied MongoDB database instance based on the provided settings.

#### Static Constructor

Registers serializers and conventions.

## MongoDatabaseOptions

Default connection information for MongoDB databases.

### Properties

- **ConnectionString**: Gets or sets the connection string for the MongoDB database.
- **DatabaseName**: Gets or sets the name of the MongoDB database.

### Methods

#### ToString

Returns a string that represents the current object.

## MongoDatabaseRegistration

Represents a registration of types for MongoDB databases.

### Properties

- **Types**: Gets the read-only collection of types registered for MongoDB databases.

## MongoDispatchProxy

This proxy allows for dynamic creation of wrapper classes to expose MongoDatabase instances.

## ServiceCollectionExtensions

Common libraries to enable MongoDB Support.

### Methods

#### TryAddMongoServices

Enable common infrastructure.

#### TryAddMongoDatabase<T>

Register a MongoDatabase instance with custom configuration options.

#### TryAddMongoDatabase<T>

Register a MongoDatabase instance that will use the MongoDatabaseOptions configuration options.
