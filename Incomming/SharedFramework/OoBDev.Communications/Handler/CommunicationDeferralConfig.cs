using OoBDev.Communications.Contracts;
using OoBDev.Communications.Contracts.Handler;
using Microsoft.Extensions.Configuration;

namespace OoBDev.Communications.Handler
{
    public class CommunicationDeferralConfig : ICommunicationDeferralConfig
    {
        public int MaxCount { get; }

        public CommunicationDeferralConfig(
            IConfiguration config
            )
        {
            MaxCount = int.TryParse(config[CommunicationConfiguration.Deferral.MaxCount], out var mc) ?
                mc :
                CommunicationConfiguration.Deferral.MaxCountDefault;
        }
    }
}
