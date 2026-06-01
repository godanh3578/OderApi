<script setup>
import { ref, computed, onMounted } from 'vue'
import axios from 'axios'

const API = 'http://192.168.29.23:5002'

const activePanel = ref('dashboard')

const customers = ref([])
const orders = ref([])
const suppliers = ref([])
const products = ref([])

async function loadAll() {
  try {
    const [c, o, s] = await Promise.all([
      axios.get(`${API}/api/Customers`),
      axios.get(`${API}/api/Orders`),
      axios.get(`${API}/api/Suppliers`)
    ])

    customers.value = c.data
    orders.value = o.data
    suppliers.value = s.data

    products.value = [
      {
        id: 1,
        name: 'Laptop Gaming',
        price: 25000000,
        stock: 12
      },
      {
        id: 2,
        name: 'Chuột Logitech',
        price: 500000,
        stock: 40
      }
    ]
  }
  catch (err) {
    console.log(err)
  }
}

function openPanel(panel) {
  activePanel.value = panel
}

function isActive(panel) {
  return activePanel.value === panel
}

function totalRevenue() {
  return orders.value.reduce(
    (sum, o) => sum + Number(o.totalAmount || 0),
    0
  )
}

const dashboardTotal = computed(() => {
  const total = products.value.length + customers.value.length + orders.value.length + suppliers.value.length
  return total === 0 ? 1 : total
})

const dashboardChartStyle = computed(() => {
  const productPercent = products.value.length / dashboardTotal.value * 100
  const customerPercent = customers.value.length / dashboardTotal.value * 100
  const orderPercent = orders.value.length / dashboardTotal.value * 100
  const supplierPercent = suppliers.value.length / dashboardTotal.value * 100

  const productEnd = productPercent
  const customerEnd = productEnd + customerPercent
  const orderEnd = customerEnd + orderPercent
  const supplierEnd = orderEnd + supplierPercent

  return {
    background: `conic-gradient(
      #1976d2 0% ${productEnd}%,
      #2e7d32 ${productEnd}% ${customerEnd}%,
      #ed6c02 ${customerEnd}% ${orderEnd}%,
      #0288d1 ${orderEnd}% ${supplierEnd}%
    )`
  }
})

function getNextId(list) {
  if (list.length === 0) return 1
  return Math.max(...list.map(item => Number(item.id || 0))) + 1
}

function addProduct() {
  const name = window.prompt('Nhập tên sản phẩm:')
  if (!name) return

  const price = Number(window.prompt('Nhập giá sản phẩm:') || 0)
  const stock = Number(window.prompt('Nhập số lượng tồn kho:') || 0)

  products.value.push({
    id: getNextId(products.value),
    name,
    price,
    stock
  })
}

function editProduct(product) {
  const name = window.prompt('Sửa tên sản phẩm:', product.name)
  if (!name) return

  const price = Number(window.prompt('Sửa giá sản phẩm:', product.price) || product.price)
  const stock = Number(window.prompt('Sửa tồn kho:', product.stock) || product.stock)

  product.name = name
  product.price = price
  product.stock = stock
}

function deleteProduct(id) {
  if (!window.confirm('Bạn có chắc muốn xóa sản phẩm này không?')) return
  products.value = products.value.filter(item => item.id !== id)
}

function addCustomer() {
  const name = window.prompt('Nhập tên khách hàng:')
  if (!name) return

  const phone = window.prompt('Nhập số điện thoại:') || ''
  const address = window.prompt('Nhập địa chỉ:') || ''

  customers.value.push({
    id: getNextId(customers.value),
    name,
    phone,
    address
  })
}

function editCustomer(customer) {
  const name = window.prompt('Sửa tên khách hàng:', customer.name)
  if (!name) return

  customer.name = name
  customer.phone = window.prompt('Sửa số điện thoại:', customer.phone) || customer.phone
  customer.address = window.prompt('Sửa địa chỉ:', customer.address) || customer.address
}

