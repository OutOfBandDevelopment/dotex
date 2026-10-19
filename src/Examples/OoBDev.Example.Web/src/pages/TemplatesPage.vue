<script setup lang="ts">
import { onMounted } from 'vue';
import { TemplatesViewModel } from './TemplatesViewModel';

const vm = new TemplatesViewModel();
onMounted(() => vm.load.execute());
</script>

<template>
  <h1>Templates</h1>
  <p><label>Template name<br /><input v-model="vm.name.value" size="40" /></label></p>
  <p><label>Data (JSON)<br /><textarea v-model="vm.data.value" rows="4" cols="60" /></label></p>
  <p><fluent-button appearance="primary" :disabled="!vm.apply.canExecute.value" @click="vm.apply.execute()">Apply</fluent-button></p>
  <p v-if="vm.apply.error.value" class="error">{{ vm.apply.error.value }}</p>
  <h2>Output</h2>
  <pre class="result">{{ vm.output.value }}</pre>
  <h2>Supported template types</h2>
  <pre class="result">{{ vm.supported.value }}</pre>
</template>
