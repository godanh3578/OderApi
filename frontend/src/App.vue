<script setup>
import { ref, computed, onMounted } from 'vue'
import axios from 'axios'

const API = (import.meta.env.VITE_API_URL || 'http://160.250.132.117:3000').replace(/\/$/, '')

const activePanel = ref('dashboard')

const customers = ref([])
const orders = ref([])
const suppliers = ref([])
const products = ref([])
const payments = ref([])
const debts = ref([])
const outboxMessages = ref([])
const showCustomerModal = ref(false)
const editingCustomer = ref(null)
const loadError = ref('')

const customerForm = ref({
  name: '',
  phone: '',
  email: '',
  address: '',
  debt: 0
})

const checkout = ref({
  customerId: '',
  discountAmount: 0,
  paymentMethod: 'Cash',
  paidAmount: 0
})

const cart = ref([])

async function safeGet(url, fallback = []) {
  try {
    const res = await axios.get(url)
    return Array.isArray(res.data) ? res.data : fallback
  } catch (err) {
    console.error(`Failed to load ${url}`, err)
    loadError.value = 'Khong the tai mot so du lieu tu API. Dang hien thi du lieu hien co hoac du lieu mau.'
    return fallback
  }
}

async function loadAll() {
  loadError.value = ''

  customers.value = await safeGet(`${API}/api/Customers`)
  orders.value = await safeGet(`${API}/api/Orders`)
  suppliers.value = await safeGet(`${API}/api/Suppliers`)

  products.value = await safeGet(`${API}/api/ProductStockCaches`, [
    {
      id: 1,
      productId: 1,
      productCode: 'SP001',
      productName: 'Laptop Gaming',
      sellingPrice: 25000000,
      quantityAvailable: 12,
      stockStatus: 'InStock'
    },
    {
      id: 2,
      productId: 2,
      productCode: 'SP002',
      productName: 'Chuột Logitech',
      sellingPrice: 500000,
      quantityAvailable: 40,
      stockStatus: 'InStock'
    },
    {
      id: 3,
      productId: 3,
      productCode: 'SP003',
      productName: 'Bàn phím cơ',
      sellingPrice: 1200000,
      quantityAvailable: 0,
      stockStatus: 'OutOfStock'
    }
  ])

  payments.value = await safeGet(`${API}/api/Payments`, [])
  debts.value = await safeGet(`${API}/api/Debts`, [])
  outboxMessages.value = await safeGet(`${API}/api/OutboxMessages`, [])
}

function showApiError(action, err) {
  console.error(action, err)
  alert(`${action} khong thanh cong. Vui long kiem tra API va thu lai.`)
}

function openPanel(panel) {
  activePanel.value = panel
}

function isActive(panel) {
  return activePanel.value === panel
}

function totalRevenue() {
  return orders.value.reduce((sum, o) => {
    return sum + Number(o.finalAmount || o.totalAmount || 0)
  }, 0)
}

const paidOrders = computed(() => {
  return orders.value.filter(o => {
    const status = String(o.paymentStatus || o.orderStatus || '').toLowerCase()
    return status === 'paid' || Number(o.status) === 2
  }).length
})

const debtOrders = computed(() => {
  return orders.value.filter(o => {
    const status = String(o.paymentStatus || o.orderStatus || '').toLowerCase()
    return Number(o.debtAmount || 0) > 0 || status === 'partial' || status === 'debt'
  }).length
})

const totalDebt = computed(() => {
  return debts.value.reduce((sum, d) => {
    return sum + Number(d.remainingAmount || d.debtAmount || 0)
  }, 0)
})

const dashboardTotal = computed(() => {
  const total = products.value.length + customers.value.length + orders.value.length + suppliers.value.length
  return total === 0 ? 1 : total
})

const dashboardChartStyle = computed(() => {
  const pp = products.value.length / dashboardTotal.value * 100
  const cp = customers.value.length / dashboardTotal.value * 100
  const op = orders.value.length / dashboardTotal.value * 100
  const sp = suppliers.value.length / dashboardTotal.value * 100

  const p1 = pp
  const p2 = p1 + cp
  const p3 = p2 + op
  const p4 = p3 + sp

  return {
    background: `conic-gradient(
      #1976d2 0% ${p1}%,
      #2e7d32 ${p1}% ${p2}%,
      #ed6c02 ${p2}% ${p3}%,
      #0288d1 ${p3}% ${p4}%
    )`
  }
})

function productId(p) {
  return p.productId || p.id
}

function productCode(p) {
  return p.productCode || `SP${String(productId(p)).padStart(3, '0')}`
}

function productName(p) {
  return p.productName || p.name || 'Chưa có tên'
}

function productPrice(p) {
  return Number(p.sellingPrice || p.price || 0)
}

function productStock(p) {
  return Number(p.quantityAvailable ?? p.stock ?? 0)
}

function addToCart(product) {
  if (productStock(product) <= 0) {
    alert('Sản phẩm đã hết hàng!')
    return
  }

  const id = productId(product)
  const existed = cart.value.find(item => item.productId === id)

  if (existed) {
    if (existed.quantity + 1 > productStock(product)) {
      alert('Số lượng bán vượt quá tồn kho!')
      return
    }

    existed.quantity++
  } else {
    cart.value.push({
      productId: id,
      productCode: productCode(product),
      productName: productName(product),
      unitPrice: productPrice(product),
      quantity: 1,
      stock: productStock(product)
    })
  }
}

