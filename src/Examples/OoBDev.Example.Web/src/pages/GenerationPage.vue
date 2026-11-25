<script setup lang="ts">
import { GenerationViewModel } from './GenerationViewModel';

const vm = new GenerationViewModel();
</script>

<template>
  <h1>Test data generation</h1>
  <p>Builds a sample object graph with <code>OoBDev.Generations</code>. The same sample type and seed always give the same graph, and nothing is real data. Needs the right <code>generation.use</code>.</p>
  <p>
    <label>Sample type
      <select v-model="vm.sampleType.value" aria-label="Sample type">
        <option v-for="t in vm.sampleTypes" :key="t" :value="t">{{ t }}</option>
      </select>
    </label>
    <label>Seed <input v-model="vm.seed.value" aria-label="Seed" /></label>
  </p>
  <p><fluent-button appearance="primary" :disabled="!vm.generate.canExecute.value" @click="vm.generate.execute()">Generate</fluent-button></p>
  <p v-if="vm.generate.error.value" class="error">{{ vm.generate.error.value }}</p>
  <pre v-if="vm.result.value" aria-label="Result">{{ JSON.stringify(vm.result.value.value, null, 2) }}</pre>
</template>
