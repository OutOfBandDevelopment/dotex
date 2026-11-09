<script setup lang="ts">
import { SearchViewModel } from './SearchViewModel';

const vm = new SearchViewModel();
const modes = [
  { key: 'lexical', title: 'Keywords (BM25)', note: 'Counts shared words; a paraphrase with no shared word finds nothing.' },
  { key: 'semantic', title: 'Meaning (embeddings)', note: 'Compares sentence vectors; finds paraphrases, ranks everything.' },
  { key: 'hybrid', title: 'Hybrid (rank fusion)', note: 'Adds 1 / (60 + rank) from both lists, so a passage both modes like rises.' },
] as const;
</script>

<template>
  <h1>Search</h1>
  <p>The same query runs three ways over the passages below: by keywords, by meaning and by both fused. Compare the lists to see what each mode finds.</p>
  <p><label>Passages (separate with a blank line)<br /><textarea v-model="vm.passages.value" rows="8" cols="80" /></label></p>
  <p><label>Query<br /><input v-model="vm.query.value" size="60" /></label></p>
  <p><label>Hits per mode <input v-model.number="vm.limit.value" type="number" min="1" max="20" /></label></p>
  <p><fluent-button appearance="primary" :disabled="!vm.search.canExecute.value" @click="vm.search.execute()">Search</fluent-button></p>
  <p v-if="vm.search.error.value" class="error">{{ vm.search.error.value }}</p>
  <template v-if="vm.result.value">
    <section v-for="m in modes" :key="m.key">
      <h2>{{ m.title }}</h2>
      <p>{{ m.note }}</p>
      <p v-if="vm.result.value[m.key].length === 0">No passage matched.</p>
      <table v-else class="grid">
        <thead><tr><th>Passage</th><th>Score</th><th>Text</th></tr></thead>
        <tbody>
          <tr v-for="h in vm.result.value[m.key]" :key="h.id"><td>{{ h.id }}</td><td>{{ h.score.toFixed(3) }}</td><td>{{ h.text }}</td></tr>
        </tbody>
      </table>
    </section>
  </template>
</template>
