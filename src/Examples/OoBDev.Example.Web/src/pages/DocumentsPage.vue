<script setup lang="ts">
import { onMounted } from 'vue';
import { DocumentsViewModel, targetTypes } from './DocumentsViewModel';

const vm = new DocumentsViewModel();
onMounted(() => vm.refresh.execute());
const onFile = (e: Event) => { vm.file.value = (e.target as HTMLInputElement).files?.[0] ?? null; };
</script>

<template>
  <h1>Documents</h1>
  <p><label>File<br /><input type="file" @change="onFile" /></label></p>
  <h2>Convert</h2>
  <p>
    <label>To <select v-model="vm.targetType.value"><option v-for="t in targetTypes" :key="t">{{ t }}</option></select></label>
    <fluent-button appearance="primary" :disabled="!vm.convert.canExecute.value" @click="vm.convert.execute()">Convert</fluent-button>
  </p>
  <h2>Stored documents</h2>
  <p><fluent-button :disabled="!vm.refresh.canExecute.value" @click="vm.refresh.execute()">Refresh</fluent-button></p>
  <p v-if="vm.refresh.error.value" class="error">{{ vm.refresh.error.value }}</p>
  <p><a href="/Document/Package" data-package>Download all as ZIP</a></p>
  <ul class="stored-list">
    <li v-for="d in vm.stored.value" :key="d.fileName">
      <fluent-button :appearance="vm.storeName.value === d.fileName ? 'primary' : 'neutral'" @click="vm.pick.execute(d)">{{ d.fileName }}</fluent-button> {{ d.contentType }}
    </li>
  </ul>
  <h2>Store</h2>
  <p>
    <label>Name in store <input v-model="vm.storeName.value" size="40" /></label>
    <fluent-button :disabled="!vm.upload.canExecute.value" @click="vm.upload.execute()">Upload</fluent-button>
    <fluent-button :disabled="!vm.view.canExecute.value" @click="vm.view.execute()">View as text</fluent-button>
    <fluent-button :disabled="!vm.viewHtml.canExecute.value" @click="vm.viewHtml.execute()">View as HTML</fluent-button>
    <fluent-button :disabled="!vm.viewSummary.canExecute.value" @click="vm.viewSummary.execute()">Summarize</fluent-button>
    <a v-if="vm.storeName.value" :href="`/Document/Download/${vm.storeName.value}`">Download</a>
    <a v-if="vm.storeName.value" :href="`/Document/Pdf/${vm.storeName.value}`" target="_blank" rel="noopener">PDF</a>
  </p>
  <p v-if="vm.convert.error.value || vm.upload.error.value || vm.view.error.value || vm.viewHtml.error.value || vm.viewSummary.error.value" class="error">
    {{ vm.convert.error.value ?? vm.upload.error.value ?? vm.view.error.value ?? vm.viewHtml.error.value ?? vm.viewSummary.error.value }}
  </p>
  <pre class="result">{{ vm.output.value }}</pre>
</template>
