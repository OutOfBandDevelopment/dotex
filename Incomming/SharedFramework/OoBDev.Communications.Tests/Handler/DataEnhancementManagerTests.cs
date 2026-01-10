using OoBDev.Communications.Contracts;
using OoBDev.Communications.Contracts.Handler;
using OoBDev.Communications.Handler;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Communications.Tests.Handler
{
    [TestClass]
    public class DataEnhancementManagerTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public async Task EnhanceAsyncTest()
        {
            //Stage
            var targetPersonId = Guid.NewGuid();
            var messageType = "test message type";
            var data = "starting point";
            var sourceData = new JObject();
            var responseData = new JObject();

            //Mock
            var mock = new MockRepository(MockBehavior.Strict);
            var mockProviders = mock.Create<IDataEnhancementProviderFactory>();

            var mockDataEnhancement = mock.Create<IDataEnhancementProvider>();
            mockDataEnhancement.Setup(m => m.EnhanceAsync(targetPersonId, messageType, sourceData))
                               .ReturnsAsync(responseData);

            mockProviders.Setup(m => m.GetData(data)).Returns(sourceData);
            mockProviders.Setup(m => m.GetProviders(messageType))
                         .Returns(new[] { mockDataEnhancement.Object }.AsEnumerable());

            //Test
            var provider = new DataEnhancementManager(
                mockProviders.Object
                );
            var response = await provider.EnhanceAsync(targetPersonId, messageType, data);

            //Assert
            Assert.AreEqual(responseData, response);

            //Verify
            mock.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public async Task EnhanceAsyncTest_NullInput()
        {
            //Stage
            var targetPersonId = Guid.NewGuid();
            var messageType = "test message type";
            var data = "starting point";
            JObject sourceData = null;
            var responseData = new JObject();

            //Mock
            var mock = new MockRepository(MockBehavior.Strict);
            var mockProviders = mock.Create<IDataEnhancementProviderFactory>();

            var mockDataEnhancement = mock.Create<IDataEnhancementProvider>();

            mockDataEnhancement.Setup(m => m.EnhanceAsync(
                It.Is<Guid>(i => i == targetPersonId),
                It.Is<string>(i => i == messageType),
                It.Is<JObject>(i => i != null)
                )).ReturnsAsync(responseData);

            mockProviders.Setup(m => m.GetData(data)).Returns(sourceData);
            mockProviders.Setup(m => m.GetProviders(messageType))
                         .Returns(new[] { mockDataEnhancement.Object }.AsEnumerable());

            //Test
            var provider = new DataEnhancementManager(
                mockProviders.Object
                );
            var response = await provider.EnhanceAsync(targetPersonId, messageType, data);

            //Assert
            Assert.AreEqual(responseData, response);

            //Verify
            mock.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        [ExpectedException(typeof(DataEnhancementException))]
        public async Task EnhanceAsyncTest_Exception()
        {
            //Stage
            var targetPersonId = Guid.NewGuid();
            var messageType = "test message type";
            var data = "starting point";
            var exception = new ApplicationException();

            //Mock
            var mock = new MockRepository(MockBehavior.Strict);
            var mockProviders = mock.Create<IDataEnhancementProviderFactory>();

            mockProviders.Setup(m => m.GetData(data)).Throws(exception);

            //Test
            var provider = new DataEnhancementManager(
                mockProviders.Object
                );
            try
            {
                var response = await provider.EnhanceAsync(targetPersonId, messageType, data);

                //Assert
            }
            catch (Exception ex)
            {
                Assert.AreEqual(exception, ex.InnerException);
                throw;
            }
            finally
            {
                //Verify
                mock.VerifyAll();
            }
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public void SeedDataTest()
        {
            //Stage
            var targetPersonId = Guid.NewGuid();
            var data = new { };
            var seeded = new JObject();
            var nested = new JObject();

            var responseData = new JObject
            {
                ["test payload"] = nested
            };

            //Mock
            var mock = new MockRepository(MockBehavior.Strict);
            var mockProviders = mock.Create<IDataEnhancementProviderFactory>();

            var mockDataEnhancement = mock.Create<IDataEnhancementProvider>();

            mockProviders.Setup(s => s.GetData(data)).Returns(seeded);
            mockProviders.Setup(s => s.GetData("data")).Returns(nested);

            //Test
            var provider = new DataEnhancementManager(
                mockProviders.Object
                );

            var response = provider.SeedData(data, (null, "test data 1"), ("", "test data 2"), ("test payload", "data"), ("test null", null));

            //Assert
            Assert.AreEqual(responseData.ToString(), response.ToString());

            //Verify
            mock.VerifyAll();
        }
    }
}
