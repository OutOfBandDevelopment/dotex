using System.Collections.Generic;
using System.Xml.Linq;

namespace OoBDev.System.Xml.Linq;

public static class XFragmentEx
{
    public static XFragment ToXFragment(this IEnumerable<XNode> nodes) => new(nodes);
}
