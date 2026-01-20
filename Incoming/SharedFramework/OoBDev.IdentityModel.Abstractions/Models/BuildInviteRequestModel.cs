using System;

namespace OoBDev.IdentityModel.Abstractions.Models
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class BuildInviteRequestModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public Guid UserHistoryId { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
