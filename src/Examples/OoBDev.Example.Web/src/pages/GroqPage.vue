<script setup lang="ts">
import CompletionConsole from './CompletionConsole.vue';
</script>

<template>
  <CompletionConsole title="Groq cloud" base-path="/api/GroqCloud" note="Needs a Groq API key configured for the API; without one the call returns an error." />
</template>
