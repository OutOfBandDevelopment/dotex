<script setup lang="ts">
import { ToolCatalogViewModel } from './ToolCatalogViewModel';

const vm = new ToolCatalogViewModel();
</script>

<template>
  <h1>Command-line tools</h1>
  <p>The tools in <code>src/Tools</code>. They run on a developer machine or in a pipeline, never from this page; each project has a readme with its options.</p>
  <p><label>Filter<br /><input v-model="vm.filter.value" size="30" aria-label="Filter tools" /></label></p>
  <table class="grid" aria-label="Command-line tools">
    <thead><tr><th>Command</th><th>What it does</th><th>Run it</th></tr></thead>
    <tbody>
      <tr v-for="t in vm.tools.value" :key="t.command">
        <td><code>{{ t.command }}</code></td>
        <td>{{ t.summary }}</td>
        <td><code>{{ t.install }}</code></td>
      </tr>
    </tbody>
  </table>
  <p v-if="vm.tools.value.length === 0">No tool matches the filter.</p>
</template>