function deleteCustomer(id) {
  if (!window.confirm('Bạn có chắc muốn xóa khách hàng này không?')) return
  customers.value = customers.value.filter(item => item.id !== id)
}

function addOrder() {
  const customerName = window.prompt('Nhập tên khách hàng:')
  if (!customerName) return

  const totalAmount = Number(window.prompt('Nhập tổng tiền đơn hàng:') || 0)

  orders.value.push({
    id: getNextId(orders.value),
    customerName,
    totalAmount
  })
}

function editOrder(order) {
  const customerName = window.prompt('Sửa tên khách hàng:', order.customerName)
  if (!customerName) return

  const totalAmount = Number(window.prompt('Sửa tổng tiền:', order.totalAmount) || order.totalAmount)

  order.customerName = customerName
  order.totalAmount = totalAmount
}

function deleteOrder(id) {
  if (!window.confirm('Bạn có chắc muốn xóa đơn hàng này không?')) return
  orders.value = orders.value.filter(item => item.id !== id)
}

function addSupplier() {
  const name = window.prompt('Nhập tên nhà cung cấp:')
  if (!name) return

  const phone = window.prompt('Nhập số điện thoại:') || ''

  suppliers.value.push({
    id: getNextId(suppliers.value),
    name,
    phone
  })
}

function editSupplier(supplier) {
  const name = window.prompt('Sửa tên nhà cung cấp:', supplier.name)
  if (!name) return

  supplier.name = name
  supplier.phone = window.prompt('Sửa số điện thoại:', supplier.phone) || supplier.phone
}

function deleteSupplier(id) {
  if (!window.confirm('Bạn có chắc muốn xóa nhà cung cấp này không?')) return
  suppliers.value = suppliers.value.filter(item => item.id !== id)
}

onMounted(loadAll)
</script>

