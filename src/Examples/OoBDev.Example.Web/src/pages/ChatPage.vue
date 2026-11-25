<script setup lang="ts">
import { ChatViewModel } from './ChatViewModel';

const vm = new ChatViewModel();
</script>

<template>
  <h1>AI chat</h1>
  <p><label>Instructions<br /><input v-model="vm.promptDetails.value" size="60" /></label></p>
  <p><label>Context data<br /><textarea v-model="vm.data.value" rows="3" cols="60" /></label></p>
  <p><label>Prompt<br /><textarea v-model="vm.userInput.value" rows="3" cols="60" /></label></p>
  <p>
    <fluent-button appearance="primary" :disabled="!vm.send.canExecute.value" @click="vm.send.execute()">Send</fluent-button>
    <fluent-button :disabled="!vm.stream.canExecute.value" @click="vm.stream.execute()">Stream</fluent-button>
    <fluent-button :disabled="!vm.stream.running.value" @click="vm.stream.cancel()">Cancel</fluent-button>
    <fluent-button @click="vm.clear.execute()">Clear</fluent-button>
  </p>
  <p v-if="vm.error.value" class="error">{{ vm.error.value }}</p>
  <pre class="result">{{ vm.response.value }}</pre>
</template>
