using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.Generations.Tests;

public sealed class Person
{
    [GenerateString(Preset = StringPreset.Name)]
    public string Name { get; set; } = string.Empty;

    [GenerateString(Preset = StringPreset.Email)]
    public string Email { get; set; } = string.Empty;

    [GenerateNumber(Min = 18, Max = 90)]
    public int Age { get; set; }

    [GenerateDateTime(DaysBack = 10)]
    public DateTimeOffset Joined { get; set; }

    [GenerateCollection(MinCount = 2, MaxCount = 4)]
    public List<string> Tags { get; set; } = [];

    public Person? Friend { get; set; }
}

public interface IAccount
{
    string Id { get; }

    [GenerateNumber(Min = 1, Max = 5)]
    int Level { get; }
}

public sealed class Unsupported
{
    public Uri? Home { get; set; }
}

[TestCategory("Unit")]
[TestClass]
public sealed class GeneratorTests
{
    public TestContext TestContext { get; set; } = null!;

    private static readonly DateTimeOffset Now = new(2026, 11, 19, 12, 0, 0, TimeSpan.Zero);

    private static Generator With(int seed, int depth = 3) => new(seed, new FakeTimeProvider(Now), depth);

    [TestMethod]
    public void Create_SameSeed_IdenticalGraphs()
    {
        var a = With(7).Create<Person>();
        var b = With(7).Create<Person>();
        TestContext.WriteLine($"{a.Name} / {a.Email} / {a.Age}");

        Assert.AreEqual(a.Name, b.Name);
        Assert.AreEqual(a.Email, b.Email);
        Assert.AreEqual(a.Age, b.Age);
        Assert.AreEqual(a.Joined, b.Joined);
        CollectionAssert.AreEqual(a.Tags, b.Tags);
    }

    [TestMethod]
    public void Create_DifferentSeeds_GraphsDiffer()
    {
        var names = Enumerable.Range(1, 20).Select(s => With(s).Create<Person>().Email).Distinct().Count();

        Assert.IsTrue(names > 10);
    }

    [TestMethod]
    public void Create_ManySeeds_ValuesStayInsideTheirRanges()
    {
        for (var seed = 0; seed < 200; seed++)
        {
            var p = With(seed).Create<Person>();
            Assert.IsTrue(p.Age is >= 18 and <= 90, $"age {p.Age}");
            Assert.IsTrue(p.Tags.Count is >= 2 and <= 4);
            Assert.IsTrue(p.Joined <= Now && p.Joined >= Now.AddDays(-10));
            Assert.IsTrue(p.Email.EndsWith("@example.test", StringComparison.Ordinal));
        }
    }

    [TestMethod]
    public void Create_Interface_PropertiesFilledAndStable()
    {
        var account = With(3).Create<IAccount>();

        Assert.IsFalse(string.IsNullOrEmpty(account.Id));
        var id = account.Id;
        Assert.AreEqual(id, account.Id);
        Assert.IsTrue(account.Level is >= 1 and <= 5);
    }

    [TestMethod]
    public void Create_Cycle_StopsAtTheDepth()
    {
        var p = With(1, depth: 2).Create<Person>();

        Assert.IsNotNull(p.Friend);
        Assert.IsNull(p.Friend.Friend);
    }

    [TestMethod]
    public void Create_UnsupportedType_MessageNamesThePath()
    {
        var ex = Assert.ThrowsExactly<NotSupportedException>(() => With(1).Create<Unsupported>());

        TestContext.WriteLine(ex.Message);
        StringAssert.Contains(ex.Message, "Unsupported.Home");
    }
}
