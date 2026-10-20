<script setup lang="ts">
import { ToolboxViewModel, hashAlgorithms, hmacAlgorithms } from './ToolboxViewModel';

const vm = new ToolboxViewModel();
</script>

<template>
  <h1>Toolbox</h1>
  <h2>Hash</h2>
  <p><label>Algorithm <select v-model="vm.hashAlgorithm.value"><option v-for="a in hashAlgorithms" :key="a">{{ a }}</option></select></label></p>
  <p><label>Text <input v-model="vm.hashText.value" size="50" /></label></p>
  <p><fluent-button appearance="primary" :disabled="!vm.hash.canExecute.value" @click="vm.hash.execute()">Hash</fluent-button></p>
  <p v-if="vm.hash.error.value" class="error">{{ vm.hash.error.value }}</p>
  <pre class="result" aria-label="Hash result">{{ vm.hashResult.value }}</pre>

  <h2>Sign a message (HMAC)</h2>
  <p><label>Algorithm <select v-model="vm.hmacAlgorithm.value"><option v-for="a in hmacAlgorithms" :key="a">{{ a }}</option></select></label></p>
  <p><label>Secret <input v-model="vm.secret.value" size="30" /></label></p>
  <p><label>Message <input v-model="vm.message.value" size="50" /></label></p>
  <p><fluent-button appearance="primary" :disabled="!vm.hmac.canExecute.value" @click="vm.hmac.execute()">Sign</fluent-button></p>
  <p v-if="vm.hmac.error.value" class="error">{{ vm.hmac.error.value }}</p>
  <pre class="result" aria-label="Signature">{{ vm.hmacResult.value }}</pre>

  <h2>Vector distance</h2>
  <p>Numbers separated by commas; both vectors need the same length.</p>
  <p><label>Vector A <input v-model="vm.vectorA.value" size="40" /></label></p>
  <p><label>Vector B <input v-model="vm.vectorB.value" size="40" /></label></p>
  <p><fluent-button appearance="primary" :disabled="!vm.distance.canExecute.value" @click="vm.distance.execute()">Compare</fluent-button></p>
  <p v-if="vm.distance.error.value" class="error">{{ vm.distance.error.value }}</p>
  <table v-if="vm.distances.value" class="grid" aria-label="Distances">
    <tbody>
      <tr><th>Cosine distance</th><td>{{ vm.distances.value.cosine.toFixed(6) }}</td></tr>
      <tr><th>Euclidean</th><td>{{ vm.distances.value.euclidean.toFixed(6) }}</td></tr>
      <tr><th>Dot product</th><td>{{ vm.distances.value.dotProduct.toFixed(6) }}</td></tr>
      <tr><th>Manhattan</th><td>{{ vm.distances.value.manhattan.toFixed(6) }}</td></tr>
    </tbody>
  </table>
</template>
