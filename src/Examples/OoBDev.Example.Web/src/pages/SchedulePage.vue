<script setup lang="ts">
import { onMounted, onUnmounted } from 'vue';
import { ScheduleViewModel } from './ScheduleViewModel';

const vm = new ScheduleViewModel();
let timer: ReturnType<typeof setInterval> | undefined;
onMounted(() => { vm.refresh.execute(); timer = setInterval(() => vm.refresh.execute(), 3000); });
onUnmounted(() => clearInterval(timer));
</script>

<template>
  <h1>Schedule</h1>
  <p>Schedules a message with <code>OoBDev.ComplexEvents</code>. A hosted runner polls the schedule store and handles each item when it is due; scheduling the same key again moves the due time. Needs the right <code>schedule.use</code>.</p>
  <p>
    <label>Key <input v-model="vm.key.value" aria-label="Key" /></label>
    <label>Due in (seconds) <input v-model.number="vm.inSeconds.value" type="number" min="0" max="3600" aria-label="Due in seconds" /></label>
    <label>Message <input v-model="vm.message.value" aria-label="Message" /></label>
  </p>
  <p><fluent-button appearance="primary" :disabled="!vm.add.canExecute.value" @click="vm.add.execute()">Schedule</fluent-button></p>
  <p v-if="vm.add.error.value" class="error">{{ vm.add.error.value }}</p>
  <h2>Pending</h2>
  <p v-if="vm.state.value.pending.length === 0">Nothing is waiting.</p>
  <table v-else class="grid" aria-label="Pending">
    <thead><tr><th>Key</th><th>Due</th><th>Message</th><th></th></tr></thead>
    <tbody><tr v-for="p in vm.state.value.pending" :key="p.key"><td>{{ p.key }}</td><td>{{ p.dueAt }}</td><td>{{ p.message }}</td><td><fluent-button @click="vm.release.execute(p.key)">Remove</fluent-button></td></tr></tbody>
  </table>
  <h2>Fired</h2>
  <p v-if="vm.state.value.fired.length === 0">Nothing has fired yet.</p>
  <table v-else class="grid" aria-label="Fired">
    <thead><tr><th>Key</th><th>Was due</th><th>Fired</th><th>Message</th></tr></thead>
    <tbody><tr v-for="(f, i) in vm.state.value.fired" :key="i"><td>{{ f.key }}</td><td>{{ f.dueAt }}</td><td>{{ f.firedAt }}</td><td>{{ f.message }}</td></tr></tbody>
  </table>
</template>
