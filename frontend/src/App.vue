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

const newOrder = ref({
  customerName: '',
  totalAmount: 0
})

const newSupplier = ref({
  name: '',
  phone: ''
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

  newCustomer.value = {
    name: '',
    phone: '',
    address: ''
  }

  loadCustomers()
}

async function deleteCustomer(id) {
  await fetch(`${API}/api/Customers/${id}`, {
    method: 'DELETE'
  })

  loadCustomers()
}

async function loadOrders() {
  const res = await fetch(`${API}/api/Orders`)
  orders.value = await res.json()
}

async function addOrder() {
  await fetch(`${API}/api/Orders`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      customerName: newOrder.value.customerName,
      totalAmount: Number(newOrder.value.totalAmount),
      items: []
    })
  })

  newOrder.value = {
    customerName: '',
    totalAmount: 0
  }

  loadOrders()
}

async function deleteOrder(id) {
  await fetch(`${API}/api/Orders/${id}`, {
    method: 'DELETE'
  })

  loadOrders()
}

async function loadSuppliers() {
  const res = await fetch(`${API}/api/Suppliers`)
  suppliers.value = await res.json()
}

async function addSupplier() {
  await fetch(`${API}/api/Suppliers`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(newSupplier.value)
  })

  newSupplier.value = {
    name: '',
    phone: ''
  }

  loadSuppliers()
}

async function deleteSupplier(id) {
  await fetch(`${API}/api/Suppliers/${id}`, {
    method: 'DELETE'
  })

  loadSuppliers()
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
        <div class="header">
            <h1>Hệ thống quản lý bán hàng</h1>
            <button @click="loadAll">Refresh</button>
        </div>

        <div class="grid">

            <!-- Customers -->
            <div class="card">
                <h2>Khách hàng</h2>

                <div class="form">
                    <input v-model="newCustomer.name" placeholder="Tên" />
                    <input v-model="newCustomer.phone" placeholder="SĐT" />
                    <input v-model="newCustomer.address" placeholder="Địa chỉ" />
                    <button @click="addCustomer">Thêm</button>
                </div>

                <div v-for="c in customers" :key="c.id" class="item">
                    <div>
                        <b>{{ c.name }}</b>
                        <p>{{ c.phone }}</p>
                        <small>{{ c.address }}</small>
                    </div>

                    <button class="delete" @click="deleteCustomer(c.id)">
                        Xóa
                    </button>
                </div>
            </div>

            <!-- Orders -->
            <div class="card">
                <h2>Đơn hàng</h2>

                <div class="form">
                    <input v-model="newOrder.customerName"
                           placeholder="Tên khách" />

                    <input v-model="newOrder.totalAmount"
                           type="number"
                           placeholder="Tổng tiền" />

                    <button @click="addOrder">Thêm</button>
                </div>

                <div v-for="o in orders" :key="o.id" class="item">
                    <div>
                        <b>{{ o.customerName }}</b>
                        <p>{{ o.totalAmount.toLocaleString() }} VNĐ</p>
                    </div>

                    <button class="delete" @click="deleteOrder(o.id)">
                        Xóa
                    </button>
                </div>
            </div>

            <!-- Suppliers -->
            <div class="card">
                <h2>Nhà cung cấp</h2>

                <div class="form">
                    <input v-model="newSupplier.name" placeholder="Tên NCC" />
                    <input v-model="newSupplier.phone" placeholder="SĐT" />
                    <button @click="addSupplier">Thêm</button>
                </div>

                <div v-for="s in suppliers" :key="s.id" class="item">
                    <div>
                        <b>{{ s.name }}</b>
                        <p>{{ s.phone }}</p>
                    </div>

                    <button class="delete" @click="deleteSupplier(s.id)">
                        Xóa
                    </button>
                </div>
            </div>

        </div>
    </div>
</template>

<style scoped>
    .page {
        min-height: 100vh;
        background: #f1f5f9;
        padding: 30px;
        font-family: Arial;
    }

    .header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 30px;
    }

    .grid {
        display: grid;
        grid-template-columns: repeat(3, 1fr);
        gap: 20px;
    }

    .card {
        background: white;
        padding: 20px;
        border-radius: 20px;
        box-shadow: 0 10px 20px rgba(0,0,0,0.08);
    }

    .form {
        display: flex;
        flex-direction: column;
        gap: 10px;
        margin-bottom: 20px;
    }

    input {
        padding: 10px;
        border-radius: 10px;
        border: 1px solid #ccc;
    }

    button {
        border: none;
        padding: 10px;
        border-radius: 10px;
        background: #2563eb;
        color: white;
        cursor: pointer;
    }

        button:hover {
            opacity: 0.9;
        }

    .delete {
        background: #ef4444;
    }

    .item {
        background: #f8fafc;
        padding: 15px;
        border-radius: 14px;
        margin-bottom: 12px;
        display: flex;
        justify-content: space-between;
        align-items: center;
    }

        .item p {
            margin: 5px 0;
            color: #2563eb;
        }

        .item small {
            color: gray;
        }

    @media (max-width: 900px) {
        .grid {
            grid-template-columns: 1fr;
        }
    }
</style>