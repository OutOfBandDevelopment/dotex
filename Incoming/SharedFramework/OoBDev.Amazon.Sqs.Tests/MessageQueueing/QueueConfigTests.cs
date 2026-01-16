using Amazon;
using OoBDev.Amazon.Sqs.MessageQueueing;
using OoBDev.MessageQueueing;
using OoBDev.MessageQueueing.Contracts;
using OoBDev.MessageQueueing.Contracts.Services;
using OoBDev.MessageQueueing.Resolvers;
using OoBDev.TestUtilities;
using OoBDev.Toolkit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace OoBDev.Amazon.Sqs.Tests.MessageQueueing
{
    [TestClass]
    public class QueueConfigTests
    {
        public TestContext? TestContext { get; set; }

        [MessageQueue(QueueType = QueueTypes.AmazonSimpleQueue, SimpleName ="{Class or Simple Name}")]
        public class TestTarget { }

        [TestMethod, TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.MessageQueueing)]
        public void ConstructionTest()
        {
            var moq = new MockRepository(MockBehavior.Strict);

            //Mock
            var mockQueueResolver = moq.Create<IQueueResolver<QueueConfigTests>>();
            mockQueueResolver.Setup(m => m.GetConnectionString()).Returns((QueueConnectionString)"Region=us-east-2;AccessKeyId=FakeKeyId;SecretAccessKey=FakeAccessSecret");
            mockQueueResolver.Setup(m => m.GetConfigurationValue(nameof(QueueConfig<QueueConfigTests>.DelaySeconds))).Returns("10");
            mockQueueResolver.Setup(m => m.GetConfigurationValue(nameof(QueueConfig<QueueConfigTests>.MaxNumberOfMessages))).Returns("4");
            mockQueueResolver.Setup(m => m.GetConfigurationValue(nameof(QueueConfig<QueueConfigTests>.WaitTimeSeconds))).Returns("5");
            mockQueueResolver.Setup(m => m.GetConfigurationValue(nameof(QueueConfig<QueueConfigTests>.LeadOutSeconds))).Returns("7");

            //Setup
            var sp = new ServiceCollection()
                .AddTransient(sp => mockQueueResolver.Object)
                .BuildServiceProvider();

            //Test
            var config = ActivatorUtilities.CreateInstance<QueueConfig<QueueConfigTests>>(sp);

            //Assert
            Assert.AreEqual("US East (Ohio)", config.Region.DisplayName);
            Assert.AreEqual("FakeAccessSecret", config.SecretAccessKey);
            Assert.AreEqual("FakeKeyId", config.AccessKeyId);
            Assert.AreEqual(10, config.DelaySeconds);
            Assert.AreEqual(4, config.MaxNumberOfMessages);
            Assert.AreEqual(5, config.WaitTimeSeconds);
            Assert.AreEqual(7, config.LeadOutSeconds);

            //Verify
            moq.VerifyAll();
        }

        [TestMethod, TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.MessageQueueing)]
        public void ConstructionTest_Defaults()
        {
            var moq = new MockRepository(MockBehavior.Strict);

            //Mock
            var mockQueueResolver = moq.Create<IQueueResolver<QueueConfigTests>>();
            mockQueueResolver.Setup(m => m.GetConnectionString()).Returns<QueueConnectionString>(null);
            mockQueueResolver.Setup(m => m.GetConfigurationValue(nameof(QueueConfig<QueueConfigTests>.DelaySeconds))).Returns<string>(null);
            mockQueueResolver.Setup(m => m.GetConfigurationValue(nameof(QueueConfig<QueueConfigTests>.MaxNumberOfMessages))).Returns<string>(null);
            mockQueueResolver.Setup(m => m.GetConfigurationValue(nameof(QueueConfig<QueueConfigTests>.WaitTimeSeconds))).Returns<string>(null);
            mockQueueResolver.Setup(m => m.GetConfigurationValue(nameof(QueueConfig<QueueConfigTests>.LeadOutSeconds))).Returns<string>(null);

            //Setup
            var sp = new ServiceCollection()
                .AddTransient(sp => mockQueueResolver.Object)
                .BuildServiceProvider();

            //Test
            var config = ActivatorUtilities.CreateInstance<QueueConfig<QueueConfigTests>>(sp);

            //Assert
            Assert.AreEqual("US East (N. Virginia)", config.Region.DisplayName);
            Assert.IsNull(config.SecretAccessKey);
            Assert.IsNull(config.AccessKeyId);
            Assert.AreEqual(0, config.DelaySeconds);
            Assert.AreEqual(10, config.MaxNumberOfMessages);
            Assert.AreEqual(20, config.WaitTimeSeconds);
            Assert.AreEqual(10, config.LeadOutSeconds);

            //Verify
            moq.VerifyAll();
        }

        [TestMethod, TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.MessageQueueing)]
        public void ConstructionTest_NoConfig()
        {
            var moq = new MockRepository(MockBehavior.Strict);

            //Mock
            var mockQueueResolver = moq.Create<IQueueResolver<QueueConfigTests>>();
            mockQueueResolver.Setup(m => m.GetConnectionString()).Returns((QueueConnectionString)"");
            mockQueueResolver.Setup(m => m.GetConfigurationValue(nameof(QueueConfig<QueueConfigTests>.DelaySeconds))).Returns("");
            mockQueueResolver.Setup(m => m.GetConfigurationValue(nameof(QueueConfig<QueueConfigTests>.MaxNumberOfMessages))).Returns("");
            mockQueueResolver.Setup(m => m.GetConfigurationValue(nameof(QueueConfig<QueueConfigTests>.WaitTimeSeconds))).Returns("");
            mockQueueResolver.Setup(m => m.GetConfigurationValue(nameof(QueueConfig<QueueConfigTests>.LeadOutSeconds))).Returns("");

            //Setup
            var sp = new ServiceCollection()
                .AddTransient(sp => mockQueueResolver.Object)
                .BuildServiceProvider();

            //Test
            var config = ActivatorUtilities.CreateInstance<QueueConfig<QueueConfigTests>>(sp);

            //Assert
            Assert.AreEqual("US East (N. Virginia)", config.Region.DisplayName);
            Assert.IsNull(config.SecretAccessKey);
            Assert.IsNull(config.AccessKeyId);
            Assert.AreEqual(0, config.DelaySeconds);
            Assert.AreEqual(10, config.MaxNumberOfMessages);
            Assert.AreEqual(20, config.WaitTimeSeconds);
            Assert.AreEqual(10, config.LeadOutSeconds);

            //Verify
            moq.VerifyAll();
        }

        [TestMethod, TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.MessageQueueing)]
        public void ConstructionTest_Null()
        {
            IQueueResolver<QueueConfigTests>? queueResolver = null;

            //Mock

            //Setup

            //Test
            var config = new QueueConfig<QueueConfigTests>(queueResolver);

            //Assert
            Assert.AreEqual("US East (N. Virginia)", config.Region.DisplayName);
            Assert.IsNull(config.SecretAccessKey);
            Assert.IsNull(config.AccessKeyId);
            Assert.AreEqual(0, config.DelaySeconds);
            Assert.AreEqual(10, config.MaxNumberOfMessages);
            Assert.AreEqual(20, config.WaitTimeSeconds);
            Assert.AreEqual(10, config.LeadOutSeconds);

            //Verify
        }


        [TestMethod, TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.MessageQueueing)]
        public void ConstructionTest_Max()
        {
            var moq = new MockRepository(MockBehavior.Strict);

            //Mock
            var mockQueueResolver = moq.Create<IQueueResolver<QueueConfigTests>>();
            mockQueueResolver.Setup(m => m.GetConnectionString()).Returns((QueueConnectionString)"Region=us-east-2;AccessKeyId=FakeKeyId;SecretAccessKey=FakeAccessSecret");
            mockQueueResolver.Setup(m => m.GetConfigurationValue(nameof(QueueConfig<QueueConfigTests>.DelaySeconds))).Returns("10000");
            mockQueueResolver.Setup(m => m.GetConfigurationValue(nameof(QueueConfig<QueueConfigTests>.MaxNumberOfMessages))).Returns("10000");
            mockQueueResolver.Setup(m => m.GetConfigurationValue(nameof(QueueConfig<QueueConfigTests>.WaitTimeSeconds))).Returns("10000");
            mockQueueResolver.Setup(m => m.GetConfigurationValue(nameof(QueueConfig<QueueConfigTests>.LeadOutSeconds))).Returns("10000");

            //Setup
            var sp = new ServiceCollection()
                .AddTransient(sp => mockQueueResolver.Object)
                .BuildServiceProvider();

            //Test
            var config = ActivatorUtilities.CreateInstance<QueueConfig<QueueConfigTests>>(sp);

            //Assert
            Assert.AreEqual("US East (Ohio)", config.Region.DisplayName);
            Assert.AreEqual("FakeAccessSecret", config.SecretAccessKey);
            Assert.AreEqual("FakeKeyId", config.AccessKeyId);
            Assert.AreEqual(900, config.DelaySeconds);
            Assert.AreEqual(10, config.MaxNumberOfMessages);
            Assert.AreEqual(20, config.WaitTimeSeconds);
            Assert.AreEqual(300, config.LeadOutSeconds);

            //Verify
            moq.VerifyAll();
        }

        [TestMethod, TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.MessageQueueing)]
        public void ConstructionTest_Min()
        {
            var moq = new MockRepository(MockBehavior.Strict);

            //Mock
            var mockQueueResolver = moq.Create<IQueueResolver<QueueConfigTests>>();
            mockQueueResolver.Setup(m => m.GetConnectionString()).Returns((QueueConnectionString)"Region=us-east-2;AccessKeyId=FakeKeyId;SecretAccessKey=FakeAccessSecret");
            mockQueueResolver.Setup(m => m.GetConfigurationValue(nameof(QueueConfig<QueueConfigTests>.DelaySeconds))).Returns("-10000");
            mockQueueResolver.Setup(m => m.GetConfigurationValue(nameof(QueueConfig<QueueConfigTests>.MaxNumberOfMessages))).Returns("-10000");
            mockQueueResolver.Setup(m => m.GetConfigurationValue(nameof(QueueConfig<QueueConfigTests>.WaitTimeSeconds))).Returns("-10000");
            mockQueueResolver.Setup(m => m.GetConfigurationValue(nameof(QueueConfig<QueueConfigTests>.LeadOutSeconds))).Returns("-10000");

            //Setup
            var sp = new ServiceCollection()
                .AddTransient(sp => mockQueueResolver.Object)
                .BuildServiceProvider();

            //Test
            var config = ActivatorUtilities.CreateInstance<QueueConfig<QueueConfigTests>>(sp);

            //Assert
            Assert.AreEqual("US East (Ohio)", config.Region.DisplayName);
            Assert.AreEqual("FakeAccessSecret", config.SecretAccessKey);
            Assert.AreEqual("FakeKeyId", config.AccessKeyId);
            Assert.AreEqual(0, config.DelaySeconds);
            Assert.AreEqual(0, config.MaxNumberOfMessages);
            Assert.AreEqual(0, config.WaitTimeSeconds);
            Assert.AreEqual(5, config.LeadOutSeconds);

            //Verify
            moq.VerifyAll();
        }

        [TestMethod, TestCategory(TestCategories.Simulation)]
        [TestCategory(TestCategories.Feature.MessageQueueing)]
        public void ConfigKeys()
        {
            //Setup
            var sp = new ServiceCollection()
                .AddDebugTestConfigurations()
                .AddDebugTestServices(this.TestContext)
                .AddMessageQueueingServices()
                .AddToolkitServices()
                .BuildServiceProvider();

            //Test
            var resolver = ActivatorUtilities.CreateInstance<QueueResolver<TestTarget>>(sp);

            var suffixes = new[]
            {
                nameof(IQueueConfig<TestTarget>.DelaySeconds),
                nameof(IQueueConfig<TestTarget>.LeadOutSeconds),
                nameof(IQueueConfig<TestTarget>.MaxNumberOfMessages),
                nameof(IQueueConfig<TestTarget>.WaitTimeSeconds),

                QueueResolver<TestTarget>.ConnectionString,
                nameof(MessageQueueAttribute.QueueName),
            };

            foreach (var suffix in suffixes)
                foreach (var key in resolver.ConfigurationKeys(suffix))
                    this.TestContext?.WriteLine(key);
        }


        [TestMethod, TestCategory(TestCategories.Simulation)]
        [TestCategory(TestCategories.Feature.MessageQueueing)]
        public void RegionEndpoints()
        {
            foreach (var region in RegionEndpoint.EnumerableAllRegions)
                this.TestContext?.WriteLine($"|{region.DisplayName,-30} | {region.SystemName,-15} |");
        }
    }
}
