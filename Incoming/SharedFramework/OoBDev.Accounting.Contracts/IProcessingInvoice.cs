using System;
using System.Collections.Generic;

namespace OoBDev.Accounting.Contracts
{
    public interface IProcessingInvoice
    {
        string Description { get; }
        DateTimeOffset? InvoiceDate { get; }
        string InvoiceNumber { get; }
        IEnumerable<IProcessingLineItem> Lines { get; }
        string Payee { get; }
    }
}