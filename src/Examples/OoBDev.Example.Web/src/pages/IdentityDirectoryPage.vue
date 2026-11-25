<script setup lang="ts">
import { onMounted } from 'vue';
import { IdentityDirectoryViewModel } from './IdentityDirectoryViewModel';

const vm = new IdentityDirectoryViewModel();
onMounted(() => vm.load.execute());
</script>

<template>
  <h1>Users and roles</h1>
  <p>A read-only view of the Keycloak realm. Roles come from the directory; the rights column shows what the role translation grants each user. Accounts are changed in Keycloak, not here.</p>
  <p v-if="vm.load.error.value" class="error">{{ vm.load.error.value }}</p>
  <p><label>Search<br /><input v-model="vm.search.value" size="30" aria-label="Search" @keyup.enter="vm.load.execute()" /></label></p>
  <p><fluent-button appearance="primary" :disabled="!vm.load.canExecute.value" @click="vm.load.execute()">Search</fluent-button></p>
  <h2>Users</h2>
  <table class="identity-users">
    <thead><tr><th>User</th><th>Name</th><th>Email</th><th>Enabled</th><th>Roles</th><th>Rights</th></tr></thead>
    <tbody>
      <tr v-for="u in vm.users.value" :key="u.id">
        <td>{{ u.userName }}</td><td>{{ u.displayName }}</td><td>{{ u.email }}</td><td>{{ u.enabled ? 'yes' : 'no' }}</td>
        <td>{{ u.roles.join(', ') }}</td><td>{{ u.rights.join(', ') }}</td>
      </tr>
    </tbody>
  </table>
  <h2>Roles</h2>
  <table class="identity-roles">
    <thead><tr><th>Role</th><th>Rights granted</th></tr></thead>
    <tbody>
      <tr v-for="r in vm.roles.value" :key="r.name"><td>{{ r.name }}</td><td>{{ vm.rightsOf(r) }}</td></tr>
    </tbody>
  </table>
</template>
