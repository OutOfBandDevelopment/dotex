namespace OoBDev.AspNetCore.Extensions;

public interface IEnvironmentSettings
{
    bool EnableSwagger { get; }
    bool EnableHttpsRedirection { get; }
}

