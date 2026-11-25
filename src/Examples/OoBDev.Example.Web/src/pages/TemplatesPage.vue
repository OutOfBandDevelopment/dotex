<script setup lang="ts">
import { onMounted } from 'vue';
import { TemplatesViewModel } from './TemplatesViewModel';

const vm = new TemplatesViewModel();
onMounted(() => {
  vm.load.execute();
  vm.list.execute();
});
</script>

<template>
  <h1>Templates</h1>
  <h2>Template files</h2>
  <ul class="template-list">
    <li v-for="t in vm.templates.value" :key="t.file">
      <fluent-button :appearance="vm.selected.value?.file === t.file ? 'primary' : 'neutral'" @click="vm.open.execute(t)">{{ t.file }}</fluent-button>
    </li>
  </ul>
  <p v-if="vm.open.error.value" class="error">{{ vm.open.error.value }}</p>

  <h2>Source</h2>
  <p><textarea v-model="vm.text.value" rows="10" cols="80" aria-label="Template source" /></p>
  <p><label>Data (JSON)<br /><textarea v-model="vm.data.value" rows="4" cols="60" /></label></p>
  <p>
    <fluent-button appearance="primary" :disabled="!vm.preview.canExecute.value" @click="vm.preview.execute()">Preview draft</fluent-button>
    <fluent-button :disabled="!vm.save.canExecute.value" @click="vm.save.execute()">Save to file</fluent-button>
    <span class="hint"> Rendering a draft changes nothing; Save writes the text over the selected template file.</span>
  </p>
  <p v-if="vm.preview.error.value" class="error">{{ vm.preview.error.value }}</p>
  <p v-if="vm.save.error.value" class="error">{{ vm.save.error.value }}</p>

  <h2>Apply a named template</h2>
  <p><label>Template name<br /><input v-model="vm.name.value" size="40" /></label></p>
  <p><fluent-button :disabled="!vm.apply.canExecute.value" @click="vm.apply.execute()">Apply</fluent-button></p>
  <p v-if="vm.apply.error.value" class="error">{{ vm.apply.error.value }}</p>

  <h2>Output</h2>
  <pre class="result">{{ vm.output.value }}</pre>
  <h2>Supported template types</h2>
  <pre class="result">{{ vm.supported.value }}</pre>
</template>
