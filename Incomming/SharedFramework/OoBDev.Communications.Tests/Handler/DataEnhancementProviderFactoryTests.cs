using OoBDev.Communications.Contracts;
using OoBDev.Communications.Contracts.Handler;
using OoBDev.Communications.Handler;
using OoBDev.TestUtilities;
using OoBDev.Toolkit;
using OoBDev.Toolkit.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OoBDev.Communications.Tests.Handler
{
    [TestClass]
    public class DataEnhancementProviderFactoryTests
    {
        public TestContext TestContext { get; set; }

        private IDataEnhancementProviderFactory CreateProvider(params IDataEnhancementProvider[] providers)
        {
            var services =
            new ServiceCollection()
                .AddToolkitServices()
                .AddCommunicationsServices()
            ;

            foreach (var provider in providers)
            {
                services.AddTransient(_ => provider);
            }

            return services
                .BuildServiceProvider()
                .GetRequiredService<IDataEnhancementProviderFactory>()
                ;
        }

        [TestMethod]
        [TestCategory(TestCategories.Simulation)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public void GetDataTest_Null()
        {
            //Stage
            object input = null;

            //Test
            var provider = CreateProvider();
            var result = provider.GetData(input);

            //Assert
            Assert.IsNotNull(result);
            this.TestContext?.WriteLine($"^{result}^");
        }

        [TestMethod]
        [TestCategory(TestCategories.Simulation)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public void GetDataTest_SimpleObject()
        {
            //Stage
            object input = new
            {
                Hello = "world!",
                Value = 1.23,
            };

            //Test
            var provider = CreateProvider();
            var result = provider.GetData(input);

            //Assert
            Assert.IsNotNull(result);
            this.TestContext?.WriteLine($"^{result}^");
        }

        [TestMethod]
        [TestCategory(TestCategories.Simulation)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public void GetDataTest_EmptyString()
        {
            //Stage
            object input = "";

            //Test
            var provider = CreateProvider();
            var result = provider.GetData(input);
            this.TestContext?.WriteLine($"^{result}^");

            //Assert
            Assert.IsNotNull(result);
        }


        [TestMethod]
        [TestCategory(TestCategories.Simulation)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public void GetDataTest_SimpleString()
        {
            //Stage
            object input = "Test String";

            //Test
            var provider = CreateProvider();
            var result = provider.GetData(input);
            this.TestContext?.WriteLine($"^{result}^");

            //Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [TestCategory(TestCategories.Simulation)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public void GetDataTest_SimpleNumber()
        {
            //Stage
            object input = 1.23m;

            //Test
            var provider = CreateProvider();
            var result = provider.GetData(input);
            this.TestContext?.WriteLine($"^{result}^");

            //Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [TestCategory(TestCategories.Simulation)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public void GetDataTest_JsonEmpty()
        {
            //Stage
            object input = "{}";

            //Test
            var provider = CreateProvider();
            var result = provider.GetData(input);

            //Assert
            Assert.IsNotNull(result);

            this.TestContext?.WriteLine($"^{result}^");
        }


        [TestMethod]
        [TestCategory(TestCategories.Simulation)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public void GetDataTest_Json()
        {
            //Stage
            object input = "{\"hello\":\"world\"}";

            //Test
            var provider = CreateProvider();
            var result = provider.GetData(input);

            //Assert
            Assert.IsNotNull(result);
            this.TestContext?.WriteLine($"^{result}^");
        }


        [TestMethod]
        [TestCategory(TestCategories.Simulation)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public void GetDataTest_JsonArrayEmpty()
        {
            //Stage
            object input = "[]";

            //Test
            var provider = CreateProvider();
            var result = provider.GetData(input);

            //Assert
            Assert.IsNotNull(result);
            this.TestContext?.WriteLine($"^{result}^");
        }

        [TestMethod]
        [TestCategory(TestCategories.Simulation)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public void GetDataTest_JsonArray()
        {
            //Stage
            object input = "[1,2,3]";

            //Test
            var provider = CreateProvider();
            var result = provider.GetData(input);

            //Assert
            Assert.IsNotNull(result);
            this.TestContext?.WriteLine($"^{result}^");
        }

        [TestMethod]
        [TestCategory(TestCategories.Simulation)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public void GetDataTest_EmptyDictionary()
        {
            //Stage
            object input = new Dictionary<string, object>();

            //Test
            var provider = CreateProvider();
            var result = provider.GetData(input);

            //Assert
            Assert.IsNotNull(result);
            this.TestContext?.WriteLine($"^{result}^");
        }

        [TestMethod]
        [TestCategory(TestCategories.Simulation)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public void GetDataTest_Dictionary()
        {
            //Stage
            object input = new Dictionary<string, object>()
            {
                {"Hello", "world" },
                {"Number", 123.5m },
            };

            //Test
            var provider = CreateProvider();
            var result = provider.GetData(input);

            //Assert
            Assert.IsNotNull(result);
            this.TestContext?.WriteLine($"^{result}^");
        }

        [TestMethod]
        [TestCategory(TestCategories.Simulation)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public void GetDataTest_ArrayEmpty()
        {
            //Stage
            object input = Array.Empty<string>();

            //Test
            var provider = CreateProvider();
            var result = provider.GetData(input);

            //Assert
            Assert.IsNotNull(result);
            this.TestContext?.WriteLine($"^{result}^");
        }

        [TestMethod]
        [TestCategory(TestCategories.Simulation)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public void GetDataTest_Array()
        {
            //Stage
            object input = new[] { 1, 2, 3, 4, 5 };

            //Test
            var provider = CreateProvider();
            var result = provider.GetData(input);

            //Assert
            Assert.IsNotNull(result);
            this.TestContext?.WriteLine($"^{result}^");
        }

        [TestMethod]
        [TestCategory(TestCategories.Simulation)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public void GetDataTest_JObject()
        {
            //Stage
            object input = new JObject();

            //Test
            var provider = CreateProvider();
            var result = provider.GetData(input);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(input, result);
            this.TestContext?.WriteLine($"^{result}^");
        }

        [TestMethod]
        [TestCategory(TestCategories.Simulation)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public void GetDataTest_Other()
        {
            //Stage
            object input = new { };
            var json = @"{""hello"":""world""}";
            var services = Array.Empty<ServiceDescriptor>();
            

            //Mock
            var mock = new MockRepository(MockBehavior.Strict);
            var mockServiceProvider = mock.Create<IServiceProvider>();
            var mockSerializer = mock.Create<IObjectSerializer>();
            mockSerializer.Setup(m => m.GetAsSerialized(input)).Returns(json);

            //Test
            var provider = new DataEnhancementProviderFactory(
                services,
                mockServiceProvider.Object,
                mockSerializer.Object
                );
            var result = provider.GetData(input);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("world", (string)result["hello"]);
            this.TestContext?.WriteLine($"^{result}^");

            //Verify
            mock.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Simulation)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public void GetProvidersTest()
        {
            //Stage
            var messageType = "Matched Type";

            //Test
            var provider = CreateProvider(
                new TestDataNoAttribute(),
                new TestDataEmptyAttribute(),
                new TestDataPriorityAttribute(),
                new TestDataMatchedAttribute(),
                new TestDataUnmatchedAttribute(),
                new TestDataMultipleAttribute()
                );
            var result = provider.GetProviders(messageType);

            //Assert
            Assert.IsNotNull(result);
            var set = result.ToArray();
            Assert.AreEqual(5, set.Length);
            Assert.IsInstanceOfType(set.Last(), typeof(TestDataPriorityAttribute));
        }

        public class TestDataBase : IDataEnhancementProvider
        {
            public Task<JObject> EnhanceAsync(Guid targetPersonId, string messageType, JObject data) => Task.FromResult(data);
        }
        public class TestDataNoAttribute : TestDataBase { }
        [DataEnhancer]
        public class TestDataEmptyAttribute : TestDataBase { }
        [DataEnhancer(Priority = 1)]
        public class TestDataPriorityAttribute : TestDataBase { }
        [DataEnhancer(TargetedMessageType = "Matched Type")]
        public class TestDataMatchedAttribute : TestDataBase { }
        [DataEnhancer(TargetedMessageType = "Unmatched Type")]
        public class TestDataUnmatchedAttribute : TestDataBase { }

        [DataEnhancer(TargetedMessageType = "Unmatched Type")]
        [DataEnhancer(TargetedMessageType = "Matched Type")]
        public class TestDataMultipleAttribute : TestDataBase { }
    }
}