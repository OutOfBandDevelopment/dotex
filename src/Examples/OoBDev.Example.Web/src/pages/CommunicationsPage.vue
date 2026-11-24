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

  <h2>Inbox</h2>
  <p>Fetches unread mail over IMAP (requires the <code>communications.receive</code> right); fetched mail is marked seen.</p>
  <p><fluent-button appearance="primary" :disabled="!vm.poll.canExecute.value" @click="vm.poll.execute()">Check mail</fluent-button></p>
  <p v-if="vm.poll.error.value" class="error">{{ vm.poll.error.value }}</p>
  <p v-else-if="vm.polled.value && vm.inbox.value.length === 0">No unread mail.</p>
  <table v-if="vm.inbox.value.length" class="inbox">
    <thead><tr><th>From</th><th>Subject</th><th>Preview</th></tr></thead>
    <tbody><tr v-for="m in vm.inbox.value" :key="m.messageId"><td>{{ m.from }}</td><td>{{ m.subject }}</td><td>{{ m.preview }}</td></tr></tbody>
  </table>
</template>
