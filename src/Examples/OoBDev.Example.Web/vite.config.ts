import { defineConfig } from 'vitest/config';
import vue from '@vitejs/plugin-vue';

// The Aspire host (OoBDev.Example.AppHost) provides the API address; otherwise use the default local port.
const api = process.env['services__example-api__http__0'] ?? 'http://localhost:5000';

export default defineConfig({
  plugins: [vue({ template: { compilerOptions: { isCustomElement: tag => tag.startsWith('fluent-') } } })],
  server: {
    port: 5173,
    proxy: {
      '/api': api,
      '/Document': api,
      '/Search': api,
      '/openapi': api,
      '/scalar': api,
      '/asyncapi': api,
    },
  },
  test: { environment: 'node', include: ['src/**/*.test.ts'] },
});
