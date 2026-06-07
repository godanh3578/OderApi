<script setup>
import { ref, computed, onMounted } from 'vue'
import axios from 'axios'

const API = (import.meta.env.VITE_API_URL || 'http://160.250.132.117:3000').replace(/\/$/, '')

// ===== PAGE STATE =====
const activePage = ref('shop')
const searchText = ref('')
const showSearchPanel = ref(false)
const selectedCategory = ref('Tất cả')
const cartOpen = ref(false)

// ===== AUTH STATE =====
const showAuthModal = ref(false)
const authMode = ref('login')
const authError = ref('')
const currentUser = ref(null)

const loginForm = ref({
  phone: '',
  password: ''
})

const registerForm = ref({
  fullName: '',
  phone: '',
  email: '',
  address: '',
  password: ''
})

// ===== MODAL STATE =====
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

const showSupplierModal = ref(false)
const editingSupplier = ref(null)
const supplierForm = ref({
  name: '',
  contactPerson: '',
  phone: '',
  email: '',
  address: '',
  status: 'Active'
})

const showDebtModal = ref(false)
const selectedDebt = ref(null)
const debtPayForm = ref({
  amount: 0,
  paymentMethod: 'Cash'
})

// ===== DATA =====
const products = ref([])
const customers = ref([])
const suppliers = ref([])
const orders = ref([])
const payments = ref([])
const debts = ref([])
const outboxMessages = ref([])
const cart = ref([])

const checkout = ref({
  customerId: null,
  discountAmount: 0,
  paymentMethod: 'Cash',
  paidAmount: 0
})

// ===== DEMO DATA =====
const demoProducts = [
  {
    id: 1,
    productId: 1,
    productCode: 'GD001',
    productName: 'Nồi cơm điện Sunhouse 1.8L',
    categoryName: 'Gia dụng',
    sellingPrice: 650000,
    quantityAvailable: 12,
    stockStatus: 'InStock',
    image: '🍚'
  },
  {
    id: 2,
    productId: 2,
    productCode: 'GD002',
    productName: 'Máy xay sinh tố Philips',
    categoryName: 'Gia dụng',
    sellingPrice: 890000,
    quantityAvailable: 8,
    stockStatus: 'InStock',
    image: '🥤'
  },
  {
    id: 3,
    productId: 3,
    productCode: 'GD003',
    productName: 'Quạt điện Panasonic',
    categoryName: 'Gia dụng',
    sellingPrice: 780000,
    quantityAvailable: 0,
    stockStatus: 'OutOfStock',
    image: '🌀'
  },
  {
    id: 4,
    productId: 4,
    productCode: 'DT001',
    productName: 'Chuột Logitech M331',
    categoryName: 'Điện tử',
    sellingPrice: 320000,
    quantityAvailable: 30,
    stockStatus: 'InStock',
    image: '🖱️'
  },
  {
    id: 5,
    productId: 5,
    productCode: 'DT002',
    productName: 'Bàn phím cơ AKKO',
    categoryName: 'Điện tử',
    sellingPrice: 1290000,
    quantityAvailable: 9,
    stockStatus: 'InStock',
    image: '⌨️'
  },
  {
    id: 6,
    productId: 6,
    productCode: 'TP001',
    productName: 'Gạo ST25 túi 5kg',
    categoryName: 'Thực phẩm',
    sellingPrice: 185000,
    quantityAvailable: 25,
    stockStatus: 'InStock',
    image: '🍚'
  },
  {
    id: 7,
    productId: 7,
    productCode: 'TT001',
    productName: 'Áo thun cotton nam',
    categoryName: 'Thời trang',
    sellingPrice: 150000,
    quantityAvailable: 18,
    stockStatus: 'InStock',
    image: '👕'
  },
  {
    id: 8,
    productId: 8,
    productCode: 'VP001',
    productName: 'Bút bi Thiên Long hộp 20 cây',
    categoryName: 'Văn phòng phẩm',
    sellingPrice: 65000,
    quantityAvailable: 50,
    stockStatus: 'InStock',
    image: '🖊️'
  }
]

const demoCustomers = [
  {
    id: 1,
    name: 'Nguyễn Văn A',
    phone: '0912345678',
    email: 'a@gmail.com',
    address: 'Hà Nội',
    debt: 0
  }
]

const demoSuppliers = [
  {
    id: 1,
    name: 'Công ty Gia dụng ABC',
    contactPerson: 'Anh Minh',
    phone: '0988000111',
    email: 'abc@gmail.com',
    address: 'Hà Nội',
    status: 'Active'
  }
]

// ===== API =====
async function safeGet(url, fallback = []) {
  try {
    const res = await axios.get(url)
    return Array.isArray(res.data) ? res.data : fallback
  } catch {
    return fallback
  }
}

