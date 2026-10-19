<script setup lang="ts">
import { onMounted } from 'vue';
import { CurrentUserViewModel } from './CurrentUserViewModel';

const vm = new CurrentUserViewModel();
onMounted(() => vm.refresh.execute());
</script>

<template>
  <h1>Current user</h1>
  <p v-if="vm.refresh.error.value" class="error">{{ vm.refresh.error.value }}</p>
  <table class="grid">
    <thead><tr><th>Type</th><th>Value</th><th>Issuer</th></tr></thead>
    <tbody>
      <tr v-for="(c, i) in vm.claims.value" :key="i"><td>{{ c.type }}</td><td>{{ c.value }}</td><td>{{ c.issuer }}</td></tr>
    </tbody>
  </table>
</template>
