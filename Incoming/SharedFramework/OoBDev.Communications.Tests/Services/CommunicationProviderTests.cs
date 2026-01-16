using OoBDev.Communications.Contracts;
using OoBDev.Communications.Contracts.Handler;
using OoBDev.Communications.Contracts.Models;
using OoBDev.Communications.Contracts.Services;
using OoBDev.Communications.Services;
using OoBDev.MessageQueueing.Contracts;
using OoBDev.TestUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;

namespace OoBDev.Communications.Tests.Services
{
    [TestClass]
    public class CommunicationProviderTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod, TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public async Task SendAsyncTest_SendRequestModel()
        {
            //Stage
            var request = new SendRequestModel
            {
                MessageType = "test type",
                TargetPersonId = Guid.NewGuid(),
            };
            var coorId = "test coor";

            //Mock
            var mock = new MockRepository(MockBehavior.Strict);
            var mockMesasgeType = mock.Create<IAttributeResolver>();
            var mockQueue = mock.Create<IMessageSender<CommunicationProvider>>();
            var mockLog = mock.Create<ILogger<CommunicationProvider>>(MockBehavior.Loose);

            mockQueue.Setup(s => s.SendAsync(
                It.Is<ISendRequest>(i => i == request),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<string>()
                )).ReturnsAsync(coorId);

            //Test
            var provider = new CommunicationProvider(
                mockMesasgeType.Object,
                mockQueue.Object,
                mockLog.Object
                );
            var result = await provider.SendAsync(request);

            //Assert
            Assert.AreEqual(coorId, result);

            //Verify
            mock.VerifyAll();
        }

