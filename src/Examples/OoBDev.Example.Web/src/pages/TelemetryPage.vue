<script setup lang="ts">
import { TelemetryViewModel, traceUrl } from './TelemetryViewModel';

const vm = new TelemetryViewModel();
vm.refresh.execute();
</script>

<template>
  <h1>Telemetry</h1>
  <p>The API sends traces and logs over OTLP to the Aspire dashboard. Here are its most recent requests; open the dashboard for spans, logs and metrics.</p>
  <p>
    <fluent-button appearance="primary" @click="vm.refresh.execute()">Refresh</fluent-button>
    <a v-if="vm.recent.value" :href="vm.recent.value.dashboardUrl" target="_blank" rel="noopener">Open the dashboard</a>
  </p>
  <p v-if="vm.refresh.error.value" class="error">{{ vm.refresh.error.value }}</p>
  <p v-if="vm.recent.value && vm.recent.value.traces.length === 0">No traces yet. Use another screen, then refresh.</p>
  <table v-if="vm.recent.value && vm.recent.value.traces.length" aria-label="Recent traces">
    <thead><tr><th>Started</th><th>Span</th><th>Duration (ms)</th><th>Trace</th></tr></thead>
    <tbody>
      <tr v-for="t in vm.recent.value.traces" :key="t.traceId + t.started">
        <td>{{ new Date(t.started).toLocaleTimeString() }}</td>
        <td>{{ t.name }}</td>
        <td>{{ t.durationMs.toFixed(1) }}</td>
        <td><a :href="traceUrl(vm.recent.value.dashboardUrl, t.traceId)" target="_blank" rel="noopener">{{ t.traceId.slice(0, 8) }}</a></td>
      </tr>
    </tbody>
  </table>
</template>
