import { defineConfig } from 'vitest/config';
import vue from '@vitejs/plugin-vue';

// The Aspire host (OoBDev.Example.AppHost) provides the API address; otherwise use the default local port.
const api = process.env['services__example-api__http__0'] ?? 'http://localhost:5000';

export default defineConfig({
  plugins: [vue({ template: { compilerOptions: { isCustomElement: tag => tag.startsWith('fluent-') } } })],
  server: {
    port: 5173,
    // The browser's Host header is kept so the OpenAPI documents name the site's own origin; otherwise Scalar sends its
    // requests straight to the API's port, which is another origin, and the browser blocks them.
    proxy: Object.fromEntries(
      ['/api', '/Document', '/Search', '/openapi', '/scalar', '/asyncapi', '/cgi-bin'].map(path => [path, { target: api, changeOrigin: false }]),
    ),
  },
  test: { environment: 'node', include: ['src/**/*.test.ts'] },
});
