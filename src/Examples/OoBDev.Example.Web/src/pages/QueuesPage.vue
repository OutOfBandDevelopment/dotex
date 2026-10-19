<script setup lang="ts">
import { QueuesViewModel } from './QueuesViewModel';

const vm = new QueuesViewModel();
</script>

<template>
  <h1>Queues</h1>
  <p>
    Queue:
    <label><input type="radio" value="public" v-model="vm.queue.value" /> Public</label>
    <label><input type="radio" value="private" v-model="vm.queue.value" /> Private (requires sign in)</label>
  </p>
  <p><label>Input<br /><input v-model="vm.input.value" size="50" /></label></p>
  <p><label>Data (JSON)<br /><textarea v-model="vm.data.value" rows="4" cols="60" /></label></p>
  <p>
    <fluent-button appearance="primary" :disabled="!vm.enqueue.canExecute.value" @click="vm.enqueue.execute()">Enqueue</fluent-button>
    <a href="/asyncapi" target="_blank" rel="noopener">Open event reference</a>
  </p>
  <p v-if="vm.enqueue.error.value" class="error">{{ vm.enqueue.error.value }}</p>
  <pre class="result">{{ vm.result.value }}</pre>
</template>
