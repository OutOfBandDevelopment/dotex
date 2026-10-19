<script setup lang="ts">
import { CommunicationsViewModel } from './CommunicationsViewModel';

const vm = new CommunicationsViewModel();
</script>

<template>
  <h1>Communications</h1>
  <p>
    Channel:
    <label><input type="radio" value="public" v-model="vm.channel.value" /> Public</label>
    <label><input type="radio" value="queued" v-model="vm.channel.value" /> Queued</label>
    <label><input type="radio" value="private" v-model="vm.channel.value" /> Private (requires sign in)</label>
  </p>
  <p><label>To<br /><input v-model="vm.to.value" size="50" /></label></p>
  <p><label>Subject<br /><input v-model="vm.subject.value" size="50" /></label></p>
  <p><label>Body<br /><textarea v-model="vm.body.value" rows="4" cols="60" /></label></p>
  <p><fluent-button appearance="primary" :disabled="!vm.send.canExecute.value" @click="vm.send.execute()">Send</fluent-button></p>
  <p v-if="vm.send.error.value" class="error">{{ vm.send.error.value }}</p>
  <pre class="result">{{ vm.result.value }}</pre>
</template>
