<script setup lang="ts">
import { DistanceViewModel } from './DistanceViewModel';

const vm = new DistanceViewModel();
</script>

<template>
  <h1>Distance</h1>
  <p>Measures the great-circle distance and initial bearing between two coordinates with <code>OoBDev.SpatialServices</code> (<code>GeoMath</code>). The calculation runs on the server and nothing is sent elsewhere. Needs the right <code>geocode.use</code>.</p>
  <p>
    <label>From latitude <input v-model="vm.fromLatitude.value" aria-label="From latitude" /></label>
    <label>From longitude <input v-model="vm.fromLongitude.value" aria-label="From longitude" /></label>
  </p>
  <p>
    <label>To latitude <input v-model="vm.toLatitude.value" aria-label="To latitude" /></label>
    <label>To longitude <input v-model="vm.toLongitude.value" aria-label="To longitude" /></label>
  </p>
  <p><fluent-button appearance="primary" :disabled="!vm.measure.canExecute.value" @click="vm.measure.execute()">Measure</fluent-button></p>
  <p v-if="vm.measure.error.value" class="error">{{ vm.measure.error.value }}</p>
  <table v-if="vm.result.value" class="grid" aria-label="Result">
    <tbody>
      <tr><th>Kilometres</th><td>{{ vm.result.value.kilometers.toFixed(1) }}</td></tr>
      <tr><th>Miles</th><td>{{ vm.result.value.miles.toFixed(1) }}</td></tr>
      <tr><th>Bearing</th><td>{{ vm.result.value.bearingDegrees.toFixed(1) }}&deg;</td></tr>
    </tbody>
  </table>
</template>
