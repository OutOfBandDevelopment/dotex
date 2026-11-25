<script setup lang="ts">
import CompletionConsole from './CompletionConsole.vue';
import { OllamaEmbedViewModel } from './CompletionViewModel';

const embed = new OllamaEmbedViewModel();
</script>

<template>
  <CompletionConsole title="Ollama" base-path="/api/Ollama" note="Local models served by Ollama (phi3 is baked into the test image)." />
  <h2>Embedding</h2>
  <p>
    <label>Text <input v-model="embed.text.value" size="50" /></label>
    <label>Model <input v-model="embed.model.value" size="20" /></label>
    <fluent-button :disabled="!embed.embed.canExecute.value" @click="embed.embed.execute()">Embed</fluent-button>
  </p>
  <p v-if="embed.embed.error.value" class="error">{{ embed.embed.error.value }}</p>
  <pre v-if="embed.vector.value.length" class="result">{{ embed.vector.value.length }} dimensions: {{ embed.vector.value.slice(0, 8).join(', ') }} ...</pre>
</template>
