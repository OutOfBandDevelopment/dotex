using OoBDev.Generations.Tests.TestTargets;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace OoBDev.Generations.Tests
{
    [TestClass]
    public class ProcedualGenerationSeedGeneratorTests
    {
        public TestContext? TestContext { get; set; }

        [DataTestMethod]
        [DataRow("GenerateTest_Type", 0, 0, 1594910990, typeof(double), 1, 2, 3)]
        [DataRow("GenerateTest_Type", 1, 0, 1594910734, typeof(double), 1, 2, 3)]
        [DataRow("GenerateTest_Type", 0, 1, 1594910991, typeof(double), 1, 2, 3)]
        [DataRow("GenerateTest_Type", 1, 1, 1594910735, typeof(double), 1, 2, 3)]

        [DataRow("GenerateTest_Type_All", 0, 0, 1398699548)]
        [DataRow("GenerateTest_Type_All", 1, 0, 1398699804)]
        [DataRow("GenerateTest_Type_All", 0, 1, 1398699549)]
        [DataRow("GenerateTest_Type_All", 1, 1, 1398699805)]
        public void GenerateTest_Method(string methodName, int parentSeed, int index, int expected, params object[] arguments)
        {
            var procedualGenerationSeedGenerator = new ProcedualGenerationSeedGenerator();

            var method = this.GetType().GetMethod(methodName) ?? throw new NotSupportedException($"\"{methodName}\" not defined");

            var result = procedualGenerationSeedGenerator.Generate(
                new FakeProcedualGenerationContext() { Seed = parentSeed },
                index,
                method,
                arguments
                );

            Assert.AreEqual(expected, result);

        }


        [TestMethod]
        [TestCategory(TestCategories.DevLocal)]
        public void GenerateTest_Method_All()
        {
            // Arrange
            var procedualGenerationSeedGenerator = new ProcedualGenerationSeedGenerator();

            var inputs = from method in this.GetType().GetMethods()
                         select (
                            parentSeed: 0,
                            index: 0,
                            method: method,
                            arguments: new object[] { typeof(double), 1, 2, 3, 4, 5 },
                            expectedSeed: 0
                         );


            var sb = new StringBuilder();

            foreach (var input in inputs)
            {
                var result = procedualGenerationSeedGenerator.Generate(
                    new FakeProcedualGenerationContext() { Seed = input.parentSeed },
                    input.index,
                    input.method,
                    input.arguments.Take(input.method.GetParameters().Length).ToArray()
                    );

                var matched = result == input.expectedSeed;
                sb.AppendLine(string.Join('\t', input.method, input.expectedSeed, matched, result));
            }

            this.TestContext?.WriteLine(sb.ToString());
            this.TestContext.AddResult(sb.ToString());

            Assert.Inconclusive("Some of the items above failed to match");
        }

        [DataTestMethod]
        [TestCategory(TestCategories.Unit)]
        [DataRow(typeof(Array), 0, 0, 241924874)]
        [DataRow(typeof(ArrayList), 0, 0, 205866578)]
        [DataRow(typeof(bool), 0, 0, 3610919)]
        [DataRow(typeof(bool?), 0, 0, 695408935)]
        [DataRow(typeof(byte), 0, 0, 1966940430)]
        [DataRow(typeof(byte?), 0, 0, 824643853)]
        [DataRow(typeof(byte[]), 0, 0, 1427987059)]
        [DataRow(typeof(Collection<double>), 0, 0, 1702261071)]
        [DataRow(typeof(DateTime), 0, 0, 407663467)]
        [DataRow(typeof(DateTime?), 0, 0, 1665301628)]
        [DataRow(typeof(DateTimeOffset), 0, 0, 1579383141)]
        [DataRow(typeof(DateTimeOffset?), 0, 0, 1762736718)]
        [DataRow(typeof(decimal), 0, 0, 238884642)]
        [DataRow(typeof(decimal?), 0, 0, 662050605)]
        [DataRow(typeof(Dictionary<,>), 0, 0, 488406650)]
        [DataRow(typeof(Dictionary<string, int>), 0, 0, 1327378973)]
        [DataRow(typeof(double), 0, 0, 1412655988)]
        [DataRow(typeof(double?), 0, 0, 2063609950)]
        [DataRow(typeof(EnumValues), 0, 0, 2136099330)]
        [DataRow(typeof(EnumValues?), 0, 0, 2052482626)]
        [DataRow(typeof(FakePersonModel), 0, 0, 205813251)]
        [DataRow(typeof(float), 0, 0, 1328905329)]
        [DataRow(typeof(float?), 0, 0, 1812278104)]
        [DataRow(typeof(Guid), 0, 0, 1748050959)]
        [DataRow(typeof(Guid?), 0, 0, 874908673)]
        [DataRow(typeof(ICollection<>), 0, 0, 1362649646)]
        [DataRow(typeof(ICollection<string>), 0, 0, 544298568)]
        [DataRow(typeof(IDictionary<,>), 0, 0, 739641201)]
        [DataRow(typeof(IDictionary<string, int>), 0, 0, 777674787)]
        [DataRow(typeof(IEnumerable), 0, 0, 320287092)]
        [DataRow(typeof(IEnumerable<>), 0, 0, 1245538338)]
        [DataRow(typeof(IEnumerable<int>), 0, 0, 458185535)]
        [DataRow(typeof(IEnumerable<KeyValuePair<string, int>>), 0, 0, 761945927)]
        [DataRow(typeof(IList<>), 0, 0, 522606174)]
        [DataRow(typeof(IList<int>), 0, 0, 742065964)]
        [DataRow(typeof(IncompleteClassBase), 0, 0, 2013866849)]
        [DataRow(typeof(int), 0, 0, 142030936)]
        [DataRow(typeof(int?), 0, 0, 1426869351)]
        [DataRow(typeof(int[,,]), 0, 0, 606889332)]
        [DataRow(typeof(int[,]), 0, 0, 1500583941)]
        [DataRow(typeof(int[]), 0, 0, 671892856)]
        [DataRow(typeof(IQueryable<>), 0, 0, 1981888307)]
        [DataRow(typeof(IQueryable<FakeEntity2>), 0, 0, 1578448755)]
        [DataRow(typeof(IQueryable<string>), 0, 0, 124257109)]
        [DataRow(typeof(ITargetInterface), 0, 0, 1512178472)]
        [DataRow(typeof(KeyValuePair<,>), 0, 0, 475738750)]
        [DataRow(typeof(KeyValuePair<string, int>), 0, 0, 1346134140)]
        [DataRow(typeof(List<>), 0, 0, 841961003)]
        [DataRow(typeof(List<FakeEntity2>), 0, 0, 103747075)]
        [DataRow(typeof(List<int>), 0, 0, 1075598202)]
        [DataRow(typeof(long), 0, 0, 142032477)]
        [DataRow(typeof(long?), 0, 0, 1393118311)]
        [DataRow(typeof(ModelWithProperties), 0, 0, 1795753285)]
        [DataRow(typeof(object), 0, 0, 1261399923)]
        [DataRow(typeof(object[]), 0, 0, 810700627)]
        [DataRow(typeof(sbyte), 0, 0, 89879839)]
        [DataRow(typeof(sbyte?), 0, 0, 407585099)]
        [DataRow(typeof(short), 0, 0, 142031962)]
        [DataRow(typeof(short?), 0, 0, 1359891559)]
        [DataRow(typeof(string), 0, 0, 1395293823)]
        [DataRow(typeof(string[]), 0, 0, 676814431)]
        [DataRow(typeof(TimeSpan), 0, 0, 224224352)]
        [DataRow(typeof(TimeSpan?), 0, 0, 1950968173)]
        [DataRow(typeof(Tuple<,>), 0, 0, 1329599605)]
        [DataRow(typeof(Tuple<int, double>), 0, 0, 292562764)]
        [DataRow(typeof(uint), 0, 0, 1329735010)]
        [DataRow(typeof(uint?), 0, 0, 890645295)]
        [DataRow(typeof(ulong), 0, 0, 1329864802)]
        [DataRow(typeof(ulong?), 0, 0, 806759209)]
        [DataRow(typeof(ushort), 0, 0, 1329997666)]
        [DataRow(typeof(ushort?), 0, 0, 924199723)]
        [DataRow(typeof(ushort?), 0, 1, 924199722)]
        [DataRow(typeof(ushort?), 0, 2, 924199721)]
        [DataRow(typeof(ushort?), 1, 0, 924199467)]
        [DataRow(typeof(ushort?), 1, 1, 924199466)]
        [DataRow(typeof(ushort?), 1, 2, 924199465)]
        public void GenerateTest_Type(Type type, int parentSeed, int index, int expectedSeed)
        {
            var procedualGenerationSeedGenerator = new ProcedualGenerationSeedGenerator();
            var result = procedualGenerationSeedGenerator.Generate(
                new FakeProcedualGenerationContext() { Seed = parentSeed },
                index,
                type);
            Assert.AreEqual(expectedSeed, result);
        }

        [TestMethod]
        [TestCategory(TestCategories.DevLocal)]
        public void GenerateTest_Type_All()
        {
            // Arrange
            var procedualGenerationSeedGenerator = new ProcedualGenerationSeedGenerator();
            var inputs = new (Type type, int parentSeed, int index, int expectedSeed)[]
            {
                (typeof(Array), 0,0,241924874),
                (typeof(ArrayList), 0,0,205866578),
                (typeof(bool), 0,0,3610919),
                (typeof(bool?), 0,0,695408935),
                (typeof(byte), 0,0,1966940430),
                (typeof(byte?), 0,0,824643853),
                (typeof(byte[]), 0,0,1427987059),
                (typeof(Collection<double>), 0,0,1702261071),
                (typeof(DateTime), 0,0,407663467),
                (typeof(DateTime?), 0,0,1665301628),
                (typeof(DateTimeOffset), 0,0,1579383141),
                (typeof(DateTimeOffset?), 0,0,1762736718),
                (typeof(decimal), 0,0,238884642),
                (typeof(decimal?), 0,0,662050605),
                (typeof(Dictionary<,>), 0,0,488406650),
                (typeof(Dictionary<string, int>), 0,0,1327378973),
                (typeof(double), 0,0,1412655988),
                (typeof(double?), 0,0,2063609950),
                (typeof(EnumValues), 0,0,2136099330),
                (typeof(EnumValues?), 0,0,2052482626),
                (typeof(FakePersonModel), 0,0,205813251),
                (typeof(float), 0,0,1328905329),
                (typeof(float?), 0,0,1812278104),
                (typeof(Guid), 0,0,1748050959),
                (typeof(Guid?), 0,0,874908673),
                (typeof(ICollection<>), 0,0,1362649646),
                (typeof(ICollection<string>), 0,0,544298568),
                (typeof(IDictionary<,>), 0,0,739641201),
                (typeof(IDictionary<string, int>), 0,0,777674787),
                (typeof(IEnumerable), 0,0,320287092),
                (typeof(IEnumerable<>), 0,0,1245538338),
                (typeof(IEnumerable<int>), 0,0,458185535),
                (typeof(IEnumerable<KeyValuePair<string, int>>), 0,0,761945927),
                (typeof(IList<>), 0,0,522606174),
                (typeof(IList<int>), 0,0,742065964),
                (typeof(IncompleteClassBase), 0,0,2013866849),
                (typeof(int), 0,0,142030936),
                (typeof(int?), 0,0,1426869351),
                (typeof(int[,,]), 0,0,606889332),
                (typeof(int[,]), 0,0,1500583941),
                (typeof(int[]), 0,0,671892856),
                (typeof(IQueryable<>), 0,0,1981888307),
                (typeof(IQueryable<FakeEntity2>), 0,0,1578448755),
                (typeof(IQueryable<string>), 0,0,124257109),
                (typeof(ITargetInterface), 0,0,1512178472),
                (typeof(KeyValuePair<,>), 0,0,475738750),
                (typeof(KeyValuePair<string, int>), 0,0,1346134140),
                (typeof(List<>), 0,0,841961003),
                (typeof(List<FakeEntity2>), 0,0,103747075),
                (typeof(List<int>), 0,0,1075598202),
                (typeof(long), 0,0,142032477),
                (typeof(long?), 0,0,1393118311),
                (typeof(ModelWithProperties), 0,0,1795753285),
                (typeof(object), 0,0,1261399923),
                (typeof(object[]), 0,0,810700627),
                (typeof(sbyte), 0,0,89879839),
                (typeof(sbyte?), 0,0,407585099),
                (typeof(short), 0,0,142031962),
                (typeof(short?), 0,0,1359891559),
                (typeof(string), 0,0,1395293823),
                (typeof(string[]), 0,0,676814431),
                (typeof(TimeSpan), 0,0,224224352),
                (typeof(TimeSpan?), 0,0,1950968173),
                (typeof(Tuple<,>), 0,0,1329599605),
                (typeof(Tuple<int, double>), 0,0,292562764),
                (typeof(uint), 0,0,1329735010),
                (typeof(uint?), 0,0,890645295),
                (typeof(ulong), 0,0,1329864802),
                (typeof(ulong?), 0,0,806759209),
                (typeof(ushort), 0,0,1329997666),
                (typeof(ushort?), 0,0,924199723),
            };

            var someFailed = false;

            var sb = new StringBuilder();

            foreach (var input in inputs)
            {
                var result = procedualGenerationSeedGenerator.Generate(
                new FakeProcedualGenerationContext() { Seed = input.parentSeed },
                    input.index,
                    input.type);

                var matched = result == input.expectedSeed;
                if (!matched) someFailed = true;

                sb.AppendLine(string.Join('\t', input.type, input.parentSeed, input.index, input.expectedSeed, matched, result));
            }

            this.TestContext?.WriteLine(sb.ToString());
            this.TestContext.AddResult(sb.ToString());

            Assert.IsFalse(someFailed, "Some of the items above failed to match");
        }
    }
}
