<script setup lang="ts">
import { DataLoadViewModel } from './DataLoadViewModel';

const vm = new DataLoadViewModel();
</script>

<template>
  <h1>Data loading</h1>
  <p>Loads a small CSV or JSON text into a throwaway in-memory table with <code>OoBDev.DataLoader</code>: rows are read, checked against the key column and stored, and a report says what was written, replaced, skipped or rejected. Nothing is kept between loads. Needs the right <code>data.load</code>.</p>
  <p>
    <label>Format <select v-model="vm.format.value" aria-label="Format"><option value="csv">csv</option><option value="json">json</option></select></label>
    <label>Key column <input v-model="vm.keyColumn.value" aria-label="Key column" /></label>
    <label>Duplicates <select v-model="vm.duplicates.value" aria-label="Duplicates"><option>Skip</option><option>Replace</option><option>Fail</option></select></label>
  </p>
  <p><textarea v-model="vm.text.value" rows="8" cols="70" aria-label="File text"></textarea></p>
  <p><fluent-button appearance="primary" :disabled="!vm.load.canExecute.value" @click="vm.load.execute()">Load</fluent-button></p>
  <p v-if="vm.load.error.value" class="error">{{ vm.load.error.value }}</p>
  <template v-if="vm.preview.value">
    <table class="grid" aria-label="Report">
      <tbody>
        <tr><th>Read</th><td>{{ vm.preview.value.read }}</td></tr>
        <tr><th>Written</th><td>{{ vm.preview.value.written }}</td></tr>
        <tr><th>Replaced</th><td>{{ vm.preview.value.replaced }}</td></tr>
        <tr><th>Skipped</th><td>{{ vm.preview.value.skipped }}</td></tr>
        <tr><th>Rejected</th><td>{{ vm.preview.value.errors.length }}</td></tr>
      </tbody>
    </table>
    <ul v-if="vm.preview.value.errors.length" aria-label="Rejected rows">
      <li v-for="e in vm.preview.value.errors" :key="e.line">Line {{ e.line }}: {{ e.message }}</li>
    </ul>
    <h2>Stored rows</h2>
    <table class="grid" aria-label="Stored rows">
      <thead><tr><th v-for="c in vm.preview.value.columns" :key="c">{{ c }}</th></tr></thead>
      <tbody><tr v-for="(r, i) in vm.preview.value.rows" :key="i"><td v-for="(v, j) in r" :key="j">{{ v }}</td></tr></tbody>
    </table>
  </template>
</template>