<template>
  <div class="layout">
    <!-- SIDEBAR -->
    <aside class="sidebar">
      <div class="brand">
        <div class="brand-logo">
          🏪
        </div>

        <div>
          <h2>RetailERP</h2>
          <p>Management System</p>
        </div>
      </div>

      <nav>
        <div class="menu-group">
          <div class="menu-title">
            TỔNG QUAN
          </div>

          <button
            :class="{ active: isActive('dashboard') }"
            @click="openPanel('dashboard')"
          >
            📊 Dashboard
          </button>
        </div>

        <div class="menu-group">
          <div class="menu-title">
            KHO & SẢN PHẨM
          </div>

          <button
            :class="{ active: isActive('products') }"
            @click="openPanel('products')"
          >
            📦 Sản phẩm
          </button>

          <button
            :class="{ active: isActive('categories') }"
            @click="openPanel('categories')"
          >
            🗂️ Danh mục
          </button>

          <button
            :class="{ active: isActive('inventory') }"
            @click="openPanel('inventory')"
          >
            📚 Tồn kho
          </button>

          <button
            :class="{ active: isActive('imports') }"
            @click="openPanel('imports')"
          >
            🚚 Nhập kho
          </button>

          <button
            :class="{ active: isActive('alerts') }"
            @click="openPanel('alerts')"
          >
            ⚠️ Cảnh báo kho
          </button>
        </div>

        <div class="menu-group">
          <div class="menu-title">
            BÁN HÀNG
          </div>

          <button
            :class="{ active: isActive('orders') }"
            @click="openPanel('orders')"
          >
            🛒 Đơn hàng
          </button>

          <button
            :class="{ active: isActive('customers') }"
            @click="openPanel('customers')"
          >
            👥 Khách hàng
          </button>

          <button
            :class="{ active: isActive('suppliers') }"
            @click="openPanel('suppliers')"
          >
            🏬 Nhà cung cấp
          </button>

          <button
            :class="{ active: isActive('debts') }"
            @click="openPanel('debts')"
          >
            💳 Công nợ
          </button>
        </div>

        <div class="menu-group">
          <div class="menu-title">
            BÁO CÁO & HỆ THỐNG
          </div>

          <button
            :class="{ active: isActive('reports') }"
            @click="openPanel('reports')"
          >
            📈 Doanh thu
          </button>

          <button
            :class="{ active: isActive('analytics') }"
            @click="openPanel('analytics')"
          >
            📊 Thống kê
          </button>

          <button
            :class="{ active: isActive('topproducts') }"
            @click="openPanel('topproducts')"
          >
            🏆 Top sản phẩm
          </button>

          <button
            :class="{ active: isActive('users') }"
            @click="openPanel('users')"
          >
            👤 Người dùng
          </button>

          <button
            :class="{ active: isActive('roles') }"
            @click="openPanel('roles')"
          >
            🔐 Phân quyền
          </button>
        </div>

        <div class="menu-group">
          <div class="menu-title">
            HỆ THỐNG
          </div>

          <button
            :class="{ active: isActive('settings') }"
            @click="openPanel('settings')"
          >
            ⚙️ Cài đặt
          </button>

          <button class="logout">
            🚪 Đăng xuất
          </button>
        </div>
      </nav>
    </aside>

    <!-- MAIN -->
    <main class="main">
      <!-- TOPBAR -->
      <header class="topbar">
        <div>
          <h1>Hệ thống quản lý bán hàng</h1>

          <p class="subtitle">
            ASP.NET Core + VueJS + SQL Server
          </p>
        </div>

        <div class="topbar-right">
          <button
            class="refresh"
            @click="loadAll"
          >
            🔄 Refresh
          </button>

          <div class="user-box">
            👨‍💻 Admin
          </div>
        </div>
      </header>

      <!-- DASHBOARD -->
      <section v-if="activePanel === 'dashboard'">
        <div class="stats">
          <div class="stat primary">
            <span>Sản phẩm</span>
            <b>{{ products.length }}</b>
          </div>

          <div class="stat success">
            <span>Khách hàng</span>
            <b>{{ customers.length }}</b>
          </div>

          <div class="stat warning">
            <span>Đơn hàng</span>
            <b>{{ orders.length }}</b>
          </div>

          <div class="stat info">
            <span>Nhà cung cấp</span>
            <b>{{ suppliers.length }}</b>
          </div>
        </div>

        <div class="dashboard-box">
          <div class="panel-head">
            <div>
              <h2>📊 Tổng quan hệ thống</h2>
              <p>Biểu đồ tròn thể hiện số lượng dữ liệu chính trong hệ thống</p>
            </div>
          </div>

          <div class="chart-section">
            <div class="pie-chart" :style="dashboardChartStyle">
              <div class="pie-center">
                <strong>{{ dashboardTotal }}</strong>
                <span>Tổng mục</span>
              </div>
            </div>

            <div class="chart-legend">
              <div class="legend-item">
                <span class="legend-color product"></span>
                <p>Sản phẩm</p>
                <b>{{ products.length }}</b>
              </div>

              <div class="legend-item">
                <span class="legend-color customer"></span>
                <p>Khách hàng</p>
                <b>{{ customers.length }}</b>
              </div>

              <div class="legend-item">
                <span class="legend-color order"></span>
                <p>Đơn hàng</p>
                <b>{{ orders.length }}</b>
              </div>

              <div class="legend-item">
                <span class="legend-color supplier"></span>
                <p>Nhà cung cấp</p>
                <b>{{ suppliers.length }}</b>
              </div>
            </div>
          </div>
        </div>
      </section>

      <!-- PRODUCTS -->
      <section
        v-if="activePanel === 'products'"
        class="panel"
      >
        <div class="panel-head">
          <h2>Quản lý sản phẩm</h2>
          <button @click="addProduct">+ Thêm sản phẩm</button>
        </div>

        <table>
          <thead>
            <tr>
              <th>ID</th>
              <th>Tên sản phẩm</th>
              <th>Giá</th>
              <th>Tồn kho</th>
              <th>Thao tác</th>
            </tr>
          </thead>

          <tbody>
  <tr
    v-for="p in products"
    :key="p.id"
  >
    <td>{{ p.id }}</td>
    <td>{{ p.name }}</td>
    <td>{{ Number(p.price).toLocaleString() }} VNĐ</td>
    <td>{{ p.stock }}</td>
    <td>
      <button class="edit" @click="editProduct(p)">Sửa</button>
      <button class="delete" @click="deleteProduct(p.id)">Xóa</button>
    </td>
  </tr>
