using OoBDev.IdentityModel.Contracts.Claims;
using OoBDev.Toolkit.Globalization;
using System.Globalization;
using System.Security.Claims;

namespace OoBDev.IdentityModel.Extensions.Globalization
{
    public class ClaimsCultureInfoAccessor : CultureInfoAccessor
    {
        public const string CultureClaimKey = "lw_co__user_culture";

        private readonly ClaimsPrincipal _principal;

        public ClaimsCultureInfoAccessor(
            ClaimsPrincipal principal
            )
        {
            _principal = principal;
        }

        protected override CultureInfo? GetCultureInfo()
        {
            var fromClaims = _principal.GetClaimValue(CultureClaimKey);
            if (fromClaims == null)
                return default;

            var culture = CultureInfo.GetCultureInfo(fromClaims);
            return culture;
        }
    }
}
