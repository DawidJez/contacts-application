<script setup lang="ts">
import { ref, onMounted, onUnmounted } from "vue";

type Contact = {
  id: number;
  firstName: string;
  lastName: string;
  phoneNumber: string | null;
  email: string;
  categoryId: number;
  categoryName: string;
  subcategoryId: number | null;
  subcategoryName: string | null;
  customSubcategory: string | null;
};

const contact = ref<Contact | null>();
const props = defineProps<{ id: string }>();

onMounted(async () => {
  const res = await fetch(`/api/contacts/${props.id}`);
  contact.value = await res.json();
});

onUnmounted(async () => {contact.value = null});
</script>

<template>
  <h1>Contact details</h1>

  <div v-if="contact">
    <h3>{{ contact.firstName }} {{ contact.lastName }}</h3>
    <h4 v-if="contact.email">{{ contact.email }}</h4>
    <p v-if="contact.phoneNumber">{{ contact.phoneNumber }}</p>
    <p v-if="contact.categoryName">Category: {{ contact.categoryName }}</p>
    <p v-if="contact.subcategoryName">Subcategory: {{ contact.subcategoryName }}</p>
    <p v-if="contact.customSubcategory">Custom subcategory: {{ contact.customSubcategory }}</p>
  </div>
  <div v-else>No contact details</div>
</template>