</tbody>
        </table>
      </section>

      <!-- CUSTOMERS -->
      <section
        v-if="activePanel === 'customers'"
        class="panel"
      >
        <div class="panel-head">
          <h2>Quản lý khách hàng</h2>
          <button @click="addCustomer">+ Thêm khách hàng</button>
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
            <tr
              v-for="c in customers"
              :key="c.id"
            >
              <td>{{ c.id }}</td>
              <td>{{ c.name }}</td>
              <td>{{ c.phone }}</td>
              <td>{{ c.address }}</td>
              <td>
                <button class="edit">Sửa</button>
                <button class="delete">Xóa</button>
              </td>
            </tr>
          </tbody>
        </table>
      </section>

      <!-- ORDERS -->
      <section
        v-if="activePanel === 'orders'"
        class="panel"
      >
        <div class="panel-head">
          <h2>Quản lý đơn hàng</h2>
          <button @click="addOrder">+ Thêm đơn hàng</button>
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
            <tr
              v-for="o in orders"
              :key="o.id"
            >
              <td>{{ o.id }}</td>
              <td>{{ o.customerName }}</td>
              <td>{{ Number(o.totalAmount || 0).toLocaleString() }} VNĐ</td>
              <td>
                <button class="edit">Sửa</button>
                <button class="delete">Xóa</button>
              </td>
            </tr>
          </tbody>
        </table>
      </section>

      <!-- SUPPLIERS -->
      <section
        v-if="activePanel === 'suppliers'"
        class="panel"
      >
        <div class="panel-head">
          <h2>Quản lý nhà cung cấp</h2>
          <button @click="addSupplier">+ Thêm nhà cung cấp</button>
        </div>

        <table>
          <thead>
            <tr>
              <th>ID</th>
              <th>Tên</th>
              <th>SĐT</th>
              <th>Thao tác</th>
            </tr>
          </thead>

          <tbody>
            <tr
              v-for="s in suppliers"
              :key="s.id"
            >
              <td>{{ s.id }}</td>
              <td>{{ s.name }}</td>
              <td>{{ s.phone }}</td>
              <td>
                <button class="edit">Sửa</button>
                <button class="delete">Xóa</button>
              </td>
            </tr>
          </tbody>
        </table>
      </section>

      <!-- REPORT -->
      <section
        v-if="activePanel === 'reports'"
        class="panel"
      >
        <h2>📈 Báo cáo doanh thu</h2>

        <div class="stats report-stats">
          <div class="stat warning">
            <span>Tổng doanh thu</span>
            <b>{{ totalRevenue().toLocaleString() }} VNĐ</b>
          </div>

          <div class="stat success">
            <span>Tổng khách hàng</span>
            <b>{{ customers.length }}</b>
          </div>
        </div>
      </section>

      <!-- SETTINGS -->
      <section
        v-if="activePanel === 'settings'"
        class="panel"
      >
        <h2>⚙️ Cài đặt hệ thống</h2>

        <div class="arch-grid">
          <div class="arch-card primary-card">
            <h3>JWT Authentication</h3>
            <p>Đăng nhập và phân quyền</p>
          </div>

          <div class="arch-card info-card">
            <h3>API Gateway</h3>
            <p>Routing tới microservices</p>
          </div>

          <div class="arch-card success-card">
            <h3>SQL Server</h3>
            <p>Database hệ thống</p>
          </div>
        </div>
      </section>
    </main>
  </div>
</template>

