using System;

namespace OoBDev.IdentityModel.Contracts
{
    public static class ConfigKeys
    {
        public static class Azure
        {
            public static class ADB2C
            {
                public const string ClientID = "Azure:ADB2C:ClientId";
                public const string Tenant = "Azure:ADB2C:Tenant";
                public const string Policy = "Azure:ADB2C:Policy";
                public const string Issuer = "Azure:ADB2C:Issuer";
                public const string RedirectUri = "Azure:ADB2C:RedirectUri";
                public const string ClientSecret = "Azure:ADB2C:ClientSecret";

                public static class Invite
                {
                    public const string RequestWindow = "Azure:ADB2C:Invite:RequestWindow";
                    public const string SignUpUrl = "Azure:ADB2C:Invite:SignUpUrl";
                    public const string DefaultSignUpUrl = "https://{0}.b2clogin.com/{0}.onmicrosoft.com/{1}/oauth2/v2.0/authorize?client_id={2}&nonce={4}&redirect_uri={3}&scope=openid&response_type=id_token";
                    public static readonly TimeSpan DefaultRequestWindow = new TimeSpan(7, 0, 0, 0);
                }

                public static class Oidc
                {
                    public const string Authority = "Azure:ADB2C:Oidc:Authority";

                    public static class SigningCert
                    {
                        public const string Thumbprint = "Azure:ADB2C:Oidc:SigningCert:Thumbprint";
                    }
                }
            }
        }
    }
}
