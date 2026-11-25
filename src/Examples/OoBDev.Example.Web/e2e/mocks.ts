import type { Page, Route } from '@playwright/test';

const json = (route: Route, body: unknown, status = 200) =>
  route.fulfill({ status, contentType: 'application/json', body: JSON.stringify(body) });

const text = (route: Route, body: string) => route.fulfill({ status: 200, contentType: 'text/plain', body });

const vector = (seed: number) => Array.from({ length: 384 }, (_, i) => Math.sin(seed + i * 0.37) / 4);

/**
 * Stand-in for OoBDev.Example.WebApi so journeys are repeatable and the screenshots in the guide do not change
 * between runs. Real behaviour is covered by running the journeys against the compose stack (E2E_BASE_URL).
 */
export async function mockApi(page: Page): Promise<void> {
  let turn = 0;
  const scheduled: { key: string; dueAt: string; message: string; firedAt: string | null }[] = [];
  await page.route(/\/(api|Document|openapi|asyncapi|cgi-bin|webhooks)\//, async route => {
    const url = new URL(route.request().url());
    const path = url.pathname;
    const method = route.request().method();

    if (path === '/api/v1/analytics/screen-views') return route.fulfill({ status: 204 });
    if (path === '/api/Ollama' && method === 'POST') return text(route, 'An embedding turns text into a list of numbers so similar meanings end up close together.');
    if (path === '/api/Ollama/Streamed') return json(route, ['An embedding ', 'turns text into ', 'numbers.']);
    if (path === '/api/Ollama/Completion') {
      turn++;
      return json(route, { context: [1, 2, 3, turn], response: turn === 1 ? 'Hello! How can I help you today?' : 'Photosynthesis is how plants turn light into sugar.' });
    }
    if (path === '/api/Operations/health') return json(route, { status: 'Healthy', entries: [{ name: 'apache-tika', status: 'Healthy', description: null, durationMs: 4.2 }, { name: 'ollama', status: 'Healthy', description: null, durationMs: 6.8 }, { name: 'sbert', status: 'Degraded', description: 'model is loading', durationMs: 12.5 }] });
    if (path === '/api/Operations/info') return json(route, { environment: 'Development', application: 'OoBDev.Example.WebApi', runtime: '.NET 10.0.0', machine: 'example-api', started: '2026-10-19T08:00:00+00:00', configuration: [{ key: 'AllowedHosts', value: '*' }, { key: 'FileTemplatingOptions:TemplatePath', value: 'Templates' }, { key: 'Identity:Keycloak:ClientSecret', value: '***' }, { key: 'Logging:LogLevel:Default', value: 'Information' }] });
    if (path === '/api/CgiPrograms') return json(route, [{ name: 'greet.exe', path: '/cgi-bin/greet.exe' }]);
    if (path === '/api/UiDefinitions') return json(route, [{ key: 'bench-supply', name: 'Bench power supply', description: 'Written by hand.', source: 'hand-built' }]);
    if (path === '/api/UiDefinitions/bench-supply' && method === 'GET') return json(route, { name: 'Bench power supply', sections: [
      { label: 'Output', controls: [
        { kind: 'toggle', id: 'output', label: 'Output enabled' },
        { kind: 'numeric', id: 'voltage', label: 'Voltage', unit: 'V', minimum: 0, maximum: 30 },
        { kind: 'choice', id: 'mode', label: 'Mode', style: 'radioGroup', options: ['Constant voltage', 'Constant current'] },
      ] },
      { label: 'Current limit', controls: [
        { kind: 'slider', id: 'current', label: 'Current', unit: 'A', minimum: 0, maximum: 5, step: 0.1 },
        { kind: 'button', id: 'reset', label: 'Reset limit', commandId: 'reset-limit' },
      ] },
    ] });
    if (path === '/api/UiDefinitions/bench-supply/evaluate' && method === 'POST') {
      const v = JSON.parse(route.request().postData() ?? '{}').values ?? {};
      const on = v.output === 'true';
      const cc = v.mode === 'Constant current';
      const volts = v.voltage ?? '5';
      const bad = Number(volts) > 30 || Number.isNaN(Number(volts));
      return json(route, [
        { id: 'output', visible: true, enabled: true, valid: true, value: v.output ?? 'false' },
        { id: 'voltage', visible: true, enabled: on, disabledReason: on ? null : 'Switch the output on first.', valid: !bad, value: volts, error: bad ? 'Enter a number from 0 to 30.' : null },
        { id: 'mode', visible: true, enabled: true, valid: true, value: v.mode ?? 'Constant voltage' },
        { id: 'current', visible: cc, enabled: true, valid: true, value: v.current ?? '1' },
        { id: 'reset', visible: cc, enabled: true, valid: true, value: '' },
      ]);
    }
    if (path === '/api/Rights/endpoints') return json(route, [{ method: 'GET', route: '/api/Cache', access: 'anonymous', rights: [] }, { method: 'GET', route: '/api/Rights/protected', access: 'rights', rights: ['rights.demo'] }, { method: 'GET', route: '/api/User/claims', access: 'authenticated', rights: [] }]);
    if (path === '/api/v1/right-audit') {
      const rows = [{ at: '2026-11-20T09:00:00Z', subject: 'ann', rights: 'docs.read', result: 'Allowed' }, { at: '2026-11-20T08:59:00Z', subject: 'bob', rights: 'identity.read', result: 'Forbidden' }];
      const wanted = new URL(route.request().url()).searchParams.get('result');
      return json(route, wanted ? rows.filter(r => r.result === wanted) : rows);
    }
    if (path === '/api/Rights/check') return json(route, { required: ['rights.demo'], held: [], result: 'Unauthenticated' });
    if (path === '/api/Identity/users') return json(route, [{ id: '1', userName: 'ann', email: 'ann@example.test', displayName: 'Ann Lee', enabled: true, roles: ['editor'], rights: ['docs.read'] }]);
    if (path === '/api/Identity/roles') return json(route, [{ name: 'editor', rights: ['docs.read'] }, { name: 'viewer', rights: [] }]);
    if (path === '/cgi-bin/greet.exe') return route.fulfill({ status: 200, contentType: 'text/html', body: '<h1>Hello, Ada!</h1>' });
    if (path.startsWith('/webhooks/')) return route.fulfill({ status: 404, contentType: 'application/problem+json', body: JSON.stringify({ title: 'Webhook rejected', status: 404, detail: 'Channel not found' }) });
    if (path === '/api/Cache') {
      if (route.request().method() === 'GET') return json(route, { key: 'greeting', found: true, value: 'hello from the cache', provider: 'MicrosoftMemoryCachingProvider' });
      return route.fulfill({ status: 204 });
    }
    if (path === '/api/Toolbox/calculate') return json(route, { result: 54, optimized: '54', variables: ['A'] });
    if (path === '/api/Toolbox/otp') return json(route, { secret: 'JBSWY3DPEHPK3PXP', code: '492039', uri: 'otpauth://totp/OoBDev%20Example:demo?secret=JBSWY3DPEHPK3PXP', valid: null });
    if (path === '/api/VectorStore/collections') return json(route, ['notes']);
    if (path === '/api/VectorStore/index') return json(route, { ids: ['1', '2', '3', '4'], store: 'memory' });
    if (path === '/api/VectorStore/query') return json(route, [{ id: '3', score: 0.31, text: 'A kitten curled up on the sofa to sleep.' }, { id: '1', score: 0.44, text: 'The cat sat on the warm windowsill.' }]);
    if (path === '/api/VectorLab/matrix') return json(route, { matrix: [[0, 1, 0.006, 0.43], [1, 0, 0.99, 0.43], [0.006, 0.99, 0, 0.45], [0.43, 0.43, 0.45, 0]], magnitudes: [1, 1, 0.906, 0.995], centroid: [0.6, 0.4, 0.175] });
    if (path === '/api/Images/classify') return json(route, [{ label: 'a cat', probability: 0.91 }, { label: 'a dog', probability: 0.06 }, { label: 'a car', probability: 0.02 }, { label: 'a landscape', probability: 0.01 }]);
    if (path === '/api/Images/embed') return json(route, { length: 512, norm: 1, first: [0.012, -0.034, 0.051, 0.007, -0.022, 0.018, 0.043, -0.009] });
    if (path === '/api/ModelCompare/similarity') return json(route, [{ model: 'all-minilm-l6-v2', length: 384, similarity: 0.71, milliseconds: 18 }]);
    if (path === '/api/Telemetry/recent') return json(route, { dashboardUrl: 'http://localhost:18888', traces: [{ traceId: '507e6ae8ea5f53be57e12a42af804fca', name: 'Microsoft.AspNetCore.Hosting.HttpRequestIn', started: '2026-10-20T08:15:18Z', durationMs: 12.4 }] });
    if (path === '/api/TextTools/xpath') return json(route, { count: 2, matches: ['<total>12.5</total>', '<total>40</total>'], xml: '<root><orders><total>12.5</total><total>40</total></orders></root>' });
    if (path === '/api/TextTools/markdown') return json(route, { html: '<h1>Title</h1>\n<ul><li>one</li><li>two</li></ul>\n' });
    if (path === '/api/Toolbox/hash') return json(route, { algorithm: 'sha512', result: '9b71d224bd62f3785d96d46ad3ea3d73319bfbc2890caadae2dff72519673ca72323c3d99ba5c11d7c7acc6e14b8c5da0c4663475c2e5c3adef46f73bcdec043' });
    if (path === '/api/Toolbox/hmac') return json(route, { algorithm: 'hmac-256', result: 'Zm9yIGRvY3VtZW50YXRpb24gb25seQ==' });
    if (path === '/api/Toolbox/vector-distance') return json(route, { cosine: 0.001234, euclidean: 0.5, dotProduct: 27.5, manhattan: 0.5 });
    if (path === '/api/Rag') return json(route, { answer: 'Queues are supported through adapters for RabbitMQ, Azure Service Bus, Azure Storage queues and Amazon SQS (passage p3).', sources: [{ id: 'p3', score: 0.712, text: 'Messages are put on a queue by an adapter: RabbitMQ, Azure Service Bus, Azure Storage queues or Amazon SQS.' }, { id: 'p1', score: 0.318, text: 'The framework resolves every provider through a keyed service, chosen from a configuration path.' }] });
    if (path === '/api/Search') return json(route, { lexical: [{ id: 'p3', score: 1.412, text: 'Messages are put on a queue by an adapter: RabbitMQ, Azure Service Bus, Azure Storage queues or Amazon SQS.' }], semantic: [{ id: 'p3', score: 0.655, text: 'Messages are put on a queue by an adapter: RabbitMQ, Azure Service Bus, Azure Storage queues or Amazon SQS.' }, { id: 'p1', score: 0.201, text: 'The framework resolves every provider through a keyed service, chosen from a configuration path.' }], hybrid: [{ id: 'p3', score: 0.033, text: 'Messages are put on a queue by an adapter: RabbitMQ, Azure Service Bus, Azure Storage queues or Amazon SQS.' }, { id: 'p1', score: 0.016, text: 'The framework resolves every provider through a keyed service, chosen from a configuration path.' }] });
    if (path.startsWith('/api/AllMiniLM') || path.startsWith('/api/SBERT')) {
      const t = url.searchParams.get('text') ?? '';
      return json(route, vector(t.length % 2 === 0 ? 1 : 1.2));
    }
    if (path === '/api/TextTemplate/SupportedTemplates') return json(route, [{ extension: '.xslt', contentType: 'application/xslt+xml', isTemplateType: true }, { extension: '.hbs', contentType: 'text/x-handlebars-template', isTemplateType: true }]);
    if (path === '/api/TextTemplate/List') return json(route, [{ name: 'order-shipped', file: 'order-shipped.html.hbs', templateContentType: 'text/x-handlebars-template', targetExtension: '.html' }]);
    if (path === '/api/TextTemplate/Read') return json(route, { info: { name: 'order-shipped', file: 'order-shipped.html.hbs', templateContentType: 'text/x-handlebars-template', targetExtension: '.html' }, text: '<h1>Hello {{name}}</h1><p>Your order {{orderId}} has shipped.</p>' });
    if (path === '/api/TextTemplate/Preview') return text(route, '<h1>Hello Ada</h1><p>Your order 1042 has shipped.</p>');
    if (path.startsWith('/api/TextTemplate/Apply')) return text(route, '<h1>Hello Ada</h1><p>Your order 1042 has shipped.</p>');
    if (path === '/api/DataAccess/query') return json(route, { columns: ['name', 'create_date'], rows: [['master', '2003-04-08 09:13:36'], ['model', '2003-04-08 09:13:36'], ['msdb', '2025-04-19 08:37:53'], ['tempdb', '2026-11-20 08:00:00']], truncated: false, elapsedMs: 4 });
    if (path === '/api/Geocode') return json(route, { matched: true, latitude: 38.8977, longitude: -77.0365, quality: 'Interpolated', normalizedAddress: '1600 PENNSYLVANIA AVE NW, WASHINGTON, DC, 20500' });
    if (path === '/api/Distance') return json(route, { meters: 343556.2, kilometers: 343.6, miles: 213.5, bearingDegrees: 148.1 });
    if (path === '/api/Generation') return json(route, { sampleType: 'customer', seed: 42, value: { Person: { Name: 'Ada Lovelace', Email: 'ada@example.org', Age: 36 }, Orders: [{ ShipTo: '1 Main St', Lines: [{ Product: 'Widget', Quantity: 3 }] }] } });
    if (path === '/api/SampleCatalog') return json(route, { '@odata.count': 3, value: [{ Id: 10, Name: 'Bench supply', Category: 'Instruments', Price: 119, InStock: true }, { Id: 7, Name: 'Logic analyzer', Category: 'Instruments', Price: 59, InStock: true }, { Id: 5, Name: 'Multimeter', Category: 'Instruments', Price: 39, InStock: true }] });
    if (path === '/api/DataLoad/preview') return json(route, { read: 3, written: 2, replaced: 0, skipped: 1, errors: [], columns: ['id', 'name', 'city'], rows: [['1', 'Ann', 'Oslo'], ['2', 'Bo', 'Lima']] });
    if (path === '/api/Schedule' || path.startsWith('/api/Schedule/')) {
      if (method === 'POST') scheduled.push({ key: 'reminder-1', dueAt: '2026-11-20T08:00:10+00:00', message: 'Stand up and stretch', firedAt: null });
      if (method === 'DELETE') scheduled.length = 0;
      return json(route, { pending: scheduled, fired: [{ key: 'backup-check', dueAt: '2026-11-20T07:59:00+00:00', message: 'Check last night\'s backup', firedAt: '2026-11-20T07:59:01+00:00' }] });
    }
    if (path === '/api/Communications/inbox/poll') return json(route, [{ messageId: 'm-1', from: 'grace@example.test', to: ['ada@example.test'], subject: 'Re: Your order has shipped', preview: 'Thanks, it arrived today.', timestamp: '2026-11-20T09:30:00Z' }]);
    if (path.startsWith('/api/Communications/')) return text(route, 'Mail accepted');
    if (path === '/api/MessageQueueing/channels') return json(route, [{ channel: 'EmailMessageModel', provider: 'rabbit-mq', queueName: 'email-receive', hostName: 'localhost', receiverEnabled: false }, { channel: 'ExampleMessageProvider', provider: 'rabbit-mq', queueName: 'email-receive', hostName: 'localhost', receiverEnabled: false }]);
    if (path === '/api/MessageQueueing/received') return json(route, [{ receivedAt: '2026-11-09T12:00:00Z', correlationId: 'demo-1', message: '{"Input":"hello"}' }]);
    if (path.startsWith('/api/MessageQueueing/')) return text(route, '7f3c1c1e-0b53-4b7c-9d1f-2a9d8e0a1111');
    if (path === '/api/User/claims') return json(route, [{ type: 'name', value: 'Ada Lovelace' }, { type: 'role', value: 'Reader' }]);
    if (path === '/Document/List') return json(route, [{ fileName: 'quarterly-report.docx', contentType: 'application/vnd.openxmlformats-officedocument.wordprocessingml.document' }]);
    if (path.startsWith('/Document/Convert')) return text(route, 'Quarterly report\n\nRevenue grew 12 percent.');
    if (path.startsWith('/Document/Upload')) return text(route, 'stored');
    if (path.startsWith('/Document/Text')) return text(route, 'Quarterly report\n\nRevenue grew 12 percent.');
    if (path.startsWith('/openapi/') || path.startsWith('/asyncapi/')) return json(route, {});
    return route.continue();
  });
}
