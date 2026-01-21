using System;
using System.Threading.Tasks;

namespace OoBDev.Communications.Abstractions.Composers
{
    public interface IPersonContactProvider
    {
        Task<string> GetEmailAsync(Guid personId);
        Task<string> GetSmsAsync(Guid personId);
    }
}