function removeFromCart(id) {
  cart.value = cart.value.filter(item => item.productId !== id)
}

function updateCartQuantity(item) {
  if (item.quantity < 1) item.quantity = 1

  if (item.quantity > item.stock) {
    alert('Số lượng bán không được vượt quá tồn kho!')
    item.quantity = item.stock
  }
}

const cartTotal = computed(() => {
  return cart.value.reduce((sum, item) => {
    return sum + Number(item.unitPrice || 0) * Number(item.quantity || 0)
  }, 0)
})

const finalAmount = computed(() => {
  const discount = Number(checkout.value.discountAmount || 0)
  const final = cartTotal.value - discount
  return final < 0 ? 0 : final
})

const debtAmount = computed(() => {
  const paid = Number(checkout.value.paidAmount || 0)
  const debt = finalAmount.value - paid
  return debt > 0 ? debt : 0
})

async function createCheckoutOrder() {
  if (!checkout.value.customerId) {
    alert('Vui lòng chọn khách hàng!')
    return
  }

  if (cart.value.length === 0) {
    alert('Vui lòng thêm sản phẩm vào đơn hàng!')
    return
  }

  if (Number(checkout.value.discountAmount || 0) > cartTotal.value) {
    alert('Chiết khấu không được lớn hơn tổng tiền!')
    return
  }

  const payload = {
    customerId: Number(checkout.value.customerId),
    discountAmount: Number(checkout.value.discountAmount || 0),
    paymentMethod: checkout.value.paymentMethod,
    paidAmount: Number(checkout.value.paidAmount || 0),
    items: cart.value.map(item => ({
      productId: item.productId,
      productCode: item.productCode,
      productName: item.productName,
      quantity: Number(item.quantity),
      unitPrice: Number(item.unitPrice),
      subTotal: Number(item.quantity) * Number(item.unitPrice)
    }))
  }

  try {
    await axios.post(`${API}/api/sales/checkout`, payload)
    await loadAll()

    alert('Tạo đơn hàng thành công!')

    cart.value = []
    checkout.value = {
      customerId: '',
      discountAmount: 0,
      paymentMethod: 'Cash',
      paidAmount: 0
    }

    openPanel('orders')
  } catch (err) {
    showApiError('Tao don hang', err)
  }
}

// ===== CUSTOMERS =====
function addCustomer() {
  editingCustomer.value = null

  customerForm.value = {
    name: '',
    phone: '',
    email: '',
    address: '',
    debt: 0
  }

  showCustomerModal.value = true
}

function editCustomer(customer) {
  editingCustomer.value = customer

  customerForm.value = {
    name: customer.name || '',
    phone: customer.phone || '',
    email: customer.email || '',
    address: customer.address || '',
    debt: Number(customer.debt || customer.currentDebt || 0)
  }

  showCustomerModal.value = true
}

function closeCustomerModal() {
  showCustomerModal.value = false
  editingCustomer.value = null
}

async function saveCustomer() {
  if (!customerForm.value.name.trim()) {
    alert('Vui lòng nhập tên khách hàng!')
    return
  }

  const payload = {
    name: customerForm.value.name,
    phone: customerForm.value.phone,
    email: customerForm.value.email,
    address: customerForm.value.address,
    debt: Number(customerForm.value.debt || 0)
  }

  try {
    if (editingCustomer.value) {
      const res = await axios.put(`${API}/api/Customers/${editingCustomer.value.id}`, {
        ...editingCustomer.value,
        ...payload
      })

      Object.assign(editingCustomer.value, res.data)
    } else {
      const res = await axios.post(`${API}/api/Customers`, payload)
      customers.value.push(res.data)
    }

    closeCustomerModal()
  } catch (err) {
    showApiError(editingCustomer.value ? 'Cap nhat khach hang' : 'Them khach hang', err)
  }
}
async function deleteCustomer(id) {
  if (!window.confirm('Bạn có chắc muốn xóa khách hàng này không?')) return

  try {
    await axios.delete(`${API}/api/Customers/${id}`)
    customers.value = customers.value.filter(item => item.id !== id)
  } catch (err) {
    showApiError('Xoa khach hang', err)
  }
}

// ===== ORDERS =====
function addOrder() {
  openPanel('sales')
}

async function editOrder(order) {
  const statusInput = window.prompt('Trạng thái (0=Pending, 1=Confirmed, 2=Paid, 3=Debt, 4=Cancelled):', order.status ?? 0)
  if (statusInput === null) return

  try {
    const res = await axios.put(`${API}/api/Orders/${order.id}`, {
      ...order,
      status: Number(statusInput)
    })

    Object.assign(order, res.data)
  } catch (err) {
    showApiError('Cap nhat don hang', err)
  }
}

async function deleteOrder(id) {
  if (!window.confirm('Bạn có chắc muốn xóa đơn hàng này không?')) return

  try {
    await axios.delete(`${API}/api/Orders/${id}`)
    orders.value = orders.value.filter(item => item.id !== id)
  } catch (err) {
    showApiError('Xoa don hang', err)
  }
}

