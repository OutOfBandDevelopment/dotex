<script setup lang="ts">
import { onMounted } from 'vue';
import { CgiViewModel } from './CgiViewModel';

const vm = new CgiViewModel();
onMounted(() => vm.load.execute());
</script>

<template>
  <h1>CGI programs</h1>
  <p>Programs in the CGI script root that this host can start, served by <code>OoBDev.AspNetCore.Cgi</code>. Their OpenAPI descriptions are in the <a href="/scalar" target="_blank" rel="noopener">API reference</a>.</p>
  <p v-if="vm.load.error.value" class="error">{{ vm.load.error.value }}</p>
  <p>
    <label>Program<br />
      <select v-model="vm.selected.value" aria-label="Program">
        <option v-for="p in vm.programs.value" :key="p.path" :value="p.path">{{ p.path }}</option>
      </select>
    </label>
  </p>
  <p><label>Name<br /><input v-model="vm.name.value" size="30" /></label></p>
  <p>
    <fluent-button appearance="primary" :disabled="!vm.open.canExecute.value" @click="vm.open.execute()">Get</fluent-button>
    <fluent-button :disabled="!vm.post.canExecute.value" @click="vm.post.execute()">Post name</fluent-button>
  </p>
  <p v-if="vm.status.value">Response: <strong class="cgi-status">{{ vm.status.value }}</strong></p>
  <pre class="result">{{ vm.body.value }}</pre>
</template>
