namespace OoBDev.System.IO.Devices;

/// <summary>One validation complaint about a settings value, keyed by the form field id it concerns.</summary>
public sealed record DeviceConfigIssue(string FieldId, string Message);
