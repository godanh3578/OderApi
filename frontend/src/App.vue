<script setup>
import { ref, onMounted } from 'vue'

const API = 'http://localhost:5296'

const customers = ref([])
const orders = ref([])
const suppliers = ref([])

const newCustomer = ref({
  name: '',
  phone: '',
  address: ''
})

async function loadCustomers() {
  const res = await fetch(`${API}/api/Customers`)
  customers.value = await res.json()
}

async function addCustomer() {
  await fetch(`${API}/api/Customers`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(newCustomer.value)
  })

  newCustomer.value = { name: '', phone: '', address: '' }
  await loadCustomers()
}

async function deleteCustomer(id) {
  await fetch(`${API}/api/Customers/${id}`, {
    method: 'DELETE'
  })

  await loadCustomers()
}

async function loadOrders() {
  const res = await fetch(`${API}/api/Orders`)
  orders.value = await res.json()
}

async function loadSuppliers() {
  const res = await fetch(`${API}/api/Suppliers`)
  suppliers.value = await res.json()
}

async function loadAll() {
  await loadCustomers()
  await loadOrders()
  await loadSuppliers()
}

onMounted(loadAll)
</script>

<template>
    <div class="page">
        <header class="hero">
            <div>
                <p class="tag">Order & Sales Service</p>
                <h1>Hệ thống quản lý bán hàng</h1>
                <p>Trang chủ gọi API từ backend nhóm 2.</p>
            </div>

            <button @click="loadAll">Tải tất cả dữ liệu</button>
        </header>

        <section class="grid">
            <div class="card big">
                <h2>Khách hàng</h2>

                <div class="form">
                    <input v-model="newCustomer.name" placeholder="Tên khách hàng" />
                    <input v-model="newCustomer.phone" placeholder="Số điện thoại" />
                    <input v-model="newCustomer.address" placeholder="Địa chỉ" />
                    <button @click="addCustomer">+ Thêm</button>
                </div>

                <div v-if="customers.length === 0" class="empty">Chưa có dữ liệu</div>

                <div v-for="c in customers" :key="c.id" class="item">
                    <div>
                        <b>{{ c.name }}</b>
                        <span>{{ c.phone }}</span>
                        <small>{{ c.address }}</small>
                    </div>

                    <button class="danger" @click="deleteCustomer(c.id)">Xóa</button>
                </div>
            </div>

            <div class="card">
                <h2>Đơn hàng</h2>

                <div v-if="orders.length === 0" class="empty">Chưa có dữ liệu</div>

                <div v-for="o in orders" :key="o.id" class="item">
                    <div>
                        <b>{{ o.customerName }}</b>
                        <span>{{ o.totalAmount.toLocaleString() }} VNĐ</span>
                    </div>
                </div>
            </div>

            <div class="card">
                <h2>Nhà cung cấp</h2>

                <div v-if="suppliers.length === 0" class="empty">Chưa có dữ liệu</div>

                <div v-for="s in suppliers" :key="s.id" class="item">
                    <div>
                        <b>{{ s.name }}</b>
                        <span>{{ s.phone }}</span>
                    </div>
                </div>
            </div>
        </section>
    </div>
</template>

<style scoped>
    .page {
        min-height: 100vh;
        padding: 40px;
        background: linear-gradient(135deg, #e0f2fe, #f8fafc);
        font-family: Arial, sans-serif;
    }

    .hero {
        background: white;
        border-radius: 24px;
        padding: 32px;
        display: flex;
        justify-content: space-between;
        align-items: center;
        box-shadow: 0 20px 40px rgba(0, 0, 0, 0.08);
        margin-bottom: 28px;
    }

    .tag {
        color: #2563eb;
        font-weight: bold;
    }

    h1 {
        margin: 8px 0;
        color: #0f172a;
    }

    .grid {
        display: grid;
        grid-template-columns: 2fr 1fr 1fr;
        gap: 22px;
    }

    .card {
        background: white;
        border-radius: 22px;
        padding: 24px;
        box-shadow: 0 14px 30px rgba(0, 0, 0, 0.08);
    }

    .form {
        display: grid;
        grid-template-columns: 1fr 1fr 1fr auto;
        gap: 10px;
        margin-bottom: 18px;
    }

    input {
        padding: 12px;
        border-radius: 12px;
        border: 1px solid #cbd5e1;
    }

    button {
        border: none;
        background: #2563eb;
        color: white;
        padding: 10px 16px;
        border-radius: 12px;
        cursor: pointer;
        font-weight: bold;
    }

        button:hover {
            background: #1d4ed8;
        }

    .danger {
        background: #ef4444;
    }

        .danger:hover {
            background: #dc2626;
        }

    .item {
        background: #f8fafc;
        border: 1px solid #e2e8f0;
        padding: 14px;
        border-radius: 14px;
        margin-bottom: 12px;
        display: flex;
        justify-content: space-between;
        gap: 12px;
    }

        .item div {
            display: flex;
            flex-direction: column;
            gap: 5px;
        }

        .item b {
            color: #0f172a;
        }

        .item span {
            color: #2563eb;
        }

        .item small {
            color: #64748b;
        }

    .empty {
        color: #94a3b8;
        background: #f8fafc;
        padding: 14px;
        border-radius: 14px;
    }

    @media (max-width: 1000px) {
        .grid,
        .form {
            grid-template-columns: 1fr;
        }

        .hero {
            flex-direction: column;
            align-items: flex-start;
            gap: 16px;
        }
    }
</style>