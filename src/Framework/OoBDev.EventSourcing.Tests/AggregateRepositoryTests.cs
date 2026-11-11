using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.Threading.Tasks;

namespace OoBDev.EventSourcing.Tests;

[TestClass]
public class AggregateRepositoryTests
{
    [EventType("account-opened")]
    public sealed record AccountOpened(string Id);

    public sealed record MoneyDeposited(decimal Amount);

    public sealed class Account : AggregateRoot
    {
        public decimal Balance { get; private set; }

        public void Open(string id) => Raise(new AccountOpened(id));

        public void Deposit(decimal amount) => Raise(new MoneyDeposited(amount));

        protected override void Apply(object @event)
        {
            switch (@event)
            {
                case AccountOpened opened:
                    Id = opened.Id;
                    break;
                case MoneyDeposited deposited:
                    Balance += deposited.Amount;
                    break;
            }
        }
    }

    private static AggregateRepository<Account> Create(IEventStore? store = null)
        => new(store ?? new InMemoryEventStore(TimeProvider.System), new EventTypeRegistry().Register<AccountOpened>().Register<MoneyDeposited>());

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task SaveAsync_ThenLoadAsync_RebuildsState()
    {
        // Stage
        var repository = Create();
        var account = new Account();
        account.Open("a1");
        account.Deposit(5m);
        account.Deposit(7.5m);

        // Test
        await repository.SaveAsync(account);
        var loaded = await repository.LoadAsync("a1");

        // Assert
        Assert.AreEqual(3, account.Version);
        Assert.AreEqual(0, account.UncommittedEvents.Count);
        Assert.IsNotNull(loaded);
        Assert.AreEqual(12.5m, loaded.Balance);
        Assert.AreEqual(3, loaded.Version);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task LoadAsync_UnknownId_ReturnsNull()
        => Assert.IsNull(await Create().LoadAsync("missing"));

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task SaveAsync_UsesAttributeNameAndClassName()
    {
        // Stage
        var store = new InMemoryEventStore(TimeProvider.System);
        var account = new Account();
        account.Open("a1");
        account.Deposit(1m);

        // Test
        await Create(store).SaveAsync(account);

        // Assert
        var records = await store.ReadAsync(AggregateRepository<Account>.StreamIdOf("a1"));
        Assert.AreEqual("account-opened", records[0].Type);
        Assert.AreEqual("MoneyDeposited", records[1].Type);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task SaveAsync_ConcurrentWriter_Throws()
    {
        // Stage
        var repository = Create();
        var first = new Account();
        first.Open("a1");
        await repository.SaveAsync(first);
        var one = (await repository.LoadAsync("a1"))!;
        var two = (await repository.LoadAsync("a1"))!;
        one.Deposit(1m);
        two.Deposit(2m);
        await repository.SaveAsync(one);

        // Test and Assert
        await Assert.ThrowsExactlyAsync<EventStreamConflictException>(() => repository.SaveAsync(two));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task SaveAsync_NoChanges_DoesNothing()
    {
        // Stage
        var store = new InMemoryEventStore(TimeProvider.System);

        // Test
        await Create(store).SaveAsync(new Account());

        // Assert
        Assert.AreEqual(0, (await store.ReadAllAsync(0)).Count);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Register_DuplicateName_Throws()
        => Assert.ThrowsExactly<InvalidOperationException>(
            () => new EventTypeRegistry().Register<AccountOpened>().Register(typeof(Other)));

    [EventType("account-opened")]
    private sealed record Other;

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task AddEventSourcing_ResolvesTheRepository()
    {
        // Stage
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        using var provider = services.TryAddEventSourcing().BuildServiceProvider();
        provider.GetRequiredService<EventTypeRegistry>().Register<AccountOpened>().Register<MoneyDeposited>();
        var account = new Account();
        account.Open("a9");

        // Test
        var repository = provider.GetRequiredService<IAggregateRepository<Account>>();
        await repository.SaveAsync(account);

        // Assert
        Assert.IsNotNull(await repository.LoadAsync("a9"));
    }
}
