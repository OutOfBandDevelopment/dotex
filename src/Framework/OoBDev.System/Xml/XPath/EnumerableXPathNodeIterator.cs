using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;

namespace OoBDev.System.Xml.XPath;

/// <summary>
/// Provides an XPathNodeIterator implementation for enumerating over a set of IXPathNavigable items.
/// </summary>
/// <param name="set">The collection of navigable items to iterate over.</param>
public class EnumerableXPathNodeIterator(IEnumerable<IXPathNavigable> set) : XPathNodeIterator
{
    private int _pointer = -1;
    private readonly IEnumerable<IXPathNavigable> _set = set.ToArray();
    private readonly IEnumerator<IXPathNavigable> _enumerator = set.GetEnumerator();

    /// <summary>
    /// Gets the current node in the iteration as an XPathNavigator.
    /// </summary>
    public override XPathNavigator? Current => _enumerator.Current.CreateNavigator();

    /// <summary>
    /// Gets the index of the current position in the iteration.
    /// </summary>
    public override int CurrentPosition => _pointer;

    /// <summary>
    /// Creates a copy of this iterator.
    /// </summary>
    /// <returns>A new iterator positioned at the same location.</returns>
    public override XPathNodeIterator Clone()
    {
        var newIterator = new EnumerableXPathNodeIterator(_set);
        while (newIterator.CurrentPosition < _pointer && newIterator.MoveNext()) ;
        return newIterator;
    }

    /// <summary>
    /// Moves to the next node in the iteration.
    /// </summary>
    /// <returns>True if the iterator moved to the next node, false if there are no more nodes.</returns>
    public override bool MoveNext()
    {
        if (_enumerator.MoveNext())
        {
            _pointer++;
            return true;
        }
        return false;
    }
}