<style scoped>
:global(:root) {
  --color-primary: #1976d2;
  --color-primary-bg: #e3f2fd;
  --color-primary-bg-hover: #bbdefb;
  --color-primary-border: #90caf9;
  --color-primary-border-hover: #64b5f6;
  --color-primary-hover: #1565c0;
  --color-primary-active: #0d47a1;
  --color-primary-text: #1976d2;
  --color-primary-text-hover: #1565c0;
  --color-primary-text-active: #0d47a1;

  --color-success: #2e7d32;
  --color-success-bg: #e8f5e9;
  --color-success-bg-hover: #c8e6c9;
  --color-success-border: #a5d6a7;
  --color-success-border-hover: #81c784;
  --color-success-hover: #1b5e20;
  --color-success-active: #0d3c0d;
  --color-success-text: #2e7d32;

  --color-warning: #ed6c02;
  --color-warning-bg: #fff3e0;
  --color-warning-bg-hover: #ffe0b2;
  --color-warning-border: #ffcc02;
  --color-warning-border-hover: #ffb74d;
  --color-warning-hover: #e65100;
  --color-warning-active: #bf360c;
  --color-warning-text: #ed6c02;

  --color-error: #d32f2f;
  --color-error-bg: #ffebee;
  --color-error-bg-hover: #ffcdd2;
  --color-error-border: #ef9a9a;
  --color-error-border-hover: #e57373;
  --color-error-hover: #c62828;
  --color-error-active: #b71c1c;
  --color-error-text: #d32f2f;

  --color-info: #0288d1;
  --color-info-bg: #e1f5fe;
  --color-info-bg-hover: #b3e5fc;
  --color-info-border: #81d4fa;
  --color-info-border-hover: #4fc3f7;
  --color-info-hover: #0277bd;
  --color-info-active: #01579b;
  --color-info-text: #0288d1;

  --color-text-base: #121212;
  --color-text: rgba(0, 0, 0, 0.87);
  --color-text-secondary: rgba(0, 0, 0, 0.6);
  --color-text-tertiary: rgba(0, 0, 0, 0.38);
  --color-text-disabled: rgba(0, 0, 0, 0.38);

  --color-bg-base: #fafafa;
  --color-bg-container: #ffffff;
  --color-bg-elevated: #ffffff;
  --color-bg-layout: #f5f5f5;
  --color-bg-mask: rgba(0, 0, 0, 0.5);

  --color-border: #e0e0e0;
  --color-border-secondary: #eeeeee;

  --radius-xs: 2px;
  --radius-sm: 3px;
  --radius-md: 4px;
  --radius-lg: 8px;

  --padding-sm: 8px;
  --padding: 16px;
  --padding-lg: 24px;

  --margin-sm: 8px;
  --margin: 16px;
  --margin-lg: 24px;

  --shadow-main: 0px 2px 1px -1px rgba(0,0,0,0.2), 0px 1px 1px 0px rgba(0,0,0,0.14), 0px 1px 3px 0px rgba(0,0,0,0.12);
  --shadow-secondary: 0px 3px 3px -2px rgba(0,0,0,0.2), 0px 3px 4px 0px rgba(0,0,0,0.14), 0px 1px 8px 0px rgba(0,0,0,0.12);
}

* {
  box-sizing: border-box;
}

.layout {
  min-height: 100vh;
  display: flex;
  background: var(--color-bg-layout);
  color: var(--color-text);
  font-family: Arial, sans-serif;
}

/* SIDEBAR */
.sidebar {
  width: 280px;
  background: var(--color-bg-container);
  color: var(--color-text);
  padding: 28px;
  border-right: 1px solid var(--color-border-secondary);
  box-shadow: var(--shadow-main);
}

.brand {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-bottom: 40px;
}

.brand-logo {
  width: 65px;
  height: 65px;
  border-radius: var(--radius-lg);
  background: linear-gradient(
    135deg,
    var(--color-primary),
    var(--color-primary-active)
  );
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 30px;
  box-shadow: var(--shadow-main);
}

