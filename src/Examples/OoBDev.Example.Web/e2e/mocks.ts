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
  await page.route(/\/(api|Document|openapi|asyncapi)\//, async route => {
    const url = new URL(route.request().url());
    const path = url.pathname;
    const method = route.request().method();

    if (path === '/api/Ollama' && method === 'POST') return text(route, 'An embedding turns text into a list of numbers so similar meanings end up close together.');
    if (path === '/api/Ollama/Streamed') return json(route, ['An embedding ', 'turns text into ', 'numbers.']);
    if (path === '/api/Ollama/Completion') {
      turn++;
      return json(route, { context: [1, 2, 3, turn], response: turn === 1 ? 'Hello! How can I help you today?' : 'Photosynthesis is how plants turn light into sugar.' });
    }
    if (path === '/api/Operations/info') return json(route, { environment: 'Development', application: 'OoBDev.Example.WebApi', runtime: '.NET 10.0.0', machine: 'example-api', started: '2026-10-19T08:00:00+00:00', configuration: [{ key: 'AllowedHosts', value: '*' }, { key: 'FileTemplatingOptions:TemplatePath', value: 'Templates' }, { key: 'Identity:Keycloak:ClientSecret', value: '***' }, { key: 'Logging:LogLevel:Default', value: 'Information' }] });
    if (path === '/api/Cache') {
      if (route.request().method() === 'GET') return json(route, { key: 'greeting', found: true, value: 'hello from the cache', provider: 'MicrosoftMemoryCachingProvider' });
      return route.fulfill({ status: 204 });
    }
    if (path === '/api/Toolbox/calculate') return json(route, { result: 54, optimized: '54', variables: ['A'] });
    if (path === '/api/Toolbox/otp') return json(route, { secret: 'JBSWY3DPEHPK3PXP', code: '492039', uri: 'otpauth://totp/OoBDev%20Example:demo?secret=JBSWY3DPEHPK3PXP', valid: null });
    if (path === '/api/VectorStore/index') return json(route, { ids: ['1', '2', '3', '4'], store: 'memory' });
    if (path === '/api/VectorStore/query') return json(route, [{ id: '3', score: 0.31, text: 'A kitten curled up on the sofa to sleep.' }, { id: '1', score: 0.44, text: 'The cat sat on the warm windowsill.' }]);
    if (path === '/api/VectorLab/matrix') return json(route, { matrix: [[0, 1, 0.006, 0.43], [1, 0, 0.99, 0.43], [0.006, 0.99, 0, 0.45], [0.43, 0.43, 0.45, 0]], magnitudes: [1, 1, 0.906, 0.995], centroid: [0.6, 0.4, 0.175] });
    if (path === '/api/Images/classify') return json(route, [{ label: 'a cat', probability: 0.91 }, { label: 'a dog', probability: 0.06 }, { label: 'a car', probability: 0.02 }, { label: 'a landscape', probability: 0.01 }]);
    if (path === '/api/Images/embed') return json(route, { length: 512, norm: 1, first: [0.012, -0.034, 0.051, 0.007, -0.022, 0.018, 0.043, -0.009] });
    if (path === '/api/TextTools/xpath') return json(route, { count: 2, matches: ['<total>12.5</total>', '<total>40</total>'], xml: '<root><orders><total>12.5</total><total>40</total></orders></root>' });
    if (path === '/api/TextTools/markdown') return json(route, { html: '<h1>Title</h1>\n<ul><li>one</li><li>two</li></ul>\n' });
    if (path === '/api/Toolbox/hash') return json(route, { algorithm: 'sha512', result: '9b71d224bd62f3785d96d46ad3ea3d73319bfbc2890caadae2dff72519673ca72323c3d99ba5c11d7c7acc6e14b8c5da0c4663475c2e5c3adef46f73bcdec043' });
    if (path === '/api/Toolbox/hmac') return json(route, { algorithm: 'hmac-256', result: 'Zm9yIGRvY3VtZW50YXRpb24gb25seQ==' });
    if (path === '/api/Toolbox/vector-distance') return json(route, { cosine: 0.001234, euclidean: 0.5, dotProduct: 27.5, manhattan: 0.5 });
    if (path === '/api/Rag') return json(route, { answer: 'Queues are supported through adapters for RabbitMQ, Azure Service Bus, Azure Storage queues and Amazon SQS (passage p3).', sources: [{ id: 'p3', score: 0.712, text: 'Messages are put on a queue by an adapter: RabbitMQ, Azure Service Bus, Azure Storage queues or Amazon SQS.' }, { id: 'p1', score: 0.318, text: 'The framework resolves every provider through a keyed service, chosen from a configuration path.' }] });
    if (path.startsWith('/api/AllMiniLM') || path.startsWith('/api/SBERT')) {
      const t = url.searchParams.get('text') ?? '';
      return json(route, vector(t.length % 2 === 0 ? 1 : 1.2));
    }
    if (path === '/api/TextTemplate/SupportedTemplates') return json(route, [{ extension: '.xslt', contentType: 'application/xslt+xml', isTemplateType: true }, { extension: '.hbs', contentType: 'text/x-handlebars-template', isTemplateType: true }]);
    if (path.startsWith('/api/TextTemplate/Apply')) return text(route, '<h1>Hello Ada</h1><p>Your order 1042 has shipped.</p>');
    if (path.startsWith('/api/Communications/')) return text(route, 'Mail accepted');
    if (path.startsWith('/api/MessageQueueing/')) return text(route, '7f3c1c1e-0b53-4b7c-9d1f-2a9d8e0a1111');
    if (path === '/api/User/claims') return json(route, [{ type: 'name', value: 'Ada Lovelace' }, { type: 'role', value: 'Reader' }]);
    if (path.startsWith('/Document/Convert')) return text(route, 'Quarterly report\n\nRevenue grew 12 percent.');
    if (path.startsWith('/Document/Upload')) return text(route, 'stored');
    if (path.startsWith('/Document/Text')) return text(route, 'Quarterly report\n\nRevenue grew 12 percent.');
    if (path.startsWith('/openapi/') || path.startsWith('/asyncapi/')) return json(route, {});
    return route.continue();
  });
}
