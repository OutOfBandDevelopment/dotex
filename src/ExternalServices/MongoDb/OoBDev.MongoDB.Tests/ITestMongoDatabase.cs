using MongoDB.Driver;

namespace OoBDev.MongoDB.Tests;

public interface ITestMongoDatabase
{
    IMongoCollection<TestCollection> Tests { get; }
}
