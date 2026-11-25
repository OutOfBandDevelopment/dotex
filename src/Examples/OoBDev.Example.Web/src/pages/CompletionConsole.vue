<script setup lang="ts">
import { CompletionViewModel } from './CompletionViewModel';

const props = defineProps<{ title: string; basePath: string; note?: string }>();
const vm = new CompletionViewModel(props.basePath);
</script>

<template>
  <h1>{{ title }}</h1>
  <p v-if="note">{{ note }}</p>
  <p>
    <label>Model <input v-model="vm.model.value" size="30" placeholder="provider default" /></label>
    <label><input type="checkbox" v-model="vm.keepContext.value" /> Keep context between turns</label>
  </p>
  <p><label>System<br /><input v-model="vm.system.value" size="70" /></label></p>
  <div class="turns">
    <p v-for="(t, i) in vm.turns.value" :key="i" :class="t.role"><strong>{{ t.role }}:</strong> {{ t.text }}</p>
  </div>
  <p><label>Prompt<br /><textarea v-model="vm.prompt.value" rows="3" cols="70" /></label></p>
  <p>
    <fluent-button appearance="primary" :disabled="!vm.send.canExecute.value" @click="vm.send.execute()">Send</fluent-button>
    <fluent-button @click="vm.reset.execute()">New conversation</fluent-button>
    <span v-if="vm.context.value.length">context tokens: {{ vm.context.value.length }}</span>
  </p>
  <p v-if="vm.send.error.value" class="error">{{ vm.send.error.value }}</p>
</template>

<style scoped>
.turns .user { color: var(--shell-muted); }
</style>
