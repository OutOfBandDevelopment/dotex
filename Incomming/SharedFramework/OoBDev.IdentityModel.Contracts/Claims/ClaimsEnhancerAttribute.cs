using System;

namespace OoBDev.IdentityModel.Contracts.Claims
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ClaimsEnhancerAttribute : Attribute
    {
        public int Priority { get; set; }
    }
}