// ===== SUPPLIERS =====
async function addSupplier() {
  const name = window.prompt('Nhập tên nhà cung cấp:')
  if (!name) return

  const phone = window.prompt('Nhập số điện thoại:') || ''
  const email = window.prompt('Nhập email:') || ''
  const address = window.prompt('Nhập địa chỉ:') || ''
  const contactPerson = window.prompt('Nhập tên người liên hệ:') || ''

  try {
    const res = await axios.post(`${API}/api/Suppliers`, {
      name,
      phone,
      email,
      address,
      contactPerson
    })

    suppliers.value.push(res.data)
  } catch (err) {
    showApiError('Them nha cung cap', err)
  }
}

async function editSupplier(supplier) {
  const name = window.prompt('Sửa tên nhà cung cấp:', supplier.name)
  if (!name) return

  const phone = window.prompt('Sửa số điện thoại:', supplier.phone) || supplier.phone
  const email = window.prompt('Sửa email:', supplier.email) || supplier.email
  const address = window.prompt('Sửa địa chỉ:', supplier.address) || supplier.address
  const contactPerson = window.prompt('Sửa người liên hệ:', supplier.contactPerson) || supplier.contactPerson

  try {
    const res = await axios.put(`${API}/api/Suppliers/${supplier.id}`, {
      ...supplier,
      name,
      phone,
      email,
      address,
      contactPerson
    })

    Object.assign(supplier, res.data)
  } catch (err) {
    showApiError('Cap nhat nha cung cap', err)
  }
}

async function deleteSupplier(id) {
  if (!window.confirm('Bạn có chắc muốn xóa nhà cung cấp này không?')) return

  try {
    await axios.delete(`${API}/api/Suppliers/${id}`)
    suppliers.value = suppliers.value.filter(item => item.id !== id)
  } catch (err) {
    showApiError('Xoa nha cung cap', err)
  }
}

// ===== DEBTS =====
async function payDebt(debt) {
  const amount = Number(window.prompt('Nhập số tiền khách trả:', debt.remainingAmount || debt.debtAmount || 0) || 0)
  if (amount <= 0) return

  const remainingAmount = Number(debt.remainingAmount || debt.debtAmount || 0)
  if (amount > remainingAmount) {
    alert('So tien tra khong duoc lon hon so cong no con lai.')
    return
  }

  try {
    await axios.post(`${API}/api/Debts/${debt.id}/pay`, { amount })
    await loadAll()
  } catch (err) {
    showApiError('Thanh toan cong no', err)
  }
}

function orderStatusValue(order) {
  if (order.status !== undefined && order.status !== null) return order.status
  return order.orderStatus || order.paymentStatus || 'Pending'
}

function statusLabel(s) {
  if (typeof s === 'string') return s

  return ['Pending', 'Confirmed', 'Paid', 'Debt', 'Cancelled'][s] ?? 'Unknown'
}

function statusClass(s) {
  const value = typeof s === 'string' ? s.toLowerCase() : s

  if (value === 0 || value === 'pending' || value === 'unpaid') return 'status-pending'
  if (value === 1 || value === 'confirmed') return 'status-confirmed'
  if (value === 2 || value === 'paid') return 'status-confirmed'
  if (value === 3 || value === 'debt' || value === 'partial') return 'status-pending'
  if (value === 4 || value === 'cancelled' || value === 'outofstock') return 'status-cancelled'

  return ''
}

onMounted(loadAll)
</script>

