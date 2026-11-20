<script setup lang="ts">
import { DataAccessViewModel } from './DataAccessViewModel';

const vm = new DataAccessViewModel();
</script>

<template>
  <h1>Data access</h1>
  <p>Runs one SELECT on the instance named in <code>OoBDev:DataAccess:ConnectionString</code> (a test or reporting instance). The text is checked by a parser, runs with the read-only intent, and is capped in rows and time. Needs the right <code>data.query</code>.</p>
  <p><textarea v-model="vm.sql.value" rows="6" cols="90" aria-label="SQL"></textarea></p>
  <p><fluent-button appearance="primary" :disabled="!vm.run.canExecute.value" @click="vm.run.execute()">Run</fluent-button></p>
  <p v-if="vm.run.error.value" class="error">{{ vm.run.error.value }}</p>
  <template v-if="vm.result.value">
    <p>{{ vm.result.value.rows.length }} row(s) in {{ vm.result.value.elapsedMs }} ms<span v-if="vm.result.value.truncated"> (more rows exist; capped)</span></p>
    <table class="grid">
      <thead><tr><th v-for="c in vm.result.value.columns" :key="c">{{ c }}</th></tr></thead>
      <tbody><tr v-for="(row, i) in vm.result.value.rows" :key="i"><td v-for="(cell, j) in row" :key="j">{{ cell ?? 'NULL' }}</td></tr></tbody>
    </table>
  </template>
</template>
