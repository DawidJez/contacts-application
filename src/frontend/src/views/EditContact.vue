<script setup lang="ts">
import { ref, watch, computed, onMounted } from "vue";
import { useRouter } from "vue-router";

const props = defineProps<{ id: string }>();
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

onMounted(async () => {
  const res = await fetch(`/api/contacts/${props.id}`, {
    headers: { 
      Authorization: `Bearer ${localStorage.getItem("token")}` },
  });

  if (!res.ok) {
    message.value = await res.text();
    return;
  }

  const c = await res.json();

  firstName.value = c.firstName ?? "";
  lastName.value = c.lastName ?? "";
  email.value = c.email ?? "";
  phoneNumber.value = c.phoneNumber ?? null;

  categoryId.value = c.categoryId ?? 1;
  subcategoryId.value = c.subcategoryId ?? null;
  customSubcategory.value = c.customSubcategory ?? null;
    
  password.value = "";
});

async function onSubmit() {
  // payload so we can check later what is to be sent
  const payload: Record<string, any> = {
    firstName: firstName.value,
    lastName: lastName.value,
    phoneNumber: phoneNumber.value,
    email: email.value,
    password: password.value,
    categoryId: categoryId.value,
    subcategoryId: subcategoryId.value,
    customSubcategory: customSubcategory.value,
  };

  // checking what can be deleted
  Object.keys(payload).forEach((k) => {
    const pl = payload[k];
    if (pl === "" || pl === null || pl === undefined) delete payload[k];
  });

  // using patch method to edit only changed values
  const res = await fetch(`/api/contacts?id=${props.id}`, {
    method: "PATCH",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${localStorage.getItem("token")}`,
    },
    body: JSON.stringify(payload),
  });
  console.log("payload after clean:", payload);

  if (res.ok) {
    message.value = "Contact updated";
    setTimeout(() => router.push(`/contacts/${props.id}`), 1000);
  }

  const resText = await res.text();
  message.value = resText || `${res.status} ${res.statusText}`;
}
</script>

<template>
  <main style="display: grid; place-items: center;">
    <h1>Edit contact</h1>

    <form @submit.prevent="onSubmit" style="display: grid; gap: 10px; max-width: 320px;">
      <input v-model="firstName" placeholder="First name" />
      <input v-model="lastName" placeholder="Last name" />
      <input v-model="email" type="email" placeholder="Email" />
      <input v-model="password" type="password" placeholder="New password (optional)" />

      <input v-model="phoneNumber" placeholder="Phone number (optional)" />

      <select v-model.number="categoryId">
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

      <button type="submit">Save</button>

      <p v-if="message">{{ message }}</p>
    </form>
  </main>
</template>