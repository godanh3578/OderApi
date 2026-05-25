<script setup>
import { ref, onMounted } from 'vue'
import axios from 'axios'

const API = 'http://160.250.132.117:5296'

const activePanel = ref('dashboard')
const modalType = ref('')
const isEditing = ref(false)

const customers = ref([])
const orders = ref([])
const suppliers = ref([])

const customerForm = ref({ id: null, name: '', phone: '', address: '' })
const orderForm = ref({ id: null, customerName: '', totalAmount: 0 })
const supplierForm = ref({ id: null, name: '', phone: '' })

async function loadAll() {
  const [c, o, s] = await Promise.all([
    axios.get(`${API}/api/Customers`),
    axios.get(`${API}/api/Orders`),
    axios.get(`${API}/api/Suppliers`)
  ])

  customers.value = c.data
  orders.value = o.data
  suppliers.value = s.data
}

function openPanel(panel) {
  activePanel.value = panel
}

function openAddModal(type) {
  modalType.value = type
  isEditing.value = false

  if (type === 'customer') customerForm.value = { id: null, name: '', phone: '', address: '' }
  if (type === 'order') orderForm.value = { id: null, customerName: '', totalAmount: 0 }
  if (type === 'supplier') supplierForm.value = { id: null, name: '', phone: '' }
}

function openEditModal(type, item) {
  modalType.value = type
  isEditing.value = true

  if (type === 'customer') customerForm.value = { ...item }
  if (type === 'order') {
    orderForm.value = {
      id: item.id,
      customerName: item.customerName,
      totalAmount: item.totalAmount
    }
  }
  if (type === 'supplier') supplierForm.value = { ...item }
}

function closeModal() {
  modalType.value = ''
  isEditing.value = false
}

async function saveCustomer() {
  if (isEditing.value) {
    await axios.put(`${API}/api/Customers/${customerForm.value.id}`, customerForm.value)
  } else {
    await axios.post(`${API}/api/Customers`, {
      name: customerForm.value.name,
      phone: customerForm.value.phone,
      address: customerForm.value.address
    })
  }

  closeModal()
  loadAll()
}

async function deleteCustomer(id) {
  await axios.delete(`${API}/api/Customers/${id}`)
  loadAll()
}

async function saveOrder() {
  const payload = {
    customerName: orderForm.value.customerName,
    totalAmount: Number(orderForm.value.totalAmount),
    items: []
  }

  if (isEditing.value) {
    await axios.put(`${API}/api/Orders/${orderForm.value.id}`, {
      id: orderForm.value.id,
      ...payload
    })
  } else {
    await axios.post(`${API}/api/Orders`, payload)
  }

  closeModal()
  loadAll()
}

async function deleteOrder(id) {
  await axios.delete(`${API}/api/Orders/${id}`)
  loadAll()
}

async function saveSupplier() {
  if (isEditing.value) {
    await axios.put(`${API}/api/Suppliers/${supplierForm.value.id}`, supplierForm.value)
  } else {
    await axios.post(`${API}/api/Suppliers`, {
      name: supplierForm.value.name,
      phone: supplierForm.value.phone
    })
  }

  closeModal()
  loadAll()
}

async function deleteSupplier(id) {
  await axios.delete(`${API}/api/Suppliers/${id}`)
  loadAll()
}

onMounted(loadAll)
</script>

