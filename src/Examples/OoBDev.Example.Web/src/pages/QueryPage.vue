<script setup lang="ts">
import { QueryViewModel } from './QueryViewModel';

const vm = new QueryViewModel();
</script>

<template>
  <h1>Query</h1>
  <p>Queries a small sample catalog with the OData system query options (<code>$filter</code>, <code>$orderby</code>, <code>$top</code>, <code>$count</code>) through <code>OoBDev.AspNetCore.OData</code>. The server enforces the cost limits and answers a bad query with a problem. Needs the right <code>catalog.read</code>.</p>
  <p><label>Filter <input v-model="vm.filter.value" aria-label="Filter" size="48" /></label></p>
  <p>
    <label>Order by <input v-model="vm.orderBy.value" aria-label="Order by" /></label>
    <label>Top <input v-model="vm.top.value" aria-label="Top" size="4" /></label>
  </p>
  <p><fluent-button appearance="primary" :disabled="!vm.run.canExecute.value" @click="vm.run.execute()">Run query</fluent-button></p>
  <p v-if="vm.run.error.value" class="error">{{ vm.run.error.value }}</p>
  <p v-if="vm.total.value !== null">{{ vm.total.value }} rows match.</p>
  <table v-if="vm.rows.value" class="grid" aria-label="Rows">
    <thead><tr><th>Id</th><th>Name</th><th>Category</th><th>Price</th><th>In stock</th></tr></thead>
    <tbody>
      <tr v-for="row in vm.rows.value" :key="String(row.Id ?? row.id)">
        <td>{{ row.Id ?? row.id }}</td><td>{{ row.Name ?? row.name }}</td><td>{{ row.Category ?? row.category }}</td><td>{{ row.Price ?? row.price }}</td><td>{{ (row.InStock ?? row.inStock) ? 'yes' : 'no' }}</td>
      </tr>
    </tbody>
  </table>
</template>
