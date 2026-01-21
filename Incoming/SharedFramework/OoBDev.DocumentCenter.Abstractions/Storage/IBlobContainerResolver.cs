using System;
using System.Collections.Generic;
using System.Text;

namespace OoBDev.DocumentCenter.Abstractions.Storage
{
    public interface IBlobContainerResolver
    {
        string GetContainerName<T>();
    }
}
