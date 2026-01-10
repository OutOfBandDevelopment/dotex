namespace OoBDev.Accounting.Contracts
{
    public interface IProcessingLineItem
    {
        string AccountingCode { get; }
        decimal Amount { get; }
        string BillingCode { get; }
        string Description { get; }
        string SummaryDescription { get; }
    }
}