<template>
  <div class="layout">
    <!-- SIDEBAR -->
    <aside class="sidebar">
      <div class="brand">
        <div class="brand-logo">🏪</div>
        <div>
          <h2>RetailERP</h2>
          <p>Sales & Inventory</p>
        </div>
      </div>

      <nav>
        <div class="menu-group">
          <div class="menu-title">TRANG CHỦ</div>
          <button :class="{ active: isActive('dashboard') }" @click="openPanel('dashboard')">
            📊 Tổng quan hệ thống
          </button>
        </div>

        <div class="menu-group">
          <div class="menu-title">NHÓM 1 - KHO & SẢN PHẨM</div>
          <button :class="{ active: isActive('products') }" @click="openPanel('products')">
            📦 Sản phẩm & tồn kho
          </button>
        </div>

        <div class="menu-group">
          <div class="menu-title">NHÓM 2 - BÁN HÀNG</div>
          <button :class="{ active: isActive('sales') }" @click="openPanel('sales')">
            🧾 Bán hàng
          </button>

          <button :class="{ active: isActive('orders') }" @click="openPanel('orders')">
            🛒 Đơn hàng
          </button>

          <button :class="{ active: isActive('customers') }" @click="openPanel('customers')">
            👥 Khách hàng
          </button>

          <button :class="{ active: isActive('suppliers') }" @click="openPanel('suppliers')">
            🏬 Nhà cung cấp
          </button>

          <button :class="{ active: isActive('payments') }" @click="openPanel('payments')">
            💳 Thanh toán
          </button>

          <button :class="{ active: isActive('debts') }" @click="openPanel('debts')">
            💰 Công nợ
          </button>

          <button :class="{ active: isActive('integration') }" @click="openPanel('integration')">
            🔄 Đồng bộ dữ liệu
          </button>
        </div>

        <div class="menu-group">
          <div class="menu-title">NHÓM 3 - USER & REPORT</div>
          <button :class="{ active: isActive('reports') }" @click="openPanel('reports')">
            📈 Báo cáo tổng hợp
          </button>
        </div>

        <div class="menu-group">
          <div class="menu-title">HỆ THỐNG</div>
          <button :class="{ active: isActive('settings') }" @click="openPanel('settings')">
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
      <header class="topbar">
        <div>
          <h1>Hệ thống quản lý bán hàng & kho hàng</h1>
          <p class="subtitle">Microservices · ASP.NET Core · VueJS · SQL Server</p>
        </div>

        <div class="topbar-right">
          <button class="refresh" @click="loadAll">
            🔄 Refresh
          </button>

          <div class="user-box">
            👨‍💻 Admin
          </div>
        </div>
      </header>

      <div v-if="loadError" class="api-alert">
        {{ loadError }}
      </div>

      <!-- DASHBOARD -->
      <section v-if="activePanel === 'dashboard'">
        <div class="stats">
          <div class="stat primary">
            <span>Sản phẩm / tồn kho</span>
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
              <p>Trang chủ hiển thị tổng quan cho cả 3 nhóm microservices.</p>
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
                <p>Nhóm 1 - Sản phẩm & tồn kho</p>
                <b>{{ products.length }}</b>
              </div>

              <div class="legend-item">
                <span class="legend-color customer"></span>
                <p>Nhóm 2 - Khách hàng</p>
                <b>{{ customers.length }}</b>
              </div>

              <div class="legend-item">
                <span class="legend-color order"></span>
                <p>Nhóm 2 - Đơn hàng</p>
                <b>{{ orders.length }}</b>
              </div>

              <div class="legend-item">
                <span class="legend-color supplier"></span>
                <p>Nhóm 2 - Nhà cung cấp</p>
                <b>{{ suppliers.length }}</b>
              </div>
            </div>
          </div>
        </div>

        <div class="dashboard-box">
          <h2>🏗️ Kiến trúc 3 nhóm microservices</h2>

          <div class="arch-grid">
            <div class="arch-card primary-card">
              <h3>Nhóm 1 — Product & Inventory</h3>
              <p>Quản lý sản phẩm, danh mục, tồn kho, nhập kho và phát event stock.updated.</p>
            </div>

            <div class="arch-card success-card">
              <h3>Nhóm 2 — Order & Sales</h3>
              <p>Tạo đơn bán hàng, quản lý khách hàng, thanh toán, công nợ và phát event order.created.</p>
            </div>

            <div class="arch-card info-card">
              <h3>Nhóm 3 — User & Report</h3>
              <p>JWT login, phân quyền, báo cáo doanh thu và thống kê tổng hợp.</p>
            </div>
          </div>
        </div>
      </section>

      <!-- PRODUCTS -->
      <section v-if="activePanel === 'products'" class="panel">
        <div class="panel-head">
          <div>
            <h2>📦 Sản phẩm & tồn kho</h2>
            <p>Dữ liệu thuộc Nhóm 1. Nhóm 2 chỉ dùng để kiểm tra hàng còn hay hết khi bán hàng.</p>
          </div>
        </div>

        <table>
          <thead>
            <tr>
              <th>Mã SP</th>
              <th>Tên sản phẩm</th>
              <th>Giá bán</th>
              <th>Tồn kho</th>
              <th>Trạng thái</th>
            </tr>
          </thead>

          <tbody>
            <tr v-for="p in products" :key="productId(p)">
              <td>{{ productCode(p) }}</td>
              <td>{{ productName(p) }}</td>
              <td>{{ productPrice(p).toLocaleString() }} VNĐ</td>
              <td>{{ productStock(p) }}</td>
              <td>
                <span :class="productStock(p) > 0 ? 'status-confirmed' : 'status-cancelled'">
                  {{ productStock(p) > 0 ? 'Còn hàng' : 'Hết hàng' }}
                </span>
              </td>
            </tr>
          </tbody>
        </table>
      </section>

      <!-- SALES -->
      <section v-if="activePanel === 'sales'" class="panel">
        <div class="panel-head">
          <div>
            <h2>🧾 Bán hàng</h2>
            <p>Tạo đơn bán hàng, tính tổng tiền, áp dụng chiết khấu và ghi nhận thanh toán.</p>
          </div>
        </div>

        <div class="sales-grid">
          <div>
            <h3>Danh sách sản phẩm</h3>

            <table>
              <thead>
                <tr>
                  <th>Mã SP</th>
                  <th>Tên sản phẩm</th>
                  <th>Giá</th>
                  <th>Tồn</th>
                  <th>Chọn</th>
                </tr>
              </thead>

              <tbody>
                <tr v-for="p in products" :key="productId(p)">
                  <td>{{ productCode(p) }}</td>
                  <td>{{ productName(p) }}</td>
                  <td>{{ productPrice(p).toLocaleString() }} VNĐ</td>
                  <td>{{ productStock(p) }}</td>
                  <td>
                    <button :disabled="productStock(p) <= 0" @click="addToCart(p)">
                      + Thêm
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <div>
            <h3>Giỏ hàng / đơn bán</h3>

            <div class="form-box">
              <label>Khách hàng</label>
              <select v-model="checkout.customerId">
                <option value="">-- Chọn khách hàng --</option>
                <option v-for="c in customers" :key="c.id" :value="c.id">
                  {{ c.name }} - {{ c.phone }}
                </option>
              </select>

              <label>Chiết khấu</label>
              <input type="number" v-model.number="checkout.discountAmount" min="0" />

              <label>Phương thức thanh toán</label>
              <select v-model="checkout.paymentMethod">
                <option value="Cash">Tiền mặt</option>
                <option value="BankTransfer">Chuyển khoản</option>
                <option value="EWallet">Ví điện tử</option>
                <option value="QR">QR</option>
              </select>

              <label>Số tiền khách trả</label>
              <input type="number" v-model.number="checkout.paidAmount" min="0" />
            </div>

            <table>
              <thead>
                <tr>
                  <th>Sản phẩm</th>
                  <th>SL</th>
                  <th>Đơn giá</th>
                  <th>Thành tiền</th>
                  <th>Xóa</th>
                </tr>
              </thead>

              <tbody>
                <tr v-for="item in cart" :key="item.productId">
                  <td>{{ item.productName }}</td>
                  <td>
                    <input
                      class="qty-input"
                      type="number"
                      v-model.number="item.quantity"
                      min="1"
                      :max="item.stock"
                      @change="updateCartQuantity(item)"
                    />
                  </td>
                  <td>{{ Number(item.unitPrice).toLocaleString() }}</td>
                  <td>{{ (item.unitPrice * item.quantity).toLocaleString() }}</td>
                  <td>
                    <button class="delete" @click="removeFromCart(item.productId)">
                      Xóa
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>

            <div class="checkout-summary">
              <p>
                Tổng tiền:
                <b>{{ cartTotal.toLocaleString() }} VNĐ</b>
              </p>

              <p>
                Chiết khấu:
                <b>{{ Number(checkout.discountAmount || 0).toLocaleString() }} VNĐ</b>
              </p>

              <p>
                Thành tiền:
                <b>{{ finalAmount.toLocaleString() }} VNĐ</b>
              </p>

              <p>
                Khách trả:
                <b>{{ Number(checkout.paidAmount || 0).toLocaleString() }} VNĐ</b>
              </p>

              <p>
                Còn nợ:
                <b>{{ debtAmount.toLocaleString() }} VNĐ</b>
              </p>

              <button @click="createCheckoutOrder">
                ✅ Tạo đơn bán hàng
              </button>
            </div>
          </div>
        </div>
      </section>

      <!-- CUSTOMERS -->
      <section v-if="activePanel === 'customers'" class="panel">
        <div class="panel-head">
          <div>
            <h2>👥 Quản lý khách hàng</h2>
            <p>Quản lý thông tin, lịch sử mua và công nợ khách hàng.</p>
          </div>

          <button @click="addCustomer">
            + Thêm khách hàng
          </button>
        </div>

        <table>
          <thead>
            <tr>
              <th>ID</th>
              <th>Tên</th>
              <th>SĐT</th>
              <th>Email</th>
              <th>Địa chỉ</th>
              <th>Công nợ</th>
              <th>Thao tác</th>
            </tr>
          </thead>

          <tbody>
            <tr v-for="c in customers" :key="c.id">
              <td>{{ c.id }}</td>
              <td>{{ c.name }}</td>
              <td>{{ c.phone }}</td>
              <td>{{ c.email }}</td>
              <td>{{ c.address }}</td>
              <td>{{ Number(c.debt || c.currentDebt || 0).toLocaleString() }} VNĐ</td>
              <td>
                <button class="edit" @click="editCustomer(c)">
                  Sửa
                </button>

                <button class="delete" @click="deleteCustomer(c.id)">
                  Xóa
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </section>

      <!-- ORDERS -->
      <section v-if="activePanel === 'orders'" class="panel">
        <div class="panel-head">
          <div>
            <h2>🛒 Quản lý đơn hàng</h2>
            <p>Danh sách đơn bán hàng do Nhóm 2 xử lý.</p>
          </div>

          <button @click="addOrder">
            + Tạo đơn bán hàng
          </button>
        </div>

        <table>
          <thead>
            <tr>
              <th>ID</th>
              <th>Khách hàng</th>
              <th>Ngày đặt</th>
              <th>Tổng tiền</th>
              <th>Giảm giá</th>
              <th>Thành tiền</th>
              <th>Đã trả</th>
              <th>Còn nợ</th>
              <th>Trạng thái</th>
              <th>Thao tác</th>
            </tr>
          </thead>

          <tbody>
            <tr v-for="o in orders" :key="o.id">
              <td>{{ o.id }}</td>
              <td>{{ o.customer?.name || 'ID: ' + o.customerId }}</td>
              <td>{{ o.orderDate ? new Date(o.orderDate).toLocaleDateString('vi-VN') : '' }}</td>
              <td>{{ Number(o.totalAmount || 0).toLocaleString() }} VNĐ</td>
              <td>{{ Number(o.discountAmount || o.discount || 0).toLocaleString() }} VNĐ</td>
              <td>{{ Number(o.finalAmount || o.totalAmount || 0).toLocaleString() }} VNĐ</td>
              <td>{{ Number(o.paidAmount || 0).toLocaleString() }} VNĐ</td>
              <td>{{ Number(o.debtAmount || 0).toLocaleString() }} VNĐ</td>
              <td>
                <span :class="statusClass(orderStatusValue(o))">
                  {{ statusLabel(orderStatusValue(o)) }}
                </span>
              </td>
              <td>
                <button class="edit" @click="editOrder(o)">
                  Sửa
                </button>

                <button class="delete" @click="deleteOrder(o.id)">
                  Xóa
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </section>

      <!-- SUPPLIERS -->
      <section v-if="activePanel === 'suppliers'" class="panel">
        <div class="panel-head">
          <div>
            <h2>🏬 Quản lý nhà cung cấp</h2>
            <p>Nhóm 2 chỉ quản lý thông tin liên hệ nhà cung cấp, không xử lý nhập kho.</p>
          </div>

          <button @click="addSupplier">
            + Thêm nhà cung cấp
          </button>
        </div>

        <table>
          <thead>
            <tr>
              <th>ID</th>
              <th>Tên</th>
              <th>SĐT</th>
              <th>Email</th>
              <th>Địa chỉ</th>
              <th>Người liên hệ</th>
              <th>Thao tác</th>
            </tr>
          </thead>

          <tbody>
            <tr v-for="s in suppliers" :key="s.id">
              <td>{{ s.id }}</td>
              <td>{{ s.name }}</td>
              <td>{{ s.phone }}</td>
              <td>{{ s.email }}</td>
              <td>{{ s.address }}</td>
              <td>{{ s.contactPerson }}</td>
              <td>
                <button class="edit" @click="editSupplier(s)">
                  Sửa
                </button>

                <button class="delete" @click="deleteSupplier(s.id)">
                  Xóa
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </section>

      <!-- PAYMENTS -->
      <section v-if="activePanel === 'payments'" class="panel">
        <div class="panel-head">
          <div>
            <h2>💳 Thanh toán</h2>
            <p>Ghi nhận và theo dõi các giao dịch thanh toán của đơn hàng.</p>
          </div>
        </div>

        <table>
          <thead>
            <tr>
              <th>ID</th>
              <th>Mã thanh toán</th>
              <th>Mã đơn</th>
              <th>Phương thức</th>
              <th>Số tiền</th>
              <th>Ngày thanh toán</th>
              <th>Trạng thái</th>
            </tr>
          </thead>

          <tbody>
            <tr v-for="p in payments" :key="p.id">
              <td>{{ p.id }}</td>
              <td>{{ p.paymentCode || 'PAY' + p.id }}</td>
              <td>{{ p.orderId || '-' }}</td>
              <td>{{ p.paymentMethod }}</td>
              <td>{{ Number(p.amount || 0).toLocaleString() }} VNĐ</td>
              <td>{{ p.paymentDate ? new Date(p.paymentDate).toLocaleDateString('vi-VN') : '' }}</td>
              <td>
                <span :class="statusClass(p.paymentStatus)">
                  {{ p.paymentStatus || 'Paid' }}
                </span>
              </td>
            </tr>
          </tbody>
        </table>
      </section>

      <!-- DEBTS -->
      <section v-if="activePanel === 'debts'" class="panel">
        <div class="panel-head">
          <div>
            <h2>💰 Công nợ khách hàng</h2>
            <p>Theo dõi số tiền khách hàng còn nợ và ghi nhận trả nợ.</p>
          </div>
        </div>

        <div class="stats report-stats">
          <div class="stat warning">
            <span>Tổng công nợ</span>
            <b>{{ totalDebt.toLocaleString() }} VNĐ</b>
          </div>

          <div class="stat success">
            <span>Số khoản nợ</span>
            <b>{{ debts.length }}</b>
          </div>
        </div>

        <table>
          <thead>
            <tr>
              <th>ID</th>
              <th>Khách hàng</th>
              <th>Mã đơn</th>
              <th>Số tiền nợ</th>
              <th>Đã trả</th>
              <th>Còn lại</th>
              <th>Trạng thái</th>
              <th>Thao tác</th>
            </tr>
          </thead>

          <tbody>
            <tr v-for="d in debts" :key="d.id">
              <td>{{ d.id }}</td>
              <td>{{ d.customer?.name || 'ID: ' + d.customerId }}</td>
              <td>{{ d.orderId || '-' }}</td>
              <td>{{ Number(d.debtAmount || 0).toLocaleString() }} VNĐ</td>
              <td>{{ Number(d.paidAmount || 0).toLocaleString() }} VNĐ</td>
              <td>{{ Number(d.remainingAmount || d.debtAmount || 0).toLocaleString() }} VNĐ</td>
              <td>
                <span :class="statusClass(d.debtStatus)">
                  {{ d.debtStatus || 'Unpaid' }}
                </span>
              </td>
              <td>
                <button class="edit" @click="payDebt(d)">
                  Trả nợ
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </section>

      <!-- INTEGRATION -->
      <section v-if="activePanel === 'integration'" class="panel">
        <div class="panel-head">
          <div>
            <h2>🔄 Đồng bộ dữ liệu microservices</h2>
            <p>Thể hiện luồng Nhóm 2 nhận stock.updated và gửi order.created.</p>
          </div>
        </div>

        <div class="arch-grid">
          <div class="arch-card primary-card">
            <h3>Consume stock.updated</h3>
            <p>Nhận dữ liệu tồn kho từ Product & Inventory Service.</p>
            <b>{{ products.length }} sản phẩm/tồn kho</b>
          </div>

          <div class="arch-card success-card">
            <h3>Publish order.created</h3>
            <p>Gửi dữ liệu đơn hàng sang User & Report Service.</p>
            <b>{{ orders.length }} đơn hàng</b>
          </div>

          <div class="arch-card info-card">
            <h3>Outbox Messages</h3>
            <p>Lưu event trước khi gửi để tránh mất dữ liệu.</p>
            <b>{{ outboxMessages.length }} event</b>
          </div>
        </div>

        <table>
          <thead>
            <tr>
              <th>ID</th>
              <th>Event</th>
              <th>Trạng thái</th>
              <th>Ngày tạo</th>
            </tr>
          </thead>

          <tbody>
            <tr v-for="m in outboxMessages" :key="m.id">
              <td>{{ m.id }}</td>
              <td>{{ m.eventName }}</td>
              <td>{{ m.status }}</td>
              <td>{{ m.createdAt ? new Date(m.createdAt).toLocaleString('vi-VN') : '' }}</td>
            </tr>
          </tbody>
        </table>
      </section>

      <!-- REPORTS -->
      <section v-if="activePanel === 'reports'" class="panel">
        <h2>📈 Báo cáo tổng hợp — Nhóm 3</h2>
        <p>Phần này thuộc User & Report Service. Dữ liệu được tổng hợp từ event order.created do Nhóm 2 gửi sang.</p>

        <div class="stats report-stats">
          <div class="stat warning">
            <span>Tổng doanh thu</span>
            <b>{{ totalRevenue().toLocaleString() }} VNĐ</b>
          </div>

          <div class="stat success">
            <span>Tổng đơn hàng</span>
            <b>{{ orders.length }}</b>
          </div>

          <div class="stat primary">
            <span>Đơn đã thanh toán</span>
            <b>{{ paidOrders }}</b>
          </div>

          <div class="stat info">
            <span>Đơn còn công nợ</span>
            <b>{{ debtOrders }}</b>
          </div>
        </div>
      </section>

      <!-- SETTINGS -->
      <section v-if="activePanel === 'settings'" class="panel">
        <h2>⚙️ Cài đặt hệ thống</h2>

        <div class="arch-grid">
          <div class="arch-card primary-card">
            <h3>JWT Authentication</h3>
            <p>Đăng nhập và phân quyền Admin, Sales, Warehouse.</p>
          </div>

          <div class="arch-card info-card">
            <h3>Ocelot API Gateway</h3>
            <p>Điểm vào duy nhất, định tuyến request tới đúng service.</p>
          </div>

          <div class="arch-card success-card">
            <h3>SQL Server</h3>
            <p>Mỗi service có database riêng: ProductDB, OrderDB, UserDB.</p>
          </div>
        </div>
      </section>
            <!-- CUSTOMER MODAL -->
      <div v-if="showCustomerModal" class="modal-backdrop">
        <div class="modal-card">
          <div class="modal-head">
            <h2>
              {{ editingCustomer ? 'Sửa khách hàng' : 'Thêm khách hàng' }}
            </h2>

            <button class="modal-close" @click="closeCustomerModal">
              ✕
            </button>
          </div>

          <div class="customer-form-grid">
            <div class="form-field">
              <label>Tên khách hàng</label>
              <input
                v-model="customerForm.name"
                type="text"
                placeholder="Nhập tên khách hàng"
              />
            </div>

            <div class="form-field">
              <label>Số điện thoại</label>
              <input
                v-model="customerForm.phone"
                type="text"
                placeholder="Nhập số điện thoại"
              />
            </div>

            <div class="form-field">
              <label>Email</label>
              <input
                v-model="customerForm.email"
                type="email"
                placeholder="Nhập email"
              />
            </div>

            <div class="form-field">
              <label>Công nợ</label>
              <input
                v-model.number="customerForm.debt"
                type="number"
                min="0"
                placeholder="Nhập công nợ"
              />
            </div>

            <div class="form-field full">
              <label>Địa chỉ</label>
              <textarea
                v-model="customerForm.address"
                rows="3"
                placeholder="Nhập địa chỉ"
              ></textarea>
            </div>
          </div>

          <div class="modal-actions">
            <button class="cancel-btn" @click="closeCustomerModal">
              Hủy
            </button>

            <button @click="saveCustomer">
              {{ editingCustomer ? 'Lưu thay đổi' : 'Thêm khách hàng' }}
            </button>
          </div>
        </div>
      </div>
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
  --color-success-hover: #1b5e20;
  --color-success-active: #0d3c0d;
  --color-success-text: #2e7d32;

  --color-warning: #ed6c02;
  --color-warning-bg: #fff3e0;
  --color-warning-border: #ffcc02;
  --color-warning-hover: #e65100;
  --color-warning-active: #bf360c;
  --color-warning-text: #ed6c02;

  --color-error: #d32f2f;
  --color-error-bg: #ffebee;
  --color-error-border: #ef9a9a;
  --color-error-hover: #c62828;
  --color-error-text: #d32f2f;

  --color-info: #0288d1;
  --color-info-bg: #e1f5fe;
  --color-info-border: #81d4fa;
  --color-info-hover: #0277bd;
  --color-info-active: #01579b;
  --color-info-text: #0288d1;

  --color-text-base: #121212;
  --color-text: rgba(0, 0, 0, 0.87);
  --color-text-secondary: rgba(0, 0, 0, 0.6);
  --color-text-tertiary: rgba(0, 0, 0, 0.38);
  --color-bg-base: #fafafa;
  --color-bg-container: #ffffff;
  --color-bg-layout: #f5f5f5;
  --color-border: #e0e0e0;
  --color-border-secondary: #eeeeee;

  --radius-md: 4px;
  --radius-lg: 8px;
  --padding-lg: 24px;
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

