import { createRouter, createWebHistory } from 'vue-router';
import { auth } from '../core/auth';

export const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', component: () => import('../pages/OverviewPage.vue'), meta: { crumbs: ['Overview'] } },
    {
      path: '/signin-callback',
      component: () => import('../pages/OverviewPage.vue'),
      beforeEnter: async () => {
        await auth.completeSignIn();
        return '/';
      },
    },
    { path: '/ai/chat', component: () => import('../pages/ChatPage.vue'), meta: { crumbs: ['AI', 'Chat'] } },
    { path: '/identity/me', component: () => import('../pages/CurrentUserPage.vue'), meta: { crumbs: ['Identity and security', 'Current user'] } },
    { path: '/developer/api', component: () => import('../pages/ExternalLinkPage.vue'), props: { title: 'API reference', href: '/scalar' }, meta: { crumbs: ['Developer', 'API reference'] } },
    { path: '/developer/events', component: () => import('../pages/ExternalLinkPage.vue'), props: { title: 'Event reference', href: '/asyncapi' }, meta: { crumbs: ['Developer', 'Event reference'] } },
  ],
});
