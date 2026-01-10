using OoBDev.Communications.Contracts;
using OoBDev.Communications.Contracts.Channels;
using OoBDev.Communications.Contracts.Composers;
using OoBDev.Communications.Contracts.DeliveryLog;
using OoBDev.Communications.Contracts.Models;
using OoBDev.Toolkit.Contracts.Common;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using static OoBDev.Communications.Contracts.DeliveryChannels;

namespace OoBDev.Communications.Composers
{
    [Composer(DeliveryChannel = Email)]
    public class EmailMessageComposer : IMessageComposer
    {
        private readonly ITemplateProvider _template;
        private readonly IPersonContactProvider _person;
        private readonly ISendEmailProvider _email;
        private readonly IDeliveryPersitenceProvider _delivery;
        private readonly ILogger<EmailMessageComposer> _log;
        private readonly IDateTools _date;
        private readonly IEmailMessageComposerConfig _config;

        public EmailMessageComposer(
            ITemplateProvider template,
            IPersonContactProvider person,
            ISendEmailProvider email,
            IDeliveryPersitenceProvider delivery,
            ILogger<EmailMessageComposer> log,
            IDateTools date,
            IEmailMessageComposerConfig config
            )
        {
            _template = template;
            _person = person;
            _email = email;
            _delivery = delivery;
            _log = log;
            _date = date;
            _config = config;
        }

        public async Task ComposeAndSendAsync(Guid targetPersonId, string messageType, CultureInfo? culture, JObject data, Guid requestGroupId, IDictionary<string, object> headers)
        {
            _log.LogDebug($@"Target: {targetPersonId} for ""{messageType}""/{culture} ({requestGroupId})");
            var model = new EmailMessageModel
            {
                MessageType = messageType,
                Subject = await _template.GetTemplateAsync(messageType, Email, "Subject", culture, data).ConfigureAwait(false),
                TextContent = await _template.GetTemplateAsync(messageType, Email, "Body", culture, data).ConfigureAwait(false),
                HtmlContent = await _template.GetTemplateAsync(messageType, Email, "Html", culture, data).ConfigureAwait(false),
                FromAddress = await _template.GetTemplateAsync(messageType, Email, "From", culture, data).ConfigureAwait(false),

                ToAddresses =
                {
                    await _person.GetEmailAsync(personId: targetPersonId).ConfigureAwait(false),
                },

                Headers = headers ?? new Dictionary<string, object>(),
            };

            if (_config.EnableTracing)
            {
                if (!string.IsNullOrWhiteSpace(model.TextContent))
                    model.TextContent = string.Join(Environment.NewLine, model.TextContent, await _config.TracingTemplateAsync(data).ConfigureAwait(false));

                if (!string.IsNullOrWhiteSpace(model.HtmlContent))
                    model.HtmlContent = string.Join(Environment.NewLine, model.HtmlContent, "<pre>", await _config.TracingTemplateAsync(data).ConfigureAwait(false), "</pre>");
            }

            model.RequestId = await _delivery.RequestedAsync(new CreateDeliveryRequestModel
            {
                PersonId = targetPersonId,
                Requested = _date.Now(),
                RequestGroupId = requestGroupId,

                DeliveryChannel = DeliveryChannelType.Email,
                MessageType = messageType,

                SentTo = string.Join(';', model.ToAddresses),
                Subject = model.Subject,
                Text = model.TextContent,
                Html = model.HtmlContent,

                EnhancedData = data.ToString(),
            });

            _log.LogInformation($@"Target: {targetPersonId} for ""{messageType}""/{culture} ({requestGroupId}) as {model.RequestId}");
            try
            {
                await _email.ScheduleSendMessageAsync(model).ConfigureAwait(false);
                _log.LogDebug($@"Target: {targetPersonId} for ""{messageType}""/{culture} ({requestGroupId}) as {model.RequestId} Scheduled");
            }
            catch (Exception ex)
            {
                _log.LogError($@"Target: {targetPersonId} for ""{messageType}""/{culture} ({requestGroupId}) as {model.RequestId} !! {ex.Message}");
                _log.LogDebug($@"Target: {targetPersonId} for ""{messageType}""/{culture} ({requestGroupId}) as {model.RequestId} !! {ex}");
                if (!await _delivery.FailedAsync(model.RequestId, _date.Now(), ex))
                    throw;
            }
        }
    }
}