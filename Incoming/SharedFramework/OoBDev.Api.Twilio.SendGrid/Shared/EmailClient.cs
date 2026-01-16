using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading.Tasks;

namespace OoBDev.Api.Twilio.SendGrid.Shared
{
    [ExcludeFromCodeCoverage]
    public class EmailClient : IEmailClient
    {
        private readonly string _apiKey;

        public EmailClient(
            IConfiguration config
            )
        {
            _apiKey = config["Twilio:SendGrid:ApiKey"];
        }

        public async Task<Response> SendMessageAsync(SendGridMessage message)
        {
            var client = new SendGridClient(_apiKey);
            var response = await client.SendEmailAsync(message);

            if (response.StatusCode >= HttpStatusCode.BadRequest)
            {
                var bodyContent = await response.Body.ReadAsStringAsync();
                throw new ApplicationException(bodyContent);
            }

            return response;
        }
    }
}
