namespace OoBDev.Accounting.Contracts
{
    public interface IAccountDetail
    {
        string Name { get; }
        string ShortName { get; }
        string CompanyName { get; }
        string Description { get; }
        string AccountNumber { get; }
    }
}
