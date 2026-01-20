using System;
using System.Threading.Tasks;

namespace OoBDev.Communications.Abstractions.Handler
{
    public interface ICommunicationDeferralProvider
    {
        /// <summary>
        /// </summary>
        /// <param name="checkTime"></param>
        /// <returns>true if messages were processed.  false if not</returns>
        Task<bool> ExecuteAsync(DateTimeOffset checkTime);
    }
}
