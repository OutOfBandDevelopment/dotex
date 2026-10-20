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
