<script setup lang="ts">
import { onMounted } from 'vue';
import { RightsViewModel } from './RightsViewModel';

const vm = new RightsViewModel();
onMounted(() => vm.load.execute());
</script>

<template>
  <h1>Rights and roles</h1>
  <p>Every endpoint services expose and the access it needs. An endpoint with <code>[ApplicationRight]</code> needs the user to hold at least one of the listed application rights; roles are translated to rights in middleware before the check.</p>
  <p v-if="vm.load.error.value" class="error">{{ vm.load.error.value }}</p>
  <h2>Check your rights</h2>
  <p><label>Rights (space or comma separated)<br /><input v-model="vm.rights.value" size="40" aria-label="Rights" /></label></p>
  <p><fluent-button appearance="primary" :disabled="!vm.run.canExecute.value" @click="vm.run.execute()">Check</fluent-button></p>
  <p v-if="vm.run.error.value" class="error">{{ vm.run.error.value }}</p>
  <div v-if="vm.check.value">
    <p>Result: <strong class="rights-result">{{ vm.check.value.result }}</strong></p>
    <p>Rights you hold: <span class="rights-held">{{ vm.check.value.held.length > 0 ? vm.check.value.held.join(', ') : 'none' }}</span></p>
  </div>
  <h2>Endpoints</h2>
  <p><label>Filter<br /><input v-model="vm.filter.value" size="30" aria-label="Filter" /></label></p>
  <table class="rights-table">
    <thead><tr><th>Method</th><th>Route</th><th>Access</th><th>Rights</th></tr></thead>
    <tbody>
      <tr v-for="e in vm.visible()" :key="e.method + e.route">
        <td>{{ e.method }}</td><td><code>{{ e.route }}</code></td><td>{{ e.access }}</td><td>{{ e.rights.join(', ') }}</td>
      </tr>
    </tbody>
  </table>
</template>
