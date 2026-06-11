import { createRouter, createWebHistory } from 'vue-router'

const EmptyRoute = { render: () => null }

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    { path: '/', name: 'shop', component: EmptyRoute, meta: { page: 'shop' } },
    { path: '/cart', name: 'cart', component: EmptyRoute, meta: { page: 'cart' } },
    { path: '/lookup', name: 'lookup', component: EmptyRoute, meta: { page: 'lookup' } },
    { path: '/my-orders', name: 'myOrders', component: EmptyRoute, meta: { page: 'myOrders' } },
    { path: '/account', name: 'account', component: EmptyRoute, meta: { page: 'account' } },
    { path: '/orders/:id', name: 'orderDetail', component: EmptyRoute, meta: { page: 'orderDetail' } },
    { path: '/manage/dashboard', name: 'dashboard', component: EmptyRoute, meta: { page: 'dashboard', staff: true } },
    { path: '/manage/orders', name: 'orders', component: EmptyRoute, meta: { page: 'orders', staff: true } },
    { path: '/manage/customers', name: 'customers', component: EmptyRoute, meta: { page: 'customers', staff: true } },
    { path: '/manage/suppliers', name: 'suppliers', component: EmptyRoute, meta: { page: 'suppliers', staff: true } },
    { path: '/manage/payments', name: 'payments', component: EmptyRoute, meta: { page: 'payments', staff: true } },
    { path: '/manage/debts', name: 'debts', component: EmptyRoute, meta: { page: 'debts', staff: true } },
    { path: '/manage/integration', name: 'integration', component: EmptyRoute, meta: { page: 'integration', staff: true } },
    { path: '/manage/warehouse', name: 'warehouse', component: EmptyRoute, meta: { page: 'warehouse', staff: true } }
  ]
})

export default router
