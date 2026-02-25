<script setup lang="ts">
import { ref } from "vue";

const email = ref("");
const password = ref("");
const message = ref("");

async function onSubmit() {
    try {
        const res = await fetch("/api/auth/register", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ email: email.value, password: password.value }),
        });

        const body = await res.text();
        message.value = body || `Status: ${res.status} (empty body)`;
        } catch (e: any) {
        message.value = `Fetch error: ${e?.message ?? e}`;
    }
}
</script>

<template>
    <main style="display: grid; place-items: center;">
        <h1>Register your account</h1>
        <form @submit.prevent="onSubmit" style="display: grid; gap: 10px; max-width: 320px;">
            <input v-model="email" type="email" placeholder="email" required/>
            <input v-model="password" type="password" placeholder="password" required/>
            <button type="submit">Register</button>
            <p v-if="message" class="color:red;">{{message}}</p>
        </form>
    </main>
</template>