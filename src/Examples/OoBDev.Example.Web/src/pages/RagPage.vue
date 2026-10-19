<script setup lang="ts">
import { RagViewModel } from './RagViewModel';

const vm = new RagViewModel();
</script>

<template>
  <h1>Retrieval augmented answers</h1>
  <p>The passages and the question are embedded in the API, the closest passages become the context for the model, and the answer names its sources.</p>
  <p><label>Passages (separate with a blank line)<br /><textarea v-model="vm.passages.value" rows="10" cols="80" /></label></p>
  <p><label>Question<br /><input v-model="vm.question.value" size="60" /></label></p>
  <p><label>Passages to use <input v-model.number="vm.topK.value" type="number" min="1" max="10" /></label></p>
  <p><fluent-button appearance="primary" :disabled="!vm.ask.canExecute.value" @click="vm.ask.execute()">Ask</fluent-button></p>
  <p v-if="vm.ask.error.value" class="error">{{ vm.ask.error.value }}</p>
  <template v-if="vm.result.value">
    <h2>Answer</h2>
    <pre class="result">{{ vm.result.value.answer }}</pre>
    <h2>Sources</h2>
    <table class="grid">
      <thead><tr><th>Passage</th><th>Similarity</th><th>Text</th></tr></thead>
      <tbody>
        <tr v-for="s in vm.result.value.sources" :key="s.id"><td>{{ s.id }}</td><td>{{ s.score.toFixed(3) }}</td><td>{{ s.text }}</td></tr>
      </tbody>
    </table>
  </template>
</template>
