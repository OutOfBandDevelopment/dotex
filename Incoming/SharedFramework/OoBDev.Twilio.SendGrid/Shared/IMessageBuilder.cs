using OoBDev.Communications.Contracts.Channels;
using SendGrid.Helpers.Mail;

namespace OoBDev.Twilio.SendGrid.Shared
{
    public interface IMessageBuilder
    {
        void AddFromAddress(IEmailMessage message, SendGridMessage email);
        void AddToAddresses(IEmailMessage message, SendGridMessage email);
        void AddCcAddresses(IEmailMessage message, SendGridMessage email);
        void AddBccAddresses(IEmailMessage message, SendGridMessage email);
        void AddSubject(IEmailMessage message, SendGridMessage email);
        void AddBody(IEmailMessage message, SendGridMessage email);
    }
}
