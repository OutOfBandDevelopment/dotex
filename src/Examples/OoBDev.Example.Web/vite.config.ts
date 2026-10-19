import { defineConfig } from 'vitest/config';
import vue from '@vitejs/plugin-vue';

export default defineConfig({
  plugins: [vue({ template: { compilerOptions: { isCustomElement: tag => tag.startsWith('fluent-') } } })],
  server: {
    port: 5173,
    proxy: {
      '/api': 'http://localhost:5000',
      '/Document': 'http://localhost:5000',
      '/Search': 'http://localhost:5000',
      '/openapi': 'http://localhost:5000',
    },
  },
  test: { environment: 'node' },
});