        [TestMethod, TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task SendAsyncTest_SendRequestModel_NullRequest()
        {
            //Stage
            SendRequestModel request = null;

            //Mock
            var mock = new MockRepository(MockBehavior.Strict);
            var mockMesasgeType = mock.Create<IAttributeResolver>();
            var mockQueue = mock.Create<IMessageSender<CommunicationProvider>>();
            var mockLog = mock.Create<ILogger<CommunicationProvider>>(MockBehavior.Loose);

            //Test
            var provider = new CommunicationProvider(
                mockMesasgeType.Object,
                mockQueue.Object,
                mockLog.Object
                );

            try
            {
                var result = await provider.SendAsync(request);

                //Assert
                Assert.Fail("You shouldn't get here!");
            }
            finally
            {
                //Verify
                mock.VerifyAll();
            }
        }

        [TestMethod, TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        [ExpectedException(typeof(ArgumentException))]
        public async Task SendAsyncTest_SendRequestModel_EmptyId()
        {
            //Stage
            var request = new SendRequestModel
            {
                MessageType = "test type",
                TargetPersonId = Guid.Empty,
            };

            //Mock
            var mock = new MockRepository(MockBehavior.Strict);
            var mockMesasgeType = mock.Create<IAttributeResolver>();
            var mockQueue = mock.Create<IMessageSender<CommunicationProvider>>();
            var mockLog = mock.Create<ILogger<CommunicationProvider>>(MockBehavior.Loose);

            //Test
            var provider = new CommunicationProvider(
                mockMesasgeType.Object,
                mockQueue.Object,
                mockLog.Object
                );

            try
            {
                var result = await provider.SendAsync(request);

                //Assert
                Assert.Fail("You shouldn't get here!");
            }
            finally
            {
                //Verify
                mock.VerifyAll();
            }
        }


        [TestMethod, TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        [ExpectedException(typeof(ArgumentException))]
        public async Task SendAsyncTest_SendRequestModel_EmptyMessageType()
        {
            //Stage
            var request = new SendRequestModel
            {
                MessageType = "",
                TargetPersonId = Guid.NewGuid(),
            };

            //Mock
            var mock = new MockRepository(MockBehavior.Strict);
            var mockMesasgeType = mock.Create<IAttributeResolver>();
            var mockQueue = mock.Create<IMessageSender<CommunicationProvider>>();
            var mockLog = mock.Create<ILogger<CommunicationProvider>>(MockBehavior.Loose);

            //Test
            var provider = new CommunicationProvider(
                mockMesasgeType.Object,
                mockQueue.Object,
                mockLog.Object
                );

            try
            {
                var result = await provider.SendAsync(request);

                //Assert
                Assert.Fail("You shouldn't get here!");
            }
            finally
            {
                //Verify
                mock.VerifyAll();
            }
        }

        [TestMethod, TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public async Task SendAsyncTest_SendRequestModel_TargetPersonMessageTypeObject()
        {
            //Stage
            var personId = Guid.NewGuid();
            var messageType = "test messageType";
            var request = new object { };
            var coorId = "test coor";

            //Mock
            var mock = new MockRepository(MockBehavior.Strict);
            var mockMesasgeType = mock.Create<IAttributeResolver>();
            var mockQueue = mock.Create<IMessageSender<CommunicationProvider>>();
            var mockLog = mock.Create<ILogger<CommunicationProvider>>(MockBehavior.Loose);
            mockQueue.Setup(m => m.SendAsync<ISendRequest>(
                It.Is<SendRequestModel>(i =>
                    i.TargetPersonId == personId &&
                    i.MessageType == messageType &&
                    i.Data == request
                ), 
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<string>()
                )).ReturnsAsync(coorId);

            //Test
            var provider = new CommunicationProvider(
                mockMesasgeType.Object,
                mockQueue.Object,
                mockLog.Object
                );
            var result = await provider.SendAsync(personId, messageType, request);

            //Assert
            Assert.AreEqual(coorId, result);

            //Verify
            mock.VerifyAll();
        }


        [TestMethod, TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public async Task SendAsyncTest_SendRequestModel_TargetPersonMessageObject()
        {
            //Stage
            var personId = Guid.NewGuid();
            var messageType = "test message type";
            var request = new TestPayload { };
            var coorId = "test coor";

            //Mock
            var mock = new MockRepository(MockBehavior.Strict);
            var mockMesasgeType = mock.Create<IAttributeResolver>();
            var mockQueue = mock.Create<IMessageSender<CommunicationProvider>>();
            var mockLog = mock.Create<ILogger<CommunicationProvider>>(MockBehavior.Loose);
            mockMesasgeType.Setup(m => m.GetMessageType<TestPayload>()).Returns(messageType);
            mockMesasgeType.Setup(s => s.GetPriority<TestPayload>()).Returns(RequestPriorities.Normal);
            mockQueue.Setup(m => m.SendAsync<ISendRequest>(
                It.Is<SendRequestModel>(i =>
                    i.TargetPersonId == personId &&
                    i.MessageType == messageType &&
                    i.Data == request
                ), 
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<string>()
                )).ReturnsAsync(coorId);

            //Test
            var provider = new CommunicationProvider(
                mockMesasgeType.Object,
                mockQueue.Object,
                mockLog.Object
                );
            var result = await provider.SendAsync(personId, request);

            //Assert
            Assert.AreEqual(coorId, result);

            //Verify
            mock.VerifyAll();
        }

        [TestMethod, TestCategory(TestCategories.DevLocal)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        [Obsolete]
        public async Task SendAsyncTest_3() =>
         this.TestContext?.WriteLine($@"CorrelationId: {await this.TestContext
                .GetService<ICommunicationProvider>()
                .SendAsync(Guid.NewGuid(), "TestMessage", new TestPayload { TestData = "1234", })}");

        [TestMethod, TestCategory(TestCategories.DevLocal)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public async Task SendAsyncTest_2() =>
          await this.TestContext
            .GetService<ICommunicationProvider>()
            .SendAsync(Guid.NewGuid(), new TestPayload
            {
                TestData = "1234",
            });

        [Communication(MessageType = "TestPayload")]
        public class TestPayload
        {
            public string TestData { get; internal set; }
        }
    }
}
