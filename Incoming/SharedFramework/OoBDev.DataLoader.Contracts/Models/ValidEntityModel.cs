using System.Runtime.CompilerServices;

namespace OoBDev.DataLoader.Models
{
    public class ValidEntityModel : IEntityModel
    {
        public ITuple Key { get; internal set; }
        public object[] KeyData { get; internal set; }
        public object Entity { get; internal set; }
    }
}