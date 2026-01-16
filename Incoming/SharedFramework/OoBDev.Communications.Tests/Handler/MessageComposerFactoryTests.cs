#nullable enable

using OoBDev.TestUtilities;
using OoBDev.Communications.Handler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using OoBDev.Communications.Contracts.Channels;
using System.Threading.Tasks;
using System.Globalization;
using Newtonsoft.Json.Linq;
using OoBDev.Communications.Contracts;
using System.Collections.Generic;

namespace OoBDev.Communications.Tests.Handler
{
    [TestClass]
    public class MessageComposerFactoryTests
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private MockRepository mockRepository;
        private Mock<IServiceProvider> mockServiceProvider;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockServiceProvider = this.mockRepository.Create<IServiceProvider>();
        }

        public abstract class MessageComposerBase : IMessageComposer
        {
            public Task ComposeAndSendAsync(Guid targetPersonId, string messageType, CultureInfo? culture, JObject data, Guid requestGroupId, IDictionary<string, object> headers)
            {
                throw new NotImplementedException();
            }
        }
        public class MessageComposerNoAttribute : MessageComposerBase { }
        [Composer]
        public class MessageComposerEmptyAttribute : MessageComposerBase { }
        [Composer(DeliveryChannel = "Matched")]
        public class MessageComposerMatched : MessageComposerBase { }
        [Composer(DeliveryChannel = "Unmatched")]
        public class MessageComposerUnmatched : MessageComposerBase { }


        private MessageComposerFactory CreateFactory()
        {
            this.mockServiceProvider.Setup(m => m.GetService(typeof(IEnumerable<IMessageComposer>)))
                                    .Returns(new IMessageComposer[] {
                                        new MessageComposerNoAttribute(),
                                        new MessageComposerEmptyAttribute(),
                                        new MessageComposerMatched(),
                                        new MessageComposerUnmatched(),
                                    });
            return new MessageComposerFactory(
                this.mockServiceProvider.Object);
        }


        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public void GetComposerTest_Null()
        {
            // Stage
            string? channel = null;


            // Mock

            // Test
            var factory = this.CreateFactory();

            var result = factory.GetComposer(
                channel);

            // Assert
            Assert.IsNull(result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public void GetComposerTest_Empty()
        {
            // Stage
            string channel = "";


            // Mock

            // Test
            var factory = this.CreateFactory();

            var result = factory.GetComposer(
                channel);

            // Assert
            Assert.IsNull(result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public void GetComposerTest_Matched()
        {
            // Stage
            string channel = "Matched";


            // Mock

            // Test
            var factory = this.CreateFactory();

            var result = factory.GetComposer(
                channel);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(MessageComposerMatched));

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
