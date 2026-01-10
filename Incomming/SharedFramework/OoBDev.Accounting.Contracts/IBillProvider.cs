using OoBDev.Toolkit.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.Accounting.Contracts
{
    [ContractConfig]
    public interface IBillProvider
    {
        Task<string?> CreateAsync(IProcessingInvoice invoice);
        Task<IBillingSummary?> GetAsync(string invoiceNumber, string vendorName);
        Task<IBillingSummary?> GetBillByIdAsync(string billingId);
        Task<IEnumerable<IBillPaySummary>> GetPaymentsForBillAsync(string billingId);
    }
}
