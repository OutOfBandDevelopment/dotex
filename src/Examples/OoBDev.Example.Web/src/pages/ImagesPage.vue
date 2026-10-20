<script setup lang="ts">
import { ImagesViewModel } from './ImagesViewModel';

const vm = new ImagesViewModel();
const onFile = (e: Event) => { vm.file.value = (e.target as HTMLInputElement).files?.[0] ?? null; };
</script>

<template>
  <h1>Image labels</h1>
  <p>Pick an image and candidate labels: CLIP ranks how well each label describes it, and can also embed the image as a vector. The model downloads on first use, so the first call is slow.</p>
  <p><label>Image<br /><input type="file" accept="image/*" @change="onFile" /></label></p>
  <p><label>Labels (comma separated) <input v-model="vm.labels.value" size="50" /></label></p>
  <p>
    <fluent-button appearance="primary" :disabled="!vm.classify.canExecute.value" @click="vm.classify.execute()">Classify</fluent-button>
    <fluent-button :disabled="!vm.embed.canExecute.value" @click="vm.embed.execute()">Embed</fluent-button>
  </p>
  <p v-if="vm.classify.error.value" class="error">{{ vm.classify.error.value }}</p>
  <p v-if="vm.embed.error.value" class="error">{{ vm.embed.error.value }}</p>
  <table v-if="vm.ranked.value.length" aria-label="Ranked labels">
    <thead><tr><th>Label</th><th>Probability</th></tr></thead>
    <tbody><tr v-for="r in vm.ranked.value" :key="r.label"><td>{{ r.label }}</td><td>{{ (r.probability * 100).toFixed(1) }}%</td></tr></tbody>
  </table>
  <p v-if="vm.embedding.value" aria-label="Embedding">Embedding: {{ vm.embedding.value.length }} values, length {{ vm.embedding.value.norm.toFixed(3) }}, starting {{ vm.embedding.value.first.map(v => v.toFixed(3)).join(', ') }}</p>
</template>