async function loadAll() {
  products.value = await safeGet(`${API}/api/ProductStockCaches`, demoProducts)
  customers.value = await safeGet(`${API}/api/Customers`, demoCustomers)
  suppliers.value = await safeGet(`${API}/api/Suppliers`, demoSuppliers)
  orders.value = await safeGet(`${API}/api/Orders`, [])
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

// ===== HELPERS =====
function openPage(page) {
  activePage.value = page
}

function isActive(page) {
  return activePage.value === page
}

function getNextId(list) {
  if (!list || list.length === 0) return 1

  return Math.max(
    ...list.map(item => Number(item.id || item.customerId || item.supplierId || item.orderId || item.paymentId || item.debtId || 0))
  ) + 1
}

function productId(product) {
  return Number(product.productId || product.id)
}

function productCode(product) {
  return product.productCode || `SP${String(productId(product)).padStart(3, '0')}`
}

function productName(product) {
  return product.productName || product.name || 'Chưa có tên'
}

function productCategory(product) {
  return product.categoryName || product.category || 'Khác'
}

function productPrice(product) {
  return Number(product.sellingPrice || product.price || 0)
}

function productStock(product) {
  return Number(product.quantityAvailable ?? product.stock ?? 0)
}

function customerId(customer) {
  return Number(customer.customerId || customer.id)
}

function customerName(customer) {
  return customer.fullName || customer.name || 'Khách hàng'
}

function supplierId(supplier) {
  return Number(supplier.supplierId || supplier.id)
}

function supplierName(supplier) {
  return supplier.supplierName || supplier.name || 'Nhà cung cấp'
}

function formatMoney(value) {
  return Number(value || 0).toLocaleString('vi-VN') + ' VNĐ'
}

function statusLabel(status) {
  if (typeof status === 'string') return status

  return ['Pending', 'Confirmed', 'Paid', 'Debt', 'Cancelled'][Number(status)] || 'Unknown'
}

function statusClass(status) {
  const value = typeof status === 'string' ? status.toLowerCase() : Number(status)

  if (value === 'paid' || value === 2 || value === 'confirmed' || value === 1) return 'status-success'
  if (value === 'partial' || value === 'debt' || value === 3 || value === 'pending' || value === 0 || value === 'unpaid') return 'status-warning'
  if (value === 'cancelled' || value === 4 || value === 'outofstock') return 'status-error'

  return 'status-info'
}

function selectSearchProduct(product) {
  searchText.value = productName(product)
  selectedCategory.value = productCategory(product)
  showSearchPanel.value = false
}

function closeSearchPanelLater() {
  window.setTimeout(() => {
    showSearchPanel.value = false
  }, 160)
}

function clearSearchText() {
  searchText.value = ''
  selectedCategory.value = 'Tất cả'
  showSearchPanel.value = true
}

// ===== COMPUTED =====
const categories = computed(() => {
  const values = new Set(products.value.map(productCategory))
  return ['Tất cả', ...Array.from(values)]
})

const filteredProducts = computed(() => {
  const keyword = searchText.value.trim().toLowerCase()

  return products.value.filter(product => {
    const matchCategory =
      selectedCategory.value === 'Tất cả' || productCategory(product) === selectedCategory.value

    const matchSearch =
      !keyword ||
      productName(product).toLowerCase().includes(keyword) ||
      productCode(product).toLowerCase().includes(keyword)

    return matchCategory && matchSearch
  })
})

const searchSuggestions = computed(() => {
  const keyword = searchText.value.trim().toLowerCase()

  if (!keyword) return []

  return products.value
    .filter(product => {
      return (
        productName(product).toLowerCase().includes(keyword) ||
        productCode(product).toLowerCase().includes(keyword) ||
        productCategory(product).toLowerCase().includes(keyword)
      )
    })
    .slice(0, 6)
})

const popularProducts = computed(() => {
  return [...products.value]
    .sort((a, b) => {
      const soldA = Number(a.soldCount || a.totalSold || a.popularScore || 0)
      const soldB = Number(b.soldCount || b.totalSold || b.popularScore || 0)

      if (soldB !== soldA) return soldB - soldA

      return productStock(b) - productStock(a)
    })
    .slice(0, 6)
})

const cartTotalQuantity = computed(() => {
  return cart.value.reduce((sum, item) => sum + Number(item.quantity || 0), 0)
})

const cartTotal = computed(() => {
  return cart.value.reduce((sum, item) => {
    return sum + Number(item.unitPrice || 0) * Number(item.quantity || 0)
  }, 0)
})

const finalAmount = computed(() => {
  const value = cartTotal.value - Number(checkout.value.discountAmount || 0)
  return value > 0 ? value : 0
})

const debtAmount = computed(() => {
  const value = finalAmount.value - Number(checkout.value.paidAmount || 0)
  return value > 0 ? value : 0
})

const totalRevenue = computed(() => {
  return orders.value.reduce((sum, order) => sum + Number(order.finalAmount || order.totalAmount || 0), 0)
})

const totalDebt = computed(() => {
  return debts.value.reduce((sum, debt) => sum + Number(debt.remainingAmount || debt.debtAmount || 0), 0)
})

const myOrders = computed(() => {
  if (!currentUser.value) return []

  return orders.value.filter(order => Number(order.customerId) === Number(currentUser.value.customerId))
})

// ===== AUTH =====
function openAuth(mode = 'login') {
  authMode.value = mode
  authError.value = ''
  showAuthModal.value = true
}

function closeAuth() {
  showAuthModal.value = false
  authError.value = ''
}

async function registerCustomer() {
  authError.value = ''

  if (!registerForm.value.fullName.trim()) {
    authError.value = 'Vui lòng nhập họ tên.'
    return
  }

  if (!registerForm.value.phone.trim()) {
    authError.value = 'Vui lòng nhập số điện thoại.'
    return
  }

  if (!registerForm.value.password.trim()) {
    authError.value = 'Vui lòng nhập mật khẩu.'
    return
  }

  const payload = {
    name: registerForm.value.fullName,
    fullName: registerForm.value.fullName,
    phone: registerForm.value.phone,
    email: registerForm.value.email,
    address: registerForm.value.address,
    debt: 0,
    currentDebt: 0
  }

  let createdCustomer = null

  try {
    const res = await axios.post(`${API}/api/Customers`, payload)
    createdCustomer = res.data
  } catch {
    const id = getNextId(customers.value)

    createdCustomer = {
      id,
      customerId: id,
      ...payload
    }
  }

  customers.value.push(createdCustomer)

  currentUser.value = {
    role: 'Customer',
    customerId: customerId(createdCustomer),
    fullName: customerName(createdCustomer),
    phone: createdCustomer.phone,
    email: createdCustomer.email,
    address: createdCustomer.address
  }

  checkout.value.customerId = currentUser.value.customerId

  registerForm.value = {
    fullName: '',
    phone: '',
    email: '',
    address: '',
    password: ''
  }

  closeAuth()
}

function loginCustomer() {
  authError.value = ''

  if (!loginForm.value.phone.trim()) {
    authError.value = 'Vui lòng nhập số điện thoại.'
    return
  }

  const found = customers.value.find(customer => String(customer.phone) === String(loginForm.value.phone))

  if (!found) {
    authError.value = 'Không tìm thấy khách hàng. Vui lòng đăng ký.'
    return
  }

  currentUser.value = {
    role: 'Customer',
    customerId: customerId(found),
    fullName: customerName(found),
    phone: found.phone,
    email: found.email,
    address: found.address
  }

  checkout.value.customerId = currentUser.value.customerId

  loginForm.value = {
    phone: '',
    password: ''
  }

  closeAuth()
}

function logout() {
  currentUser.value = null
  checkout.value.customerId = null
  openPage('shop')
}

// ===== CART =====
function productStock(p) {
  return Number(p.quantityAvailable ?? p.stock ?? 0)
}

function addToCart(product) {
  if (productStock(product) <= 0) {
    alert('Sản phẩm đã hết hàng.')
    return
  }

  const id = productId(product)
  const existed = cart.value.find(item => item.productId === id)

  if (existed) {
    if (existed.quantity + 1 > productStock(product)) {
      alert('Số lượng mua không được vượt quá tồn kho.')
      return
    }

    existed.quantity++
  } else {
    cart.value.push({
      productId: id,
      productCode: productCode(product),
      productName: productName(product),
      categoryName: productCategory(product),
      unitPrice: productPrice(product),
      quantity: 1,
      stock: productStock(product),
      image: product.image || '📦'
    })
  }

  cartOpen.value = true
}

function removeFromCart(id) {
  cart.value = cart.value.filter(item => item.productId !== id)
}

function updateQuantity(item, value) {
  const quantity = Number(value)

  if (quantity < 1) {
    item.quantity = 1
    return
  }

  if (quantity > item.stock) {
    alert('Số lượng mua không được vượt quá tồn kho.')
    item.quantity = item.stock
    return
  }

  item.quantity = quantity
}

function reduceProductStock(items) {
  for (const item of items) {
    const product = products.value.find(p => productId(p) === Number(item.productId))
    if (!product) continue

    const remain = productStock(product) - Number(item.quantity || 0)

    if (product.quantityAvailable !== undefined) {
      product.quantityAvailable = remain > 0 ? remain : 0
    } else {
      product.stock = remain > 0 ? remain : 0
    }

    product.stockStatus = productStock(product) > 0 ? 'InStock' : 'OutOfStock'
  }
}

// ===== ORDER CHECKOUT =====
async function createOrder() {
  if (cart.value.length === 0) {
    alert('Giỏ hàng đang trống.')
    return
  }

  if (!currentUser.value) {
    alert('Vui lòng đăng nhập hoặc đăng ký khách hàng trước khi đặt hàng.')
    openAuth('login')
    return
  }

  checkout.value.customerId = currentUser.value.customerId
  checkout.value.discountAmount = 0
  checkout.value.paidAmount = cartTotal.value

  const payload = {
    customerId: Number(currentUser.value.customerId),
    discountAmount: 0,
    paymentMethod: checkout.value.paymentMethod,
    paidAmount: cartTotal.value,
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
    let createdOrder = null

    try {
      const res = await axios.post(`${API}/api/sales/checkout`, payload)
      createdOrder = res.data?.data || res.data
    } catch {
      const res = await axios.post(`${API}/api/Orders`, {
        customerId: payload.customerId,
        totalAmount: cartTotal.value,
        discountAmount: 0,
        finalAmount: cartTotal.value,
        paidAmount: cartTotal.value,
        debtAmount: 0,
        paymentMethod: payload.paymentMethod,
        paymentStatus: 'Paid',
        orderStatus: 'Paid',
        status: 2,
        items: payload.items
      })

      createdOrder = res.data
    }

    if (!createdOrder || typeof createdOrder !== 'object') {
      createdOrder = {
        id: getNextId(orders.value),
        customerId: payload.customerId,
        customerName: currentUser.value.fullName,
        customer: {
          id: payload.customerId,
          name: currentUser.value.fullName,
          phone: currentUser.value.phone
        },
        orderDate: new Date().toISOString(),
        totalAmount: cartTotal.value,
        discountAmount: 0,
        finalAmount: cartTotal.value,
        paidAmount: cartTotal.value,
        debtAmount: 0,
        paymentMethod: payload.paymentMethod,
        paymentStatus: 'Paid',
        orderStatus: 'Paid',
        status: 2,
        items: payload.items
      }
    }

    orders.value.push(createdOrder)

    const orderId = createdOrder.id || createdOrder.orderId || getNextId(orders.value)
    const paymentId = getNextId(payments.value)

    payments.value.push({
      id: paymentId,
      orderId,
      paymentCode: `PAY${String(paymentId).padStart(6, '0')}`,
      paymentMethod: payload.paymentMethod,
      amount: payload.paidAmount,
      paymentStatus: 'Paid',
      paymentDate: new Date().toISOString()
    })

    outboxMessages.value.push({
      id: getNextId(outboxMessages.value),
      eventName: 'order.created',
      status: 'Pending',
      createdAt: new Date().toISOString()
    })

    reduceProductStock(payload.items)
    await axios.post(`${API}/api/sales/checkout`, payload)
    await loadAll()

    alert('Đặt hàng thành công.')

    cart.value = []
    cartOpen.value = false

    checkout.value = {
      customerId: currentUser.value.customerId,
      discountAmount: 0,
      paymentMethod: 'Cash',
      paidAmount: 0
    }

    openPage('myOrders')
  } catch (error) {
    console.log(error)
    alert('Lỗi khi tạo đơn hàng.')
    openPanel('orders')
  } catch (err) {
    showApiError('Tao don hang', err)
  }
}

// ===== CUSTOMER CRUD =====
function openCustomerModal(customer = null) {
  editingCustomer.value = customer

  customerForm.value = {
    name: customer ? customerName(customer) : '',
    phone: customer?.phone || '',
    email: customer?.email || '',
    address: customer?.address || '',
    debt: Number(customer?.debt || customer?.currentDebt || 0)
  }

  showCustomerModal.value = true
}

function closeCustomerModal() {
  showCustomerModal.value = false
  editingCustomer.value = null
}

async function saveCustomer() {
  if (!customerForm.value.name.trim()) {
    alert('Vui lòng nhập tên khách hàng.')
    return
  }

  const payload = {
    name: customerForm.value.name,
    fullName: customerForm.value.name,
    phone: customerForm.value.phone,
    email: customerForm.value.email,
    address: customerForm.value.address,
    debt: Number(customerForm.value.debt || 0),
    currentDebt: Number(customerForm.value.debt || 0)
  }

  if (editingCustomer.value) {
    try {
      const res = await axios.put(`${API}/api/Customers/${customerId(editingCustomer.value)}`, {
        ...editingCustomer.value,
        ...payload
      })

      Object.assign(editingCustomer.value, res.data)
    } catch {
      Object.assign(editingCustomer.value, payload)
    }
  } else {
    try {
      const res = await axios.post(`${API}/api/Customers`, payload)
      customers.value.push(res.data)
    } catch {
      customers.value.push({
        id: getNextId(customers.value),
        ...payload
      })
    }
  }

  closeCustomerModal()
}

// ===== SUPPLIER CRUD =====
function openSupplierModal(supplier = null) {
  editingSupplier.value = supplier

  supplierForm.value = {
    name: supplier ? supplierName(supplier) : '',
    contactPerson: supplier?.contactPerson || '',
    phone: supplier?.phone || '',
    email: supplier?.email || '',
    address: supplier?.address || '',
    status: supplier?.status || 'Active'
  }

  showSupplierModal.value = true
}

function closeSupplierModal() {
  showSupplierModal.value = false
  editingSupplier.value = null
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

async function saveSupplier() {
  if (!supplierForm.value.name.trim()) {
    alert('Vui lòng nhập tên nhà cung cấp.')
    return
  }

  const payload = {
    name: supplierForm.value.name,
    supplierName: supplierForm.value.name,
    contactPerson: supplierForm.value.contactPerson,
    phone: supplierForm.value.phone,
    email: supplierForm.value.email,
    address: supplierForm.value.address,
    status: supplierForm.value.status
  }

  if (editingSupplier.value) {
    try {
      const res = await axios.put(`${API}/api/Suppliers/${supplierId(editingSupplier.value)}`, {
        ...editingSupplier.value,
        ...payload
      })

      Object.assign(editingSupplier.value, res.data)
    } catch {
      Object.assign(editingSupplier.value, payload)
    }
  } else {
    try {
      const res = await axios.post(`${API}/api/Suppliers`, payload)
      suppliers.value.push(res.data)
    } catch {
      suppliers.value.push({
        id: getNextId(suppliers.value),
        ...payload
      })
    }
  }

  closeSupplierModal()
}

// ===== DEBT PAYMENT =====
function openDebtModal(debt) {
  selectedDebt.value = debt
  debtPayForm.value = {
    amount: Number(debt.remainingAmount || debt.debtAmount || 0),
    paymentMethod: 'Cash'
  }
  showDebtModal.value = true
}

function closeDebtModal() {
  selectedDebt.value = null
  showDebtModal.value = false
}

async function payDebt() {
  if (!selectedDebt.value) return

  const amount = Number(debtPayForm.value.amount || 0)

  if (amount <= 0) {
    alert('Số tiền trả nợ phải lớn hơn 0.')
    return
  }

  const remainingBefore = Number(selectedDebt.value.remainingAmount || selectedDebt.value.debtAmount || 0)

  if (amount > remainingBefore) {
    alert('Số tiền trả không được lớn hơn số tiền còn nợ.')
    return
  }
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
    await axios.post(`${API}/api/Debts/${selectedDebt.value.id || selectedDebt.value.debtId}/pay`, {
      amount,
      paymentMethod: debtPayForm.value.paymentMethod
    })

    await loadAll()
  } catch {
    const remaining = remainingBefore - amount
    selectedDebt.value.remainingAmount = remaining > 0 ? remaining : 0
    selectedDebt.value.paidAmount = Number(selectedDebt.value.paidAmount || 0) + amount
    selectedDebt.value.debtStatus = selectedDebt.value.remainingAmount === 0 ? 'Paid' : 'Partial'

    payments.value.push({
      id: getNextId(payments.value),
      orderId: selectedDebt.value.orderId,
      paymentCode: `PAY${String(getNextId(payments.value)).padStart(6, '0')}`,
      paymentMethod: debtPayForm.value.paymentMethod,
      amount,
      paymentStatus: selectedDebt.value.remainingAmount === 0 ? 'Paid' : 'Partial',
      paymentDate: new Date().toISOString()
    })
  } catch (err) {
    showApiError('Thanh toan cong no', err)
  }

  closeDebtModal()
}

onMounted(loadAll)
</script>

<template>
  <div class="app">
    <!-- HEADER -->
    <header class="site-header">
      <div class="header-top">
        <div class="header-top-inner">
          <span>✅ Order & Sales Service</span>
          <span>🚚 Tạo đơn nhanh</span>
          <span>📦 Dữ liệu sản phẩm từ Nhóm 1</span>
          <span>🔄 Event order.created gửi Nhóm 3</span>
        </div>
      </div>

      <div class="header-main">
        <button class="logo" @click="openPage('shop')">
          <span class="logo-icon">R</span>
          <span>RetailERP</span>
        </button>

        <button class="header-action category-button">
          ☰ Danh mục
        </button>

        <button class="header-action location-button">
          📍 Hà Nội
        </button>

        <div class="search-box search-wrapper">
          <span>🔎</span>
          <input
            v-model="searchText"
            type="text"
            placeholder="Bạn muốn mua gì hôm nay?"
            @focus="showSearchPanel = true"
            @input="showSearchPanel = true"
            @blur="closeSearchPanelLater"
          />
          <button v-if="searchText" class="search-clear" @mousedown.prevent="clearSearchText">✕</button>

          <div v-if="showSearchPanel" class="search-panel">
            <div v-if="searchText.trim() && searchSuggestions.length > 0" class="search-section">
              <div class="search-title">
                <span>🔎 Kết quả từ kho sản phẩm Nhóm 1</span>
                <b>{{ searchSuggestions.length }} gợi ý</b>
              </div>

              <button
                v-for="product in searchSuggestions"
                :key="productId(product)"
                class="search-item"
                @mousedown.prevent="selectSearchProduct(product)"
              >
                <span class="search-icon">{{ product.image || '📦' }}</span>
                <span class="search-info">
                  <b>{{ productName(product) }}</b>
                  <small>{{ productCode(product) }} · {{ productCategory(product) }} · Còn {{ productStock(product) }}</small>
                </span>
                <strong>{{ formatMoney(productPrice(product)) }}</strong>
              </button>
            </div>

            <div v-else-if="searchText.trim() && searchSuggestions.length === 0" class="search-empty">
              <div class="empty-mini-icon">🔍</div>
              <b>Không có sản phẩm “{{ searchText }}” trong kho Nhóm 1</b>
              <p>Hãy thử nhập tên khác, mã sản phẩm khác hoặc chọn danh mục bên dưới.</p>
            </div>

            <div v-else class="search-section">
              <div class="search-title">
                <span>🔥 Sản phẩm được mua nhiều / nên gợi ý</span>
                <b>Dựa trên kho Nhóm 1</b>
              </div>

              <button
                v-for="product in popularProducts"
                :key="productId(product)"
                class="search-item"
                @mousedown.prevent="selectSearchProduct(product)"
              >
                <span class="search-icon">{{ product.image || '📦' }}</span>
                <span class="search-info">
                  <b>{{ productName(product) }}</b>
                  <small>{{ productCode(product) }} · {{ productCategory(product) }} · Còn {{ productStock(product) }}</small>
                </span>
                <strong>{{ formatMoney(productPrice(product)) }}</strong>
              </button>
            </div>

            <div class="search-categories">
              <button
                v-for="category in categories"
                :key="category"
                @mousedown.prevent="selectedCategory = category; showSearchPanel = false"
              >
                {{ category }}
              </button>
            </div>
          </div>
        </div>

        <button class="header-action cart-button" @click="cartOpen = true">
          🛒 Giỏ hàng
          <b v-if="cartTotalQuantity > 0">{{ cartTotalQuantity }}</b>
        </button>

        <button v-if="!currentUser" class="header-action login-button" @click="openAuth('login')">
          Đăng nhập 👤
        </button>

        <div v-else class="user-actions">
          <button class="header-action" @click="openPage('myOrders')">
            👤 {{ currentUser.fullName }}
          </button>
          <button class="header-action logout-button" @click="logout">
            Đăng xuất
          </button>
        </div>
      </div>

      <nav class="quick-nav customer-nav">
        <button :class="{ active: isActive('shop') }" @click="openPage('shop')">
          🏠 Trang chủ
        </button>

        <button :class="{ active: isActive('shop') && selectedCategory !== 'Tất cả' }" @click="openPage('shop')">
          🗂️ Danh mục
        </button>

        <button @click="cartOpen = true">
          🛒 Giỏ hàng
          <b v-if="cartTotalQuantity > 0">{{ cartTotalQuantity }}</b>
        </button>

        <button :class="{ active: isActive('myOrders') }" @click="openPage('myOrders')">
          📦 Đơn mua
        </button>

        <button @click="currentUser ? openPage('myOrders') : openAuth('login')">
          🔎 Tra cứu đơn
        </button>

        <button @click="currentUser ? openPage('myOrders') : openAuth('login')">
          👤 Tài khoản
        </button>
      </nav>
    </header>

    <!-- SHOP PAGE -->
    <main v-if="activePage === 'shop'" class="page shop-page">
      <section class="home-layout">
        <aside class="category-list">
          <button
            v-for="category in categories"
            :key="category"
            :class="{ active: selectedCategory === category }"
            @click="selectedCategory = category"
          >
            <span>{{ category === 'Tất cả' ? '🏷️' : category === 'Gia dụng' ? '🏠' : category === 'Điện tử' ? '💻' : category === 'Thực phẩm' ? '🥫' : category === 'Thời trang' ? '👕' : '📦' }}</span>
            <b>{{ category }}</b>
            <em>›</em>
          </button>
        </aside>

        <section class="hero-banner">
          <div class="hero-tabs">
            <span>ORDER & SALES</span>
            <span>CHECKOUT</span>
            <span>ORDERDB</span>
          </div>

          <div class="hero-content">
            <p class="tag">NHÓM 2 — TRANG BÁN HÀNG MINI</p>
            <h1>Chọn sản phẩm, tạo giỏ hàng và đặt đơn ngay</h1>
            <p>
              Sản phẩm lấy từ ProductStockCaches/Nhóm 1. Khi khách tạo đơn, dữ liệu được lưu vào OrderDB và phát sinh event order.created.
            </p>

            <div class="hero-actions">
              <button @click="cartOpen = true">Xem giỏ hàng</button>
              <button class="ghost-btn" @click="openPage('integration')">Xem đồng bộ</button>
            </div>
          </div>
        </section>
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

        <aside class="member-card">
          <h3>👋 Tài khoản khách hàng</h3>

          <template v-if="!currentUser">
            <p>Đăng nhập hoặc đăng ký để đặt hàng và xem lịch sử đơn.</p>
            <button @click="openAuth('login')">Đăng nhập</button>
            <button class="ghost-btn" @click="openAuth('register')">Đăng ký</button>
          </template>

          <template v-else>
            <p><b>{{ currentUser.fullName }}</b></p>
            <p>{{ currentUser.phone }}</p>
            <button @click="openPage('myOrders')">Đơn của tôi</button>
          </template>

          <div class="mini-service">
            <span>📦 {{ products.length }} sản phẩm</span>
            <span>🧾 {{ orders.length }} đơn hàng</span>
            <span>💰 {{ formatMoney(totalDebt) }} công nợ</span>
          </div>

          <div class="admin-shortcuts">
            <p>Quản trị Nhóm 2</p>
            <button @click="openPage('orders')">Quản lý đơn</button>
            <button @click="openPage('customers')">Khách hàng</button>
            <button @click="openPage('suppliers')">Nhà cung cấp</button>
            <button @click="openPage('payments')">Thanh toán</button>
            <button @click="openPage('debts')">Công nợ</button>
            <button @click="openPage('integration')">Đồng bộ</button>
          </div>
        </aside>
      </section>

      <section class="promo-strip">
        <div>
          <b>Sales Checkout</b>
          <span>Tính tổng tiền, chiết khấu, thanh toán, công nợ</span>
        </div>
        <div>
          <b>stock.updated</b>
          <span>Nhận tồn kho từ Nhóm 1</span>
        </div>
        <div>
          <b>order.created</b>
          <span>Gửi đơn hàng sang Nhóm 3</span>
        </div>
      </section>

      <section class="section-head">
        <div>
          <h2>Sản phẩm đang bán</h2>
          <p>Đang lọc: <b>{{ selectedCategory }}</b> · Tìm thấy {{ filteredProducts.length }} sản phẩm</p>
        </div>

        <button class="outline-btn" @click="loadAll">
          Làm mới dữ liệu
        </button>
      </section>

      <section class="product-grid">
        <article v-for="product in filteredProducts" :key="productId(product)" class="product-card">
          <div class="product-image">{{ product.image || '📦' }}</div>

          <div class="product-meta">
            <span>{{ productCategory(product) }}</span>
            <span>{{ productCode(product) }}</span>
          </div>

          <h3>{{ productName(product) }}</h3>

          <p class="price">{{ formatMoney(productPrice(product)) }}</p>

          <p class="stock" :class="productStock(product) > 0 ? 'in-stock' : 'out-stock'">
            {{ productStock(product) > 0 ? `Còn ${productStock(product)} sản phẩm` : 'Hết hàng' }}
          </p>

          <button :disabled="productStock(product) <= 0" @click="addToCart(product)">
            + Thêm vào giỏ
          </button>
        </article>
      </section>
    </main>

    <!-- MY ORDERS -->
    <main v-if="activePage === 'myOrders'" class="page">
      <section class="panel">
        <h2>Đơn hàng của tôi</h2>
        <p v-if="!currentUser">Bạn cần đăng nhập để xem đơn hàng của mình.</p>

        <table v-else>
          <thead>
            <tr>
              <th>Mã đơn</th>
              <th>Ngày tạo</th>
              <th>Thành tiền</th>
              <th>Đã trả</th>
              <th>Còn nợ</th>
              <th>Trạng thái</th>
            </tr>
          </thead>

          <tbody>
            <tr v-for="order in myOrders" :key="order.id || order.orderId">
              <td>{{ order.orderCode || 'ORD' + (order.id || order.orderId) }}</td>
              <td>{{ order.orderDate ? new Date(order.orderDate).toLocaleString('vi-VN') : '' }}</td>
              <td>{{ formatMoney(order.finalAmount || order.totalAmount) }}</td>
              <td>{{ formatMoney(order.paidAmount) }}</td>
              <td>{{ formatMoney(order.debtAmount) }}</td>
              <td>
                <span :class="statusClass(order.orderStatus || order.paymentStatus || order.status)">
                  {{ statusLabel(order.orderStatus || order.paymentStatus || order.status) }}
                </span>
              </td>
            </tr>
          </tbody>
        </table>
      </section>
    </main>

    <!-- ORDERS -->
    <main v-if="activePage === 'orders'" class="page">
      <section class="stats">
        <div class="stat-card">
          <span>Tổng đơn hàng</span>
          <b>{{ orders.length }}</b>
        </div>

        <div class="stat-card">
          <span>Doanh thu</span>
          <b>{{ formatMoney(totalRevenue) }}</b>
        </div>

        <div class="stat-card">
          <span>Tổng công nợ</span>
          <b>{{ formatMoney(totalDebt) }}</b>
        </div>
      </section>

      <section class="panel">
        <h2>Quản lý đơn hàng</h2>

        <table>
          <thead>
            <tr>
              <th>Mã đơn</th>
              <th>Khách hàng</th>
              <th>Ngày tạo</th>
              <th>Tổng tiền</th>
              <th>Đã trả</th>
              <th>Còn nợ</th>
              <th>Trạng thái</th>
            </tr>
          </thead>

          <tbody>
            <tr v-for="order in orders" :key="order.id || order.orderId">
              <td>{{ order.orderCode || 'ORD' + (order.id || order.orderId) }}</td>
              <td>{{ order.customer?.name || order.customerName || 'ID: ' + order.customerId }}</td>
              <td>{{ order.orderDate ? new Date(order.orderDate).toLocaleString('vi-VN') : '' }}</td>
              <td>{{ formatMoney(order.finalAmount || order.totalAmount) }}</td>
              <td>{{ formatMoney(order.paidAmount) }}</td>
              <td>{{ formatMoney(order.debtAmount) }}</td>
              <td>
                <span :class="statusClass(order.orderStatus || order.paymentStatus || order.status)">
                  {{ statusLabel(order.orderStatus || order.paymentStatus || order.status) }}
                </span>
              </td>
            </tr>
          </tbody>
        </table>
      </section>
    </main>

    <!-- CUSTOMERS -->
    <main v-if="activePage === 'customers'" class="page">
      <section class="panel">
        <div class="panel-head">
          <div>
            <h2>Khách hàng</h2>
            <p>Hồ sơ khách hàng phục vụ tạo đơn, lịch sử mua hàng và công nợ.</p>
          </div>

          <button class="primary-btn" @click="openCustomerModal()">
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
            <tr v-for="customer in customers" :key="customerId(customer)">
              <td>{{ customerId(customer) }}</td>
              <td>{{ customerName(customer) }}</td>
              <td>{{ customer.phone }}</td>
              <td>{{ customer.email }}</td>
              <td>{{ customer.address }}</td>
              <td>{{ formatMoney(customer.debt || customer.currentDebt) }}</td>
              <td>
                <button class="small-btn" @click="openCustomerModal(customer)">
                  Sửa
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </section>
    </main>

    <!-- SUPPLIERS -->
    <main v-if="activePage === 'suppliers'" class="page">
      <section class="panel">
        <div class="panel-head">
          <div>
            <h2>Nhà cung cấp</h2>
            <p>Nhóm 2 chỉ quản lý thông tin liên hệ nhà cung cấp, không nhập kho.</p>
          </div>

          <button class="primary-btn" @click="openSupplierModal()">
            + Thêm nhà cung cấp
          </button>
        </div>

        <table>
          <thead>
            <tr>
              <th>ID</th>
              <th>Tên nhà cung cấp</th>
              <th>Người liên hệ</th>
              <th>SĐT</th>
              <th>Email</th>
              <th>Địa chỉ</th>
              <th>Trạng thái</th>
              <th>Thao tác</th>
            </tr>
          </thead>

          <tbody>
            <tr v-for="supplier in suppliers" :key="supplierId(supplier)">
              <td>{{ supplierId(supplier) }}</td>
              <td>{{ supplierName(supplier) }}</td>
              <td>{{ supplier.contactPerson }}</td>
              <td>{{ supplier.phone }}</td>
              <td>{{ supplier.email }}</td>
              <td>{{ supplier.address }}</td>
              <td>{{ supplier.status || 'Active' }}</td>
              <td>
                <button class="small-btn" @click="openSupplierModal(supplier)">
                  Sửa
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </section>
    </main>

    <!-- PAYMENTS -->
    <main v-if="activePage === 'payments'" class="page">
      <section class="panel">
        <h2>Thanh toán</h2>

        <table>
          <thead>
            <tr>
              <th>Mã thanh toán</th>
              <th>Mã đơn</th>
              <th>Phương thức</th>
              <th>Số tiền</th>
              <th>Ngày thanh toán</th>
              <th>Trạng thái</th>
            </tr>
          </thead>

          <tbody>
            <tr v-for="payment in payments" :key="payment.id || payment.paymentId">
              <td>{{ payment.paymentCode || 'PAY' + (payment.id || payment.paymentId) }}</td>
              <td>{{ payment.orderId }}</td>
              <td>{{ payment.paymentMethod }}</td>
              <td>{{ formatMoney(payment.amount) }}</td>
              <td>{{ payment.paymentDate ? new Date(payment.paymentDate).toLocaleString('vi-VN') : '' }}</td>
              <td>
                <span :class="statusClass(payment.paymentStatus)">
                  {{ payment.paymentStatus || 'Paid' }}
                </span>
              </td>
            </tr>
          </tbody>
        </table>
      </section>
    </main>

    <!-- DEBTS -->
    <main v-if="activePage === 'debts'" class="page">
      <section class="panel">
        <h2>Công nợ khách hàng</h2>

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
            <tr v-for="debt in debts" :key="debt.id || debt.debtId">
              <td>{{ debt.id || debt.debtId }}</td>
              <td>{{ debt.customerName || debt.customer?.name || 'ID: ' + debt.customerId }}</td>
              <td>{{ debt.orderId }}</td>
              <td>{{ formatMoney(debt.debtAmount) }}</td>
              <td>{{ formatMoney(debt.paidAmount) }}</td>
              <td>{{ formatMoney(debt.remainingAmount || debt.debtAmount) }}</td>
              <td>
                <span :class="statusClass(debt.debtStatus)">
                  {{ debt.debtStatus || 'Unpaid' }}
                </span>
              </td>
              <td>
                <button class="small-btn" @click="openDebtModal(debt)">
                  Trả nợ
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </section>
    </main>

    <!-- INTEGRATION -->
    <main v-if="activePage === 'integration'" class="page">
      <section class="stats">
        <div class="stat-card">
          <span>ProductStockCaches / stock.updated</span>
          <b>{{ products.length }}</b>
        </div>

        <div class="stat-card">
          <span>Orders / order.created</span>
          <b>{{ orders.length }}</b>
        </div>

        <div class="stat-card">
          <span>Outbox Messages</span>
          <b>{{ outboxMessages.length }}</b>
        </div>
      </section>

      <section class="panel">
        <h2>Đồng bộ dữ liệu microservices</h2>
        <p>
          Trang này mô phỏng Nhóm 2 nhận stock.updated từ Nhóm 1 và phát order.created sang Nhóm 3.
        </p>

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
            <tr v-for="message in outboxMessages" :key="message.id || message.outboxMessageId">
              <td>{{ message.id || message.outboxMessageId }}</td>
              <td>{{ message.eventName }}</td>
              <td>{{ message.status }}</td>
              <td>{{ message.createdAt ? new Date(message.createdAt).toLocaleString('vi-VN') : '' }}</td>
            </tr>
          </tbody>
        </table>
      </section>
    </main>

    <!-- CART DRAWER -->
    <div v-if="cartOpen" class="drawer-backdrop" @click.self="cartOpen = false">
      <aside class="cart-drawer">
        <div class="cart-header">
          <div>
            <p>RetailERP</p>
            <h2>Giỏ hàng của bạn</h2>
          </div>
          <button class="close-btn" @click="cartOpen = false">✕</button>
        </div>

        <div v-if="cart.length === 0" class="cart-empty-state">
          <div class="empty-icon">🛒</div>
          <h3>Giỏ hàng đang trống</h3>
          <p>Hãy chọn sản phẩm từ trang bán hàng để tạo đơn.</p>
          <button @click="cartOpen = false">Tiếp tục mua hàng</button>
        </div>

        <template v-else>
          <div class="cart-summary-top">
            <span>{{ cart.length }} loại sản phẩm</span>
            <b>{{ cartTotalQuantity }} sản phẩm</b>
          </div>

          <div class="cart-list">
            <div v-for="item in cart" :key="item.productId" class="cart-item">
              <div class="cart-icon">{{ item.image }}</div>

              <div class="cart-info">
                <div class="cart-row-head">
                  <h4>{{ item.productName }}</h4>
                  <button class="remove-link" @click="removeFromCart(item.productId)">
                    Xóa
                  </button>
                </div>

                <p class="cart-code">{{ item.productCode }} · Còn {{ item.stock }} sản phẩm</p>

                <div class="cart-row-bottom">
                  <b>{{ formatMoney(item.unitPrice) }}</b>

                  <div class="quantity-control">
                    <button @click="updateQuantity(item, item.quantity - 1)">−</button>
                    <input
                      :value="item.quantity"
                      type="number"
                      min="1"
                      :max="item.stock"
                      @input="updateQuantity(item, $event.target.value)"
                    />
                    <button @click="updateQuantity(item, item.quantity + 1)">+</button>
                  </div>
                </div>

                <div class="item-total">
                  Thành tiền: <b>{{ formatMoney(item.unitPrice * item.quantity) }}</b>
                </div>
              </div>
            </div>
          </div>

          <div class="checkout-box customer-checkout">
            <h3>Thông tin đặt hàng</h3>

            <div class="checkout-field">
              <label>Phương thức thanh toán</label>
              <select v-model="checkout.paymentMethod">
                <option value="Cash">Tiền mặt khi nhận hàng</option>
                <option value="BankTransfer">Chuyển khoản</option>
                <option value="EWallet">Ví điện tử</option>
                <option value="QR">Thanh toán QR</option>
              </select>
            </div>

            <div class="checkout-total-box">
              <div class="money-line">
                <span>Tổng tiền hàng</span>
                <b>{{ formatMoney(cartTotal) }}</b>
              </div>

              <div class="money-line">
                <span>Ưu đãi hệ thống</span>
                <b>- {{ formatMoney(0) }}</b>
              </div>

              <div class="money-line final">
                <span>Tổng cần thanh toán</span>
                <b>{{ formatMoney(cartTotal) }}</b>
              </div>
            </div>

            <div v-if="currentUser" class="customer-order-info">
              <b>Thông tin nhận hàng</b>
              <span>{{ currentUser.fullName }} · {{ currentUser.phone }}</span>
              <small>{{ currentUser.address || 'Chưa có địa chỉ' }}</small>
            </div>

            <button class="checkout-btn" @click="createOrder">
              Đặt hàng
            </button>

            <button class="continue-btn" @click="cartOpen = false">
              Tiếp tục chọn sản phẩm
            </button>
          </div>
        </template>
      </aside>
    </div>

    <!-- AUTH MODAL -->
    <div v-if="showAuthModal" class="modal-backdrop">
      <div class="modal-card">
        <div class="drawer-head">
          <h2>{{ authMode === 'login' ? 'Đăng nhập khách hàng' : 'Đăng ký tài khoản khách hàng' }}</h2>
          <button class="close-btn" @click="closeAuth">✕</button>
        </div>

        <div v-if="authMode === 'login'" class="form-grid one">
          <label>Số điện thoại</label>
          <input v-model="loginForm.phone" type="text" placeholder="Nhập số điện thoại" />

          <label>Mật khẩu</label>
          <input v-model="loginForm.password" type="password" placeholder="Nhập mật khẩu demo" />

          <p v-if="authError" class="error-text">{{ authError }}</p>

          <button class="checkout-btn" @click="loginCustomer">
            Đăng nhập
          </button>

          <p class="switch-auth">
            Chưa có tài khoản?
            <button @click="authMode = 'register'; authError = ''">Đăng ký ngay</button>
          </p>
        </div>

        <div v-else class="form-grid one">
          <label>Họ tên</label>
          <input v-model="registerForm.fullName" type="text" placeholder="Nhập họ tên" />

          <label>Số điện thoại</label>
          <input v-model="registerForm.phone" type="text" placeholder="Nhập số điện thoại" />

          <label>Email</label>
          <input v-model="registerForm.email" type="email" placeholder="Nhập email" />

          <label>Địa chỉ</label>
          <input v-model="registerForm.address" type="text" placeholder="Nhập địa chỉ" />

          <label>Mật khẩu</label>
          <input v-model="registerForm.password" type="password" placeholder="Nhập mật khẩu" />

          <p v-if="authError" class="error-text">{{ authError }}</p>

          <button class="checkout-btn" @click="registerCustomer">
            Đăng ký
          </button>

          <p class="switch-auth">
            Đã có tài khoản?
            <button @click="authMode = 'login'; authError = ''">Đăng nhập</button>
          </p>
        </div>
      </div>
    </div>

    <!-- CUSTOMER MODAL -->
    <div v-if="showCustomerModal" class="modal-backdrop">
      <div class="modal-card">
        <div class="drawer-head">
          <h2>{{ editingCustomer ? 'Sửa khách hàng' : 'Thêm khách hàng' }}</h2>
          <button class="close-btn" @click="closeCustomerModal">✕</button>
        </div>

        <div class="form-grid">
          <label>Tên khách hàng</label>
          <input v-model="customerForm.name" type="text" placeholder="Nhập tên khách hàng" />

          <label>Số điện thoại</label>
          <input v-model="customerForm.phone" type="text" placeholder="Nhập số điện thoại" />

          <label>Email</label>
          <input v-model="customerForm.email" type="email" placeholder="Nhập email" />

          <label>Công nợ</label>
          <input v-model.number="customerForm.debt" type="number" min="0" placeholder="Công nợ ban đầu" />

          <label class="full">Địa chỉ</label>
          <textarea v-model="customerForm.address" class="full" rows="3" placeholder="Nhập địa chỉ"></textarea>
        </div>

        <div class="modal-actions">
          <button class="cancel-btn" @click="closeCustomerModal">Hủy</button>
          <button class="primary-btn" @click="saveCustomer">Lưu</button>
        </div>
      </div>
    </div>

    <!-- SUPPLIER MODAL -->
    <div v-if="showSupplierModal" class="modal-backdrop">
      <div class="modal-card">
        <div class="drawer-head">
          <h2>{{ editingSupplier ? 'Sửa nhà cung cấp' : 'Thêm nhà cung cấp' }}</h2>
          <button class="close-btn" @click="closeSupplierModal">✕</button>
        </div>

        <div class="form-grid">
          <label>Tên nhà cung cấp</label>
          <input v-model="supplierForm.name" type="text" placeholder="Nhập tên nhà cung cấp" />

          <label>Người liên hệ</label>
          <input v-model="supplierForm.contactPerson" type="text" placeholder="Nhập người liên hệ" />

          <label>Số điện thoại</label>
          <input v-model="supplierForm.phone" type="text" placeholder="Nhập số điện thoại" />

          <label>Email</label>
          <input v-model="supplierForm.email" type="email" placeholder="Nhập email" />

          <label>Trạng thái</label>
          <select v-model="supplierForm.status">
            <option value="Active">Active</option>
            <option value="Inactive">Inactive</option>
          </select>

          <label class="full">Địa chỉ</label>
          <textarea v-model="supplierForm.address" class="full" rows="3" placeholder="Nhập địa chỉ"></textarea>
        </div>

        <div class="modal-actions">
          <button class="cancel-btn" @click="closeSupplierModal">Hủy</button>
          <button class="primary-btn" @click="saveSupplier">Lưu</button>
        </div>
      </div>
    </div>

    <!-- DEBT MODAL -->
    <div v-if="showDebtModal" class="modal-backdrop">
      <div class="modal-card small-modal">
        <div class="drawer-head">
          <h2>Ghi nhận trả nợ</h2>
          <button class="close-btn" @click="closeDebtModal">✕</button>
        </div>

        <div class="form-grid one">
          <label>Số tiền khách trả</label>
          <input v-model.number="debtPayForm.amount" type="number" min="0" />

          <label>Phương thức thanh toán</label>
          <select v-model="debtPayForm.paymentMethod">
            <option value="Cash">Tiền mặt</option>
            <option value="BankTransfer">Chuyển khoản</option>
            <option value="EWallet">Ví điện tử</option>
            <option value="QR">QR</option>
          </select>

          <button class="checkout-btn" @click="payDebt">
            Xác nhận trả nợ
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
:global(*) {
  box-sizing: border-box;
}

:global(body) {
  margin: 0;
  background: #f4f7fb;
  color: #172033;
  font-family: Arial, Helvetica, sans-serif;
}

button,
input,
select,
textarea {
  font-family: inherit;
}

button {
  cursor: pointer;
}

.app {
  min-height: 100vh;
}

/* HEADER */
.site-header {
  position: sticky;
  top: 0;
  z-index: 100;
  background: linear-gradient(180deg, #3454d1, #243c9d);
  color: white;
  box-shadow: 0 8px 24px rgba(36, 60, 157, 0.25);
}

.header-top {
  border-bottom: 1px solid rgba(255, 255, 255, 0.12);
}

.header-top-inner {
  max-width: 1180px;
  margin: 0 auto;
  padding: 7px 12px;
  display: flex;
  gap: 22px;
  font-size: 12px;
  white-space: nowrap;
  overflow-x: auto;
}

.header-main {
  max-width: 1180px;
  margin: 0 auto;
  padding: 12px;
  display: grid;
  grid-template-columns: auto auto auto 1fr auto auto;
  align-items: center;
  gap: 10px;
}

.logo {
  border: none;
  background: transparent;
  color: white;
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 21px;
  font-weight: 900;
}

.logo-icon {
  width: 34px;
  height: 34px;
  border-radius: 10px;
  display: grid;
  place-items: center;
  background: white;
  color: #3454d1;
}

.header-action {
  min-height: 40px;
  border: none;
  border-radius: 10px;
  padding: 0 12px;
  background: rgba(255, 255, 255, 0.14);
  color: white;
  font-weight: 800;
  white-space: nowrap;
}

.header-action:hover {
  background: rgba(255, 255, 255, 0.23);
}

.search-box {
  height: 42px;
  display: flex;
  align-items: center;
  gap: 8px;
  background: white;
  color: #172033;
  border-radius: 12px;
  padding: 0 12px;
}

.search-box input {
  width: 100%;
  border: none;
  outline: none;
  font-size: 14px;
}

.search-wrapper {
  position: relative;
}

.search-clear {
  border: none;
  width: 26px;
  height: 26px;
  border-radius: 999px;
  background: #eef2ff;
  color: #243c9d;
  font-weight: 900;
}

.search-panel {
  position: absolute;
  top: calc(100% + 10px);
  left: 0;
  right: 0;
  z-index: 250;
  background: white;
  color: #172033;
  border-radius: 18px;
  box-shadow: 0 24px 70px rgba(15, 23, 42, 0.22);
  padding: 14px;
  border: 1px solid #e5e7eb;
}

.search-panel::before {
  content: '';
  position: absolute;
  top: -8px;
  left: 34px;
  width: 16px;
  height: 16px;
  background: white;
  transform: rotate(45deg);
  border-left: 1px solid #e5e7eb;
  border-top: 1px solid #e5e7eb;
}

.search-title {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 10px;
  padding: 4px 2px 10px;
  color: #26324a;
  font-size: 14px;
  font-weight: 900;
}

.search-title b {
  color: #64748b;
  font-size: 12px;
}

.search-section {
  display: grid;
  gap: 8px;
}

.search-item {
  width: 100%;
  border: none;
  border-radius: 14px;
  padding: 10px;
  background: #f8fafc;
  display: grid;
  grid-template-columns: 46px 1fr auto;
  align-items: center;
  gap: 10px;
  text-align: left;
}

.search-item:hover {
  background: #eef2ff;
}

.search-icon {
  width: 46px;
  height: 46px;
  border-radius: 14px;
  display: grid;
  place-items: center;
  background: white;
  font-size: 24px;
}

.search-info {
  min-width: 0;
}

.search-info b {
  display: block;
  color: #172033;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.search-info small {
  display: block;
  margin-top: 3px;
  color: #64748b;
  font-weight: 700;
}

.search-item strong {
  color: #3454d1;
  white-space: nowrap;
  font-size: 13px;
}

.search-empty {
  text-align: center;
  padding: 24px 12px 16px;
}

.empty-mini-icon {
  width: 54px;
  height: 54px;
  margin: 0 auto 10px;
  border-radius: 18px;
  display: grid;
  place-items: center;
  background: #eef2ff;
  font-size: 26px;
}

.search-empty b {
  display: block;
  color: #172033;
}

.search-empty p {
  margin: 8px auto 0;
  color: #64748b;
  max-width: 420px;
}

.search-categories {
  margin-top: 12px;
  padding-top: 12px;
  border-top: 1px solid #e5e7eb;
  display: flex;
  gap: 8px;
  overflow-x: auto;
}

.search-categories button {
  border: none;
  border-radius: 999px;
  padding: 8px 12px;
  background: #eef2ff;
  color: #243c9d;
  font-size: 12px;
  font-weight: 900;
  white-space: nowrap;
}

.cart-button {
  position: relative;
}

.cart-button b {
  margin-left: 6px;
  background: white;
  color: #3454d1;
  padding: 2px 7px;
  border-radius: 999px;
}

.user-actions {
  display: flex;
  gap: 8px;
}

.quick-nav {
  max-width: 1180px;
  margin: 0 auto;
  padding: 0 12px 14px;
  display: grid;
  grid-template-columns: repeat(6, minmax(120px, 1fr));
  gap: 10px;
}

.quick-nav button {
  border: 1px solid rgba(255, 255, 255, 0.24);
  border-radius: 14px;
  padding: 11px 10px;
  color: white;
  background: rgba(255, 255, 255, 0.12);
  font-weight: 850;
  white-space: nowrap;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
}

.quick-nav button b {
  min-width: 18px;
  height: 18px;
  border-radius: 999px;
  display: grid;
  place-items: center;
  background: white;
  color: #3454d1;
  font-size: 12px;
}

.quick-nav button:hover {
  background: rgba(255, 255, 255, 0.22);
  transform: translateY(-1px);
}

.quick-nav button.active {
  background: white;
  color: #243c9d;
  border-color: white;
  box-shadow: 0 8px 18px rgba(0, 0, 0, 0.18);
}

/* PAGE */
.page {
  max-width: 1180px;
  margin: 0 auto;
  padding: 16px 12px 60px;
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

.home-layout {
  display: grid;
  grid-template-columns: 230px 1fr 250px;
  gap: 14px;
}

.category-list,
.hero-banner,
.member-card,
.panel,
.product-card,
.stat-card,
.promo-strip {
  background: white;
  border-radius: 16px;
  box-shadow: 0 2px 12px rgba(23, 32, 51, 0.08);
}

.category-list {
  padding: 8px;
}

.category-list button {
  width: 100%;
  min-height: 40px;
  border: none;
  border-radius: 10px;
  padding: 0 10px;
  display: grid;
  grid-template-columns: 28px 1fr auto;
  align-items: center;
  gap: 6px;
  background: transparent;
  color: #26324a;
  text-align: left;
}

.category-list button:hover,
.category-list button.active {
  background: #eef2ff;
  color: #243c9d;
}

.category-list em {
  font-style: normal;
  color: #9aa3b2;
}

.hero-banner {
  min-height: 310px;
  overflow: hidden;
  background:
    radial-gradient(circle at 82% 25%, rgba(99, 102, 241, .20), transparent 30%),
    linear-gradient(135deg, #f8fbff, #eef2ff);
}

.hero-tabs {
  height: 45px;
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  background: #f4f7ff;
  color: #6b7280;
  font-size: 12px;
  font-weight: 800;
  text-align: center;
}

.hero-tabs span {
  display: grid;
  place-items: center;
  border-right: 1px solid #e5e7eb;
}

.hero-content {
  padding: 34px;
}

.hero-content .tag {
  color: #3454d1;
  font-weight: 900;
  margin: 0 0 8px;
}

.hero-content h1 {
  max-width: 600px;
  margin: 0;
  font-size: 38px;
  line-height: 1.12;
  color: #172033;
}

.hero-content p {
  max-width: 660px;
  color: #5b6475;
  line-height: 1.6;
}

.hero-actions {
  display: flex;
  gap: 10px;
  margin-top: 18px;
}

.hero-actions button,
.member-card button,
.primary-btn,
.checkout-btn,
.small-btn {
  border: none;
  border-radius: 10px;
  padding: 11px 14px;
  background: #3454d1;
  color: white;
  font-weight: 900;
}

.ghost-btn {
  background: #eef2ff !important;
  color: #243c9d !important;
}

.member-card {
  padding: 16px;
}

.member-card h3 {
  margin: 0 0 10px;
}

.member-card p {
  color: #5b6475;
}

.member-card button {
  width: 100%;
  margin-bottom: 8px;
}

.mini-service {
  margin-top: 12px;
  display: grid;
  gap: 8px;
}

.mini-service span {
  display: block;
  padding: 9px;
  border-radius: 10px;
  background: #f4f7fb;
  color: #26324a;
  font-size: 13px;
  font-weight: 700;
}

.admin-shortcuts {
  margin-top: 14px;
  padding-top: 12px;
  border-top: 1px solid #e5e7eb;
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 8px;
}

.admin-shortcuts p {
  grid-column: 1 / -1;
  margin: 0 0 2px;
  color: #6b7280;
  font-size: 13px;
  font-weight: 900;
}

.admin-shortcuts button {
  width: 100%;
  margin: 0;
  padding: 9px 8px;
  border: none;
  border-radius: 10px;
  background: #f4f7fb;
  color: #243c9d;
  font-size: 12px;
  font-weight: 900;
}

.admin-shortcuts button:hover {
  background: #eef2ff;
}

.promo-strip {
  margin: 14px 0;
  padding: 12px;
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 10px;
  background: linear-gradient(90deg, #26324a, #3454d1);
  color: white;
}

.promo-strip div {
  padding: 10px 14px;
  border-radius: 12px;
  background: rgba(255,255,255,.11);
}

.promo-strip b {
  display: block;
  margin-bottom: 4px;
}

.promo-strip span {
  font-size: 13px;
  opacity: .9;
}

.section-head {
  margin: 22px 0 14px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.section-head h2 {
  margin-bottom: 4px;
}

.section-head p {
  margin-top: 0;
  color: #6b7280;
}

.outline-btn {
  border: 1px solid #3454d1;
  color: #3454d1;
  background: white;
  border-radius: 10px;
  padding: 10px 14px;
  font-weight: 800;
}

.product-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 14px;
}

.product-card {
  padding: 16px;
  transition: .2s;
}

.product-card:hover {
  transform: translateY(-2px);
}

.product-image {
  height: 124px;
  border-radius: 14px;
  background: #f7f9ff;
  display: grid;
  place-items: center;
  font-size: 54px;
}

.product-meta {
  margin-top: 12px;
  display: flex;
  justify-content: space-between;
  gap: 8px;
  color: #697386;
  font-size: 12px;
}

.product-card h3 {
  min-height: 46px;
  color: #172033;
}

.price {
  color: #3454d1;
  font-size: 20px;
  font-weight: 900;
}

.stock {
  font-weight: 800;
}

.in-stock {
  color: #15803d;
}

.out-stock {
  color: #dc2626;
}

.product-card button {
  width: 100%;
  border: none;
  border-radius: 10px;
  padding: 11px 14px;
  background: #3454d1;
  color: white;
  font-weight: 900;
}

.product-card button:disabled {
  background: #cbd5e1;
  cursor: not-allowed;
}

/* TABLE PAGES */
.stats {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 14px;
  margin-bottom: 14px;
}

.stat-card {
  padding: 18px;
}

.stat-card span {
  color: #697386;
  font-weight: 800;
}

.stat-card b {
  display: block;
  margin-top: 8px;
  font-size: 25px;
  color: #3454d1;
}

.panel {
  padding: 20px;
  overflow: hidden;
}

.panel-head {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 12px;
}

.panel h2 {
  margin-top: 0;
}

.panel p {
  color: #6b7280;
}

table {
  width: 100%;
  border-collapse: collapse;
  margin-top: 14px;
}

th,
td {
  padding: 13px;
  border-bottom: 1px solid #e5e7eb;
  text-align: left;
}

th {
  background: #eef2ff;
  color: #243c9d;
}

.status-success,
.status-warning,
.status-error,
.status-info {
  padding: 5px 10px;
  border-radius: 999px;
  font-size: 12px;
  font-weight: 900;
}

.status-success {
  background: #dcfce7;
  color: #166534;
}

.status-warning {
  background: #fef3c7;
  color: #92400e;
}

.status-error {
  background: #fee2e2;
  color: #991b1b;
}

.status-info {
  background: #dbeafe;
  color: #1d4ed8;
}

/* DRAWER & MODAL */
.drawer-backdrop,
.modal-backdrop {
  position: fixed;
  inset: 0;
  z-index: 200;
  background: rgba(15, 23, 42, 0.45);
}

.cart-drawer {
  margin-left: auto;
  width: min(520px, 100%);
  height: 100%;
  background: #f6f8ff;
  padding: 0;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
}

.cart-header {
  position: sticky;
  top: 0;
  z-index: 2;
  padding: 20px 22px;
  background: white;
  border-bottom: 1px solid #e5e7eb;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.cart-header p {
  margin: 0 0 4px;
  color: #64748b;
  font-size: 13px;
  font-weight: 800;
}

.cart-header h2 {
  margin: 0;
  color: #172033;
}

.drawer-head {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.close-btn,
.remove-btn,
.cancel-btn {
  border: none;
  border-radius: 10px;
  background: #eef2ff;
  color: #243c9d;
  padding: 9px 12px;
  font-weight: 900;
}

.cart-empty-state {
  margin: 22px;
  padding: 38px 22px;
  border-radius: 18px;
  background: white;
  text-align: center;
  box-shadow: 0 2px 12px rgba(23, 32, 51, 0.08);
}

.empty-icon {
  width: 74px;
  height: 74px;
  margin: 0 auto 14px;
  border-radius: 24px;
  display: grid;
  place-items: center;
  background: #eef2ff;
  font-size: 36px;
}

.cart-empty-state h3 {
  margin: 0 0 8px;
  color: #172033;
}

.cart-empty-state p {
  margin: 0 0 18px;
  color: #64748b;
}

.cart-empty-state button {
  border: none;
  border-radius: 12px;
  padding: 12px 18px;
  background: #3454d1;
  color: white;
  font-weight: 900;
}

.cart-summary-top {
  margin: 18px 22px 0;
  padding: 12px 14px;
  border-radius: 14px;
  background: white;
  display: flex;
  justify-content: space-between;
  color: #475569;
  box-shadow: 0 2px 12px rgba(23, 32, 51, 0.06);
}

.cart-list {
  display: grid;
  gap: 12px;
  margin: 14px 22px;
}

.cart-item {
  display: grid;
  grid-template-columns: 72px 1fr;
  gap: 14px;
  align-items: start;
  padding: 14px;
  border: 1px solid #e5e7eb;
  border-radius: 18px;
  background: white;
  box-shadow: 0 2px 12px rgba(23, 32, 51, 0.06);
}

.cart-icon {
  height: 72px;
  border-radius: 16px;
  background: #f3f4f6;
  display: grid;
  place-items: center;
  font-size: 30px;
}

.cart-row-head,
.cart-row-bottom {
  display: flex;
  justify-content: space-between;
  gap: 10px;
  align-items: flex-start;
}

.cart-info h4 {
  margin: 0;
  color: #172033;
  line-height: 1.35;
}

.cart-code {
  margin: 6px 0 10px;
  color: #64748b;
  font-size: 13px;
  font-weight: 700;
}

.cart-row-bottom b {
  color: #3454d1;
  font-size: 16px;
}

.remove-link {
  border: none;
  border-radius: 999px;
  padding: 7px 10px;
  background: #eef2ff;
  color: #243c9d;
  font-weight: 900;
  white-space: nowrap;
}

.quantity-control {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 4px;
  border-radius: 999px;
  background: #f1f5f9;
}

.quantity-control button {
  width: 30px;
  height: 30px;
  border: none;
  border-radius: 999px;
  background: white;
  color: #243c9d;
  font-weight: 900;
}

.quantity-control input {
  width: 46px;
  height: 30px;
  border: none;
  background: transparent;
  text-align: center;
  font-weight: 900;
  outline: none;
}

.item-total {
  margin-top: 10px;
  padding-top: 10px;
  border-top: 1px dashed #d1d5db;
  color: #64748b;
  font-size: 13px;
}

.item-total b {
  color: #172033;
}

.checkout-box {
  margin: 0 22px 22px;
  display: grid;
  gap: 12px;
  padding: 18px;
  border-radius: 18px;
  background: white;
  box-shadow: 0 2px 12px rgba(23, 32, 51, 0.08);
}

.checkout-box h3 {
  margin: 0;
  color: #172033;
}

.checkout-note {
  margin: 0;
  padding: 11px 12px;
  border-radius: 12px;
  background: #eef2ff;
  color: #475569;
  font-size: 13px;
  line-height: 1.45;
}

.customer-order-info {
  display: grid;
  gap: 4px;
  padding: 12px;
  border-radius: 14px;
  background: #f8fafc;
  color: #172033;
}

.customer-order-info span {
  color: #3454d1;
  font-weight: 900;
}

.customer-order-info small {
  color: #64748b;
  line-height: 1.4;
}

.checkout-field {
  display: grid;
  gap: 6px;
}

.checkout-field label {
  color: #475569;
  font-size: 13px;
  font-weight: 900;
}

.checkout-box input,
.checkout-box select,
.form-grid input,
.form-grid select,
.form-grid textarea {
  width: 100%;
  min-height: 42px;
  border: 1px solid #d1d5db;
  border-radius: 12px;
  padding: 0 12px;
  background: white;
  outline: none;
}

.checkout-box input:focus,
.checkout-box select:focus,
.form-grid input:focus,
.form-grid select:focus,
.form-grid textarea:focus {
  border-color: #3454d1;
  box-shadow: 0 0 0 3px rgba(52, 84, 209, 0.12);
}

.form-grid textarea {
  padding: 10px 12px;
}

.checkout-total-box {
  display: grid;
  gap: 8px;
  padding: 14px;
  border-radius: 14px;
  background: #f8fafc;
}

.money-line {
  display: flex;
  justify-content: space-between;
  font-size: 15px;
  color: #475569;
}

.money-line b {
  color: #172033;
}

.money-line.final {
  padding-top: 8px;
  border-top: 1px solid #e5e7eb;
  font-size: 17px;
  font-weight: 900;
}

.money-line.final b {
  color: #3454d1;
}

.money-line.paid b {
  color: #15803d;
}

.money-line.debt b {
  color: #b45309;
}

.continue-btn {
  border: none;
  border-radius: 12px;
  padding: 11px 14px;
  background: #eef2ff;
  color: #243c9d;
  font-weight: 900;
}

.modal-card {
  width: min(680px, calc(100% - 32px));
  margin: 60px auto;
  background: white;
  border-radius: 18px;
  padding: 22px;
  box-shadow: 0 20px 60px rgba(0, 0, 0, .18);
}

.small-modal {
  width: min(460px, calc(100% - 32px));
}

.form-grid {
  display: grid;
  grid-template-columns: 170px 1fr;
  gap: 12px;
  align-items: center;
}

.form-grid.one {
  grid-template-columns: 1fr;
}

.form-grid label {
  font-weight: 900;
}

.form-grid .full {
  grid-column: 1 / -1;
}

.error-text {
  background: #fee2e2;
  color: #991b1b;
  padding: 10px;
  border-radius: 10px;
}

.switch-auth {
  text-align: center;
}

.switch-auth button {
  border: none;
  background: transparent;
  color: #3454d1;
  font-weight: 900;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  margin-top: 18px;
}

/* RESPONSIVE */
@media (max-width: 1100px) {
  .header-main {
    grid-template-columns: auto 1fr auto;
  }

  .category-button,
  .location-button {
    display: none;
  }

  .quick-nav {
    grid-template-columns: repeat(3, 1fr);
  }

  .home-layout {
    grid-template-columns: 1fr;
  }

  .member-card {
    display: none;
  }

  .product-grid {
    grid-template-columns: repeat(3, 1fr);
  }

  .promo-strip {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 760px) {
  .header-main {
    grid-template-columns: 1fr;
  }

  .quick-nav {
    grid-template-columns: repeat(2, 1fr);
  }

  .search-panel {
    position: fixed;
    top: 86px;
    left: 12px;
    right: 12px;
  }

  .search-item {
    grid-template-columns: 42px 1fr;
  }

  .search-item strong {
    grid-column: 2;
  }

  .product-grid {
    grid-template-columns: repeat(2, 1fr);
  }

  .stats {
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

@media (max-width: 520px) {
  .product-grid {
    grid-template-columns: 1fr;
  }

  .form-grid {
    grid-template-columns: 1fr;
  }

  .hero-content h1 {
    font-size: 28px;
  }

  .cart-item {
    grid-template-columns: 58px 1fr;
  }

  .cart-icon {
    height: 58px;
    border-radius: 14px;
  }

  .cart-row-head,
  .cart-row-bottom {
    flex-direction: column;
    align-items: flex-start;
  }
}
</style>

