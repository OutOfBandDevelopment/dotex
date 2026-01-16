using OoBDev.Communications.Contracts;
using OoBDev.Communications.Contracts.Handler;
using OoBDev.Communications.Handler;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace OoBDev.Communications.Tests.Handler
{
    [TestClass]
    public class TargetPreferenceManagerTests
    {
        private MockRepository mockRepository;

        private Mock<INotificationPreferenceProvider> mockNotificationPreferenceProvider;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockNotificationPreferenceProvider = this.mockRepository.Create<INotificationPreferenceProvider>();
        }

        private TargetPreferenceManager CreateManager()
        {
            return new TargetPreferenceManager(
                this.mockNotificationPreferenceProvider.Object);
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public async Task GetTargetPreferencesAsyncTest_NullPreference()
        {
            // Stage
            var request = new
            {
                targetPersonId = Guid.NewGuid(),
                messageType = "test message type",
            };
            IDeliveryPreference deliveryPreference = null;
            var expected = new[] { DeliveryChannels.None, };

            // Mock

            mockNotificationPreferenceProvider.Setup(m => m.GetDeliveryPreferencesAsync(request.targetPersonId, request.messageType))
                                                  .ReturnsAsync(deliveryPreference);

            // Test
            var manager = this.CreateManager();

            var result = await manager.GetTargetPreferencesAsync(
                request.targetPersonId,
                request.messageType
                );

            // Assert
            CollectionAssert.AreEquivalent(expected, result.Channels);
            Assert.IsNull(result.Culture);
            Assert.IsFalse(result.SkipWeekends);
            Assert.IsNull(result.StartTime);
            Assert.IsNull(result.EndTime);
            Assert.IsNull(result.TimeZone);

            // Verify
            this.mockRepository.VerifyAll();
        }


        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public async Task GetTargetPreferencesAsyncTest_EmptyChannels()
        {
            // Stage
            var request = new
            {
                targetPersonId = Guid.NewGuid(),
                messageType = "test message type",
            };
            var preferences = new
            {
                Channels = Array.Empty<string>(),
                Culture = new CultureInfo("en-US"),
                SkipWeekends = true,
                StartTime = new TimeSpan(5000),
                EndTime = new TimeSpan(10000),
                TimeZone = new TimeSpan(100000),
            };
            var expected = new[] { DeliveryChannels.None, };

            // Mock
            var mockDeliveryPreference = mockRepository.Create<IDeliveryPreference>();

            mockNotificationPreferenceProvider.Setup(m => m.GetDeliveryPreferencesAsync(request.targetPersonId, request.messageType))
                                                  .ReturnsAsync(mockDeliveryPreference.Object);
            mockDeliveryPreference.Setup(m => m.Channels).Returns(preferences.Channels);
            mockDeliveryPreference.Setup(m => m.Culture).Returns(preferences.Culture);
            mockDeliveryPreference.Setup(m => m.SkipWeekends).Returns(preferences.SkipWeekends);
            mockDeliveryPreference.Setup(m => m.StartTime).Returns(preferences.StartTime);
            mockDeliveryPreference.Setup(m => m.EndTime).Returns(preferences.EndTime);
            mockDeliveryPreference.Setup(m => m.TimeZone).Returns(preferences.TimeZone);

            // Test
            var manager = this.CreateManager();

            var result = await manager.GetTargetPreferencesAsync(
                request.targetPersonId,
                request.messageType
                );

            // Assert
            CollectionAssert.AreEquivalent(expected, result.Channels);
            Assert.AreEqual(preferences.Culture, result.Culture);
            Assert.AreEqual(preferences.SkipWeekends, result.SkipWeekends);
            Assert.AreEqual(preferences.StartTime, result.StartTime);
            Assert.AreEqual(preferences.EndTime, result.EndTime);
            Assert.AreEqual(preferences.TimeZone, result.TimeZone);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public async Task GetTargetPreferencesAsyncTest_ExistingChannels()
        {
            // Stage
            var request = new
            {
                targetPersonId = Guid.NewGuid(),
                messageType = "test message type",
            };
            var preferences = new
            {
                Channels = new[] { "Test Channel"},
                Culture = new CultureInfo("en-US"),
                SkipWeekends = true,
                StartTime = new TimeSpan(5000),
                EndTime = new TimeSpan(10000),
                TimeZone = new TimeSpan(100000),
            };
            var expected = new[] { "Test Channel", };

            // Mock
            var mockDeliveryPreference = mockRepository.Create<IDeliveryPreference>();

            mockNotificationPreferenceProvider.Setup(m => m.GetDeliveryPreferencesAsync(request.targetPersonId, request.messageType))
                                                  .ReturnsAsync(mockDeliveryPreference.Object);
            mockDeliveryPreference.Setup(m => m.Channels).Returns(preferences.Channels);
            mockDeliveryPreference.Setup(m => m.Culture).Returns(preferences.Culture);
            mockDeliveryPreference.Setup(m => m.SkipWeekends).Returns(preferences.SkipWeekends);
            mockDeliveryPreference.Setup(m => m.StartTime).Returns(preferences.StartTime);
            mockDeliveryPreference.Setup(m => m.EndTime).Returns(preferences.EndTime);
            mockDeliveryPreference.Setup(m => m.TimeZone).Returns(preferences.TimeZone);

            // Test
            var manager = this.CreateManager();

            var result = await manager.GetTargetPreferencesAsync(
                request.targetPersonId,
                request.messageType
                );

            // Assert
            CollectionAssert.AreEquivalent(expected, result.Channels);
            Assert.AreEqual(preferences.Culture, result.Culture);
            Assert.AreEqual(preferences.SkipWeekends, result.SkipWeekends);
            Assert.AreEqual(preferences.StartTime, result.StartTime);
            Assert.AreEqual(preferences.EndTime, result.EndTime);
            Assert.AreEqual(preferences.TimeZone, result.TimeZone);

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
