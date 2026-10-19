<script setup lang="ts">
import { onMounted } from 'vue';
import { OverviewViewModel, isHealthy } from './OverviewViewModel';

const vm = new OverviewViewModel();
onMounted(() => vm.check.execute());
</script>

<template>
  <h1>OoBDev framework examples</h1>
  <p>Each screen exercises one framework feature through <code>OoBDev.Example.WebApi</code>. Pick a feature in the left navigation.</p>
  <h2>API status</h2>
  <table>
    <thead><tr><th>Endpoint</th><th>Status</th><th>Time</th><th /></tr></thead>
    <tbody>
      <tr v-for="p in vm.probes.value" :key="p.path">
        <td><RouterLink :to="p.to">{{ p.name }}</RouterLink></td>
        <td>{{ p.status === null ? '...' : p.status === 0 ? 'unreachable' : p.status }}</td>
        <td>{{ p.ms === null ? '' : `${p.ms} ms` }}</td>
        <td>{{ p.status === null ? '' : isHealthy(p) ? 'OK' : 'Check' }}</td>
      </tr>
    </tbody>
  </table>
  <p><fluent-button :disabled="!vm.check.canExecute.value" @click="vm.check.execute()">Recheck</fluent-button></p>
</template>
