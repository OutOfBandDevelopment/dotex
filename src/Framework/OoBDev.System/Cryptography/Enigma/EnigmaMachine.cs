using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.System.Cryptography.Enigma;

/// <summary>
/// Simulates an Enigma cipher machine used for encryption during World War II.
/// Supports configurable rotors, reflectors, ring settings, and plugboard connections.
/// See https://en.wikipedia.org/wiki/Enigma_rotor_details and http://enigmaco.de/enigma/enigma.html for more information.
/// </summary>
/// <remarks>
/// WARNING: This is a historical cipher implementation for educational purposes only. It provides no security and should never be used for protecting sensitive data.
/// </remarks>
public class EnigmaMachine
{
    private string[] _plugboard = [];
    private int[] _postions = [];
    private int[] _ringSettings = [];
    private readonly EnigmaRotor[] _rotors;
    private readonly EnigmaReflector _reflector;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnigmaMachine"/> class with the specified configuration.
    /// </summary>
    /// <param name="rotors">The array of rotors to use (must be 3-5 rotors, processed in reverse order).</param>
    /// <param name="reflector">The reflector to use (must not be null).</param>
    /// <param name="ringSettings">The ring settings as a string of letters (defaults to all 'A's if not specified).</param>
    /// <param name="plugBoard">The plugboard connections as pairs of letters (optional).</param>
    /// <exception cref="InvalidOperationException">Thrown when rotors is null, has fewer than 3 or more than 5 elements, or reflector is null.</exception>
    public EnigmaMachine(EnigmaRotor[] rotors,
                         EnigmaReflector reflector,
                         //string start = null,
                         string? ringSettings = default,
                         string? plugBoard = default)
    {
        if (rotors == null || rotors.Length < 3 || rotors.Length > 5)
            throw new InvalidOperationException("Invalid Rotor Set");
        this._rotors = [.. rotors.Reverse()];
        this._reflector = reflector ?? throw new InvalidOperationException("Invalid Reflector");
        //this.Positions = default;  //(start ?? new string('A', rotors.Length));
        RingSettings = ringSettings;
        PlugBoard = plugBoard;
    }

    /// <summary>
    /// Gets or sets the current rotor positions as a string of letters.
    /// Each letter represents the position of the corresponding rotor (e.g., "AAA" for all rotors at position A).
    /// </summary>
    public string Positions
    {
        get => (_postions?
                      .Reverse()
                      .Select(p => (char)(p + 'A'))
                      .AsString()
                      + new string('A', _rotors.Length)
                      )[.._rotors.Length]; set => _postions = [.. (value ?? new string('A', _rotors.Length)).Select(i => i - 'A')
                                                                   .Concat(new int[_rotors.Length])
                                                                   .Take(_rotors.Length)
                                                                   .Reverse()];
    }

    /// <summary>
    /// Gets the ring settings for the rotors as a string of letters.
    /// Ring settings offset the wiring of each rotor relative to its position.
    /// </summary>
    public string? RingSettings
    {
        get => (_ringSettings?
                      .Reverse()
                      .Select(p => (char)(p + 'A'))
                      .AsString()
                      + new string('A', _rotors.Length)
                      )[.._rotors.Length];
        private set => _ringSettings = [.. (value ?? new string('A', _rotors.Length)).Select(i => i - 'A')
                                                                       .Concat(new int[_rotors.Length])
                                                                       .Take(_rotors.Length)
                                                                       .Reverse()];
    }

    /// <summary>
    /// Gets or sets the plugboard connections as pairs of letters separated by spaces.
    /// Each pair swaps two letters before and after the rotor processing (e.g., "AB CD" swaps A↔B and C↔D).
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the plugboard configuration is invalid (duplicate letters or odd length).</exception>
    public string? PlugBoard
    {
        get => string.Join(" ", _plugboard ?? []);
        set
        {
            var cleaned = value?.Clean().AsString() ?? "";
            if (cleaned.Length % 2 == 0 && cleaned.GroupBy(c => c).Any(c => c.Count() != 1))
                throw new InvalidOperationException("Invalid Plug Board");

            _plugboard = cleaned?.SplitAt(2).ToArray() ?? [];
        }
    }

    /// <summary>
    /// Gets a semicolon-separated list of the rotor numbers in use.
    /// </summary>
    public string Rotors => string.Join(";", _rotors.Select(r => r.Number));

    /// <summary>
    /// Gets the reflector number in use.
    /// </summary>
    public string Reflector => _reflector.Number;

    /// <summary>
    /// Processes (encrypts or decrypts) the input text through the Enigma machine.
    /// The machine's rotors advance with each character, and the same operation both encrypts and decrypts.
    /// Non-alphabetic characters are removed from the input.
    /// </summary>
    /// <param name="input">The text to process (will be cleaned to uppercase letters only).</param>
    /// <returns>The processed ciphertext or plaintext.</returns>
    public string Process(string input)
    {
        input = input.Clean().AsString().SwapSet(_plugboard);
        var start = Positions;
        var set = _rotors;
        var rs = _ringSettings;
        var l = 26; // set[0].Length;

        var cOut = new List<char>();

        foreach (var c in input.Select(x => x - 'A'))
        {
            _postions[0] = (_postions[0] + 1) % l;
            if (_rotors[0].RotateOn.Contains((char)(_postions[0] + 'A')))
            {
                _postions[1] = (_postions[1] + 1) % l;

                if (_rotors[1].RotateOn.Contains((char)(_postions[1] + 'A')))
                {
                    _postions[2] = (_postions[2] + 1) % l;

                    if (_rotors.Length > 3 &&
                        _rotors[2].RotateOn.Contains((char)(_postions[2] + 'A')))
                    {
                        _postions[3] = (_postions[3] + 1) % l;
                    }
                }
            }

            var indexes = _postions;

            var m = c;
            for (var i = 0; i < set.Length; i++)
                m = (set[i].Wiring[(m + indexes[i] + rs[i]) % l] - indexes[i] - 'A' + l) % l;
            m = (_reflector.Wiring[m] - 'A' + l) % l;
            for (var i = set.Length - 1; i > -1; i--)
                m = (set[i].Wiring.IndexOf((char)((m + indexes[i]) % l + 'A')) - indexes[i] - rs[i] + l) % l;
            cOut.Add((char)(m + 'A'));
        }
        return cOut.AsString().SwapSet(_plugboard);
    }
}
