using OoBDev.DocumentCenter.Contracts.Storage;
using System;
using System.Linq;

namespace OoBDev.DocumentCenter.Resolvers
{
    public class BlobContainerResolver : IBlobContainerResolver
    {
        public string GetContainerName<T>()
        {
            var type = typeof(T);
            if (type.IsGenericType)
            {
                type = type.GetGenericTypeDefinition();
            }

            var name = (type.GetCustomAttributes(typeof(BlobContainerAttribute), false) ?? Array.Empty<object>())
                        .OfType<BlobContainerAttribute>()
                        .FirstOrDefault(a => !string.IsNullOrWhiteSpace(a.ContainerName))?.ContainerName;

            if (!string.IsNullOrWhiteSpace(name))
            {
                return name;
            }

            if (type?.FullName == null) throw new ArgumentNullException(nameof(type));

            name = type.FullName
                       .ToLower()
                       .Replace('.', '-')
                       .Replace('+', '-')
                       .Replace('`', '-');
            
            if (name.Length > 63)
            {
                return name.Substring(name.Length - 63, 63);
            }
            else
            {
                return name;
            }
        }
    }
}
