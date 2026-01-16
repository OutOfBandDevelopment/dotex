namespace OoBDev.DocumentCenter.Contracts.Storage
{
    public interface IDocumentRequestReference
    {
        /// <summary>
        /// This property is required to access the provided document
        /// </summary>
        string Key { get; }
        /// <summary>
        /// if this property is not provided the default container will be assumed.
        /// </summary>
        string? Container { get; }
        /// <summary>
        /// if this value is not provided the current filename from the document store will be used.
        /// </summary>
        string? FileName { get; }
    }
}
