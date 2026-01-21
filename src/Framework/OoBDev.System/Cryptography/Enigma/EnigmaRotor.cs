using System.Collections.Generic;

namespace OoBDev.System.Cryptography.Enigma;

/// <summary>
/// Represents an Enigma rotor configuration with wiring specifications and turnover notch positions.
/// Rotors are rotating cipher disks that scramble letters and advance to create a polyalphabetic substitution.
/// </summary>
public record EnigmaRotor
{
    /// <summary>
    /// Gets a collection of historical Enigma rotor configurations from various Enigma models (Commercial, Railway, Swiss K, Enigma I, M3, M4).
    /// </summary>
    public static IEnumerable<EnigmaRotor> Rotors { get; } =
    [
        new EnigmaRotor {Number="IC", Series="Commercial Enigma A, B",Wiring="DMTWSILRUYQNKFEJCAZBPGXOHV",Introduced="1924",RotateOn=['Q']},
        new EnigmaRotor {Number="IIC", Series="Commercial Enigma A, B",Wiring="HQZGPJTMOBLNCIFDYAWVEUSRKX",Introduced="1924",RotateOn=['E']},
        new EnigmaRotor {Number="IIIC", Series="Commercial Enigma A, B",Wiring="UQNTLSZFMREHDPXKIBVYGJCWOA",Introduced="1924",RotateOn=['V']},
        new EnigmaRotor {Number="I", Series="German Railway (Rocket)",Wiring="JGDQOXUSCAMIFRVTPNEWKBLZYH",Introduced="15014",RotateOn=['Q']},
        new EnigmaRotor {Number="II", Series="German Railway (Rocket)",Wiring="NTZPSFBOKMWRCJDIVLAEYUXHGQ",Introduced="15014",RotateOn=['E']},
        new EnigmaRotor {Number="III", Series="German Railway (Rocket)",Wiring="JVIUBHTCDYAKEQZPOSGXNRMWFL",Introduced="15014",RotateOn=['V']},
        // new EnigmaRotor {Number="UKW", Series="German Railway (Rocket)",Wiring="QYHOGNECVPUZTFDJAXWMKISRBL",Introduced="15014",RotateOn=new char[]{}},
        // new EnigmaRotor {Number="ETW", Series="German Railway (Rocket)",Wiring="QWERTZUIOASDFGHJKPYXCVBNML",Introduced="15014",RotateOn=new char[]{}},
        new EnigmaRotor {Number="I-K", Series="Swiss K",Wiring="PEZUOHXSCVFMTBGLRINQJWAYDK",Introduced="14277",RotateOn=['Q']},
        new EnigmaRotor {Number="II-K", Series="Swiss K",Wiring="ZOUESYDKFWPCIQXHMVBLGNJRAT",Introduced="14277",RotateOn=['E']},
        new EnigmaRotor {Number="III-K", Series="Swiss K",Wiring="EHRVXGAOBQUSIMZFLYNWKTPDJC",Introduced="14277",RotateOn=['V']},
        // new EnigmaRotor {Number="UKW-K", Series="Swiss K",Wiring="IMETCGFRAYSQBZXWLHKDVUPOJN",Introduced="14277",RotateOn=new char[]{}},
        // new EnigmaRotor {Number="ETW-K", Series="Swiss K",Wiring="QWERTZUIOASDFGHJKPYXCVBNML",Introduced="14277",RotateOn=new char[]{}},
        new EnigmaRotor {Number="I", Series="Enigma I",Wiring="EKMFLGDQVZNTOWYHXUSPAIBRCJ",Introduced="1930",RotateOn=['Q']},
        new EnigmaRotor {Number="II", Series="Enigma I",Wiring="AJDKSIRUXBLHWTMCQGZNPYFVOE",Introduced="1930",RotateOn=['E']},
        new EnigmaRotor {Number="III", Series="Enigma I",Wiring="BDFHJLCPRTXVZNYEIWGAKMUSQO",Introduced="1930",RotateOn=['V']},
        new EnigmaRotor {Number="IV", Series="M3 Army",Wiring="ESOVPZJAYQUIRHXLNFTGKDCMWB",Introduced="14215",RotateOn=['J']},
        new EnigmaRotor {Number="V", Series="M3 Army",Wiring="VZBRGITYUPSDNHLXAWMJQOFECK",Introduced="14215",RotateOn=['Z']},
        new EnigmaRotor {Number="VI", Series="M3 & M4 Naval (FEB 1942)",Wiring="JPGVOUMFYQBENHZRDKASXLICTW",Introduced="1939",RotateOn=['Z','M']},
        new EnigmaRotor {Number="VII", Series="M3 & M4 Naval (FEB 1942)",Wiring="NZJHGRCXMYSWBOUFAIVLPEKQDT",Introduced="1939",RotateOn=['Z','M']},
        new EnigmaRotor {Number="VIII", Series="M3 & M4 Naval (FEB 1942)",Wiring="FKQHTLXOCBJSPDZRAMEWNIUYGV",Introduced="1939",RotateOn=['Z','M']},
        // new EnigmaRotor {Number="Beta", Series="M4 R2",Wiring="LEYJVCNIXWPBQMDRTAKZGFUHOS",Introduced="Spring 1941",RotateOn=new char[]{}},
        // new EnigmaRotor {Number="Gamma", Series="M4 R2",Wiring="FSOKANUERHMBTIYCWLQPZXVGJD",Introduced="Spring 1942",RotateOn=new char[]{}},
        // new EnigmaRotor {Number="ETW", Series="Enigma I",Wiring="ABCDEFGHIJKLMNOPQRSTUVWXYZ",Introduced="",RotateOn=new char[]{}},
    ];

    /// <summary>
    /// Gets or initializes the year or date when this rotor was introduced.
    /// </summary>
    public required string Introduced { get; init; }

    /// <summary>
    /// Gets or initializes the rotor designation/number (e.g., "I", "II", "III", "IC").
    /// </summary>
    public required string Number { get; init; }

    /// <summary>
    /// Gets or initializes the turnover notch positions (letters at which this rotor causes the next rotor to advance).
    /// Most rotors have one notch, but some naval rotors (VI, VII, VIII) have two.
    /// </summary>
    public required char[] RotateOn { get; init; }

    /// <summary>
    /// Gets or initializes the Enigma series this rotor was used with (e.g., "Enigma I", "M3 Army", "M4 Naval").
    /// </summary>
    public required string Series { get; init; }

    /// <summary>
    /// Gets or initializes the wiring specification as a 26-letter string defining the rotor's electrical connections.
    /// </summary>
    public required string Wiring { get; init; }
}
