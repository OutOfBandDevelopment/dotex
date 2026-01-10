using OoBDev.TestUtilities;
using OoBDev.TestUtilities.Logging;
using OoBDev.TextTemplating.Templating;
using OoBDev.Toolkit.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.TextTemplating.Tests.Templating
{
    [TestClass]
    public class GenerateTextTests
    {
        public TestContext TestContext { get; set; }

        private MockRepository mockRepository;

        private Mock<IObjectSerializer> mockObjectSerializer;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockObjectSerializer = this.mockRepository.Create<IObjectSerializer>();
        }

        private GenerateText CreateGenerateText() =>
            new GenerateText(
                this.mockObjectSerializer.Object,
                this.TestContext.GetTestLoggingServices<GenerateText>()
                );

        [DataTestMethod]
        [TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.TextGeneration)]
        [DataRow("NULL")]
        [DataRow("Combination")]
        [DataRow("Condition")]
        [DataRow("DataBinding")]
        [DataRow("Repeater_Filter")]
        [DataRow("Repeater_Html")]
        [DataRow("Repeater_Html_WithScope")]
        [DataRow("Repeater_Text")]
        [DataRow("Repeater_Text_WithAt")]
        [DataRow("ValueAttribute")]
        [DataRow("ValueAttribute_WithFormat")]
        [DataRow("ValueAttribute_WithRule_True")]
        [DataRow("ValueAttribute_WithRule_False")]
        [DataRow("ValueAttribute_WithValue")]
        [DataRow("ValueOf")]
        [DataRow("ValueOf_WithFormat")]
        [DataRow("ValueOf_WithFormatNumber")]
        [DataRow("TraceTest")]
        public async Task GenerateAsyncTest(string templateName)
        {
            // Stage
            var templateBody = await this.TestContext.GetTestDataAsync<string>($"{templateName}.html");
            var expectedBody = await this.TestContext.GetTestDataAsync<string>($"{templateName}.txt");
            var model =
                 await this.TestContext.GetTestDataAsync<JObject>($"{templateName}.json") ??
                 await this.TestContext.GetTestDataAsync<JObject>("ExampleData.json");

            // Mock
            if (templateName != "NULL")
                mockObjectSerializer.Setup(s => s.GetAsSerialized(model)).Returns(model.ToString());


            // Test
            var generateText = this.CreateGenerateText();

            var result = await generateText.GenerateAsync(templateBody, model);

            this.TestContext.AddResult(expectedBody);
            this.TestContext.AddResult(result);

            // Assert
            if (!(templateBody == null && expectedBody == null))
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(result));
            }

            if (expectedBody != null)
            {

                //TODO: fix this up once timezone support is added
                CollectionAssert.AreEquivalent(
                    expectedBody.Split(Environment.NewLine.ToCharArray()).Where(l => !l.StartsWith("SentAt:")).ToArray(),
                    result.Split(Environment.NewLine.ToCharArray()).Where(l => !l.StartsWith("SentAt:")).ToArray()
                    );
            }

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.TextGeneration)]
        public async Task GenerateAsyncTest_String()
        {
            // Stage
            var templateBody = "<value-of binding='$.hello' />";
            var expectedBody = "world";
            var model = @"{""hello"":""world""}";

            // Mock
            mockObjectSerializer.Setup(s => s.GetAsSerialized(model)).Returns(model);

            // Test
            var generateText = this.CreateGenerateText();

            var result = await generateText.GenerateAsync(templateBody, model);

            // Assert
            if (!(templateBody == null && expectedBody == null))
                Assert.IsFalse(string.IsNullOrWhiteSpace(result));
            Assert.AreEqual(expectedBody, result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.TextGeneration)]
        public async Task GenerateAsyncTest_Object()
        {
            // Stage
            var templateBody = "<value-of binding='$.hello' />";
            var expectedBody = "world";
            var model = new { };
            var modelContent = @"{""hello"":""world""}";

            // Mock
            mockObjectSerializer.Setup(m => m.GetAsSerialized(model)).Returns(modelContent);

            // Test
            var generateText = this.CreateGenerateText();

            var result = await generateText.GenerateAsync<object>(templateBody, model);

            // Assert
            if (!(templateBody == null && expectedBody == null))
                Assert.IsFalse(string.IsNullOrWhiteSpace(result));
            Assert.AreEqual(expectedBody, result);

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
