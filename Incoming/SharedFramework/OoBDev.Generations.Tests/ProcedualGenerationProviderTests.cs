#define TestPrimitives
#define TestArrays
#define TestObjects
#define TestCollections
#define TestQueryable
#define TestNotSupported
#define TestRules

//#if DEBUG

//#undef TestPrimitives
//#undef TestArrays
//#undef TestObjects
//#undef TestCollections
//#undef TestQueryable
//#undef TestNotSupported
//#undef TestRules

//#endif

using OoBDev.Generations.Tests.TestTargets;
using OoBDev.Generations.Tests.TypeConverters;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;

namespace OoBDev.Generations.Tests
{
    [TestClass]
    public class ProcedualGenerationProviderTests
    {
        public TestContext? TestContext { get; set; }

        [TestMethod]
        [TestCategory(TestCategories.DevLocal)]
        public void GenerateTest()
        {
            // Arrange
            var provider = new ProcedualGenerationProviderBuilder().Build();

            // Act
            for (var x = 0; x < 2; x++)
            {
                var context = provider.CreateContext(typeof(ModelWithProperties), default, default, default);
                for (var y = 0; y < 2; y++)
                {
                    var result = provider.Generate(context);
                    var json = JsonSerializer.Serialize(result);

                    TestContext?.WriteLine($"{(context.TargetType, x, y)} = {json}");
                }
            }

            for (var x = 0; x < 1; x++)
            {
                var context = provider.CreateContext(typeof(ModelWithProperties2), default, default, default);
                for (var y = 0; y < 1; y++)
                {
                    var result = provider.Generate(context);
                    var json = JsonSerializer.Serialize(result);

                    TestContext?.WriteLine($"{(context.TargetType, x, y)} = {json}");
                }
            }

            for (var x = 0; x < 1; x++)
            {
                var context = provider.CreateContext(typeof(ModelWithProperties3), default, default, default);
                for (var y = 0; y < 1; y++)
                {
                    var result = provider.Generate(context);
                    var json = JsonSerializer.Serialize(result);

                    TestContext?.WriteLine($"{(context.TargetType, x, y)} = {json}");
                }
            }

            // Assert
        }

        [DataTestMethod]
        [TestCategory(TestCategories.Unit)]
#if TestPrimitives
        [DataRow(typeof(string), "\"amet sit. ipsum sit. sed laoreet. \\r\\nmagna lorem. dolor elit. tincidunt ipsum. \\r\\n\"")]
        [DataRow(typeof(byte), "196")]
        [DataRow(typeof(byte?), "23")]
        [DataRow(typeof(sbyte), "62")]
        [DataRow(typeof(sbyte?), "-114")]
        [DataRow(typeof(int), "194604751")]
        [DataRow(typeof(int?), "875972984")]
        [DataRow(typeof(uint), "244347279")]
        [DataRow(typeof(uint?), "84431265")]
        [DataRow(typeof(short), "-14531")]
        [DataRow(typeof(short?), "-7777")]
        [DataRow(typeof(ushort), "57233")]
        [DataRow(typeof(ushort?), "38282")]
        [DataRow(typeof(long), "1365327134122258624")]
        [DataRow(typeof(long?), "7105172791151298376")]
        [DataRow(typeof(ulong), "6826792300002752806")]
        [DataRow(typeof(ulong?), "5996626067437767002")]
        [DataRow(typeof(float), "0.7566813")]
        [DataRow(typeof(float?), null)]
        [DataRow(typeof(double), "0.0983032468232807")]
        [DataRow(typeof(double?), "0.591417812552032")]
        [DataRow(typeof(decimal), "0.878051806650149")]
        [DataRow(typeof(decimal?), "0.111088101338171")]
        [DataRow(typeof(Guid), "\"0c7ae1f7-cd9a-6a29-ef73-26a04158709b\"")]
        [DataRow(typeof(Guid?), "\"fac4d4c9-8df4-4be9-4811-cefa2c37b27e\"")]
        [DataRow(typeof(DateTime), "\"6286-03-15T17:15:40.7244352\"")]
        [DataRow(typeof(DateTime?), null)]
        [DataRow(typeof(TimeSpan), "\"10675199.02:48:05:477\"")]
        [DataRow(typeof(TimeSpan?), "\"10675199.02:48:05:477\"")]
        [DataRow(typeof(DateTimeOffset), "\"1347-11-04T22:15:02.5063293+10:00\"")]
        [DataRow(typeof(DateTimeOffset?), null)]
        [DataRow(typeof(bool), "false")]
        [DataRow(typeof(bool?), "true")]
        [DataRow(typeof(EnumValues), "0")]
        [DataRow(typeof(EnumValues?), "1")]
        [DataRow(typeof(KeyValuePair<string, int>), "{\"Key\":\"sed amet erat. amet adipiscing consectetuer. dolore dolore nonummy. amet laoreet dolore. \\r\\ntincidunt diam laoreet. dolor euismod magna. erat ipsum erat. ipsum lorem ut. \\r\\n\",\"Value\":1895698055}")]
        [DataRow(typeof(Tuple<int, double>), "{\"Item1\":1462147300,\"Item2\":0.14859850897854124}")]
#endif
#if TestArrays
        [DataRow(typeof(int[]), "[1353118279]")]
        [DataRow(typeof(byte[]), "\"XNZo\"")]
        [DataRow(typeof(string[]), "[\"lorem. \\r\\ndiam. \\r\\n\",\"ut tincidunt. erat dolore. magna euismod. \\r\\nipsum dolore. elit ut. consectetuer amet. \\r\\n\"]")]
        [DataRow(typeof(object[]), "[{},{},{}]")]

