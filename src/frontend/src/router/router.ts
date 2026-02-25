import { createWebHistory, createRouter } from 'vue-router'

import RegisterView from '../views/RegisterView.vue'
import LoginView from '../views/LoginView.vue'
import ContactList from '../views/ContactList.vue'

const routes = [
  { path: '/register', component: RegisterView },
  { path: '/login', component: LoginView },
  { path: '/list', component: ContactList }
]

export const router = createRouter({
  history: createWebHistory(),
  routes,
})