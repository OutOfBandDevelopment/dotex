using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Communications.Models;
using OoBDev.TestUtilities;

namespace OoBDev.Communications.Tests;

[TestClass]
public class EmailMessageModelExtensionsTests
{
    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void ToEmailMessage_MapsFieldsAndReplyTo()
    {
        var model = new EmailMessageModel
        {
            ReferenceId = "ref",
            FromAddress = "from@example.test",
            ToAddresses = ["a@example.test", "b@example.test"],
            BccAddresses = ["c@example.test"],
            Subject = "S",
            HtmlContent = "<p>x</p>",
            Headers = { ["Reply-To"] = "r@example.test" },
        };

        var email = model.ToEmailMessage("corr");

        Assert.AreEqual("ref", email.MessageId);
        Assert.AreEqual("corr", email.CorrelationId);
        Assert.AreEqual("from@example.test", email.From);
        CollectionAssert.AreEqual(new[] { "a@example.test", "b@example.test" }, email.To);
        CollectionAssert.AreEqual(new[] { "c@example.test" }, email.Bcc);
        Assert.AreEqual("r@example.test", email.ReplyTo);
        Assert.AreEqual("<p>x</p>", email.HtmlContent);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void ToEmailMessage_WithoutReference_UsesCorrelationThenGeneratesId()
    {
        Assert.AreEqual("corr", new EmailMessageModel().ToEmailMessage("corr").MessageId);
        Assert.IsFalse(string.IsNullOrEmpty(new EmailMessageModel().ToEmailMessage().MessageId));
        Assert.AreEqual(string.Empty, new EmailMessageModel().ToEmailMessage().Subject);
    }
}
