using OoBDev.Communications.Contracts.Channels;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Linq;

namespace OoBDev.Twilio.SendGrid.Shared
{
    public class MessageBuilder : IMessageBuilder
    {
        private readonly IConfiguration _config;

        public MessageBuilder(
            IConfiguration config
            )
        {
            _config = config;
        }

        public void AddFromAddress(IEmailMessage message, SendGridMessage email)
        {
            var possibleValues = new[] {
                message.FromAddress,
                _config["Twilio:SendGrid:Default:From:Email"],
            };
            var value = possibleValues.FirstOrDefault(i => !string.IsNullOrWhiteSpace(i));
            email.SetFrom(value);
        }

        public void AddToAddresses(IEmailMessage message, SendGridMessage email)
        {
            foreach (var address in message.ToAddresses ?? Enumerable.Empty<string>())
                email.AddTo(address);
        }

        public void AddCcAddresses(IEmailMessage message, SendGridMessage email)
        {
            foreach (var address in message.CcAddresses ?? Enumerable.Empty<string>())
                email.AddCc(address);
        }

        public void AddBccAddresses(IEmailMessage message, SendGridMessage email)
        {
            foreach (var address in message.BccAddresses ?? Enumerable.Empty<string>())
                email.AddBcc(address);
        }

        public void AddSubject(IEmailMessage message, SendGridMessage email)
        {
            var possibleValues = new[] {
                message.Subject,
                _config["Twilio:SendGrid:Default:Subject"],
                "Message for you",
            };
            var value = possibleValues.FirstOrDefault(i => !string.IsNullOrWhiteSpace(i));
            email.Subject = value;
        }

        public void AddBody(IEmailMessage message, SendGridMessage email)
        {
            if (string.IsNullOrWhiteSpace(message.TextContent) && string.IsNullOrWhiteSpace(message.HtmlContent))
                throw new NotSupportedException("You must provide Text or Html Body");

            if (!string.IsNullOrWhiteSpace(message.TextContent))
                email.AddContent(MimeType.Text, message.TextContent);
            if (!string.IsNullOrWhiteSpace(message.HtmlContent))
                email.AddContent(MimeType.Html, message.HtmlContent);
        }
    }
}
