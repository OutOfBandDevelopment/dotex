<script setup lang="ts">
import { VectorStoreViewModel } from './VectorStoreViewModel';

const vm = new VectorStoreViewModel();
</script>

<template>
  <h1>Vector store</h1>
  <p>Texts are embedded in process and stored in a collection; a query finds the closest by cosine distance. The store is SQL Server when a connection string is configured, otherwise memory.</p>
  <p><label>Collection <input v-model="vm.collection.value" size="20" /></label></p>
  <p><label>Texts (one per line) <textarea v-model="vm.texts.value" rows="5" cols="70"></textarea></label></p>
  <p>
    <fluent-button appearance="primary" :disabled="!vm.index.canExecute.value" @click="vm.index.execute()">Store texts</fluent-button>
    <fluent-button :disabled="!vm.list.canExecute.value" @click="vm.list.execute()">List collection</fluent-button>
  </p>
  <p v-if="vm.index.error.value" class="error">{{ vm.index.error.value }}</p>
  <p v-if="vm.stored.value.length" aria-label="Stored">Stored {{ vm.stored.value.length }} text(s) in the {{ vm.store.value }} store.</p>

  <h2>Find the closest</h2>
  <p><label>Query <input v-model="vm.query.value" size="50" /></label></p>
  <p><fluent-button appearance="primary" :disabled="!vm.search.canExecute.value" @click="vm.search.execute()">Search</fluent-button></p>
  <p v-if="vm.search.error.value" class="error">{{ vm.search.error.value }}</p>
  <p v-if="vm.list.error.value" class="error">{{ vm.list.error.value }}</p>
  <ol v-if="vm.hits.value.length" aria-label="Hits">
    <li v-for="h in vm.hits.value" :key="h.id">{{ h.text }} <small>(id {{ h.id }}, distance {{ h.score.toFixed(3) }})</small></li>
  </ol>
</template>
