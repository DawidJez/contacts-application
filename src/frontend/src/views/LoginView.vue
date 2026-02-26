<script setup lang="ts">
import { ref } from "vue"
import { useRouter } from "vue-router"

const email = ref("");
const password = ref("");
const message = ref("");

const router = useRouter();

async function onSubmit() {
    
    const res = await fetch("/api/auth/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email: email.value, password: password.value }),
    });

    const body = await res.json();
    
    if (res.ok) {
        const token = body.token;
        localStorage.setItem("token", token);

        message.value = "Logged in!";

        setTimeout(() => {
            window.dispatchEvent(new Event("auth-changed")); // app vue listens
            router.push("/list");
        }, 1000);
    } else { message.value = body }    
}
</script>

<template>
    <main style="display: grid; place-items: center;">
        <h1>Log in</h1>
        <form @submit.prevent="onSubmit" style="display: grid; gap: 10px; max-width: 320px;">
            <input v-model="email" type="email" placeholder="email" required/>
            <input v-model="password" type="password" placeholder="password" required/>
            <button type="submit">Log in</button>
            <p v-if="message" class="color-red;">{{message}}</p>
        </form>
    </main>
</template>