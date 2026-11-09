<script setup lang="ts">
import { WebhooksViewModel } from './WebhooksViewModel';

const vm = new WebhooksViewModel();
</script>

<template>
  <h1>Webhooks</h1>
  <p>Posts a provider call to <code>/webhooks/{channel}</code>. The vendor signature is the credential, so an unsigned call from here is normally rejected (400); an unknown channel gives 404 and a processed call 204.</p>
  <p><label>Channel<br /><input v-model="vm.channel.value" size="30" aria-label="Channel" /></label></p>
  <p><label>Body<br /><textarea v-model="vm.body.value" rows="5" cols="60" aria-label="Body" /></label></p>
  <p><fluent-button appearance="primary" :disabled="!vm.send.canExecute.value" @click="vm.send.execute()">Send webhook</fluent-button></p>
  <p v-if="vm.status.value">Response: <strong class="webhook-status">{{ vm.status.value }}</strong></p>
  <pre class="result">{{ vm.result.value }}</pre>
</template>
