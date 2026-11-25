<script setup lang="ts">
import { onMounted } from 'vue';
import { QueuesViewModel } from './QueuesViewModel';

const vm = new QueuesViewModel();
onMounted(() => {
  vm.refreshChannels.execute();
  vm.refreshReceived.execute();
});
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
    <a href="/asyncapi/all" target="_blank" rel="noopener">Open event reference</a>
  </p>
  <p v-if="vm.enqueue.error.value" class="error">{{ vm.enqueue.error.value }}</p>
  <pre class="result">{{ vm.result.value }}</pre>
  <h2>Configured channels</h2>
  <table class="channels">
    <thead><tr><th>Channel</th><th>Provider</th><th>Queue</th><th>Host</th><th>Receiver</th></tr></thead>
    <tbody>
      <tr v-for="c in vm.channels.value" :key="c.channel">
        <td>{{ c.channel }}</td><td>{{ c.provider }}</td><td>{{ c.queueName }}</td><td>{{ c.hostName }}</td><td>{{ c.receiverEnabled ? 'on' : 'off' }}</td>
      </tr>
    </tbody>
  </table>
  <p v-if="vm.refreshChannels.error.value" class="error">{{ vm.refreshChannels.error.value }}</p>
  <h2>Received messages</h2>
  <p><fluent-button :disabled="!vm.refreshReceived.canExecute.value" @click="vm.refreshReceived.execute()">Refresh</fluent-button> Newest first, the last 50 this host received (empty while the receiver is off).</p>
  <p v-if="vm.refreshReceived.error.value" class="error">{{ vm.refreshReceived.error.value }}</p>
  <ul class="received-list">
    <li v-for="(m, i) in vm.received.value" :key="i"><code>{{ m.receivedAt }}</code> {{ m.correlationId }} {{ m.message }}</li>
  </ul>
</template>
