# MailKit hosting registered in every build

**Date:** 2026-10-28
**Status:** Complete

`OoBDev.Common.Hosting` registered `TryAddMailKitHosting` in DEBUG builds only. It now registers it in every build; `HostingBuilder.DisableMailKit` still turns it off (the example Web API sets it). The leftover TODO in `RabbitMQQueueMessageSenderProviderTests` was removed; the class stays.

Verified: `OoBDev.Common.Hosting` builds with 0 errors; RabbitMQ Unit tests pass.