<template>
    <div class="layout">
        <aside class="sidebar">
            <div class="logo">🏪</div>
            <h2>Sales Dashboard</h2>
            <p>Microservice System</p>

            <nav>
                <button @click="openPanel('dashboard')">▦ Tổng quan</button>
                <button @click="openPanel('customers')">👥 Khách hàng</button>
                <button @click="openPanel('orders')">🛒 Đơn hàng</button>
                <button @click="openPanel('suppliers')">🏬 Nhà cung cấp</button>
            </nav>
        </aside>

        <main class="main">
            <header class="topbar">
                <h1>Hệ thống quản lý bán hàng & kho hàng</h1>
                <button class="refresh" @click="loadAll">Refresh</button>
            </header>

            <section v-if="activePanel === 'dashboard'">
                <div class="stats">
                    <div class="stat">
                        <span>Khách hàng</span>
                        <b>{{ customers.length }}</b>
                    </div>

                    <div class="stat">
                        <span>Đơn hàng</span>
                        <b>{{ orders.length }}</b>
                    </div>

                    <div class="stat">
                        <span>Nhà cung cấp</span>
                        <b>{{ suppliers.length }}</b>
                    </div>

                    <div class="stat">
                        <span>Doanh thu</span>
                        <b>{{ orders.reduce((sum, o) => sum + Number(o.totalAmount || 0), 0).toLocaleString() }}đ</b>
                    </div>
                </div>

                <div class="dashboard-box">
                    <h2>Kiến trúc Microservice</h2>
                    <p>VueJS + ASP.NET Core Web API + SQL Server</p>

                    <div class="arch-grid">
                        <div class="arch-card">
                            <h3>🔐 JWT Auth</h3>
                            <p>User Service cấp JWT Token</p>
                        </div>
                        <div class="arch-card">
                            <h3>API Gateway</h3>
                            <p>Routing tới services</p>
                        </div>
                        <div class="arch-card">
                            <h3>🟠 SQL Server</h3>
                            <p>Database per service</p>
                        </div>
                    </div>
                </div>
            </section>

            <section v-if="activePanel === 'customers'" class="panel">
                <div class="panel-head">
                    <h2>Quản lý khách hàng</h2>
                    <button @click="openAddModal('customer')">+ Thêm khách hàng</button>
                </div>

                <table>
                    <thead>
                        <tr>
                            <th>ID</th>
                            <th>Tên</th>
                            <th>SĐT</th>
                            <th>Địa chỉ</th>
                            <th>Thao tác</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="c in customers" :key="c.id">
                            <td>{{ c.id }}</td>
                            <td>{{ c.name }}</td>
                            <td>{{ c.phone }}</td>
                            <td>{{ c.address }}</td>
                            <td>
                                <button class="edit" @click="openEditModal('customer', c)">Sửa</button>
                                <button class="delete" @click="deleteCustomer(c.id)">Xóa</button>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </section>

            <section v-if="activePanel === 'orders'" class="panel">
                <div class="panel-head">
                    <h2>Quản lý đơn hàng</h2>
                    <button @click="openAddModal('order')">+ Thêm đơn hàng</button>
                </div>

                <table>
                    <thead>
                        <tr>
                            <th>ID</th>
                            <th>Khách hàng</th>
                            <th>Tổng tiền</th>
                            <th>Thao tác</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="o in orders" :key="o.id">
                            <td>{{ o.id }}</td>
                            <td>{{ o.customerName }}</td>
                            <td>{{ Number(o.totalAmount).toLocaleString() }} VNĐ</td>
                            <td>
                                <button class="edit" @click="openEditModal('order', o)">Sửa</button>
                                <button class="delete" @click="deleteOrder(o.id)">Xóa</button>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </section>

            <section v-if="activePanel === 'suppliers'" class="panel">
                <div class="panel-head">
                    <h2>Quản lý nhà cung cấp</h2>
                    <button @click="openAddModal('supplier')">+ Thêm nhà cung cấp</button>
                </div>

                <table>
                    <thead>
                        <tr>
                            <th>ID</th>
                            <th>Tên nhà cung cấp</th>
                            <th>SĐT</th>
                            <th>Thao tác</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="s in suppliers" :key="s.id">
                            <td>{{ s.id }}</td>
                            <td>{{ s.name }}</td>
                            <td>{{ s.phone }}</td>
                            <td>
                                <button class="edit" @click="openEditModal('supplier', s)">Sửa</button>
                                <button class="delete" @click="deleteSupplier(s.id)">Xóa</button>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </section>
        </main>

        <div v-if="modalType" class="modal-overlay">
            <div class="modal">
                <h2>{{ isEditing ? 'Sửa dữ liệu' : 'Thêm dữ liệu' }}</h2>

                <template v-if="modalType === 'customer'">
                    <input v-model="customerForm.name" placeholder="Tên khách hàng" />
                    <input v-model="customerForm.phone" placeholder="Số điện thoại" />
                    <input v-model="customerForm.address" placeholder="Địa chỉ" />

                    <div class="modal-actions">
                        <button @click="saveCustomer">Lưu</button>
                        <button class="cancel" @click="closeModal">Hủy</button>
                    </div>
                </template>

                <template v-if="modalType === 'order'">
                    <input v-model="orderForm.customerName" placeholder="Tên khách hàng" />
                    <input v-model="orderForm.totalAmount" type="number" placeholder="Tổng tiền" />

                    <div class="modal-actions">
                        <button @click="saveOrder">Lưu</button>
                        <button class="cancel" @click="closeModal">Hủy</button>
                    </div>
                </template>

                <template v-if="modalType === 'supplier'">
                    <input v-model="supplierForm.name" placeholder="Tên nhà cung cấp" />
                    <input v-model="supplierForm.phone" placeholder="Số điện thoại" />

                    <div class="modal-actions">
                        <button @click="saveSupplier">Lưu</button>
                        <button class="cancel" @click="closeModal">Hủy</button>
                    </div>
                </template>
            </div>
        </div>
    </div>
