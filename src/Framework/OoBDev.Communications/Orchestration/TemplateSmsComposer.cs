using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OoBDev.Communications.Abstractions;
using OoBDev.System.Text.Templating;

namespace OoBDev.Communications.Orchestration;

/// <summary>Builds an <see cref="SmsMessage"/> from the template <c>{MessageType}.sms</c>.</summary>
/// <remarks>The recipient comes from the request data key <see cref="TemplateComposerOptions.PhoneNumberKey"/>. No number or no template gives <c>null</c>.</remarks>
public sealed class TemplateSmsComposer(ITemplateEngine engine, IOptions<TemplateComposerOptions> options) : IMessageComposer
{
    /// <inheritdoc />
    public string ChannelName => options.Value.SmsChannelName;

    /// <inheritdoc />
    public async Task<IMessage?> ComposeAsync(CommunicationRequest request, TargetPreference preference, CancellationToken cancellationToken = default)
    {
        var number = TemplateRenderer.Address(request, options.Value.PhoneNumberKey);
        if (number is null)
        {
            return null;
        }

        var body = await TemplateRenderer.RenderAsync(engine, $"{request.MessageType}.sms", request, preference).ConfigureAwait(false);
        return string.IsNullOrWhiteSpace(body)
            ? null
            : new SmsMessage { To = number, Body = body.Trim(), CorrelationId = request.CorrelationId };
    }
}
