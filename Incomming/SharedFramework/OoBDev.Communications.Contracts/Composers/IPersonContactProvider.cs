using System;
using System.Threading.Tasks;

namespace OoBDev.Communications.Contracts.Composers
{
    public interface IPersonContactProvider
    {
        Task<string> GetEmailAsync(Guid personId);
        Task<string> GetSmsAsync(Guid personId);
    }
}