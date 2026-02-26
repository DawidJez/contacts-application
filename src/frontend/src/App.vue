<script setup lang="ts">
import { RouterLink, RouterView, useRouter } from 'vue-router'
import { ref, computed, onMounted, onBeforeUnmount } from 'vue'

const router = useRouter();
const token = ref<string | null>(localStorage.getItem("token"));
const isAuth = computed(() => !!token.value);

function syncToken() {
  token.value = localStorage.getItem("token");
}

// Listens if token changes
onMounted(() => window.addEventListener("auth-changed", syncToken));
onBeforeUnmount(() => window.removeEventListener("auth-changed", syncToken));

function logout() {
  localStorage.removeItem("token");
  token.value = null;

  window.dispatchEvent(new Event("auth-changed"));
  router.push("/login");
}
</script>

<template>
  <nav>
    <RouterLink to="/list" class="link"> Contact list </RouterLink>
    <template v-if="!isAuth">
      <RouterLink to="/register" class="link"> Register </RouterLink>
      <RouterLink to="/login" class="link"> Log in </RouterLink>
    </template>
    <template v-else>
      <RouterLink to="/contacts" class="link"> Add contact </RouterLink>
      <button @click="logout">Log out</button>
    </template>
  </nav>
  <main>
    <RouterView/>
  </main>
</template>