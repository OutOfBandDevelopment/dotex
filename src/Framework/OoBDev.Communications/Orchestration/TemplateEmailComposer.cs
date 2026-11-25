using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OoBDev.Communications.Abstractions;
using OoBDev.System.Text.Templating;

namespace OoBDev.Communications.Orchestration;

/// <summary>Builds an <see cref="EmailMessage"/> from the templates <c>{MessageType}.email.subject</c>, <c>{MessageType}.email.html</c> and <c>{MessageType}.email.text</c>.</summary>
/// <remarks>The recipient comes from the request data key <see cref="TemplateComposerOptions.EmailAddressKey"/>. No address, or no body template, gives <c>null</c>.</remarks>
public sealed class TemplateEmailComposer(ITemplateEngine engine, IOptions<TemplateComposerOptions> options) : IMessageComposer
{
    /// <inheritdoc />
    public string ChannelName => options.Value.EmailChannelName;

    /// <inheritdoc />
    public async Task<IMessage?> ComposeAsync(CommunicationRequest request, TargetPreference preference, CancellationToken cancellationToken = default)
    {
        var address = TemplateRenderer.Address(request, options.Value.EmailAddressKey);
        if (address is null)
        {
            return null;
        }

        var prefix = $"{request.MessageType}.email";
        var html = await TemplateRenderer.RenderAsync(engine, $"{prefix}.html", request, preference).ConfigureAwait(false);
        var text = await TemplateRenderer.RenderAsync(engine, $"{prefix}.text", request, preference).ConfigureAwait(false);
        if (html is null && text is null)
        {
            return null;
        }

        var subject = await TemplateRenderer.RenderAsync(engine, $"{prefix}.subject", request, preference).ConfigureAwait(false);
        return new EmailMessage
        {
            To = [address],
            Subject = subject?.Trim() ?? string.Empty,
            HtmlContent = html,
            TextContent = text,
            CorrelationId = request.CorrelationId,
        };
    }
}
