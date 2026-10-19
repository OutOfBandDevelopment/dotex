<script setup lang="ts">
import { computed } from 'vue';
import { useRoute } from 'vue-router';
import { navGroups } from './layout/nav';
import { auth, authEnabled } from './core/auth';

const route = useRoute();
const crumbs = computed(() => ['Home', ...((route.meta.crumbs as string[] | undefined) ?? [])]);
</script>

<template>
  <div class="shell">
    <header class="top">
      <strong>OoBDev Example</strong>
      <span class="spacer" />
      <template v-if="authEnabled">
        <span>{{ auth.user?.profile?.preferred_username ?? 'Signed out' }}</span>
        <button @click="auth.user ? auth.signOut() : auth.signIn()">{{ auth.user ? 'Sign out' : 'Sign in' }}</button>
      </template>
      <span v-else>Anonymous</span>
    </header>
    <nav class="left" aria-label="Primary">
      <RouterLink to="/" class="item">Overview</RouterLink>
      <section v-for="g in navGroups" :key="g.title">
        <h2>{{ g.title }}</h2>
        <RouterLink v-for="i in g.items" :key="i.to" :to="i.to" class="item">{{ i.label }}</RouterLink>
      </section>
    </nav>
    <main class="blade">
      <div class="crumbs">{{ crumbs.join(' > ') }}</div>
      <RouterView />
    </main>
  </div>
</template>

<style scoped>
.shell { display: grid; grid-template: 48px 1fr / 240px 1fr; height: 100%; }
.top { grid-column: 1 / 3; display: flex; align-items: center; gap: 12px; padding: 0 16px; background: var(--shell-top); color: #fff; }
.spacer { flex: 1; }
.left { background: var(--shell-nav-bg); border-right: 1px solid var(--shell-border); overflow: auto; padding: 8px 0; }
.left h2 { font-size: 11px; text-transform: uppercase; color: var(--shell-muted); margin: 16px 16px 4px; }
.item { display: block; padding: 6px 16px; color: var(--shell-text); text-decoration: none; }
.item.router-link-active { border-left: 3px solid var(--shell-accent); padding-left: 13px; background: var(--shell-bg); }
.blade { padding: 16px 24px; overflow: auto; }
.crumbs { color: var(--shell-muted); font-size: 12px; margin-bottom: 8px; }
@media (max-width: 700px) { .shell { grid-template-columns: 1fr; } .left { display: none; } }
</style>
