using OoBDev.Communications.Contracts.Channels;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace OoBDev.Twilio.SendGrid.Shared
{
    public class EmailMessageMapper : IEmailMessageMapper
    {
        private readonly IMessageBuilder _builder;

        public EmailMessageMapper(
            IMessageBuilder builder
            )
        {
            _builder = builder;
        }

        public Task<SendGridMessage> GetMessageAsync(IEmailMessage message)
        {
            var email = new SendGridMessage();

            _builder.AddFromAddress(message, email);
            _builder.AddToAddresses(message, email);
            _builder.AddCcAddresses(message, email);
            _builder.AddBccAddresses(message, email);
            _builder.AddSubject(message, email);
            _builder.AddBody(message, email);

            return Task.FromResult(email);
        }
    }
}
