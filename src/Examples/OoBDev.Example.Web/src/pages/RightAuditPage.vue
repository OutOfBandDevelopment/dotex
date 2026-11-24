<script setup lang="ts">
import { onMounted } from 'vue';
import { RightAuditViewModel } from './RightAuditViewModel';

const vm = new RightAuditViewModel();
onMounted(() => vm.refresh.execute());
</script>

<template>
  <h1>Right audit trail</h1>
  <p>Every application right check the API made, newest first. Needs the <code>audit.read</code> right.</p>
  <p>
    <label>Subject <input v-model="vm.subject.value" size="20" /></label>
    <label>Result
      <select v-model="vm.result.value">
        <option value="">any</option>
        <option>Allowed</option>
        <option>Forbidden</option>
        <option>Unauthenticated</option>
      </select>
    </label>
    <label>Most <input v-model.number="vm.maxCount.value" type="number" min="1" max="1000" /></label>
    <fluent-button appearance="primary" :disabled="!vm.refresh.canExecute.value" @click="vm.refresh.execute()">Refresh</fluent-button>
  </p>
  <p v-if="vm.refresh.error.value" class="error">{{ vm.refresh.error.value }}</p>
  <p v-if="vm.notConfigured.value" class="note">Not configured: the host keeps no durable trail. Set <code>OoBDev:RightAudit:ConnectionString</code> (SQL Server) and restart the API.</p>
  <template v-else>
    <p>{{ vm.summary.value }}</p>
    <table class="grid">
      <thead><tr><th>Time (UTC)</th><th>Subject</th><th>Rights</th><th>Result</th></tr></thead>
      <tbody><tr v-for="(r, i) in vm.rows.value" :key="i"><td>{{ r.at }}</td><td>{{ r.subject }}</td><td>{{ r.rights }}</td><td>{{ r.result }}</td></tr></tbody>
    </table>
  </template>
</template>
