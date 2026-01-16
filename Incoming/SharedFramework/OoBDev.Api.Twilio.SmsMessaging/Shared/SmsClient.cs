using OoBDev.Communications.Contracts.Channels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace OoBDev.Api.Twilio.SmsMessaging.Shared
{
    public class SmsClient : ISmsClient
    {
        private readonly IConfiguration _config;
        private readonly ILogger<SmsClient> _log;

        public SmsClient(
            IConfiguration config,
            ILogger<SmsClient> log
            )
        {
            _config = config;
            _log = log;

            Init();
        }

        //https://www.twilio.com/docs/libraries/csharp-dotnet/custom-http-clients-dot-net-core
        private static bool _setup = false;
        private static readonly object _setupSync = new object();
        private void Init()
        {
            lock (_setupSync)
                if (!_setup)
                {
                    _setup = true;
                    TwilioClient.Init(
                        _config["Twilio:SmsMessaging:AccountSid"],
                        _config["Twilio:SmsMessaging:AuthToken"]
                        );
                }
        }

        public async Task<MessageResource> SendMessageAsync(ISmsMessage message)
        {
            _log.LogInformation($@"Send SMS to: {message.To} > ""{message.Body}""");

            var resource = await MessageResource.CreateAsync(
                to: new PhoneNumber(message.To),
                from: new PhoneNumber(message.From ?? _config["Twilio:SmsMessaging:Default:From"]),
                body: message.Body
                );

            return resource;
        }
    }
}
