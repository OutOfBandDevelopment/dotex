<script setup lang="ts">
defineProps<{ title: string; href: string }>();
</script>

<template>
  <h1>{{ title }}</h1>
  <p><a :href="href" target="_blank" rel="noopener">Open {{ title }}</a> (served by the API host).</p>
</template>
