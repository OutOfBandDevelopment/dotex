using System.Collections.Generic;
using System.Xml.Linq;

namespace OoBDev.ToolKit.Xml.Linq;

public static class XFragmentEx
{
    public static XFragment ToXFragment(this IEnumerable<XNode> nodes) => new(nodes);
}
