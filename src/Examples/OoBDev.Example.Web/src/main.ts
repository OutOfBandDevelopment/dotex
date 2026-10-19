import { createApp } from 'vue';
import { createPinia } from 'pinia';
import { setTheme } from '@fluentui/web-components';
import { webLightTheme } from '@fluentui/tokens';
import '@fluentui/web-components/button.js';
import App from './App.vue';
import { router } from './router';
import './styles.css';

setTheme(webLightTheme);
createApp(App).use(createPinia()).use(router).mount('#app');
