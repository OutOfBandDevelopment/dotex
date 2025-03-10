﻿using System.Linq;

namespace OoBDev.System.Cryptography;

public class Vigenere : Caesar
{

    /// <summary>
    /// https://en.wikipedia.org/wiki/Vigen%C3%A8re_cipher
    /// </summary>
    /// <param name="input"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    public string Encode(string input, string code) =>
        (input, BuildKey(input.Length, code)) switch
        {
            (null, _) => "",
            (string, string key) => new string([.. input.Zip(key).Select(item => Encode(item.First, item.Second))])
        };
    public string Decode(string input, string code) =>
        (input, BuildKey(input.Length, code)) switch
        {
            (null, _) => "",
            (string, string key) => new string([.. input.Zip(key).Select(item => Decode(item.First, item.Second))])
        };

    public string BuildKey(int length, string? code)
    {
        code = new string([.. (code ?? string.Empty).Where(char.IsLetter)]);
        return string.IsNullOrWhiteSpace(code)
            ? new string([.. Enumerable.Range(0, length).Select(i => (char)('A' + i % 26))])
            : string.Join("", Enumerable.Range(0, length / code.Length + 1).Select(_ => code))[..length];
    }
}
