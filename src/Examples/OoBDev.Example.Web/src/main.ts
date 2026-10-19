import { createApp } from 'vue';
import { createPinia } from 'pinia';
import { setTheme } from '@fluentui/web-components';
import { webLightTheme } from '@fluentui/tokens';
import '@fluentui/web-components/button.js';
import App from './App.vue';
import { loadConfig } from './core/config';
import { configureAuth } from './core/auth';
import './styles.css';

setTheme(webLightTheme);

// The runtime configuration decides whether sign-in is on, so it loads before the router starts.
configureAuth(await loadConfig());
const { router } = await import('./router');
createApp(App).use(createPinia()).use(router).mount('#app');
