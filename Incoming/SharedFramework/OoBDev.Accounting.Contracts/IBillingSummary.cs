using System;

namespace OoBDev.Accounting.Contracts
{
    public interface IBillingSummary
    {
        decimal? Amount { get; }
        DateTimeOffset? Created { get; }
        string Description { get; }
        DateTimeOffset? DueDate { get; }
        string ExternalId { get; }
        string ExternalPayeeId { get; }
        DateTimeOffset? InvoiceDate { get; }
        string InvoiceNumber { get; }
        bool IsActive { get; }
        DateTimeOffset? LastUpdated { get; }
        DateTimeOffset? PostingDate { get; }
        InvoiceStatuses Status { get; }
        string StatusReason { get; }
    }
}