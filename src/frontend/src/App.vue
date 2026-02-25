<script setup lang="ts">
import RegisterComponent from './components/RegisterComponent.vue'
import { RouterLink, RouterView } from 'vue-router'
</script>

<template>
  <nav>
    <RouterLink to="/register" class="link"> Register </RouterLink>
    <RouterLink to="/login" class="link"> Log in </RouterLink>
    <RouterLink to="/list" class="link"> Contact list </RouterLink>
  </nav>
  <main>
    <RouterView/>
  </main>
</template>