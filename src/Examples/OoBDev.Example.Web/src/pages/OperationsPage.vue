<script setup lang="ts">
import { onMounted } from 'vue';
import { OperationsViewModel } from './OperationsViewModel';

const vm = new OperationsViewModel();
onMounted(() => vm.refresh.execute());
</script>

<template>
  <h1>Operations</h1>
  <p><fluent-button appearance="primary" :disabled="!vm.refresh.canExecute.value" @click="vm.refresh.execute()">Refresh</fluent-button></p>
  <p v-if="vm.refresh.error.value" class="error">{{ vm.refresh.error.value }}</p>
  <template v-if="vm.info.value">
    <table class="grid">
      <tbody>
        <tr><th>Application</th><td>{{ vm.info.value.application }}</td></tr>
        <tr><th>Environment</th><td>{{ vm.info.value.environment }}</td></tr>
        <tr><th>Runtime</th><td>{{ vm.info.value.runtime }}</td></tr>
        <tr><th>Machine</th><td>{{ vm.info.value.machine }}</td></tr>
        <tr><th>Started</th><td>{{ vm.info.value.started }}</td></tr>
      </tbody>
    </table>
    <h2>Configuration</h2>
    <p>Values for keys that look like secrets are masked by the API.</p>
    <p><label>Filter <input v-model="vm.filter.value" size="40" /></label> ({{ vm.entries.value.length }} shown)</p>
    <table class="grid">
      <thead><tr><th>Key</th><th>Value</th></tr></thead>
      <tbody><tr v-for="e in vm.entries.value" :key="e.key"><td>{{ e.key }}</td><td>{{ e.value }}</td></tr></tbody>
    </table>
  </template>
</template>
