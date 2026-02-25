<script setup lang="ts">
import { ref, watch, computed } from "vue";
import { useRouter } from "vue-router";

const router = useRouter();
const message = ref("");

const firstName = ref("");
const lastName = ref("");
const phoneNumber = ref<string | null>(null);
const email = ref("");
const password = ref("");

const categoryId = ref<number>(1);
const subcategoryId = ref<number | null>(null);
const customSubcategory = ref<string | null>(null);

// User will see names of categories instead of ids 
// it's better to fetch categories names with theirs ids from db
// but for this small application it's not really necessary
const categories = [
    { id: 1, name: "Służbowy" },
    { id: 2, name: "Prywatny" },
    { id: 3, name: "Inny" },
];

const subcategories = [
  { id: 1, categoryId: 1, name: "Szef" },
  { id: 2, categoryId: 1, name: "Klient" },
  { id: 3, categoryId: 1, name: "Współpracownik" }
];

const matchingSubcategories = computed(() =>
    subcategories.filter((el) => el.categoryId === categoryId.value)
);

// category changed -> reset other sub/custom categories
watch(categoryId, () => {
    subcategoryId.value = null;
    customSubcategory.value = null;
});

async function onSubmit() {
    const res = await fetch("/api/contacts", 
    {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${localStorage.getItem("token")}`,
    },
    body: JSON.stringify({
        firstName: firstName.value,
        lastName: lastName.value,
        phoneNumber: phoneNumber.value,
        email: email.value,
        password: password.value,
        categoryId: categoryId.value,
        subcategoryId: subcategoryId.value,
        customSubcategory: customSubcategory.value,
    }),
    });
    
    const body = await res.json();
    message.value = body;

    if (res.ok) {
        message.value = "Contact created";
        setTimeout(() => router.push("/list"), 1000);
    }
}
</script>
<template>
  <main style="display: grid; place-items: center;">
    <h1>Add contact</h1>

    <form @submit.prevent="onSubmit" style="display: grid; gap: 10px; max-width: 320px;">
      <input v-model="firstName" placeholder="First name" required />
      <input v-model="lastName" placeholder="Last name" required />
      <input v-model="email" type="email" placeholder="Email" required />
      <input v-model="password" type="password" placeholder="Password" required />

      <input v-model="phoneNumber" placeholder="Phone number (optional)" />
      <select v-model.number="categoryId" required>
        <option v-for="c in categories" :key="c.id" :value="c.id">
          {{ c.name }}
        </option>
      </select>
      <select v-if="categoryId === 1" v-model.number="subcategoryId">
        <option :value="null">Select subcategory</option>
        <option v-for="s in matchingSubcategories" :key="s.id" :value="s.id">
          {{ s.name }}
        </option>
      </select>
      <input
        v-else-if="categoryId === 3"
        v-model="customSubcategory"
        placeholder="Custom category (optional)"
      />
      <button type="submit">Add</button>

      <p v-if="message">{{ message }}</p>
    </form>
  </main>
</template>