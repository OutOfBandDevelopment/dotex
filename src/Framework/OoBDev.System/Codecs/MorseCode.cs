using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.System.Codecs;

/// <summary>
/// Provides encoding and decoding functionality for Morse code.
/// </summary>
public class MorseCode
{
    /// <summary>
    /// Encodes text into Morse code representation.
    /// </summary>
    /// <param name="input">The text to encode.</param>
    /// <returns>The Morse code representation with dots and dashes.</returns>
    public string Encode(string input) => string.Join(" ", input.Select(Map).Where(c => c != "")).Replace("   ", "  ");

    /// <summary>
    /// Decodes Morse code into text.
    /// </summary>
    /// <param name="input">The Morse code to decode.</param>
    /// <returns>The decoded text.</returns>
    public string Decode(string input) => new([.. input.Split(' ').Select(Map)]);

    /// <summary>
    /// Maps a character to its Morse code representation.
    /// </summary>
    /// <param name="input">The character to map.</param>
    /// <returns>The Morse code representation of the character.</returns>
    public string Map(char input) =>
        (char)(input > '_' ? input & 0b01011111 : input) switch
        {
            char chr when _mapping.ContainsKey(chr) => _mapping[chr],
            '\n' => Environment.NewLine,
            ' ' => " ",
            _ => "",
        };

    /// <summary>
    /// Maps a Morse code pattern to its corresponding character.
    /// </summary>
    /// <param name="input">The Morse code pattern to map.</param>
    /// <returns>The character represented by the Morse code.</returns>
    public char Map(string input) =>
        _mapping.Where(v => v.Value == input).Select(k => k.Key).FirstOrDefault(' ');

    private readonly IReadOnlyDictionary<char, string> _mapping = new Dictionary<char, string>
    {
          { 'A', ".-"    },
          { 'B', "-..."  },
          { 'C', "-.-."  },
          { 'D', "-.."   },
          { 'E', "."     },
          { 'F', "..-."  },
          { 'G', "--."   },
          { 'H', "...."  },
          { 'I', ".."    },
          { 'J', ".---"  },
          { 'K', "-.-"   },
          { 'L', ".-.."  },
          { 'M', "--"    },
          { 'N', "-."    },
          { 'O', "---"   },
          { 'P', ".--."  },
          { 'Q', "--.-"  },
          { 'R', ".-."   },
          { 'S', "..."   },
          { 'T', "-"     },
          { 'U', "..-"   },
          { 'V', "...-"  },
          { 'W', ".--"   },
          { 'X', "-..-"  },
          { 'Y', "-.--"  },
          { 'Z', "--.."  },
          { '1', ".----" },
          { '2', "..---" },
          { '3', "...--" },
          { '4', "....-" },
          { '5', "....." },
          { '6', "-...." },
          { '7', "--..." },
          { '8', "---.." },
          { '9', "----." },
          { '0', "-----" },
    };
}
