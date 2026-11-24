using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OoBDev.AspNetCore.AuthGateway;

public sealed class AuthGatewayTarget
{
    [MinLength(1)]
    public IList<InboundScheme> Inbound { get; set; } = [];

    [Required]
    public OutboundOptions Outbound { get; set; } = new();
}
