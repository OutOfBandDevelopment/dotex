using OoBDev.Communications.Contracts;
using OoBDev.Communications.Contracts.Channels;
using OoBDev.Communications.Contracts.Handler;
using OoBDev.Communications.Contracts.Models;
using OoBDev.Communications.Handler;
using OoBDev.TestUtilities;
using OoBDev.Toolkit.Common;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Communications.Tests.Handler
{
    [TestClass]
    public class CommunicationCentralProcessorTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod, TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public void IsDeferredUntilTest_Ignore_null()
        {
            var mock = new MockRepository(MockBehavior.Strict);

            var mockProcessor = mock.Create<CommunicationCentralProcessor>(
                mock.Create<IDataEnhancementManager>().Object,
                mock.Create<IMessageComposerFactory>().Object,
                mock.Create<ILogger<CommunicationCentralProcessor>>(MockBehavior.Loose).Object,
                mock.Create<IDeferralManager>().Object,
                mock.Create<IObjectSerializer>().Object
                );
            var processor = mockProcessor.Object;
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                processor.IsDeferredUntil(null, null, DateTimeOffset.UtcNow);
                Assert.Fail("you shouldn't get here");
            });
            mock.VerifyAll();
        }

        [TestMethod, TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public void IsDeferredUntilTest_Ignore_Priority()
        {
            var mock = new MockRepository(MockBehavior.Strict);

            var mockProcessor = mock.Create<CommunicationCentralProcessor>(
                mock.Create<IDataEnhancementManager>().Object,
                mock.Create<IMessageComposerFactory>().Object,
                mock.Create<ILogger<CommunicationCentralProcessor>>(MockBehavior.Loose).Object,
                mock.Create<IDeferralManager>().Object,
                mock.Create<IObjectSerializer>().Object
                );
            var processor = mockProcessor.Object;
            var result = processor.IsDeferredUntil(
                new TargetPreferenceModel(),
                new SendRequestModel() { Priority = RequestPriorities.Immediate },
                DateTimeOffset.UtcNow);
            Assert.IsNull(result);
            mock.VerifyAll();
        }
        [TestMethod, TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public void IsDeferredUntilTest_Null_Normal()
        {
            var mock = new MockRepository(MockBehavior.Strict);

            var mockProcessor = mock.Create<CommunicationCentralProcessor>(
                mock.Create<IDataEnhancementManager>().Object,
                mock.Create<IMessageComposerFactory>().Object,
                mock.Create<ILogger<CommunicationCentralProcessor>>(MockBehavior.Loose).Object,
                mock.Create<IDeferralManager>().Object,
                mock.Create<IObjectSerializer>().Object
                );
            var processor = mockProcessor.Object;
            var result = processor.IsDeferredUntil(null, new SendRequestModel(), DateTimeOffset.UtcNow);
            Assert.IsNull(result);
            mock.VerifyAll();
        }

        [TestMethod, TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public void IsDeferredUntilTest_NullStart_Normal()
        {
            var mock = new MockRepository(MockBehavior.Strict);

            var mockProcessor = mock.Create<CommunicationCentralProcessor>(
                mock.Create<IDataEnhancementManager>().Object,
                mock.Create<IMessageComposerFactory>().Object,
                mock.Create<ILogger<CommunicationCentralProcessor>>(MockBehavior.Loose).Object,
                mock.Create<IDeferralManager>().Object,
                mock.Create<IObjectSerializer>().Object
                );
            var processor = mockProcessor.Object;
            var result = processor.IsDeferredUntil(new TargetPreferenceModel
            {
                StartTime = null,
                EndTime = DateTimeOffset.Now.TimeOfDay,
            }, new SendRequestModel(), DateTimeOffset.UtcNow);
            Assert.IsNull(result);
            mock.VerifyAll();
        }
        [TestMethod, TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public void IsDeferredUntilTest_NullEnd_Normal()
        {
            var mock = new MockRepository(MockBehavior.Strict);

            var mockProcessor = mock.Create<CommunicationCentralProcessor>(
                mock.Create<IDataEnhancementManager>().Object,
                mock.Create<IMessageComposerFactory>().Object,
                mock.Create<ILogger<CommunicationCentralProcessor>>(MockBehavior.Loose).Object,
                mock.Create<IDeferralManager>().Object,
                mock.Create<IObjectSerializer>().Object
                );
            var processor = mockProcessor.Object;
            var result = processor.IsDeferredUntil(new TargetPreferenceModel
            {
                StartTime = DateTimeOffset.Now.TimeOfDay,
                EndTime = null,
            }, new SendRequestModel(), DateTimeOffset.UtcNow);
            Assert.IsNull(result);
            mock.VerifyAll();
        }
        [TestMethod, TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public void IsDeferredUntilTest_Between_Normal()
        {
            var mock = new MockRepository(MockBehavior.Strict);
            var now = new DateTimeOffset(2020, 6, 30, 10, 0, 0, new TimeSpan(-5, 0, 0));
            var times = new
            {
                tz = new TimeSpan(-5, 0, 0),
                start = new TimeSpan(8, 0, 0),
                end = new TimeSpan(21, 0, 0),
            };

            var mockProcessor = mock.Create<CommunicationCentralProcessor>(
                mock.Create<IDataEnhancementManager>().Object,
                mock.Create<IMessageComposerFactory>().Object,
                mock.Create<ILogger<CommunicationCentralProcessor>>(MockBehavior.Loose).Object,
                mock.Create<IDeferralManager>().Object,
                mock.Create<IObjectSerializer>().Object
                );
            var processor = mockProcessor.Object;
            var result = processor.IsDeferredUntil(new TargetPreferenceModel
            {
                SkipWeekends = false,
                StartTime = times.start,
                EndTime = times.end,
                TimeZone = times.tz,
            }, new SendRequestModel(), now);
            Assert.IsNull(result);
            mock.VerifyAll();
        }
        [TestMethod, TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public void IsDeferredUntilTest_Before_Normal()
        {
            var mock = new MockRepository(MockBehavior.Strict);
            var now = new DateTimeOffset(2020, 6, 30, 7, 0, 0, new TimeSpan(-5, 0, 0));
            var times = new
            {
                tz = new TimeSpan(-5, 0, 0),
                start = new TimeSpan(8, 0, 0),
                end = new TimeSpan(21, 0, 0),
            };
            var target = new DateTimeOffset(now.ToOffset(times.tz).Date.Add(times.start), times.tz);

            var mockProcessor = mock.Create<CommunicationCentralProcessor>(
                mock.Create<IDataEnhancementManager>().Object,
                mock.Create<IMessageComposerFactory>().Object,
                mock.Create<ILogger<CommunicationCentralProcessor>>(MockBehavior.Loose).Object,
                mock.Create<IDeferralManager>().Object,
                mock.Create<IObjectSerializer>().Object
                );
            var processor = mockProcessor.Object;
            var result = processor.IsDeferredUntil(new TargetPreferenceModel
            {
                SkipWeekends = false,
                StartTime = times.start,
                EndTime = times.end,
                TimeZone = times.tz,
            }, new SendRequestModel(), now);
            Assert.AreEqual(target, result);
            mock.VerifyAll();
        }
        [TestMethod, TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public void IsDeferredUntilTest_After_Normal()
        {
            var mock = new MockRepository(MockBehavior.Strict);
            var now = new DateTimeOffset(2020, 6, 30, 21, 5, 0, new TimeSpan(-5, 0, 0));
            var times = new
            {
                tz = new TimeSpan(-5, 0, 0),
                start = new TimeSpan(8, 0, 0),
                end = new TimeSpan(21, 0, 0),
            };
            var target = new DateTimeOffset(now.ToOffset(times.tz).Date.Add(times.start), times.tz).AddDays(1);

            var mockProcessor = mock.Create<CommunicationCentralProcessor>(
                mock.Create<IDataEnhancementManager>().Object,
                mock.Create<IMessageComposerFactory>().Object,
                mock.Create<ILogger<CommunicationCentralProcessor>>(MockBehavior.Loose).Object,
                mock.Create<IDeferralManager>().Object,
                mock.Create<IObjectSerializer>().Object
                ); ;
            var processor = mockProcessor.Object;
            var result = processor.IsDeferredUntil(new TargetPreferenceModel
            {
                SkipWeekends = false,
                StartTime = times.start,
                EndTime = times.end,
                TimeZone = times.tz,
            }, new SendRequestModel(), now);
            Assert.AreEqual(target, result);
            mock.VerifyAll();
        }

        [TestMethod, TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public void IsDeferredUntilTest_Between_WeekendFriday()
        {
            var mock = new MockRepository(MockBehavior.Strict);
            var now = new DateTimeOffset(2020, 6, 26, 10, 5, 0, new TimeSpan(-5, 0, 0));
            var times = new
            {
                tz = new TimeSpan(-5, 0, 0),
                start = new TimeSpan(8, 0, 0),
                end = new TimeSpan(21, 0, 0),
            };

            var mockProcessor = mock.Create<CommunicationCentralProcessor>(
                mock.Create<IDataEnhancementManager>().Object,
                mock.Create<IMessageComposerFactory>().Object,
                mock.Create<ILogger<CommunicationCentralProcessor>>(MockBehavior.Loose).Object,
                mock.Create<IDeferralManager>().Object,
                mock.Create<IObjectSerializer>().Object
                );
            var processor = mockProcessor.Object;
            var result = processor.IsDeferredUntil(new TargetPreferenceModel
            {
                SkipWeekends = true,
                StartTime = times.start,
                EndTime = times.end,
                TimeZone = times.tz,
            }, new SendRequestModel(), now);
            Assert.IsNull(result);
            mock.VerifyAll();
        }
        [TestMethod, TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public void IsDeferredUntilTest_Before_WeekendFriday()
        {
            var mock = new MockRepository(MockBehavior.Strict);
            var now = new DateTimeOffset(2020, 6, 26, 7, 5, 0, new TimeSpan(-5, 0, 0));
            var times = new
            {
                tz = new TimeSpan(-5, 0, 0),
                start = new TimeSpan(8, 0, 0),
                end = new TimeSpan(21, 0, 0),
            };
            var target = new DateTimeOffset(2020, 6, 26, 8, 0, 0, new TimeSpan(-5, 0, 0));

            var mockProcessor = mock.Create<CommunicationCentralProcessor>(
                mock.Create<IDataEnhancementManager>().Object,
                mock.Create<IMessageComposerFactory>().Object,
                mock.Create<ILogger<CommunicationCentralProcessor>>(MockBehavior.Loose).Object,
                mock.Create<IDeferralManager>().Object,
                mock.Create<IObjectSerializer>().Object
                );
            var processor = mockProcessor.Object;
            var result = processor.IsDeferredUntil(new TargetPreferenceModel
            {
                SkipWeekends = true,
                StartTime = times.start,
                EndTime = times.end,
                TimeZone = times.tz,
            }, new SendRequestModel(), now);
            Assert.AreEqual(target, result);
            mock.VerifyAll();
        }
        [TestMethod, TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public void IsDeferredUntilTest_After_WeekendFriday()
        {
            var mock = new MockRepository(MockBehavior.Strict);
            var now = new DateTimeOffset(2020, 6, 26, 21, 5, 0, new TimeSpan(-5, 0, 0));
            var times = new
            {
                tz = new TimeSpan(-5, 0, 0),
                start = new TimeSpan(8, 0, 0),
                end = new TimeSpan(21, 0, 0),
            };
            var target = new DateTimeOffset(2020, 6, 29, 8, 0, 0, new TimeSpan(-5, 0, 0));

            var mockProcessor = mock.Create<CommunicationCentralProcessor>(
                mock.Create<IDataEnhancementManager>().Object,
                mock.Create<IMessageComposerFactory>().Object,
                mock.Create<ILogger<CommunicationCentralProcessor>>(MockBehavior.Loose).Object,
                mock.Create<IDeferralManager>().Object,
                mock.Create<IObjectSerializer>().Object
                );
            var processor = mockProcessor.Object;
            var result = processor.IsDeferredUntil(new TargetPreferenceModel
            {
                SkipWeekends = true,
                StartTime = times.start,
                EndTime = times.end,
                TimeZone = times.tz,
            }, new SendRequestModel(), now);
            Assert.AreEqual(target, result);
            mock.VerifyAll();
        }


        [TestMethod, TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public void IsDeferredUntilTest_Between_WeekendSaturday()
        {
            var mock = new MockRepository(MockBehavior.Strict);
            var now = new DateTimeOffset(2020, 6, 27, 10, 5, 0, new TimeSpan(-5, 0, 0));
            var times = new
            {
                tz = new TimeSpan(-5, 0, 0),
                start = new TimeSpan(8, 0, 0),
                end = new TimeSpan(21, 0, 0),
            };
            var target = new DateTimeOffset(2020, 6, 29, 8, 0, 0, new TimeSpan(-5, 0, 0));

            var mockProcessor = mock.Create<CommunicationCentralProcessor>(
                mock.Create<IDataEnhancementManager>().Object,
                mock.Create<IMessageComposerFactory>().Object,
                mock.Create<ILogger<CommunicationCentralProcessor>>(MockBehavior.Loose).Object,
                mock.Create<IDeferralManager>().Object,
                mock.Create<IObjectSerializer>().Object
                );
            var processor = mockProcessor.Object;
            var result = processor.IsDeferredUntil(new TargetPreferenceModel
            {
                SkipWeekends = true,
                StartTime = times.start,
                EndTime = times.end,
                TimeZone = times.tz,
            }, new SendRequestModel(), now);
            Assert.AreEqual(target, result);
            mock.VerifyAll();
        }
        [TestMethod, TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public void IsDeferredUntilTest_Before_WeekendSaturday()
        {
            var mock = new MockRepository(MockBehavior.Strict);
            var now = new DateTimeOffset(2020, 6, 27, 7, 5, 0, new TimeSpan(-5, 0, 0));
            var times = new
            {
                tz = new TimeSpan(-5, 0, 0),
                start = new TimeSpan(8, 0, 0),
                end = new TimeSpan(21, 0, 0),
            };
            var target = new DateTimeOffset(2020, 6, 29, 8, 0, 0, new TimeSpan(-5, 0, 0));

            var mockProcessor = mock.Create<CommunicationCentralProcessor>(
                mock.Create<IDataEnhancementManager>().Object,
                mock.Create<IMessageComposerFactory>().Object,
                mock.Create<ILogger<CommunicationCentralProcessor>>(MockBehavior.Loose).Object,
                mock.Create<IDeferralManager>().Object,
                mock.Create<IObjectSerializer>().Object
                );
            var processor = mockProcessor.Object;
            var result = processor.IsDeferredUntil(new TargetPreferenceModel
            {
                SkipWeekends = true,
                StartTime = times.start,
                EndTime = times.end,
                TimeZone = times.tz,
            }, new SendRequestModel(), now);
            Assert.AreEqual(target, result);
            mock.VerifyAll();
        }
        [TestMethod, TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public void IsDeferredUntilTest_After_WeekendSaturday()
        {
            var mock = new MockRepository(MockBehavior.Strict);
            var now = new DateTimeOffset(2020, 6, 27, 21, 5, 0, new TimeSpan(-5, 0, 0));
            var times = new
            {
                tz = new TimeSpan(-5, 0, 0),
                start = new TimeSpan(8, 0, 0),
                end = new TimeSpan(21, 0, 0),
            };
            var target = new DateTimeOffset(2020, 6, 29, 8, 0, 0, new TimeSpan(-5, 0, 0));

            var mockProcessor = mock.Create<CommunicationCentralProcessor>(
                mock.Create<IDataEnhancementManager>().Object,
                mock.Create<IMessageComposerFactory>().Object,
                mock.Create<ILogger<CommunicationCentralProcessor>>(MockBehavior.Loose).Object,
                mock.Create<IDeferralManager>().Object,
                mock.Create<IObjectSerializer>().Object
                );
            var processor = mockProcessor.Object;
            var result = processor.IsDeferredUntil(new TargetPreferenceModel
            {
                SkipWeekends = true,
                StartTime = times.start,
                EndTime = times.end,
                TimeZone = times.tz,
            }, new SendRequestModel(), now);
            Assert.AreEqual(target, result);
            mock.VerifyAll();
        }


        [TestMethod, TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public void IsDeferredUntilTest_Between_WeekendSunday()
        {
            var mock = new MockRepository(MockBehavior.Strict);
            var now = new DateTimeOffset(2020, 6, 28, 10, 5, 0, new TimeSpan(-5, 0, 0));
            var times = new
            {
                tz = new TimeSpan(-5, 0, 0),
                start = new TimeSpan(8, 0, 0),
                end = new TimeSpan(21, 0, 0),
            };
            var target = new DateTimeOffset(2020, 6, 29, 8, 0, 0, new TimeSpan(-5, 0, 0));

            var mockProcessor = mock.Create<CommunicationCentralProcessor>(
                mock.Create<IDataEnhancementManager>().Object,
                mock.Create<IMessageComposerFactory>().Object,
                mock.Create<ILogger<CommunicationCentralProcessor>>(MockBehavior.Loose).Object,
                mock.Create<IDeferralManager>().Object,
                mock.Create<IObjectSerializer>().Object
                );
            var processor = mockProcessor.Object;
            var result = processor.IsDeferredUntil(new TargetPreferenceModel
            {
                SkipWeekends = true,
                StartTime = times.start,
                EndTime = times.end,
                TimeZone = times.tz,
            }, new SendRequestModel(), now);
            Assert.AreEqual(target, result);
            mock.VerifyAll();
        }
        [TestMethod, TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public void IsDeferredUntilTest_Before_WeekendSunday()
        {
            var mock = new MockRepository(MockBehavior.Strict);
            var now = new DateTimeOffset(2020, 6, 28, 7, 5, 0, new TimeSpan(-5, 0, 0));
            var times = new
            {
                tz = new TimeSpan(-5, 0, 0),
                start = new TimeSpan(8, 0, 0),
                end = new TimeSpan(21, 0, 0),
            };
            var target = new DateTimeOffset(2020, 6, 29, 8, 0, 0, new TimeSpan(-5, 0, 0));

            var mockProcessor = mock.Create<CommunicationCentralProcessor>(
                mock.Create<IDataEnhancementManager>().Object,
                mock.Create<IMessageComposerFactory>().Object,
                mock.Create<ILogger<CommunicationCentralProcessor>>(MockBehavior.Loose).Object,
                mock.Create<IDeferralManager>().Object,
                mock.Create<IObjectSerializer>().Object
                );
            var processor = mockProcessor.Object;
            var result = processor.IsDeferredUntil(new TargetPreferenceModel
            {
                SkipWeekends = true,
                StartTime = times.start,
                EndTime = times.end,
                TimeZone = times.tz,
            }, new SendRequestModel(), now);
            Assert.AreEqual(target, result);
            mock.VerifyAll();
        }
        [TestMethod, TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public void IsDeferredUntilTest_After_WeekendSunday()
        {
            var mock = new MockRepository(MockBehavior.Strict);
            var now = new DateTimeOffset(2020, 6, 28, 21, 5, 0, new TimeSpan(-5, 0, 0));
            var times = new
            {
                tz = new TimeSpan(-5, 0, 0),
                start = new TimeSpan(8, 0, 0),
                end = new TimeSpan(21, 0, 0),
            };
            var target = new DateTimeOffset(2020, 6, 29, 8, 0, 0, new TimeSpan(-5, 0, 0));

            var mockProcessor = mock.Create<CommunicationCentralProcessor>(
                mock.Create<IDataEnhancementManager>().Object,
                mock.Create<IMessageComposerFactory>().Object,
                mock.Create<ILogger<CommunicationCentralProcessor>>(MockBehavior.Loose).Object,
                mock.Create<IDeferralManager>().Object,
                mock.Create<IObjectSerializer>().Object
                );
            var processor = mockProcessor.Object;
            var result = processor.IsDeferredUntil(new TargetPreferenceModel
            {
                SkipWeekends = true,
                StartTime = times.start,
                EndTime = times.end,
                TimeZone = times.tz,
            }, new SendRequestModel(), now);
            Assert.AreEqual(target, result);
            mock.VerifyAll();
        }

        [TestMethod, TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public async Task DeferRequestAsyncTest()
        {
            //Stage
            var preference = new TargetPreferenceModel { };
            var correlationId = Guid.NewGuid();
            var request = new SendRequestModel
            {
                TargetPersonId = Guid.NewGuid(),
                MessageType = "test message type",
                Data = new object(),
            };
            var until = DateTimeOffset.Now;

            var coorId = Guid.NewGuid();
            var serialized = "fake serial";
            var headers = new Dictionary<string, object>();
            var temp = new JObject();

            //Mock
            var mock = new MockRepository(MockBehavior.Strict);
            var mockData = mock.Create<IDataEnhancementManager>();
            var mockMessage = mock.Create<IMessageComposerFactory>();
            var mockLog = mock.Create<ILogger<CommunicationCentralProcessor>>();
            var mockDeferral = mock.Create<IDeferralManager>();
            var mockSerializer = mock.Create<IObjectSerializer>();

            mockData.Setup(s => s.SeedData(
                request.Data,
                It.Is<(string label, object data)[]>(i =>
                    i.Any(f => f.label == "Request-Headers" && f.data == headers) &&
                    i.Any(f => f.label == "Request-MessageType" && object.Equals(f.data, request.MessageType)) &&
                    i.Any(f => f.label == "Request-CorrelationId" && object.Equals(f.data, correlationId))
                )
                )).Returns(temp);

            mockSerializer.Setup(m => m.GetAsSerialized(temp)).Returns(serialized);
            mockDeferral.Setup(m => m.PostAsync(It.Is<DeferralRequestModel>(i =>
                i.CorrelationId == correlationId &&
                i.TargetPersonId == request.TargetPersonId &&
                i.MessageType == request.MessageType &&
                i.ExtendedData == serialized &&
                i.HoldUntil == until
            ))).ReturnsAsync(coorId);

            //Test
            var processor = new CommunicationCentralProcessor(
                mockData.Object,
                mockMessage.Object,
                mockLog.Object,
                mockDeferral.Object,
                mockSerializer.Object
                );

            await processor.DeferRequestAsync(preference, correlationId, request, until, headers);

            //Assert
            //Verify
            mock.VerifyAll();
        }

        [TestMethod, TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public async Task HandleRequestAsyncTest_NoPreference()
        {
            //Stage
            TargetPreferenceModel preference = null;
            var correlationId = Guid.NewGuid();
            var request = new SendRequestModel
            {
                TargetPersonId = Guid.NewGuid(),
                MessageType = "test message type",
                Data = new object(),
            };

            //var coorId = Guid.NewGuid();
            //var serialized = "fake serial";
            var headers = new Dictionary<string, object>();

            //Mock
            var mock = new MockRepository(MockBehavior.Strict);
            var mockData = mock.Create<IDataEnhancementManager>();
            var mockMessage = mock.Create<IMessageComposerFactory>();
            var mockLog = mock.Create<ILogger<CommunicationCentralProcessor>>(MockBehavior.Loose);
            var mockDeferral = mock.Create<IDeferralManager>();
            var mockSerializer = mock.Create<IObjectSerializer>();


            //Test
            var processor = new CommunicationCentralProcessor(
                mockData.Object,
                mockMessage.Object,
                mockLog.Object,
                mockDeferral.Object,
                mockSerializer.Object
                );

            await processor.HandleRequestAsync(preference, correlationId, request, headers);

            //Assert

            //Verify
            mock.VerifyAll();
        }

        [TestMethod, TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public async Task HandleRequestAsyncTest_NoChannels()
        {
            //Stage
            var preference = new TargetPreferenceModel
            {
                Channels = Array.Empty<string>(),
            };
            var correlationId = Guid.NewGuid();
            var request = new SendRequestModel
            {
                TargetPersonId = Guid.NewGuid(),
                MessageType = "test message type",
                Data = new object(),
            };

            //var coorId = Guid.NewGuid();
            //var serialized = "fake serial";
            var headers = new Dictionary<string, object>();

            //Mock
            var mock = new MockRepository(MockBehavior.Strict);
            var mockData = mock.Create<IDataEnhancementManager>();
            var mockMessage = mock.Create<IMessageComposerFactory>();
            var mockLog = mock.Create<ILogger<CommunicationCentralProcessor>>(MockBehavior.Loose);
            var mockDeferral = mock.Create<IDeferralManager>();
            var mockSerializer = mock.Create<IObjectSerializer>();


            //Test
            var processor = new CommunicationCentralProcessor(
                mockData.Object,
                mockMessage.Object,
                mockLog.Object,
                mockDeferral.Object,
                mockSerializer.Object
                );

            await processor.HandleRequestAsync(preference, correlationId, request, headers);

            //Assert

            //Verify
            mock.VerifyAll();

        }

        [TestMethod, TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public async Task HandleRequestAsyncTest_2Channels()
        {
            //Stage
            var preference = new TargetPreferenceModel
            {
                Channels = new[] { "test channel", "test channel 2" },
                Culture = CultureInfo.GetCultureInfo("es-ES"),
            };
            var correlationId = Guid.NewGuid();
            var request = new SendRequestModel
            {
                TargetPersonId = Guid.NewGuid(),
                MessageType = "test message type",
                Data = new object(),
            };

            var enhanced = new JObject();
            //var coorId = Guid.NewGuid();
            //var serialized = "fake serial";
            var headers = new Dictionary<string, object>();
            var temp = new JObject();

            //Mock
            var mock = new MockRepository(MockBehavior.Strict);
            var mockData = mock.Create<IDataEnhancementManager>();
            var mockMessage = mock.Create<IMessageComposerFactory>();
            var mockLog = mock.Create<ILogger<CommunicationCentralProcessor>>(MockBehavior.Loose);
            var mockDeferral = mock.Create<IDeferralManager>();
            var mockSerializer = mock.Create<IObjectSerializer>();

            mockData.Setup(s => s.SeedData(
                request.Data,
                It.Is<(string label, object data)[]>(i =>
                    i.Any(f => f.label == "Request-Headers" && f.data == headers) &&
                    i.Any(f => f.label == "Request-MessageType" && object.Equals(f.data, request.MessageType)) &&
                    i.Any(f => f.label == "Request-CorrelationId" && object.Equals(f.data, correlationId))
                )
                )).Returns(temp);
            mockData.Setup(m => m.EnhanceAsync(request.TargetPersonId, request.MessageType, temp)).ReturnsAsync(enhanced);

            foreach (var testChannel in preference.Channels)
            {
                var mockComposer = mock.Create<IMessageComposer>();
                mockComposer.Setup(m => m.ComposeAndSendAsync(
                    request.TargetPersonId,
                    request.MessageType,
                    preference.Culture,
                    enhanced,
                    correlationId,
                    headers
                    )).Returns(Task.FromResult(0));

                mockMessage.Setup(m => m.GetComposer(testChannel)).Returns(mockComposer.Object);
            }

            //Test
            var processor = new CommunicationCentralProcessor(
                mockData.Object,
                mockMessage.Object,
                mockLog.Object,
                mockDeferral.Object,
                mockSerializer.Object
                );

            await processor.HandleRequestAsync(preference, correlationId, request, headers);

            //Assert

            //Verify
            mock.VerifyAll();
        }

        [TestMethod, TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public async Task HandleRequestAsyncTest_2Channels_1Composer()
        {
            //Stage
            var preference = new TargetPreferenceModel
            {
                Channels = new[] { "test channel", "test channel 2" },
                Culture = CultureInfo.GetCultureInfo("es-ES"),
            };
            var correlationId = Guid.NewGuid();
            var request = new SendRequestModel
            {
                TargetPersonId = Guid.NewGuid(),
                MessageType = "test message type",
                Data = new object(),
            };

            var enhanced = new JObject();
            //var coorId = Guid.NewGuid();
            //var serialized = "fake serial";
            var headers = new Dictionary<string, object>();
            var temp = new JObject();

            //Mock
            var mock = new MockRepository(MockBehavior.Strict);
            var mockData = mock.Create<IDataEnhancementManager>();
            var mockMessage = mock.Create<IMessageComposerFactory>();
            var mockLog = mock.Create<ILogger<CommunicationCentralProcessor>>(MockBehavior.Loose);
            var mockDeferral = mock.Create<IDeferralManager>();
            var mockSerializer = mock.Create<IObjectSerializer>();

            mockData.Setup(s => s.SeedData(
                request.Data,
                It.Is<(string label, object data)[]>(i =>
                    i.Any(f => f.label == "Request-Headers" && f.data == headers) &&
                    i.Any(f => f.label == "Request-MessageType" && object.Equals(f.data, request.MessageType)) &&
                    i.Any(f => f.label == "Request-CorrelationId" && object.Equals(f.data, correlationId))
                )
                )).Returns(temp);

            mockData.Setup(m => m.EnhanceAsync(request.TargetPersonId, request.MessageType, temp)).ReturnsAsync(enhanced);

            foreach (var testChannel in preference.Channels.Take(1))
            {
                var mockComposer = mock.Create<IMessageComposer>();
                mockComposer.Setup(m => m.ComposeAndSendAsync(
                    request.TargetPersonId,
                    request.MessageType,
                    preference.Culture,
                    enhanced,
                    correlationId,
                    headers
                    )).Returns(Task.FromResult(0));

                mockMessage.Setup(m => m.GetComposer(testChannel)).Returns(mockComposer.Object);
            }

            foreach (var testChannel in preference.Channels.Skip(1))
            {
                mockMessage.Setup(m => m.GetComposer(testChannel)).Returns((IMessageComposer)null);
            }

            //Test
            var processor = new CommunicationCentralProcessor(
                mockData.Object,
                mockMessage.Object,
                mockLog.Object,
                mockDeferral.Object,
                mockSerializer.Object
                );

            await processor.HandleRequestAsync(preference, correlationId, request, headers);

            //Assert

            //Verify
            mock.VerifyAll();
        }

        [TestMethod, TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public async Task HandleRequestAsyncTest_1ChannelsWithException()
        {
            //Stage
            var preference = new TargetPreferenceModel
            {
                Channels = new[] { "test channel", },
                Culture = CultureInfo.GetCultureInfo("es-ES"),
            };
            var correlationId = Guid.NewGuid();
            var request = new SendRequestModel
            {
                TargetPersonId = Guid.NewGuid(),
                MessageType = "test message type",
                Data = new object(),
            };

            var enhanced = new JObject();
            var exception = new InvalidOperationException();
            var headers = new Dictionary<string, object>();
            var temp = new JObject();

            //Mock
            var mock = new MockRepository(MockBehavior.Strict);
            var mockData = mock.Create<IDataEnhancementManager>();
            var mockMessage = mock.Create<IMessageComposerFactory>();
            var mockLog = mock.Create<ILogger<CommunicationCentralProcessor>>(MockBehavior.Loose);
            var mockDeferral = mock.Create<IDeferralManager>();
            var mockSerializer = mock.Create<IObjectSerializer>();

            mockData.Setup(s => s.SeedData(
                request.Data,
                It.Is<(string label, object data)[]>(i =>
                    i.Any(f => f.label == "Request-Headers" && f.data == headers) &&
                    i.Any(f => f.label == "Request-MessageType" && object.Equals(f.data, request.MessageType)) &&
                    i.Any(f => f.label == "Request-CorrelationId" && object.Equals(f.data, correlationId))
                )
                )).Returns(temp);

            mockData.Setup(m => m.EnhanceAsync(request.TargetPersonId, request.MessageType, temp)).ReturnsAsync(enhanced);

            foreach (var testChannel in preference.Channels)
            {
                mockMessage.Setup(m => m.GetComposer(testChannel)).Throws(exception);
            }

            //Test
            var processor = new CommunicationCentralProcessor(
                mockData.Object,
                mockMessage.Object,
                mockLog.Object,
                mockDeferral.Object,
                mockSerializer.Object
                );

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
            {
                await processor.HandleRequestAsync(preference, correlationId, request, headers);

                //Assert

                //Verify
                mock.VerifyAll();
            });
        }
    }
}
