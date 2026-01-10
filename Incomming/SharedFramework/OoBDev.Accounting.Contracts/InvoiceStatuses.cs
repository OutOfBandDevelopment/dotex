
namespace OoBDev.Accounting.Contracts
{
    public enum InvoiceStatuses
    {
        ApiError = -1,
        Unknown = 0,
        InProgress = 1,
        Submitted = 2,
        Rejected = 3,
        Paid = 5,
        ProcessFailure = 6,
    }
}
