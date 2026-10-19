<script setup lang="ts">
import { EmbeddingsViewModel, models } from './EmbeddingsViewModel';

const vm = new EmbeddingsViewModel();
</script>

<template>
  <h1>Embeddings lab</h1>
  <p><label>Model <select v-model="vm.model.value"><option v-for="m in models" :key="m.path" :value="m.path">{{ m.label }}</option></select></label></p>
  <p><label>Text A<br /><input v-model="vm.textA.value" size="60" /></label></p>
  <p><label>Text B<br /><input v-model="vm.textB.value" size="60" /></label></p>
  <p><fluent-button appearance="primary" :disabled="!vm.embed.canExecute.value" @click="vm.embed.execute()">Embed and compare</fluent-button></p>
  <p v-if="vm.embed.error.value" class="error">{{ vm.embed.error.value }}</p>
  <pre class="result">Dimensions: {{ vm.vectorA.value.length }}
Cosine similarity: {{ vm.vectorA.value.length ? vm.similarity.value.toFixed(4) : '-' }}
Vector A (first 8): {{ vm.vectorA.value.slice(0, 8).map(v => v.toFixed(4)).join(', ') }}</pre>
</template>
