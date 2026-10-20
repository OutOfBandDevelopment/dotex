<script setup lang="ts">
import { TextToolsViewModel, formats } from './TextToolsViewModel';

const vm = new TextToolsViewModel();
</script>

<template>
  <h1>Text tools</h1>
  <h2>Query a document with XPath</h2>
  <p>JSON, YAML and HTML are exposed as XML, so one query language works on all of them.</p>
  <p>
    <label>Format <select v-model="vm.format.value" @change="vm.useSample()"><option v-for="f in formats" :key="f">{{ f }}</option></select></label>
  </p>
  <p><label>Document <textarea v-model="vm.content.value" rows="6" cols="70"></textarea></label></p>
  <p><label>XPath <input v-model="vm.xpath.value" size="40" /></label></p>
  <p><fluent-button appearance="primary" :disabled="!vm.query.canExecute.value" @click="vm.query.execute()">Run query</fluent-button></p>
  <p v-if="vm.query.error.value" class="error">{{ vm.query.error.value }}</p>
  <div v-if="vm.result.value">
    <p aria-label="Match count">{{ vm.result.value.count }} match(es)</p>
    <ul aria-label="Matches"><li v-for="(m, i) in vm.result.value.matches" :key="i"><code>{{ m }}</code></li></ul>
    <details><summary>Document as XML</summary><pre class="result">{{ vm.result.value.xml }}</pre></details>
  </div>

  <h2>Render Markdown</h2>
  <p><label>Markdown <textarea v-model="vm.markdown.value" rows="6" cols="70"></textarea></label></p>
  <p><fluent-button appearance="primary" :disabled="!vm.render.canExecute.value" @click="vm.render.execute()">Render</fluent-button></p>
  <p v-if="vm.render.error.value" class="error">{{ vm.render.error.value }}</p>
  <div v-if="vm.html.value" class="preview" aria-label="Rendered Markdown" v-html="vm.safeHtml.value"></div>
</template>
