import { createWebHistory, createRouter } from 'vue-router'

import RegisterView from '../views/RegisterView.vue'
import LoginView from '../views/LoginView.vue'
import ContactList from '../views/ContactList.vue'

const routes = [
  { path: '/register', component: RegisterView, meta: { guestOnly: true }}, // if authenticated you can't visit register
  { path: '/login', component: LoginView, meta: { guestOnly: true }},       // or login
  { path: '/list', component: ContactList, meta: { requiresAuth: false } }, // doesn't require authentication

  { path: "/:pathMatch(.*)*", redirect: "/list" } // catches not existing paths
]

export const router = createRouter({
  history: createWebHistory(),
  routes,
})

router.beforeEach((to) => {
    const token = localStorage.getItem("token");
    const isAuth = !!token; // true/false

    if (to.meta.guestOnly && isAuth) return "/list";
    if (to.meta.requiresAuth && !isAuth) return "/login";
})