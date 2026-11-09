namespace OoBDev.System.IO.Tcp;

internal sealed record Rfc2217SignatureMessage(string Signature, bool IsFromServer) : IRfc2217Message;
