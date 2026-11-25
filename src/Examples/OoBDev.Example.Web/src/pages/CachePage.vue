<script setup lang="ts">
import { CacheViewModel } from './CacheViewModel';

const vm = new CacheViewModel();
</script>

<template>
  <h1>Cache lab</h1>
  <p>Store, look up and remove text in the caching provider chosen by configuration (<code>OoBDev:CachingProvider:Type</code>).</p>
  <p><label>Key <input v-model="vm.key.value" size="30" /></label></p>
  <p><label>Text<br /><input v-model="vm.value.value" size="60" /></label></p>
  <p><label>Lives for (seconds) <input v-model.number="vm.seconds.value" type="number" min="1" max="3600" /></label></p>
  <p>
    <fluent-button appearance="primary" :disabled="!vm.store.canExecute.value" @click="vm.store.execute()">Store</fluent-button>
    <fluent-button :disabled="!vm.lookup.canExecute.value" @click="vm.lookup.execute()">Look up</fluent-button>
    <fluent-button :disabled="!vm.flush.canExecute.value" @click="vm.flush.execute()">Remove</fluent-button>
  </p>
  <p v-if="vm.store.error.value || vm.lookup.error.value || vm.flush.error.value" class="error">
    {{ vm.store.error.value ?? vm.lookup.error.value ?? vm.flush.error.value }}
  </p>
  <p v-if="vm.status.value">{{ vm.status.value }}</p>
  <pre v-if="vm.lookupResult.value" class="result">{{ vm.lookupResult.value.found ? `Found: ${vm.lookupResult.value.value}` : 'Not found (never stored, expired or removed)' }}
Provider: {{ vm.lookupResult.value.provider }}</pre>
</template>