.sidebar {
  width: 280px;
  background: var(--color-bg-container);
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
  background: linear-gradient(135deg, var(--color-primary), var(--color-primary-active));
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 30px;
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
  background: var(--color-error-bg);
  color: var(--color-error-hover);
  border-color: var(--color-error-border);
}

.main {
  flex: 1;
  min-width: 0;
}

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

.api-alert {
  margin: var(--margin-lg) var(--margin-lg) 0;
  padding: 12px 16px;
  border: 1px solid var(--color-warning-border);
  border-radius: var(--radius-lg);
  background: var(--color-warning-bg);
  color: var(--color-warning-active);
  font-weight: 700;
}

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

button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

button:disabled:hover {
  background: var(--color-primary);
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

.status-pending {
  background: var(--color-warning-bg);
  color: var(--color-warning-text);
  padding: 4px 10px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 700;
}

.status-confirmed {
  background: var(--color-success-bg);
  color: var(--color-success-text);
  padding: 4px 10px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 700;
}

.status-cancelled {
  background: var(--color-error-bg);
  color: var(--color-error-text);
  padding: 4px 10px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 700;
}

.sales-grid {
  display: grid;
  grid-template-columns: 1.1fr 1fr;
  gap: 24px;
  margin-top: 20px;
}

.form-box {
  display: grid;
  gap: 10px;
  margin-bottom: 18px;
  padding: 16px;
  border: 1px solid var(--color-border-secondary);
  border-radius: var(--radius-lg);
  background: var(--color-bg-base);
}

.form-box label {
  font-weight: 700;
  color: var(--color-text-secondary);
}

.form-box input,
.form-box select,
.qty-input {
  width: 100%;
  padding: 10px 12px;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  outline: none;
}

.form-box input:focus,
.form-box select:focus,
.qty-input:focus {
  border-color: var(--color-primary);
}

.qty-input {
  max-width: 80px;
}

.checkout-summary {
  margin-top: 18px;
  padding: 18px;
  border-radius: var(--radius-lg);
  background: var(--color-primary-bg);
  border: 1px solid var(--color-primary-border);
}

.checkout-summary p {
  display: flex;
  justify-content: space-between;
  margin: 8px 0;
}

.checkout-summary b {
  color: var(--color-primary-active);
}

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

  .sales-grid {
    grid-template-columns: 1fr;
  }
    table {
    display: block;
    overflow-x: auto;
    white-space: nowrap;
  }
}

