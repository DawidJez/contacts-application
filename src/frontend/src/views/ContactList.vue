<script setup lang="ts">
import { ref, onMounted } from "vue";

type Contact = {
    id: number;
    firstName: string;
    lastName: string;
    email: string | null;
};

const contacts = ref<Contact[]>([]); // contact list
const message = ref("");

onMounted(async () => {
    const res = await fetch("/api/contacts");
    contacts.value = await res.json();

    if (contacts.value.length == 0 ) message.value = "No contacts";
});
</script>

<template>
  <h1>Contact list</h1>

  <p v-if="message">{{ message }}</p>

  <ul v-else>
    <li v-for="c in contacts" :key="c.id">
      {{ c.firstName }} {{ c.lastName }}
      <span v-if="c.email"> - {{ c.email }}</span>
    </li>
  </ul>
</template>