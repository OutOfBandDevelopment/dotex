using System;

namespace OoBDev.Accounting.Contracts
{
    public interface IBillPaySummary
    {
        decimal Amount { get; }
        string BillId { get; }
        string ExternalId { get; }
        string Name { get; }
        DateTimeOffset ProcessDate { get; }
    }
}