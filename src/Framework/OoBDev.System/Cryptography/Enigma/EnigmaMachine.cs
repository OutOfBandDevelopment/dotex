using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.System.Cryptography.Enigma;

// https://en.wikipedia.org/wiki/Enigma_rotor_details
// http://enigmaco.de/enigma/enigma.html
public class EnigmaMachine
{
    private string[] _plugboard;
    private int[] _postions;
    private int[] _ringSettings;
    private readonly EnigmaRotor[] _rotors;
    private readonly EnigmaReflector _reflector;

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

    public string Rotors => string.Join(";", _rotors.Select(r => r.Number));
    public string Reflector => _reflector.Number;

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
