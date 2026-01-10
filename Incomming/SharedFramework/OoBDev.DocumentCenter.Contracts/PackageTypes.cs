namespace OoBDev.DocumentCenter.Contracts
{
    public enum PackageTypes
    {
        [DocumentType(DocumentTypes.Unknown)]
        Unknown = 0,
        [DocumentType(DocumentTypes.Zip)]
        ZipFile = 1,
    }
}
