<script setup lang="ts">
import { onMounted } from 'vue';
import { KernelPlugInsViewModel } from './KernelPlugInsViewModel';

const vm = new KernelPlugInsViewModel();
onMounted(() => vm.load.execute());
</script>

<template>
  <h1>Kernel plug-ins</h1>
  <p>The Semantic Kernel plug-in functions registered with the framework. Invoke one directly with text arguments; no model is involved.</p>
  <p v-if="vm.load.error.value" class="error">{{ vm.load.error.value }}</p>
  <table class="grid" aria-label="Plug-in functions">
    <thead><tr><th>Plug-in</th><th>Function</th><th>Description</th><th></th></tr></thead>
    <tbody>
      <tr v-for="f in vm.functions.value" :key="f.plugIn + '.' + f.name">
        <td>{{ f.plugIn }}</td>
        <td><code>{{ f.name }}</code></td>
        <td>{{ f.description }}</td>
        <td><fluent-button @click="vm.select(f)">Select</fluent-button></td>
      </tr>
    </tbody>
  </table>
  <section v-if="vm.selected.value">
    <h2>{{ vm.selected.value.plugIn }}.{{ vm.selected.value.name }}</h2>
    <p v-for="(description, name) in vm.selected.value.parameters" :key="name">
      <label>{{ name }} <input v-model="vm.arguments.value[name]" size="30" :title="description ?? ''" /></label>
    </p>
    <p><fluent-button appearance="primary" @click="vm.invoke.execute()">Invoke</fluent-button></p>
    <p v-if="vm.invoke.error.value" class="error">{{ vm.invoke.error.value }}</p>
    <p v-if="vm.result.value" aria-label="Result"><code>{{ vm.result.value }}</code></p>
  </section>
</template>