.modal-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.45);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 999;
  padding: 24px;
}

.modal-card {
  width: min(720px, 100%);
  background: var(--color-bg-container);
  border-radius: 16px;
  box-shadow: var(--shadow-secondary);
  padding: 24px;
  animation: modalFadeIn 0.2s ease;
}

.modal-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 20px;
}

.modal-head h2 {
  margin: 0;
  color: var(--color-text-base);
}

.modal-close {
  width: 38px;
  height: 38px;
  padding: 0;
  border-radius: 50%;
  background: var(--color-error);
}

.customer-form-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 16px;
}

.form-field {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.form-field.full {
  grid-column: 1 / -1;
}

.form-field label {
  font-weight: 700;
  color: var(--color-text-secondary);
}

.form-field input,
.form-field textarea {
  width: 100%;
  padding: 12px 14px;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  outline: none;
  font-size: 14px;
  font-family: inherit;
}

.form-field input:focus,
.form-field textarea:focus {
  border-color: var(--color-primary);
  box-shadow: 0 0 0 3px var(--color-primary-bg);
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  margin-top: 24px;
}

.cancel-btn {
  background: #6b7280;
}

.cancel-btn:hover {
  background: #4b5563;
}

@keyframes modalFadeIn {
  from {
    opacity: 0;
    transform: translateY(12px);
  }

  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@media (max-width: 700px) {
  .customer-form-grid {
    grid-template-columns: 1fr;
  }
}
</style>