        //TODO: these are supported by the generator but not by the System.Text.Json serializer
        [DataRow(typeof(int[,]), null)]
        [DataRow(typeof(int[,,]), null)]
#endif
#if TestObjects
        [DataRow(typeof(ModelWithProperties), "{\"String\":\"aliquam consectetuer. euismod adipiscing. dolor lorem. laoreet sed. \\r\\nnibh euismod. aliquam diam. consectetuer magna. amet elit. \\r\\n\",\"String2\":\"Seed: 1462531448\",\"EnumValues\":0,\"Integers\":[2105006190,1079422362,2008690199]}")]
        [DataRow(typeof(object), "{}")]
        [DataRow(typeof(ITargetInterface), "{\"GetterProperty\":\"nibh sed. ut ipsum. euismod amet. \\r\\nlaoreet magna. adipiscing euismod. sed sit. \\r\\n\",\"GetterSetterProperty\":\"tincidunt. sit. aliquam. nonummy. \\r\\namet. magna. elit. elit. \\r\\n\"}")]

        //TODO: add the ability to support abstract classes 
        [DataRow(typeof(IncompleteClassBase), typeof(NotSupportedException))]
#endif
#if TestCollections
        [DataRow(typeof(List<int>), "[1446902553,421318725,1350586562,325002734]")]
        [DataRow(typeof(Dictionary<string, int>), "{\"aliquam. \\r\\nsit. \\r\\n\":712219475,\"dolor. amet. consectetuer. nibh. \\r\\ndolore. tincidunt. adipiscing. euismod. \\r\\n\":1909994615,\"consectetuer consectetuer. erat nibh. amet ipsum. ut amet. \\r\\nmagna lorem. nibh sit. nibh consectetuer. elit magna. \\r\\n\":1512520735,\"aliquam consectetuer amet. sit sed lorem. nonummy dolore aliquam. \\r\\nsed ut tincidunt. dolor amet tincidunt. sit lorem diam. \\r\\n\":1438841627}")]
        [DataRow(typeof(Collection<double>), "[0.8021368355500218,0.543017267968048,0.28389770038607426]")]
        [DataRow(typeof(List<FakeEntity2>), "[{\"Prop1\":\"Seed: 694303549\",\"Prop2\":\"Nelson Stanley\"},{\"Prop1\":\"Seed: 560871220\",\"Prop2\":\"Nelson Lee\"},{\"Prop1\":\"Seed: 761607742\",\"Prop2\":\"Gordon Allen\"},{\"Prop1\":\"Seed: 543634745\",\"Prop2\":\"Mitchell Gordon\"}]")]
        [DataRow(typeof(IList<int>), "[322544652,1348128480,226228661]")]
        [DataRow(typeof(IDictionary<string, Guid>), "{\"aliquam dolore. tincidunt erat. magna aliquam. \\r\\nelit dolore. erat aliquam. lorem sit. \\r\\n\":\"0494d33f-e059-e30f-aab5-615dad5d16bb\",\"nonummy elit consectetuer. aliquam magna euismod. adipiscing sit sit. \\r\\nerat dolor euismod. nonummy elit diam. elit laoreet magna. \\r\\n\":\"61d64d8b-a1b8-f60a-60ff-847aef37b5b5\",\"consectetuer. amet. \\r\\nerat. sed. \\r\\n\":\"723d1f0b-67c1-d27f-0f12-1f09cfc8793c\"}")]
        [DataRow(typeof(ICollection<short>), "[15545,4403]")]
        [DataRow(typeof(IEnumerable<int>), "[1844979616,723079797]")]
        [DataRow(typeof(IEnumerable<KeyValuePair<Guid, int>>), "[{\"Key\":\"68657203-08a1-a649-3ecc-529efa918950\",\"Value\":566042788},{\"Key\":\"114b5443-e3a4-3fe4-2a01-adc8ffb727ec\",\"Value\":921396829}]")]
#endif
#if TestQueryable
        [DataRow(typeof(IQueryable<ushort>), "[9958,20922,53565,64530]")]
        [DataRow(typeof(IQueryable<FakeEntity2>), "[{\"Prop1\":\"Seed: 1512210044\",\"Prop2\":\"Gordon Lee\"}]")]
#endif
#if TestNotSupported
        //Note: There is no intention to support open generics at this time
        [DataRow(typeof(List<>), typeof(NotSupportedException))]
        [DataRow(typeof(Dictionary<,>), typeof(NotSupportedException))]
        [DataRow(typeof(Array), typeof(NotSupportedException))]
        [DataRow(typeof(ArrayList), typeof(NotSupportedException))]

