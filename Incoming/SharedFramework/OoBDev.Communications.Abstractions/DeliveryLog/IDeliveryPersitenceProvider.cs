using System;
using System.Threading.Tasks;

namespace OoBDev.Communications.Abstractions.DeliveryLog
{
    public interface IDeliveryPersitenceProvider
    {
        Task<Guid> RequestedAsync(ICreateDeliveryRequest model);

        Task<bool> FailedAsync(Guid requestId, DateTimeOffset processed, Exception ex);
        Task<bool> SuccessAsync(Guid requestId, DateTimeOffset processed, object? response = null);
        Task ProcessIndexAsync(Guid notificationDeliveryId);
    }
}