<script setup lang="ts">
import { VectorLabViewModel, metrics } from './VectorLabViewModel';

const vm = new VectorLabViewModel();
</script>

<template>
  <h1>Vector lab</h1>
  <p>Compare a set of vectors: every pair is measured with the chosen metric, and the lengths and the centroid are computed.</p>
  <p><label>Vectors (one per line) <textarea v-model="vm.rows.value" rows="5" cols="50"></textarea></label></p>
  <p><label>Metric <select v-model="vm.metric.value"><option v-for="m in metrics" :key="m">{{ m }}</option></select></label></p>
  <p><fluent-button appearance="primary" :disabled="!vm.compute.canExecute.value" @click="vm.compute.execute()">Compare</fluent-button></p>
  <p v-if="vm.compute.error.value" class="error">{{ vm.compute.error.value }}</p>
  <div v-if="vm.result.value">
    <table aria-label="Distance matrix">
      <tbody>
        <tr v-for="(row, i) in vm.result.value.matrix" :key="i">
          <th scope="row">v{{ i + 1 }}</th>
          <td v-for="(d, j) in row" :key="j">{{ d.toFixed(3) }}</td>
        </tr>
      </tbody>
    </table>
    <p aria-label="Lengths">Lengths: {{ vm.result.value.magnitudes.map(m => m.toFixed(3)).join(', ') }}</p>
    <p aria-label="Centroid">Centroid: {{ vm.result.value.centroid.map(c => c.toFixed(3)).join(', ') }}</p>
  </div>
</template>