.brand h2 {
  margin: 0;
  color: var(--color-text-base);
}

.brand p {
  margin: 4px 0 0;
  color: var(--color-text-secondary);
}

.sidebar nav {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.menu-group {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.menu-title {
  margin: 8px 0 4px;
  color: var(--color-text-tertiary);
  font-size: 12px;
  font-weight: 700;
  letter-spacing: 0.8px;
}

.sidebar nav button {
  width: 100%;
  border: 1px solid transparent;
  background: transparent;
  color: var(--color-text-secondary);
  text-align: left;
  padding: 12px 14px;
  border-radius: var(--radius-lg);
  transition: 0.25s;
  font-weight: 600;
}

.sidebar nav button:hover {
  background: var(--color-primary-bg-hover);
  color: var(--color-primary-text-hover);
  border-color: var(--color-primary-border-hover);
}

.sidebar nav button.active {
  background: var(--color-primary-bg);
  color: var(--color-primary-text-active);
  border-color: var(--color-primary-border);
}

.sidebar nav button.logout {
  color: var(--color-error-text);
}

.sidebar nav button.logout:hover {
  background: var(--color-error-bg-hover);
  color: var(--color-error-hover);
  border-color: var(--color-error-border-hover);
}

/* MAIN */
.main {
  flex: 1;
  min-width: 0;
}

/* TOPBAR */
.topbar {
  min-height: 90px;
  background: var(--color-bg-container);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 30px;
  border-bottom: 1px solid var(--color-border-secondary);
}

.topbar h1 {
  margin: 0;
  color: var(--color-text-base);
}

.subtitle {
  margin: 6px 0 0;
  color: var(--color-text-secondary);
}

.topbar-right {
  display: flex;
  align-items: center;
  gap: 16px;
}

.user-box {
  background: var(--color-primary-bg);
  color: var(--color-primary-text-active);
  padding: 12px 18px;
  border: 1px solid var(--color-primary-border);
  border-radius: var(--radius-lg);
  font-weight: 700;
}

/* DASHBOARD */
.stats {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: var(--margin-lg);
  padding: var(--padding-lg);
}

.stat {
  border-radius: var(--radius-lg);
  padding: 28px;
  box-shadow: var(--shadow-main);
  border: 1px solid var(--color-border-secondary);
  background: var(--color-bg-container);
}

.stat span {
  color: var(--color-text-secondary);
  font-weight: 700;
}

.stat b {
  display: block;
  margin-top: 18px;
  font-size: 28px;
  color: var(--color-text-base);
}

.stat.primary {
  background: var(--color-primary-bg);
  border-color: var(--color-primary-border);
}

.stat.primary b {
  color: var(--color-primary-text-active);
}

.stat.success {
  background: var(--color-success-bg);
  border-color: var(--color-success-border);
}

.stat.success b {
  color: var(--color-success-active);
}

.stat.info {
  background: var(--color-info-bg);
  border-color: var(--color-info-border);
}

.stat.info b {
  color: var(--color-info-active);
}

.stat.warning {
  background: var(--color-warning-bg);
  border-color: var(--color-warning-border);
}

.stat.warning b {
  color: var(--color-warning-active);
}

.report-stats {
  padding: 0;
  margin-top: var(--margin-lg);
  grid-template-columns: repeat(2, 1fr);
}

.chart-section {
  display: grid;
  grid-template-columns: 320px 1fr;
  align-items: center;
  gap: 36px;
  margin-top: var(--margin-lg);
}

.pie-chart {
  width: 280px;
  height: 280px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: var(--shadow-secondary);
}

.pie-center {
  width: 140px;
  height: 140px;
  border-radius: 50%;
  background: var(--color-bg-container);
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  box-shadow: var(--shadow-main);
}

.pie-center strong {
  color: var(--color-text-base);
  font-size: 34px;
}

.pie-center span {
  color: var(--color-text-secondary);
  margin-top: 4px;
}

.chart-legend {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 16px;
}

.legend-item {
  display: grid;
  grid-template-columns: 18px 1fr auto;
  align-items: center;
  gap: 12px;
  padding: 16px;
  border: 1px solid var(--color-border-secondary);
  border-radius: var(--radius-lg);
  background: var(--color-bg-base);
}

.legend-item p {
  margin: 0;
  color: var(--color-text-secondary);
  font-weight: 700;
}

.legend-item b {
  color: var(--color-text-base);
  font-size: 20px;
}

.legend-color {
  width: 14px;
  height: 14px;
  border-radius: 50%;
}

.legend-color.product {
  background: var(--color-primary);
}

.legend-color.customer {
  background: var(--color-success);
}

.legend-color.order {
  background: var(--color-warning);
}

.legend-color.supplier {
  background: var(--color-info);
}

/* PANELS */
.dashboard-box,
.panel {
  margin: 0 var(--margin-lg) var(--margin-lg);
  background: var(--color-bg-container);
  border: 1px solid var(--color-border-secondary);
  border-radius: var(--radius-lg);
  padding: var(--padding-lg);
  box-shadow: var(--shadow-main);
}

.dashboard-box h2,
.panel h2 {
  margin-top: 0;
  color: var(--color-text-base);
}

.dashboard-box p,
.panel p {
  color: var(--color-text-secondary);
}

.arch-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 18px;
  margin-top: var(--margin-lg);
}

.arch-card {
  border-radius: var(--radius-lg);
  padding: 20px;
  border: 1px solid var(--color-border-secondary);
  background: var(--color-bg-base);
}

.arch-card h3 {
  margin-top: 0;
  color: var(--color-text-base);
}

.primary-card {
  background: var(--color-primary-bg);
  border-color: var(--color-primary-border);
}

.success-card {
  background: var(--color-success-bg);
  border-color: var(--color-success-border);
}

.info-card {
  background: var(--color-info-bg);
  border-color: var(--color-info-border);
}

/* TABLE */
.panel-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--margin);
}

