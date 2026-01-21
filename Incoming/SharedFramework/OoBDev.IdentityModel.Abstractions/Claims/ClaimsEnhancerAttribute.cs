using System;

namespace OoBDev.IdentityModel.Abstractions.Claims
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ClaimsEnhancerAttribute : Attribute
    {
        public int Priority { get; set; }
    }
}