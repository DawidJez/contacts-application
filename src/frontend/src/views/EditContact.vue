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

// helper to check what is actually changed
const original = ref<any>(null);

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
  // contacts details
  const c = await res.json();
  
  original.value = {
    firstName: c.firstName ?? "",
    lastName: c.lastName ?? "",
    email: c.email ?? "",
    phoneNumber: c.phoneNumber ?? null,

    categoryId: c.categoryId ?? 1,
    subcategoryId: c.subcategoryId ?? null,
    customSubcategory: c.customSubcategory ?? null,
  }

  // forms values
  firstName.value = original.value.firstName;
  lastName.value = original.value.lastName;
  email.value = original.value.email;
  phoneNumber.value = original.value.phoneNumber;

  categoryId.value = original.value.categoryId;
  subcategoryId.value = original.value.subcategoryId;
  customSubcategory.value = original.value.customSubcategory;

  password.value = "";
});

async function onSubmit() {
  // current so we can check later what is to be sent
  const current = {
    firstName: firstName.value,
    lastName: lastName.value,
    phoneNumber: phoneNumber.value,
    email: email.value,
    categoryId: categoryId.value,
    subcategoryId: subcategoryId.value,
    customSubcategory: customSubcategory.value,
  };
  // payload
  const payload: Record<string, any> = {};

  // checking what can be deleted
  (Object.keys(current) as (keyof typeof current)[]).forEach((k) => {
    if (current[k] !== original.value?.[k]) payload[k] = current[k];
  });

  // Send only when user entered new password
  if (password.value !== "") payload.password = password.value;

  // Nothing changed
  if (Object.keys(payload).length === 0) {
    message.value = "No changes";
    return;
  }

  // using patch method to edit only changed values
  const res = await fetch(`/api/contacts?id=${props.id}`, {
    method: "PATCH",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${localStorage.getItem("token")}`,
    },
    body: JSON.stringify(payload),
  });

  if (res.ok) {
    message.value = "Contact updated";
    setTimeout(() => router.push(`/contacts/${props.id}`), 1000);
  }

  message.value = await res.json();
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