        [DataRow(typeof(IEnumerable), typeof(NotSupportedException))]
        [DataRow(typeof(IEnumerable<>), typeof(NotSupportedException))]
        [DataRow(typeof(ICollection<>), typeof(NotSupportedException))]
        [DataRow(typeof(IList<>), typeof(NotSupportedException))]
        [DataRow(typeof(IDictionary<,>), typeof(NotSupportedException))]
        [DataRow(typeof(IQueryable<>), typeof(NotSupportedException))]
        [DataRow(typeof(KeyValuePair<,>), typeof(NotSupportedException))]
        [DataRow(typeof(Tuple<,>), typeof(NotSupportedException))]
#endif
#if TestRules
        [DataRow(typeof(FakePersonModel), "{\"FirstLast\":\"Morris Lewis\",\"LastFirst\":\"Stanley, Henry\",\"Comment\":\"amet sed aliquam. \\r\\n\"}")]
#endif
        [DataRow(typeof(TestModelWithDate), "{\"UploadedDate\":\"2027-02-26T14:14:00.8138899+13:45\"}")]
        public void GenerateByTypeTest(Type type, object expected)
        {
            try
            {
                var provider = new ProcedualGenerationProviderBuilder().Build();
                var context = provider.CreateContext(type, default, default, default);
                var result = provider.Generate(context);

                this.TestContext.AddResult(result, fileName: "GeneratedResults.json");
                this.TestContext?.WriteLine($"{nameof(result)}: {result}");

                if (result == null)
                {
                    Assert.IsTrue(Nullable.GetUnderlyingType(type) != null, $"{type} is not nullable so this should have a value");
                }
                else
                {
                    Assert.IsInstanceOfType(result, type);

                    if (type.IsArray && type.GetArrayRank() > 1)
                    {
                        //TODO: these are supported by the generator but not by the System.Text.Json serializer
                        Assert.Inconclusive($"{type} can not be serialized by System.Text.Json.JsonSerializer at this time... but if you got this far you are probably okay.");
                    }
                    else if (expected is string || expected == null)
                    {
                        var options = new JsonSerializerOptions()
                        {
                            Converters =
                            {
                                new TimeSpanJsonConverter(),
#if NET5_0_OR_GREATER
                                //Note: After .Net Core 3.1 it is not possible to serialize interfaces directly be default
                                new InterfaceConverter<ITargetInterface>(), 
#endif
                            }
                        };

                        var method = typeof(JsonSerializer)
                            .GetMethod(nameof(JsonSerializer.Serialize), 1, new[] {
                                Type.MakeGenericMethodParameter(0), 
                                typeof(JsonSerializerOptions) }
                            )?.MakeGenericMethod(type) ?? throw new InvalidOperationException()
                            ;
                        var invoked = method.Invoke(null, new object[] { result, options });
                        var json = (string?)invoked;

                        //this.TestContext.AddResult(json, fileName: "GeneratedResults.txt");
                        //this.TestContext.AddResult(expected, fileName: "ExpectedResults.txt");

                        if ((string?)expected != json) { }

                        Assert.AreEqual(expected, json);
                        //TestContext.AssertDifferences(expected as string, json, normalize: true);
                    }
                    else
                    {
                        Assert.Fail($"You shouldn't get here!");
                    }
                }
            }
            catch (AssertInconclusiveException) { throw; }
            catch (Exception ex)
            {
                this.TestContext?.WriteLine($"Error: {ex}");
                if (expected is Type expectedType)
                {
                    Assert.IsInstanceOfType(ex, expectedType);
                }
                else
                {
                    throw;
                }
            }
        }

