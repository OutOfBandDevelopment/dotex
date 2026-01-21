using System.Runtime.CompilerServices;

namespace OoBDev.DataLoader.Models
{
    public interface IEntityModel
    {
        object Entity { get; }
        ITuple Key { get; }
    }
}