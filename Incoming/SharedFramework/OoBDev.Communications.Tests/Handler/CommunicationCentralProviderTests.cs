using OoBDev.Communications.Contracts;
using OoBDev.Communications.Contracts.Handler;
using OoBDev.Communications.Handler;
using OoBDev.TestUtilities;
using OoBDev.Toolkit.Contracts.Common;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.Communications.Tests.Handler
{
    [TestClass]
    public class CommunicationCentralProviderTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public async Task ReceivedAsyncTest_Deferred()
        {
            //Stage
            var request = new
            {
                TargetPersonId = Guid.NewGuid(),
                MessageType = "Test Message Type",
                Priority = RequestPriorities.Normal,
            };
            var coorRequest = Guid.NewGuid();
            var coorResponse = coorRequest;
            var time = DateTimeOffset.UtcNow;
            DateTimeOffset? defferedUntil = time.AddMinutes(5);
            var headers = new Dictionary<string, object>();

            //Mock
            var mock = new MockRepository(MockBehavior.Strict);
            var mockTarget = mock.Create<ITargetPreferenceManager>();
            var mockProcess = mock.Create<ICommunicationCentralProcessor>();
            var mockLog = mock.Create<ILogger<CommunicationCentralProvider>>(MockBehavior.Loose);
            var mockDateTools = mock.Create<IDateTools>();
            var mockGuidTools = mock.Create<IGuidTools>();

            var mockSendRequest = mock.Create<ISendRequest>();
            var mockTargetPreference = mock.Create<ITargetPreference>();

            mockDateTools.Setup(m => m.UtcNow()).Returns(time);
            mockTarget.Setup(m => m.GetTargetPreferencesAsync(request.TargetPersonId, request.MessageType)).ReturnsAsync(mockTargetPreference.Object);
            mockProcess.Setup(m => m.IsDeferredUntil(mockTargetPreference.Object, mockSendRequest.Object, time)).Returns(defferedUntil);
            mockProcess.Setup(m => m.DeferRequestAsync(mockTargetPreference.Object, coorRequest, mockSendRequest.Object, defferedUntil.Value, headers)).Returns(Task.FromResult(0));
            mockSendRequest.Setup(m => m.TargetPersonId).Returns(request.TargetPersonId);
            mockSendRequest.Setup(m => m.MessageType).Returns(request.MessageType);
            mockSendRequest.Setup(m => m.Priority).Returns(request.Priority);

            //Test
            var provider = new CommunicationCentralProvider(
                mockTarget.Object,
                mockProcess.Object,
                mockLog.Object,
                mockDateTools.Object,
                mockGuidTools.Object
                );

            var result = await provider.ReceivedAsync(mockSendRequest.Object, coorRequest, headers);

            //Assert
            Assert.AreEqual(coorResponse, result);

            //Verify
            mock.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public async Task ReceivedAsyncTest_SendNow()
        {
            //Stage
            var request = new
            {
                TargetPersonId = Guid.NewGuid(),
                MessageType = "Test Message Type",
                Priority = RequestPriorities.Normal,
            };
            var coorRequest = Guid.NewGuid();
            var coorResponse = coorRequest;
            var time = DateTimeOffset.UtcNow;
            DateTimeOffset? defferedUntil = null;
            var headers = new Dictionary<string, object>();

            //Mock
            var mock = new MockRepository(MockBehavior.Strict);
            var mockTarget = mock.Create<ITargetPreferenceManager>();
            var mockProcess = mock.Create<ICommunicationCentralProcessor>();
            var mockLog = mock.Create<ILogger<CommunicationCentralProvider>>(MockBehavior.Loose);
            var mockDateTools = mock.Create<IDateTools>();
            var mockGuidTools = mock.Create<IGuidTools>();

            var mockSendRequest = mock.Create<ISendRequest>();
            var mockTargetPreference = mock.Create<ITargetPreference>();

            mockDateTools.Setup(m => m.UtcNow()).Returns(time);
            mockTarget.Setup(m => m.GetTargetPreferencesAsync(request.TargetPersonId, request.MessageType)).ReturnsAsync(mockTargetPreference.Object);
            mockProcess.Setup(m => m.IsDeferredUntil(mockTargetPreference.Object, mockSendRequest.Object, time)).Returns(defferedUntil);
            mockProcess.Setup(m => m.HandleRequestAsync(mockTargetPreference.Object, coorRequest, mockSendRequest.Object, headers)).Returns(Task.FromResult(0));
            mockSendRequest.Setup(m => m.TargetPersonId).Returns(request.TargetPersonId);
            mockSendRequest.Setup(m => m.MessageType).Returns(request.MessageType);
            mockSendRequest.Setup(m => m.Priority).Returns(request.Priority);

            //Test
            var provider = new CommunicationCentralProvider(
                mockTarget.Object,
                mockProcess.Object,
                mockLog.Object,
                mockDateTools.Object,
                mockGuidTools.Object
                );

            var result = await provider.ReceivedAsync(mockSendRequest.Object, coorRequest, headers);

            //Assert
            Assert.AreEqual(coorResponse, result);

            //Verify
            mock.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public async Task ReceivedAsyncTest_Immediate()
        {
            //Stage
            var request = new
            {
                TargetPersonId = Guid.NewGuid(),
                MessageType = "Test Message Type",
                Priority = RequestPriorities.Immediate,
            };
            var coorRequest = Guid.NewGuid();
            var coorResponse = coorRequest;
            var time = DateTimeOffset.UtcNow;
            DateTimeOffset? defferedUntil = time.AddMinutes(5);
            var headers = new Dictionary<string, object>();

            //Mock
            var mock = new MockRepository(MockBehavior.Strict);
            var mockTarget = mock.Create<ITargetPreferenceManager>();
            var mockProcess = mock.Create<ICommunicationCentralProcessor>();
            var mockLog = mock.Create<ILogger<CommunicationCentralProvider>>(MockBehavior.Loose);
            var mockDateTools = mock.Create<IDateTools>();
            var mockGuidTools = mock.Create<IGuidTools>();

            var mockSendRequest = mock.Create<ISendRequest>();
            var mockTargetPreference = mock.Create<ITargetPreference>();

            mockDateTools.Setup(m => m.UtcNow()).Returns(time);
            mockTarget.Setup(m => m.GetTargetPreferencesAsync(request.TargetPersonId, request.MessageType)).ReturnsAsync(mockTargetPreference.Object);
            mockProcess.Setup(m => m.IsDeferredUntil(mockTargetPreference.Object, mockSendRequest.Object, time)).Returns(defferedUntil);
            mockProcess.Setup(m => m.HandleRequestAsync(mockTargetPreference.Object, coorRequest, mockSendRequest.Object, headers)).Returns(Task.FromResult(0));
            mockSendRequest.Setup(m => m.TargetPersonId).Returns(request.TargetPersonId);
            mockSendRequest.Setup(m => m.MessageType).Returns(request.MessageType);
            mockSendRequest.Setup(m => m.Priority).Returns(request.Priority);

            //Test
            var provider = new CommunicationCentralProvider(
                mockTarget.Object,
                mockProcess.Object,
                mockLog.Object,
                mockDateTools.Object,
                mockGuidTools.Object
                );

            var result = await provider.ReceivedAsync(mockSendRequest.Object, coorRequest, headers);

            //Assert
            Assert.AreEqual(coorResponse, result);

            //Verify
            mock.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public async Task ReceivedAsyncTest_EmptyIdDeferred()
        {
            //Stage
            var request = new
            {
                TargetPersonId = Guid.NewGuid(),
                MessageType = "Test Message Type",
                Priority = RequestPriorities.Normal,
            };
            var coorRequest = Guid.Empty;
            var coorResponse = Guid.NewGuid();
            var time = DateTimeOffset.UtcNow;
            DateTimeOffset? defferedUntil = time.AddMinutes(5);
            var headers = new Dictionary<string, object>();

            //Mock
            var mock = new MockRepository(MockBehavior.Strict);
            var mockTarget = mock.Create<ITargetPreferenceManager>();
            var mockProcess = mock.Create<ICommunicationCentralProcessor>();
            var mockLog = mock.Create<ILogger<CommunicationCentralProvider>>(MockBehavior.Loose);
            var mockDateTools = mock.Create<IDateTools>();
            var mockGuidTools = mock.Create<IGuidTools>();

            var mockSendRequest = mock.Create<ISendRequest>();
            var mockTargetPreference = mock.Create<ITargetPreference>();

            mockDateTools.Setup(m => m.UtcNow()).Returns(time);
            mockGuidTools.Setup(m => m.NewGuid()).Returns(coorResponse);
            mockTarget.Setup(m => m.GetTargetPreferencesAsync(request.TargetPersonId, request.MessageType)).ReturnsAsync(mockTargetPreference.Object);
            mockProcess.Setup(m => m.IsDeferredUntil(mockTargetPreference.Object, mockSendRequest.Object, time)).Returns(defferedUntil);
            mockProcess.Setup(m => m.DeferRequestAsync(mockTargetPreference.Object, coorResponse, mockSendRequest.Object, defferedUntil.Value, headers)).Returns(Task.FromResult(0));
            mockSendRequest.Setup(m => m.TargetPersonId).Returns(request.TargetPersonId);
            mockSendRequest.Setup(m => m.MessageType).Returns(request.MessageType);
            mockSendRequest.Setup(m => m.Priority).Returns(request.Priority);

            //Test
            var provider = new CommunicationCentralProvider(
                mockTarget.Object,
                mockProcess.Object,
                mockLog.Object,
                mockDateTools.Object,
                mockGuidTools.Object
                );

            var result = await provider.ReceivedAsync(mockSendRequest.Object, coorRequest, headers);

            //Assert
            Assert.AreEqual(coorResponse, result);

            //Verify
            mock.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        [ExpectedException(typeof(ApplicationException))]
        public async Task ReceivedAsyncTest_Exception()
        {
            //Stage
            var request = new
            {
                TargetPersonId = Guid.NewGuid(),
                MessageType = "Test Message Type",
                Priority = RequestPriorities.Normal,
            };
            var coorRequest = Guid.Empty;
            var coorResponse = coorRequest;
            var exception = new ApplicationException();
            var headers = new Dictionary<string, object>();

            //Mock
            var mock = new MockRepository(MockBehavior.Strict);
            var mockTarget = mock.Create<ITargetPreferenceManager>();
            var mockProcess = mock.Create<ICommunicationCentralProcessor>();
            var mockLog = mock.Create<ILogger<CommunicationCentralProvider>>(MockBehavior.Loose);
            var mockDateTools = mock.Create<IDateTools>();
            var mockGuidTools = mock.Create<IGuidTools>();

            var mockSendRequest = mock.Create<ISendRequest>();
            var mockTargetPreference = mock.Create<ITargetPreference>();

            mockSendRequest.Setup(m => m.TargetPersonId).Returns(request.TargetPersonId);
            mockSendRequest.Setup(m => m.MessageType).Returns(request.MessageType);
            mockTarget.Setup(m => m.GetTargetPreferencesAsync(request.TargetPersonId, request.MessageType))
                      .Throws(exception);

            //Test
            var provider = new CommunicationCentralProvider(
                mockTarget.Object,
                mockProcess.Object,
                mockLog.Object,
                mockDateTools.Object,
                mockGuidTools.Object
                );
            try
            {
                var result = await provider.ReceivedAsync(mockSendRequest.Object, coorRequest, headers);

                //Assert
                Assert.Fail("you shouldn't get here");
            }
            finally
            {
                //Verify
                mock.VerifyAll();
            }
        }
    }
}
