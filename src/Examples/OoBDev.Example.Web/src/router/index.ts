import { createRouter, createWebHistory } from 'vue-router';
import { auth, authEnabled } from '../core/auth';

export const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', component: () => import('../pages/OverviewPage.vue'), meta: { crumbs: ['Overview'] } },
    {
      path: '/signin-callback',
      component: () => import('../pages/OverviewPage.vue'),
      beforeEnter: async () => {
        return await auth.completeSignIn();
      },
    },
    { path: '/ai/chat', component: () => import('../pages/ChatPage.vue'), meta: { crumbs: ['AI', 'Chat'] } },
    { path: '/ai/rag', component: () => import('../pages/RagPage.vue'), meta: { crumbs: ['AI', 'Retrieval (RAG)'] } },
    { path: '/operations/info', component: () => import('../pages/OperationsPage.vue'), meta: { crumbs: ['Operations', 'Runtime and configuration'] } },
    { path: '/data/cache', component: () => import('../pages/CachePage.vue'), meta: { crumbs: ['Data', 'Cache lab'] } },
    { path: '/utilities/toolbox', component: () => import('../pages/ToolboxPage.vue'), meta: { crumbs: ['Utilities', 'Toolbox'] } },
    { path: '/utilities/text', component: () => import('../pages/TextToolsPage.vue'), meta: { crumbs: ['Utilities', 'Text tools'] } },
    { path: '/utilities/vectors', component: () => import('../pages/VectorStorePage.vue'), meta: { crumbs: ['Utilities', 'Vector store'] } },
    { path: '/utilities/vector-lab', component: () => import('../pages/VectorLabPage.vue'), meta: { crumbs: ['Utilities', 'Vector lab'] } },
    { path: '/ai/images', component: () => import('../pages/ImagesPage.vue'), meta: { crumbs: ['AI', 'Image labels'] } },
    { path: '/ai/model-compare', component: () => import('../pages/ModelComparePage.vue'), meta: { crumbs: ['AI', 'Model compare'] } },
    { path: '/operations/telemetry', component: () => import('../pages/TelemetryPage.vue'), meta: { crumbs: ['Operations', 'Telemetry'] } },
    { path: '/ai/ollama', component: () => import('../pages/OllamaPage.vue'), meta: { crumbs: ['AI', 'Ollama'] } },
    { path: '/ai/groq', component: () => import('../pages/GroqPage.vue'), meta: { crumbs: ['AI', 'Groq cloud'] } },
    { path: '/ai/embeddings', component: () => import('../pages/EmbeddingsPage.vue'), meta: { crumbs: ['AI', 'Embeddings'] } },
    { path: '/content/documents', component: () => import('../pages/DocumentsPage.vue'), meta: { crumbs: ['Content', 'Documents'] } },
    { path: '/content/templates', component: () => import('../pages/TemplatesPage.vue'), meta: { crumbs: ['Content', 'Templates'] } },
    { path: '/messaging/communications', component: () => import('../pages/CommunicationsPage.vue'), meta: { crumbs: ['Messaging', 'Communications'] } },
    { path: '/messaging/queues', component: () => import('../pages/QueuesPage.vue'), meta: { crumbs: ['Messaging', 'Queues'] } },
    { path: '/identity/me', component: () => import('../pages/CurrentUserPage.vue'), meta: { crumbs: ['Identity and security', 'Current user'], requiresAuth: true } },
    { path: '/developer/api', component: () => import('../pages/ExternalLinkPage.vue'), props: { title: 'API reference', href: '/scalar' }, meta: { crumbs: ['Developer', 'API reference'] } },
    { path: '/developer/events', component: () => import('../pages/ExternalLinkPage.vue'), props: { title: 'Event reference', href: '/asyncapi/all' }, meta: { crumbs: ['Developer', 'Event reference'] } },
  ],
});

/** With OIDC configured every screen needs a token (the API requires one), so visitors sign in first. */
router.beforeEach(async to => {
  if (authEnabled && !auth.accessToken && to.path !== '/signin-callback') {
    await auth.signIn(to.fullPath);
    return false;
  }
});