        [DataTestMethod]
        [TestCategory(TestCategories.Unit)]
        [DataRow("Hello", 5, "{\"String\":\"amet consectetuer laoreet adipiscing. adipiscing elit ut erat. elit sit magna nonummy. sit ipsum diam adipiscing. \\r\\nnibh adipiscing magna lorem. ipsum diam aliquam dolore. diam laoreet euismod lorem. aliquam amet dolor dolore. \\r\\n\",\"Integer\":231391978}")]
        [DataRow("Hello", 9, "{\"String\":\"sed dolore laoreet. \\r\\nipsum lorem tincidunt. \\r\\n\",\"Integer\":1513546780}")]
        [DataRow("World", -10, "{\"String\":\"dolore lorem ipsum. aliquam sit euismod. \\r\\namet aliquam dolor. tincidunt magna tincidunt. \\r\\n\",\"Integer\":2046677114}")]
        public void InvokeOnInterfaceTest(string input1, int input2, string expected)
        {
            var type = typeof(ITargetInterface);
            var provider = new ProcedualGenerationProviderBuilder().Build();
            var context = provider.CreateContext(type, default, default, default);
            var generated = provider.Generate(context);
            Assert.IsNotNull(generated);
            Assert.IsInstanceOfType(generated, type);

            var unwrapped = (ITargetInterface)generated;
            var result = unwrapped.DoWork(input1, input2);

            var json = JsonSerializer.Serialize(result, new JsonSerializerOptions()
            {
                Converters =
                {
                    new TimeSpanJsonConverter(),
                }
            });
            Assert.AreEqual(expected, json);
        }

        //private static string ToLiteral(string valueTextForCompiler)
        //{
        //    return Microsoft.CodeAnalysis.CSharp.SymbolDisplay.FormatLiteral(valueTextForCompiler, false);
        //}

        //private object EnsureConverted(object input)
        //{
        //    if (input is Array array && array.Rank > 1)
        //    {
        //        var arrayRank = array.Rank;
        //        var arrayLengths = new int[arrayRank];

        //        Array arrays = null;

        //        for (var rank = 0; rank < arrayRank; rank++)
        //        {
        //            arrayLengths[rank] = array.GetLength(rank);


        //            if (rank == 0)
        //            {
        //                arrays = Array.CreateInstance(array.GetType().GetElementType(), arrayLengths[rank]);
        //            }
        //            else
        //            {
        //                var childArray = (Array)arrays.Clone();
        //                arrays = Array.CreateInstance(childArray.GetType(), arrayLengths[rank]);

        //                for (var i = 0; i < arrayLengths[rank]; i++)
        //                {
        //                    arrays.SetValue(childArray.Clone(), i);
        //                }
        //            }
        //        }


        //        static int product(IEnumerable<int> values) => values.Aggregate(1, (p, m) => p * m);
        //        var arrayLength = product(arrayLengths);

        //        for (var i = 0; i < arrayLength; i++)
        //        {
        //            var indexes = new int[arrayRank];

        //            for (var r = 0; r < arrayRank; r++)
        //            {
        //                indexes[r] = (i / product(arrayLengths.Take(r - 1))) % arrayLengths[r];
        //            }

        //            var elementValue = array.GetValue(indexes);

        //            //var chainIndexes = indexes.Skip(1).Reverse().ToArray();
        //            var chainIndexes = indexes.Take(arrayRank - 1).ToArray();
        //            Array child = arrays;
        //            foreach (var r in chainIndexes)
        //            {
        //                child = (Array)child.GetValue(r);
        //            }

        //            child.SetValue(elementValue, indexes[arrayRank - 1]);
        //        }

        //        return arrays;
        //    }

        //    return input;
        //}
    }
}
