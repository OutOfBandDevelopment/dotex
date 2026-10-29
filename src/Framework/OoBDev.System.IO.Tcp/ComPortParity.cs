namespace OoBDev.System.IO.Tcp;

/// <summary>The parity values of the RFC 2217 SET-PARITY command.</summary>
public enum ComPortParity : byte
{
    /// <summary>No parity.</summary>
    None = 1,

    /// <summary>Odd parity.</summary>
    Odd = 2,

    /// <summary>Even parity.</summary>
    Even = 3,

    /// <summary>Mark parity.</summary>
    Mark = 4,

    /// <summary>Space parity.</summary>
    Space = 5,
}
