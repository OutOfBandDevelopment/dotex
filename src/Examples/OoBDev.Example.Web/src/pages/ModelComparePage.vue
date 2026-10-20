<script setup lang="ts">
import { ModelCompareViewModel, modelKeys } from './ModelCompareViewModel';

const vm = new ModelCompareViewModel();
</script>

<template>
  <h1>Model compare</h1>
  <p>Score the same two texts with different in-process sentence models. Each model downloads the first time it is used, so tick one or two to start.</p>
  <p><label>Text A <textarea v-model="vm.a.value" rows="2" cols="50"></textarea></label></p>
  <p><label>Text B <textarea v-model="vm.b.value" rows="2" cols="50"></textarea></label></p>
  <fieldset>
    <legend>Models</legend>
    <label v-for="m in modelKeys" :key="m"><input type="checkbox" :value="m" v-model="vm.selected.value" /> {{ m }} </label>
  </fieldset>
  <p><fluent-button appearance="primary" :disabled="!vm.compare.canExecute.value" @click="vm.compare.execute()">Score</fluent-button></p>
  <p v-if="vm.compare.error.value" class="error">{{ vm.compare.error.value }}</p>
  <table v-if="vm.scores.value.length" aria-label="Model scores">
    <thead><tr><th>Model</th><th>Values</th><th>Similarity</th><th>Time (ms)</th></tr></thead>
    <tbody>
      <tr v-for="s in vm.scores.value" :key="s.model">
        <td>{{ s.model }}</td><td>{{ s.length }}</td><td>{{ s.similarity.toFixed(3) }}</td><td>{{ s.milliseconds.toFixed(0) }}</td>
      </tr>
    </tbody>
  </table>
</template>
