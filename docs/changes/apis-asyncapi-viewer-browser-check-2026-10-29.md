# AsyncAPI viewer browser check

**Date:** 2026-10-29

Checked the `/asyncapi/all` viewer of the running `OoBDev.Example.WebApi` in headless Chromium (Playwright). The page loads the pinned unpkg web component and renders the title, the RabbitMQ server, the RECEIVE operations and the SEND operation for `ReceivedEmailMessageModel`, with no console or page errors. The component renders inside a shadow root, so a text check must read `asyncapi-component.shadowRoot`.

No code changed. The viewer still needs network access to unpkg unless `AsyncApi:ViewerScriptUrl` points at a local copy.
