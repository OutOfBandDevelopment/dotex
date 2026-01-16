using OoBDev.Communications.Contracts;
using OoBDev.Communications.Contracts.Handler;
using OoBDev.Communications.Handler;
using OoBDev.TestUtilities;
using OoBDev.Toolkit.Common;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Communications.Tests.Handler
{
    [TestClass]
    public class CommunicationDeferralProviderTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod, TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public async Task ExecuteAsyncTest_None()
        {
            var requestTime = DateTimeOffset.Now;
            var maxCount = 10;
            var expectedException = new ApplicationException();
            var deferralWaitings = Enumerable.Empty<IDeferralWaiting>();

            //Mock
            var mock = new MockRepository(MockBehavior.Strict);
            var mockDeserializer = mock.Create<IObjectDeserializer>();
            var mockProvider = mock.Create<ICommunicationProvider>();
            var mockManager = mock.Create<IDeferralManager>();
            var mockLog = mock.Create<ILogger<CommunicationDeferralProvider>>(MockBehavior.Loose);
            var mockConfig = mock.Create<ICommunicationDeferralConfig>();

            mockConfig.Setup(m => m.MaxCount).Returns(maxCount);
            mockManager.Setup(m => m.GetWaitingRequestsAsync(requestTime, maxCount))
                       .ReturnsAsync(deferralWaitings);


            //Test
            var processor = new CommunicationDeferralProvider(mockDeserializer.Object,
                mockProvider.Object,
                mockManager.Object,
                mockLog.Object,
                mockConfig.Object
                );

            try
            {
                var result = await processor.ExecuteAsync(requestTime);

                //Assert
                Assert.IsTrue(result);
            }
            finally
            {
                //Verify
                mock.VerifyAll();
            }
        }

        [TestMethod, TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        [Obsolete]
        public async Task ExecuteAsyncTest_Some()
        {
            //Stage
            var request = new
            {
                TargetPersonId = Guid.NewGuid(),
                MessageType = "message type test",
                Data = "test some data",
            };
            var requestTime = DateTimeOffset.Now;
            var maxCount = 10;
            var correlationId = Guid.NewGuid();
            var correlationIdString = correlationId.ToString();
            var notificationDeferralId = Guid.NewGuid();
            var data = new object();

            //Mock
            var mock = new MockRepository(MockBehavior.Strict);
            var mockDeserializer = mock.Create<IObjectDeserializer>();
            var mockProvider = mock.Create<ICommunicationProvider>();
            var mockManager = mock.Create<IDeferralManager>();
            var mockLog = mock.Create<ILogger<CommunicationDeferralProvider>>(MockBehavior.Loose);
            var mockConfig = mock.Create<ICommunicationDeferralConfig>();

            var mockDeferralWaiting = mock.Create<IDeferralWaiting>();

            mockDeferralWaiting.Setup(m => m.TargetPersonId).Returns(request.TargetPersonId);
            mockDeferralWaiting.Setup(m => m.MessageType).Returns(request.MessageType);
            mockDeferralWaiting.Setup(m => m.ExtendedData).Returns(request.Data);
            mockDeferralWaiting.Setup(m => m.NotificationDeferralId).Returns(notificationDeferralId);

            mockConfig.Setup(m => m.MaxCount).Returns(maxCount);
            mockManager.Setup(m => m.GetWaitingRequestsAsync(requestTime, maxCount))
                       .ReturnsAsync(new[] { mockDeferralWaiting.Object }.AsEnumerable());
            mockProvider.Setup(m => m.SendAsync(It.Is<ISendRequest>(i =>
                            i.TargetPersonId == request.TargetPersonId &&
                            i.MessageType == request.MessageType &&
                            i.Data == data &&
                            i.Priority == RequestPriorities.Immediate //Note: sent messages from deferral must be 
                            ),
                            It.IsAny<string>(),
                            It.IsAny<int>(),
                            It.IsAny<string>()
                            ))
                        .ReturnsAsync(correlationIdString);
            mockManager.Setup(m => m.SendAsync(notificationDeferralId, correlationId)).Returns(Task.FromResult(0));
            mockDeserializer.Setup(m => m.Deserialize(mockDeferralWaiting.Object.ExtendedData)).Returns(data);

            //Test
            var processor = new CommunicationDeferralProvider(mockDeserializer.Object,
                mockProvider.Object,
                mockManager.Object,
                mockLog.Object,
                mockConfig.Object
                );

            try
            {
                var result = await processor.ExecuteAsync(requestTime);

                //Assert
                Assert.IsTrue(result);
            }
            finally
            {
                //Verify
                mock.VerifyAll();
            }
        }

        [TestMethod, TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public async Task ExecuteAsyncTest_ExceptionInital()
        {
            //Stage
            var requestTime = DateTimeOffset.Now;
            var maxCount = 10;
            var expectedException = new ApplicationException();

            //Mock
            var mock = new MockRepository(MockBehavior.Strict);
            var mockDeserializer = mock.Create<IObjectDeserializer>();
            var mockProvider = mock.Create<ICommunicationProvider>();
            var mockManager = mock.Create<IDeferralManager>();
            var mockLog = mock.Create<ILogger<CommunicationDeferralProvider>>();
            var mockConfig = mock.Create<ICommunicationDeferralConfig>();

            mockConfig.Setup(m => m.MaxCount).Returns(maxCount);
            mockManager.Setup(m => m.GetWaitingRequestsAsync(requestTime, maxCount))
                       .Throws(expectedException);

            //Test
            var processor = new CommunicationDeferralProvider(mockDeserializer.Object,
                mockProvider.Object,
                mockManager.Object,
                mockLog.Object,
                mockConfig.Object
                );

            await Assert.ThrowsExceptionAsync<ApplicationException>(async () =>
            {
                var result = await processor.ExecuteAsync(requestTime);

                //Assert
                Assert.Fail("you shouldn't get here");
            });

            //Verify
            mock.VerifyAll();
        }

        [TestMethod, TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public async Task ExecuteAsyncTest_ExceptionPer()
        {
            //Stage
            var request = new
            {
                TargetPersonId = Guid.NewGuid(),
                MessageType = "message type test",
                Data = "test some data",
            };
            var requestTime = DateTimeOffset.Now;
            var maxCount = 10;
            var correlationId = Guid.NewGuid();
            var correlationIdString = correlationId.ToString();
            var notificationDeferralId = Guid.NewGuid();
            var expectedException = new ApplicationException();

            //Mock
            var mock = new MockRepository(MockBehavior.Strict);
            var mockDeserializer = mock.Create<IObjectDeserializer>();
            var mockProvider = mock.Create<ICommunicationProvider>();
            var mockManager = mock.Create<IDeferralManager>();
            var mockLog = mock.Create<ILogger<CommunicationDeferralProvider>>(MockBehavior.Loose);
            var mockConfig = mock.Create<ICommunicationDeferralConfig>();

            var mockDeferralWaiting = mock.Create<IDeferralWaiting>();

            mockDeferralWaiting.Setup(m => m.TargetPersonId).Returns(request.TargetPersonId);
            mockDeferralWaiting.Setup(m => m.MessageType).Returns(request.MessageType);
            mockDeferralWaiting.Setup(m => m.ExtendedData).Returns(request.Data);
            mockDeferralWaiting.Setup(m => m.NotificationDeferralId).Returns(notificationDeferralId);

            mockConfig.Setup(m => m.MaxCount).Returns(maxCount);
            mockManager.Setup(m => m.GetWaitingRequestsAsync(requestTime, maxCount))
                       .ReturnsAsync(new[] { mockDeferralWaiting.Object }.AsEnumerable());
            mockManager.Setup(m => m.ErrorAsync(notificationDeferralId, expectedException)).Returns(Task.FromResult(0));
            mockDeserializer.Setup(m => m.Deserialize(mockDeferralWaiting.Object.ExtendedData)).Throws(expectedException);

            //Test
            var processor = new CommunicationDeferralProvider(mockDeserializer.Object,
                mockProvider.Object,
                mockManager.Object,
                mockLog.Object,
                mockConfig.Object
                );

            await Assert.ThrowsExceptionAsync<ApplicationException>(async () =>
            {
                var result = await processor.ExecuteAsync(requestTime);

                //Assert
                Assert.IsTrue(result);
            });

            //Verify
            mock.VerifyAll();
        }
    }
}