table {
  width: 100%;
  margin-top: 20px;
  border-collapse: collapse;
  overflow: hidden;
  border-radius: var(--radius-lg);
}

thead {
  background: var(--color-primary-bg);
}

th {
  color: var(--color-primary-text-active);
  font-weight: 700;
}

th,
td {
  padding: 16px;
  border-bottom: 1px solid var(--color-border-secondary);
  text-align: left;
}

tbody tr:hover {
  background: var(--color-primary-bg);
}

/* BUTTON */
button {
  border: none;
  color: #ffffff;
  padding: 10px 14px;
  border-radius: var(--radius-md);
  cursor: pointer;
  background: var(--color-primary);
  transition: 0.25s;
  font-weight: 600;
}

button:hover {
  background: var(--color-primary-hover);
}

button:active {
  background: var(--color-primary-active);
}

.refresh {
  background: var(--color-info);
}

.refresh:hover {
  background: var(--color-info-hover);
}

.edit {
  background: var(--color-warning);
  margin-right: 8px;
}

.edit:hover {
  background: var(--color-warning-hover);
}

.delete {
  background: var(--color-error);
}

.delete:hover {
  background: var(--color-error-hover);
}

/* MOBILE */
@media (max-width: 1000px) {
  .layout {
    flex-direction: column;
  }

  .sidebar {
    width: 100%;
  }

  .topbar {
    align-items: flex-start;
    flex-direction: column;
    gap: 16px;
    padding: 20px;
  }

  .stats {
    grid-template-columns: 1fr;
  }

  .report-stats {
    grid-template-columns: 1fr;
  }

  .arch-grid {
    grid-template-columns: 1fr;
  }

  .chart-section {
    grid-template-columns: 1fr;
    justify-items: center;
  }

  .chart-legend {
    width: 100%;
    grid-template-columns: 1fr;
  }

  .panel-head {
    align-items: flex-start;
    flex-direction: column;
  }

  table {
    display: block;
    overflow-x: auto;
    white-space: nowrap;
  }
}
</style>
