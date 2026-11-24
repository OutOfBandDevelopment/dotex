<script setup lang="ts">
import { GeocodeViewModel } from './GeocodeViewModel';

const vm = new GeocodeViewModel();
</script>

<template>
  <h1>Address lookup</h1>
  <p>Finds the coordinates of a US address with <code>OoBDev.Census.Geocoding</code> (<code>ILocationServices</code>). The address you type is sent from the server to the public US Census geocoder, so do not enter a real private address. Needs the right <code>geocode.use</code>.</p>
  <p>
    <label>Street <input v-model="vm.line1.value" aria-label="Street" /></label>
    <label>City <input v-model="vm.city.value" aria-label="City" /></label>
    <label>State <input v-model="vm.region.value" aria-label="State" /></label>
    <label>ZIP <input v-model="vm.postalCode.value" aria-label="ZIP" /></label>
  </p>
  <p><fluent-button appearance="primary" :disabled="!vm.lookup.canExecute.value" @click="vm.lookup.execute()">Look up</fluent-button></p>
  <p v-if="vm.lookup.error.value" class="error">{{ vm.lookup.error.value }}</p>
  <template v-if="vm.result.value">
    <p v-if="!vm.result.value.matched">No match for that address.</p>
    <table v-else class="grid" aria-label="Result">
      <tbody>
        <tr><th>Matched address</th><td>{{ vm.result.value.normalizedAddress }}</td></tr>
        <tr><th>Latitude</th><td>{{ vm.result.value.latitude }}</td></tr>
        <tr><th>Longitude</th><td>{{ vm.result.value.longitude }}</td></tr>
        <tr><th>Quality</th><td>{{ vm.result.value.quality }}</td></tr>
      </tbody>
    </table>
  </template>
</template>