</template>

<style scoped>
    .layout {
        min-height: 100vh;
        display: flex;
        background: #f4f4f4;
        font-family: Arial, sans-serif;
    }

    .sidebar {
        width: 280px;
        background: #1e2588;
        color: white;
        padding: 32px 26px;
    }

    .logo {
        width: 78px;
        height: 78px;
        border-radius: 50%;
        background: white;
        color: #1e2588;
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: 36px;
        margin: 0 auto 20px;
    }

    .sidebar h2,
    .sidebar p {
        text-align: center;
    }

    .sidebar nav {
        margin-top: 40px;
        display: flex;
        flex-direction: column;
        gap: 14px;
    }

    .sidebar button {
        background: transparent;
        text-align: left;
        font-size: 17px;
        padding: 14px;
    }

    .main {
        flex: 1;
    }

    .topbar {
        height: 74px;
        background: white;
        display: flex;
        align-items: center;
        justify-content: space-between;
        padding: 0 28px;
        border-bottom: 1px solid #ddd;
    }

        .topbar h1 {
            font-size: 23px;
            font-weight: 500;
        }

    .stats {
        display: grid;
        grid-template-columns: repeat(4, 1fr);
        gap: 24px;
        padding: 24px;
    }

    .stat {
        background: #202020;
        color: white;
        border-radius: 20px;
        padding: 28px;
        box-shadow: 0 8px 14px rgba(0, 0, 0, 0.25);
    }

        .stat span {
            color: #aaa;
        }

        .stat b {
            display: block;
            margin-top: 20px;
            font-size: 28px;
        }

    .dashboard-box,
    .panel {
        margin: 0 24px 30px;
        background: #202020;
        color: white;
        border-radius: 20px;
        padding: 26px;
        box-shadow: 0 8px 14px rgba(0, 0, 0, 0.25);
    }

    .arch-grid {
        display: grid;
        grid-template-columns: repeat(3, 1fr);
        gap: 18px;
        margin-top: 20px;
    }

    .arch-card {
        border: 1px solid #777;
        border-radius: 12px;
        padding: 20px;
    }

    .panel-head {
        display: flex;
        justify-content: space-between;
        align-items: center;
    }

    table {
        width: 100%;
        border-collapse: collapse;
        margin-top: 18px;
        background: #2b2b2b;
        border-radius: 12px;
        overflow: hidden;
    }

    th,
    td {
        padding: 14px;
        border-bottom: 1px solid #444;
        text-align: left;
    }

    th {
        color: #93c5fd;
    }

    button {
        border: none;
        background: #2563eb;
        color: white;
        padding: 10px 14px;
        border-radius: 10px;
        cursor: pointer;
        font-weight: bold;
        margin-right: 8px;
    }

    .refresh {
        background: #111827;
    }

    .edit {
        background: #f59e0b;
    }

    .delete,
    .cancel {
        background: #ef4444;
    }

    .modal-overlay {
        position: fixed;
        inset: 0;
        background: rgba(0,0,0,0.55);
        display: flex;
        align-items: center;
        justify-content: center;
        z-index: 999;
    }

    .modal {
        background: white;
        color: #111827;
        width: 430px;
        padding: 28px;
        border-radius: 20px;
        display: flex;
        flex-direction: column;
        gap: 12px;
    }

        .modal input {
            padding: 12px;
            border-radius: 10px;
            border: 1px solid #ccc;
        }

    .modal-actions {
        margin-top: 12px;
    }

    @media (max-width: 1000px) {
        .layout {
            flex-direction: column;
        }

        .sidebar {
            width: 100%;
        }

        .stats,
        .arch-grid {
            grid-template-columns: 1fr;
        }
    }
</style>