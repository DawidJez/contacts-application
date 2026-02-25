<script setup lang="ts">
import { ref, onMounted, onUnmounted } from "vue";
import { router } from "../router/router";
import { computed } from 'vue';
import { RouterLink } from "vue-router";

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

const hasToken = computed(() => !!localStorage.getItem("token"));

onMounted(async () => {
    const res = await fetch(`/api/contacts/${props.id}`);
    contact.value = await res.json();
});

onUnmounted(() => {contact.value = null});

async function deleteContact () {
    const res = await fetch(`/api/contacts?id=${props.id}`, 
    { 
        method: "DELETE", 
        headers: { 
            Authorization: `Bearer ${localStorage.getItem("token")}`
        }
    });

    if (res.ok) {
        router.push("/list");
    }else {
        const msg = await res.text();
        console.log(res.status, msg);
    }
};
</script>

<template>
  <h1>Contact details</h1>

  <div v-if="contact">
    <h3>{{ contact.firstName }} {{ contact.lastName }}</h3>
    <h4 v-if="contact.email">{{ contact.email }}</h4>
    <p v-if="contact.phoneNumber">{{ contact.phoneNumber }}</p>
    <p v-if="contact.categoryName">Category: {{ contact.categoryName }}</p>
    <p v-if="contact.subcategoryName">Subcategory: {{ contact.subcategoryName }}</p>
    <p v-if="contact.customSubcategory">Custom category: {{ contact.customSubcategory }}</p>
    <RouterLink :to="`/edit/${props.id}`" class="link"> Edit </RouterLink>
    <button v-if="hasToken" @click="deleteContact">Delte contact</button>
  </div>
  <div v-else>No contact details</div>
</template>