using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace OoBDev.History.Tests;

[TestClass]
public class InMemoryHistoryStoreTests : HistoryStoreContract
{
    protected override Task<IHistoryStore> CreateAsync(HistoryOptions options) => Task.FromResult<IHistoryStore>(new InMemoryHistoryStore(options));
}
