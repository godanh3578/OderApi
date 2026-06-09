<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import api, { setStaffToken, getStaffToken } from './api/client'
import {
  loadCustomerUser,
  saveCustomerUser,
  loadCustomerCart,
  saveCustomerCart,
  saveDemoPassword,
  verifyDemoPassword,
  saveDemoCustomer,
  loadDemoCustomer
} from './utils/customerStorage'

const route = useRoute()
const router = useRouter()

// ===== PAGE STATE =====
const activePage = ref('shop')
const searchText = ref('')
const showSearchPanel = ref(false)
const selectedCategory = ref('')
const productSort = ref('featured')
const activeShowcaseTab = ref('deals')
const showcaseCategory = ref('')
const cartOpen = ref(false)
const selectedOrder = ref(null)
const orderDetailLoading = ref(false)

// ===== AUTH STATE =====
const showAuthModal = ref(false)
const showStaffAuthModal = ref(false)
const authMode = ref('login')
const authError = ref('')
const staffAuthError = ref('')
const currentUser = ref(null)
const staffUser = ref(null)

const staffLoginForm = ref({
  username: 'sales01',
  role: 'Sales'
})

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
const showCheckoutConfirmModal = ref(false)
const checkoutSubmitting = ref(false)

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

const checkoutShipping = ref({
  fullName: '',
  phone: '',
  address: ''
})

const accountForm = ref({
  fullName: '',
  phone: '',
  email: '',
  address: ''
})

const orderLookupCode = ref('')
const orderLookupError = ref('')
const accountSaveMessage = ref('')

const demoProducts = [
  {
    id: 101,
    productId: 101,
    productCode: 'DT101',
    productName: 'Chuột không dây Logitech M331 Silent',
    categoryName: 'Điện tử',
    sellingPrice: 320000,
    quantityAvailable: 30,
    stockStatus: 'InStock',
    image: '🖱️',
    manufacturerName: 'Logitech',
    soldCount: 42,
    lastUpdatedAt: '2026-06-09T08:00:00Z'
  },
  {
    id: 102,
    productId: 102,
    productCode: 'DT102',
    productName: 'Bàn phím cơ AKKO 3087 v2',
    categoryName: 'Điện tử',
    sellingPrice: 1290000,
    quantityAvailable: 9,
    stockStatus: 'InStock',
    image: '⌨️',
    manufacturerName: 'AKKO',
    soldCount: 24,
    lastUpdatedAt: '2026-06-08T09:30:00Z'
  },
  {
    id: 103,
    productId: 103,
    productCode: 'DT103',
    productName: 'Tai nghe Bluetooth Sony WH-CH520',
    categoryName: 'Điện tử',
    sellingPrice: 990000,
    quantityAvailable: 4,
    stockStatus: 'LowStock',
    image: '🎧',
    manufacturerName: 'Sony',
    soldCount: 57,
    lastUpdatedAt: '2026-06-07T10:15:00Z'
  },
  {
    id: 104,
    productId: 104,
    productCode: 'GD101',
    productName: 'Nồi chiên không dầu Sunhouse 6L',
    categoryName: 'Gia dụng',
    sellingPrice: 1450000,
    quantityAvailable: 11,
    stockStatus: 'InStock',
    image: '🍟',
    manufacturerName: 'Sunhouse',
    soldCount: 35,
    lastUpdatedAt: '2026-06-06T12:00:00Z'
  },
  {
    id: 105,
    productId: 105,
    productCode: 'GD102',
    productName: 'Máy xay sinh tố Philips HR2221',
    categoryName: 'Gia dụng',
    sellingPrice: 890000,
    quantityAvailable: 8,
    stockStatus: 'InStock',
    image: '🥤',
    manufacturerName: 'Philips',
    soldCount: 31,
    lastUpdatedAt: '2026-06-05T08:45:00Z'
  },
  {
    id: 106,
    productId: 106,
    productCode: 'GD103',
    productName: 'Quạt đứng Panasonic F-308NH',
    categoryName: 'Gia dụng',
    sellingPrice: 780000,
    quantityAvailable: 2,
    stockStatus: 'LowStock',
    image: '🌀',
    manufacturerName: 'Panasonic',
    soldCount: 49,
    lastUpdatedAt: '2026-06-09T05:20:00Z'
  },
  {
    id: 107,
    productId: 107,
    productCode: 'VP101',
    productName: 'Bút bi Thiên Long hộp 20 cây',
    categoryName: 'Văn phòng phẩm',
    sellingPrice: 65000,
    quantityAvailable: 50,
    stockStatus: 'InStock',
    image: '🖊️',
    manufacturerName: 'Thiên Long',
    soldCount: 68,
    lastUpdatedAt: '2026-06-04T07:10:00Z'
  },
  {
    id: 108,
    productId: 108,
    productCode: 'VP102',
    productName: 'Sổ tay gáy lò xo Campus A5',
    categoryName: 'Văn phòng phẩm',
    sellingPrice: 38000,
    quantityAvailable: 5,
    stockStatus: 'LowStock',
    image: '📒',
    manufacturerName: 'Campus',
    soldCount: 22,
    lastUpdatedAt: '2026-06-09T02:40:00Z'
  },
  {
    id: 109,
    productId: 109,
    productCode: 'TP101',
    productName: 'Gạo ST25 túi 5kg',
    categoryName: 'Thực phẩm',
    sellingPrice: 185000,
    quantityAvailable: 25,
    stockStatus: 'InStock',
    image: '🌾',
    manufacturerName: 'ST25',
    soldCount: 61,
    lastUpdatedAt: '2026-06-03T11:30:00Z'
  },
  {
    id: 110,
    productId: 110,
    productCode: 'TP102',
    productName: 'Cà phê Arabica Đà Lạt 500g',
    categoryName: 'Thực phẩm',
    sellingPrice: 155000,
    quantityAvailable: 14,
    stockStatus: 'InStock',
    image: '☕',
    manufacturerName: 'LangFarm',
    soldCount: 37,
    lastUpdatedAt: '2026-06-08T14:00:00Z'
  },
  {
    id: 111,
    productId: 111,
    productCode: 'TT101',
    productName: 'Áo thun cotton nam basic',
    categoryName: 'Thời trang',
    sellingPrice: 150000,
    quantityAvailable: 18,
    stockStatus: 'InStock',
    image: '👕',
    manufacturerName: 'Đối tác Thời trang',
    soldCount: 46,
    lastUpdatedAt: '2026-06-02T09:00:00Z'
  },
  {
    id: 112,
    productId: 112,
    productCode: 'PK101',
    productName: 'Balo laptop Acer Gaming SUV',
    categoryName: 'Phụ kiện',
    sellingPrice: 590000,
    quantityAvailable: 6,
    stockStatus: 'InStock',
    image: '🎒',
    manufacturerName: 'Acer',
    soldCount: 18,
    lastUpdatedAt: '2026-06-09T09:10:00Z'
  }
]

// ===== API =====
async function loadProducts() {
  try {
    const res = await api.get('/api/ProductStockCaches')
    const apiProducts = Array.isArray(res.data) ? res.data : []
    products.value = apiProducts.length > 0 ? apiProducts : demoProducts
  } catch (error) {
    console.warn('Đang dùng sản phẩm demo vì chưa tải được dữ liệu Nhóm 1:', error)
    products.value = demoProducts
  }
}

async function loadStaffData() {
  if (!staffUser.value) return

  try {
    const ordersRes = await api.get('/api/Orders')
    orders.value = Array.isArray(ordersRes.data) ? ordersRes.data : []

    if (staffUser.value.role === 'Warehouse') return

    const [customersRes, suppliersRes, paymentsRes, debtsRes, outboxRes] = await Promise.all([
      api.get('/api/Customers'),
      api.get('/api/Suppliers'),
      api.get('/api/Payments'),
      api.get('/api/Debts'),
      api.get('/api/OutboxMessages')
    ])

    customers.value = Array.isArray(customersRes.data) ? customersRes.data : []
    suppliers.value = Array.isArray(suppliersRes.data) ? suppliersRes.data : []
    payments.value = Array.isArray(paymentsRes.data) ? paymentsRes.data : []
    debts.value = Array.isArray(debtsRes.data) ? debtsRes.data : []
    outboxMessages.value = Array.isArray(outboxRes.data) ? outboxRes.data : []
  } catch (error) {
    console.error('Không tải được dữ liệu quản trị:', error)
  }
}

async function loadMyOrders() {
  if (!currentUser.value) {
    orders.value = []
    return
  }

  try {
    const res = await api.get('/api/Orders', {
      params: { customerId: currentUser.value.customerId }
    })
    orders.value = Array.isArray(res.data) ? res.data : []
  } catch (error) {
    console.error('Không tải được đơn của khách:', error)
    orders.value = []
  }
}

async function loadAll() {
  await loadProducts()
  syncCartStock()
  if (staffUser.value) {
    await loadStaffData()
  } else if (currentUser.value) {
    await loadMyOrders()
  }
}

const pagePaths = {
  shop: '/',
  cart: '/cart',
  myOrders: '/my-orders',
  account: '/account',
  orders: '/manage/orders',
  customers: '/manage/customers',
  suppliers: '/manage/suppliers',
  payments: '/manage/payments',
  debts: '/manage/debts',
  integration: '/manage/integration',
  warehouse: '/manage/warehouse'
}

function syncCheckoutAmounts() {
  const total = finalAmount.value
  const paid = Number(checkout.value.paidAmount || 0)
  if (paid > total) {
    checkout.value.paidAmount = total
  }
}

function initCheckoutShipping() {
  checkout.value.paidAmount = finalAmount.value
  if (!currentUser.value) return
  checkoutShipping.value = {
    fullName: currentUser.value.fullName || '',
    phone: currentUser.value.phone || '',
    address: currentUser.value.address || ''
  }
}

function initAccountForm() {
  if (!currentUser.value) return
  accountForm.value = {
    fullName: currentUser.value.fullName || '',
    phone: currentUser.value.phone || '',
    email: currentUser.value.email || '',
    address: currentUser.value.address || ''
  }
}

function syncCartStock() {
  for (const item of cart.value) {
    const product = products.value.find(p => productId(p) === Number(item.productId))
    if (!product) continue

    item.stock = productBaseStock(product)
    item.unitPrice = productPrice(product)

    if (item.quantity > item.stock) {
      item.quantity = item.stock > 0 ? item.stock : 1
    }
  }
  saveCustomerCart(cart.value)
}

async function applyRoute() {
  const page = route.meta.page
  if (!page) return

  if (route.meta.staff && !staffUser.value) {
    openStaffAuth()
    router.replace('/')
    return
  }

  if (staffUser.value?.role === 'Warehouse' && route.meta.staff && page !== 'orders' && page !== 'warehouse' && page !== 'orderDetail') {
    alert('Tài khoản kho chỉ được xem đơn hàng cần xuất.')
    router.replace('/manage/warehouse')
    return
  }

  activePage.value = page

  if (page === 'orderDetail') {
    const id = Number(route.params.id)
    if (id) await openOrderDetail({ orderId: id }, false)
  } else if (page === 'myOrders') {
    await loadMyOrders()
  } else if (page === 'account') {
    initAccountForm()
  } else if (page === 'cart' || page === 'shop') {
    await loadProducts()
    syncCartStock()
    if (page === 'cart') initCheckoutShipping()
  } else if (route.meta.staff) {
    await loadStaffData()
  }
}

// ===== HELPERS =====
async function openPage(page) {
  const staffPages = ['orders', 'customers', 'suppliers', 'payments', 'debts', 'integration', 'warehouse']
  const warehousePages = ['orders', 'warehouse', 'orderDetail']

  if (['myOrders', 'account'].includes(page) && !currentUser.value) {
    openAuth('login')
    return
  }

  if (staffPages.includes(page) && !staffUser.value) {
    alert('Vui lòng đăng nhập nhân viên để truy cập trang quản trị.')
    openStaffAuth()
    return
  }

  if (staffUser.value?.role === 'Warehouse' && staffPages.includes(page) && !warehousePages.includes(page)) {
    alert('Tài khoản kho chỉ được xem đơn hàng cần xuất.')
    return
  }

  if (pagePaths[page]) {
    router.push(pagePaths[page])
    return
  }

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

function productBaseStock(product) {
  return Number(product.quantityAvailable ?? product.stock ?? 0)
}

function productStock(product) {
  return Math.max(0, productBaseStock(product) - cartQuantityFor(product))
}

function categoryIcon(category) {
  const value = String(category || '').toLowerCase()
  if (value.includes('gia dụng')) return '🏠'
  if (value.includes('điện tử')) return '💻'
  if (value.includes('thực phẩm')) return '🥫'
  if (value.includes('thời trang')) return '👕'
  if (value.includes('văn phòng')) return '✏️'
  if (value.includes('tất cả')) return '🏷️'
  return '📦'
}

function productVisual(product) {
  if (product.image) return product.image

  const name = productName(product).toLowerCase()
  if (name.includes('nồi') || name.includes('cơm')) return '🍚'
  if (name.includes('xay') || name.includes('philips')) return '🥤'
  if (name.includes('quạt')) return '🌀'
  if (name.includes('chuột')) return '🖱️'
  if (name.includes('bàn phím')) return '⌨️'
  if (name.includes('gạo')) return '🌾'
  if (name.includes('áo')) return '👕'
  if (name.includes('bút')) return '🖊️'

  return categoryIcon(productCategory(product))
}

function productTone(product) {
  const category = productCategory(product).toLowerCase()
  if (category.includes('gia dụng')) return 'tone-home'
  if (category.includes('điện tử')) return 'tone-tech'
  if (category.includes('thực phẩm')) return 'tone-food'
  if (category.includes('thời trang')) return 'tone-fashion'
  if (category.includes('văn phòng')) return 'tone-office'
  return 'tone-default'
}

function productStockLabel(product) {
  const stock = productStock(product)
  if (stock <= 0) return 'Hết hàng'
  if (stock <= 5) return 'Sắp hết'
  return 'Sẵn hàng'
}

function productStockClass(product) {
  const stock = productStock(product)
  if (stock <= 0) return 'stock-empty'
  if (stock <= 5) return 'stock-low'
  return 'stock-ready'
}

function productStockPercent(product) {
  const stock = productStock(product)
  const baseStock = productBaseStock(product)
  const maxStock = baseStock > 0 ? baseStock : 50
  return Math.max(0, Math.min(100, (stock / maxStock) * 100))
}

function cartQuantityFor(product) {
  const item = cart.value.find(line => Number(line.productId) === productId(product))
  return item ? Number(item.quantity || 0) : 0
}

function cartStockLimit(item) {
  const product = products.value.find(product => productId(product) === Number(item.productId))
  return product ? productBaseStock(product) : Number(item.stock || 0)
}

function cartRemainingForItem(item) {
  return Math.max(0, cartStockLimit(item) - Number(item.quantity || 0))
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

function formatDate(value) {
  if (!value) return ''
  return new Date(value).toLocaleDateString('vi-VN')
}

function paymentMethodLabel(method) {
  const map = {
    Cash: 'Tiền mặt',
    BankTransfer: 'Chuyển khoản',
    EWallet: 'Ví điện tử',
    QR: 'Thanh toán QR'
  }
  return map[method] || method || 'Chưa xác định'
}

function statusLabel(status) {
  if (typeof status === 'string') {
    const labels = {
      pending: 'Chờ xử lý',
      confirmed: 'Đã xác nhận',
      paid: 'Đã thanh toán',
      partial: 'Thanh toán một phần',
      debt: 'Còn công nợ',
      unpaid: 'Chưa thanh toán',
      cancelled: 'Đã hủy',
      outofstock: 'Hết hàng',
      processed: 'Đã xử lý',
      processing: 'Đang xử lý',
      completed: 'Hoàn tất',
      failed: 'Thất bại',
      active: 'Đang hoạt động',
      inactive: 'Ngừng hoạt động'
    }

    return labels[status.trim().toLowerCase()] || status
  }

  return ['Chờ xử lý', 'Đã xác nhận', 'Đã thanh toán', 'Còn công nợ', 'Đã hủy'][Number(status)] || 'Không xác định'
}

function statusClass(status) {
  const value = typeof status === 'string' ? status.trim().toLowerCase() : Number(status)

  if (value === 'paid' || value === 2 || value === 'confirmed' || value === 1 || value === 'processed') return 'status-success'
  if (value === 'partial' || value === 'debt' || value === 3 || value === 'pending' || value === 0 || value === 'unpaid') return 'status-warning'
  if (value === 'cancelled' || value === 4 || value === 'outofstock' || value === 'failed') return 'status-error'

  return 'status-info'
}

function orderTotalAmount(order) {
  return Number(order?.finalAmount ?? order?.totalAmount ?? 0)
}

function orderPaidAmount(order) {
  return Number(order?.paidAmount ?? order?.amountPaid ?? order?.paymentAmount ?? 0)
}

function orderDebtAmount(order) {
  const paymentStatus = String(order?.paymentStatus || '').trim().toLowerCase()

  if (paymentStatus === 'paid' || paymentStatus === 'completed') return 0

  const explicitDebt = Number(order?.remainingAmount ?? order?.debtAmount ?? order?.balanceDue ?? 0)
  if (explicitDebt > 0) return explicitDebt

  const calculatedDebt = orderTotalAmount(order) - orderPaidAmount(order)
  return calculatedDebt > 0 ? calculatedDebt : 0
}

function paymentStatusText(order) {
  const paymentStatus = String(order?.paymentStatus || '').trim().toLowerCase()
  const total = orderTotalAmount(order)
  const paid = orderPaidAmount(order)
  const debt = orderDebtAmount(order)

  if (paymentStatus === 'paid' || paymentStatus === 'completed' || (total > 0 && paid >= total && debt <= 0)) {
    return 'Đã thanh toán đủ'
  }

  if (paymentStatus === 'partial' || (paid > 0 && debt > 0)) {
    return 'Thanh toán một phần'
  }

  if (paymentStatus === 'debt') {
    return paid > 0 ? 'Thanh toán một phần' : 'Chưa thanh toán'
  }

  if (paymentStatus === 'unpaid' || debt > 0 || paid <= 0) {
    return 'Chưa thanh toán'
  }

  return statusLabel(order?.paymentStatus || 'unpaid')
}

function paymentStatusClass(order) {
  const text = paymentStatusText(order)

  if (text === 'Đã thanh toán đủ') return 'status-success'
  if (text === 'Thanh toán một phần') return 'status-warning'

  return 'status-error'
}

function productOrderItems(order) {
  return order?.items || order?.orderItems || order?.orderDetails || order?.details || []
}

function productCreatedTime(product) {
  const value = product.lastUpdatedAt || product.updatedAt || product.createdAt || product.importedAt || product.createdDate
  const time = value ? new Date(value).getTime() : 0
  return Number.isFinite(time) ? time : 0
}

function inferManufacturerFromName(name) {
  const value = String(name || '').toLowerCase()
  const knownNames = [
    'Thiên Long',
    'Logitech',
    'Panasonic',
    'Sunhouse',
    'Philips',
    'AKKO',
    'Apple',
    'Samsung',
    'Xiaomi',
    'Acer',
    'Lenovo',
    'Sony',
    'LG',
    'ST25'
  ]

  return knownNames.find(brand => value.includes(brand.toLowerCase())) || ''
}

function productManufacturer(product) {
  return (
    product.manufacturerName ||
    product.manufacturer ||
    product.producerName ||
    product.brandName ||
    product.brand ||
    product.supplierName ||
    product.vendorName ||
    inferManufacturerFromName(productName(product)) ||
    `Đối tác ${productCategory(product)}`
  )
}

function brandInitials(name) {
  return String(name || 'RP')
    .split(/\s+/)
    .filter(Boolean)
    .slice(0, 2)
    .map(part => part[0])
    .join('')
    .toUpperCase()
}

function selectSearchProduct(product) {
  searchText.value = productName(product)
  selectedCategory.value = productCategory(product)
  showSearchPanel.value = false
  openPage('shop')
  window.setTimeout(() => scrollToCategory(productCategory(product)), 120)
}

function closeSearchPanelLater() {
  window.setTimeout(() => {
    showSearchPanel.value = false
  }, 160)
}

function clearSearchText() {
  searchText.value = ''
  selectedCategory.value = ''
  showSearchPanel.value = true
}

function chooseCategory(category) {
  selectedCategory.value = category
  searchText.value = ''
  showSearchPanel.value = false
  openPage('shop')
  window.setTimeout(() => scrollToCategory(category), 120)
}

function scrollToCatalog() {
  document.getElementById('product-catalog')?.scrollIntoView({ behavior: 'smooth', block: 'start' })
}

function categorySectionId(category) {
  return `category-${encodeURIComponent(String(category || 'khac'))}`
}

function scrollToCategory(category) {
  const target = document.getElementById(categorySectionId(category))
  if (target) {
    target.scrollIntoView({ behavior: 'smooth', block: 'start' })
    return
  }

  scrollToCatalog()
}

const categories = computed(() => {
  const values = new Set(products.value.map(productCategory))
  return Array.from(values).sort((a, b) => a.localeCompare(b, 'vi'))
})

const categoryCounts = computed(() => {
  const counts = {}
  for (const product of products.value) {
    const category = productCategory(product)
    counts[category] = (counts[category] || 0) + 1
  }
  return counts
})

const filteredProducts = computed(() => {
  const keyword = searchText.value.trim().toLowerCase()

  const filtered = products.value.filter(product => {
    const matchSearch =
      !keyword ||
      productName(product).toLowerCase().includes(keyword) ||
      productCode(product).toLowerCase().includes(keyword) ||
      productCategory(product).toLowerCase().includes(keyword)

    return matchSearch
  })

  return filtered.sort((a, b) => {
    if (productSort.value === 'priceAsc') return productPrice(a) - productPrice(b)
    if (productSort.value === 'priceDesc') return productPrice(b) - productPrice(a)
    if (productSort.value === 'stockDesc') return productStock(b) - productStock(a)

    const stockDiff = Math.min(productStock(b), 1) - Math.min(productStock(a), 1)
    if (stockDiff !== 0) return stockDiff
    return productStock(b) - productStock(a)
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

const salesStatsByProduct = computed(() => {
  const map = new Map()

  for (const order of orders.value) {
    for (const item of productOrderItems(order)) {
      const id = Number(item.productId)
      if (!id) continue

      const quantity = Number(item.quantity || item.qty || 0)
      const revenue = Number(item.subTotal ?? item.total ?? (Number(item.unitPrice || 0) * quantity))
      const current = map.get(id) || { quantity: 0, revenue: 0, orders: 0 }

      current.quantity += quantity
      current.revenue += revenue
      current.orders += 1
      map.set(id, current)
    }
  }

  return map
})

const catalogCategorySections = computed(() => {
  return categories.value
    .map(category => ({
      category,
      products: filteredProducts.value.filter(product => productCategory(product) === category)
    }))
    .filter(section => section.products.length > 0)
})

const dealProducts = computed(() => {
  return [...products.value]
    .filter(product => productStock(product) > 0)
    .sort((a, b) => productPrice(a) - productPrice(b) || productStock(b) - productStock(a))
    .slice(0, 12)
})

const hotTrendProducts = computed(() => {
  const bestByCategory = new Map()

  for (const product of products.value) {
    const stats = salesStatsByProduct.value.get(productId(product)) || {}
    const sold = Number(stats.quantity || product.soldCount || product.totalSold || 0)
    const stockScore = productStock(product) > 0 ? Math.min(productStock(product), 50) : -100
    const score = sold * 1000 + stockScore + productPrice(product) / 100000
    const category = productCategory(product)
    const current = bestByCategory.get(category)

    if (!current || score > current.score) {
      bestByCategory.set(category, { product, score })
    }
  }

  return Array.from(bestByCategory.values())
    .sort((a, b) => b.score - a.score)
    .map(item => item.product)
    .slice(0, 12)
})

const newArrivalProducts = computed(() => {
  return [...products.value]
    .sort((a, b) => {
      const timeDiff = productCreatedTime(b) - productCreatedTime(a)
      if (timeDiff !== 0) return timeDiff
      return productId(b) - productId(a)
    })
    .slice(0, 12)
})

const lowStockProducts = computed(() => {
  return [...products.value]
    .filter(product => productStock(product) > 0 && productStock(product) <= 5)
    .sort((a, b) => productStock(a) - productStock(b) || productPrice(b) - productPrice(a))
    .slice(0, 12)
})

const showcaseTabs = computed(() => [
  { id: 'deals', label: 'Deal sốc mỗi ngày', products: dealProducts.value },
  { id: 'hot', label: 'Sản phẩm hot trend', products: hotTrendProducts.value },
  { id: 'new', label: 'Hàng mới về', products: newArrivalProducts.value },
  { id: 'lowStock', label: 'Sắp hết hàng', products: lowStockProducts.value }
])

const showcaseCategories = computed(() => {
  return categories.value.slice(0, 8)
})

const activeShowcaseProducts = computed(() => {
  const tab = showcaseTabs.value.find(item => item.id === activeShowcaseTab.value) || showcaseTabs.value[0]
  const source = tab?.products || []

  if (!showcaseCategory.value) {
    return source.slice(0, 8)
  }

  return source
    .filter(product => productCategory(product) === showcaseCategory.value)
    .slice(0, 8)
})

const topManufacturers = computed(() => {
  const map = new Map()

  for (const product of products.value) {
    const name = productManufacturer(product)
    const current = map.get(name) || {
      name,
      initials: brandInitials(name),
      productCount: 0,
      totalStock: 0,
      categories: {}
    }

    const category = productCategory(product)
    current.productCount += 1
    current.totalStock += productStock(product)
    current.categories[category] = (current.categories[category] || 0) + 1
    map.set(name, current)
  }

  return Array.from(map.values())
    .map(item => ({
      ...item,
      primaryCategory: Object.entries(item.categories).sort((a, b) => b[1] - a[1])[0]?.[0] || 'Đối tác'
    }))
    .sort((a, b) => b.productCount - a.productCount || b.totalStock - a.totalStock || a.name.localeCompare(b.name, 'vi'))
    .slice(0, 8)
})

function showcaseProductBadge(product) {
  if (activeShowcaseTab.value === 'deals') return 'Deal tốt'
  if (activeShowcaseTab.value === 'hot') {
    const stats = salesStatsByProduct.value.get(productId(product))
    return stats?.quantity ? `Đã bán ${stats.quantity}` : 'Đang được quan tâm'
  }
  if (activeShowcaseTab.value === 'new') return 'Hàng mới'
  if (activeShowcaseTab.value === 'lowStock') return 'Sắp hết'

  return productStockLabel(product)
}

const inStockProductCount = computed(() => {
  return products.value.filter(product => productStock(product) > 0).length
})

const lowStockProductCount = computed(() => {
  return products.value.filter(product => productStock(product) > 0 && productStock(product) <= 5).length
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
  return orders.value.reduce((sum, order) => sum + orderTotalAmount(order), 0)
})

const totalDebt = computed(() => {
  return debts.value.reduce((sum, debt) => sum + Number(debt.remainingAmount || debt.debtAmount || 0), 0)
})

const myOrders = computed(() => {
  if (!currentUser.value) return []
  return orders.value.filter(order => Number(order.customerId) === Number(currentUser.value.customerId))
})

const warehouseOrders = computed(() => {
  return orders.value.filter(order => {
    const status = String(order.orderStatus || order.status || '').toLowerCase()
    return status !== 'cancelled' && status !== '4'
  })
})

const isStaff = computed(() => !!staffUser.value)
const isAdmin = computed(() => staffUser.value?.role === 'Admin')
const isSales = computed(() => staffUser.value?.role === 'Sales')
const isWarehouse = computed(() => staffUser.value?.role === 'Warehouse')
const canManageSales = computed(() => isAdmin.value || isSales.value)
const canViewWarehouse = computed(() => isAdmin.value || isWarehouse.value)
const canViewIntegration = computed(() => canManageSales.value)

const myOrderCount = computed(() => myOrders.value.length)

const myTotalDebt = computed(() => {
  return myOrders.value.reduce((sum, order) => sum + orderDebtAmount(order), 0)
})

const expectedDebtDueDate = computed(() => {
  const date = new Date()
  date.setDate(date.getDate() + 30)
  return date
})

const processedOutboxCount = computed(() => {
  return outboxMessages.value.filter(message => String(message.status || '').toLowerCase() === 'processed').length
})

const pendingOutboxCount = computed(() => {
  return outboxMessages.value.filter(message => String(message.status || '').toLowerCase() === 'pending').length
})

const failedOutboxCount = computed(() => {
  return outboxMessages.value.filter(message => String(message.status || '').toLowerCase() === 'failed').length
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

async function finishCustomerAuth(customer) {
  currentUser.value = {
    role: 'Customer',
    customerId: customerId(customer),
    fullName: customerName(customer),
    phone: customer.phone,
    email: customer.email,
    address: customer.address
  }

  checkout.value.customerId = currentUser.value.customerId
  saveCustomerUser(currentUser.value)
  saveDemoCustomer(currentUser.value)
  initCheckoutShipping()
  closeAuth()
  await loadMyOrders()
}

function createLocalDemoCustomer() {
  const phone = registerForm.value.phone.trim()

  return {
    role: 'Customer',
    customerId: Date.now(),
    customerCode: `DEMO${phone.slice(-4) || '0000'}`,
    fullName: registerForm.value.fullName.trim(),
    phone,
    email: registerForm.value.email.trim(),
    address: registerForm.value.address.trim(),
    currentDebt: 0,
    totalSpent: 0,
    status: 'Active'
  }
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

  try {
    const res = await api.post('/api/Customers', {
      fullName: registerForm.value.fullName,
      phone: registerForm.value.phone,
      email: registerForm.value.email,
      address: registerForm.value.address
    })

    saveDemoPassword(registerForm.value.phone, registerForm.value.password)
    await finishCustomerAuth(res.data)
    registerForm.value = { fullName: '', phone: '', email: '', address: '', password: '' }
  } catch (error) {
    if (error.response) {
      authError.value = error.response.data?.message || 'Đăng ký thất bại.'
      return
    }

    const localCustomer = createLocalDemoCustomer()
    saveDemoPassword(localCustomer.phone, registerForm.value.password)
    await finishCustomerAuth(localCustomer)
    registerForm.value = { fullName: '', phone: '', email: '', address: '', password: '' }
  }
}

async function loginCustomer() {
  authError.value = ''

  if (!loginForm.value.phone.trim()) {
    authError.value = 'Vui lòng nhập số điện thoại.'
    return
  }

  if (!loginForm.value.password.trim()) {
    authError.value = 'Vui lòng nhập mật khẩu demo.'
    return
  }

  if (!verifyDemoPassword(loginForm.value.phone, loginForm.value.password)) {
    authError.value = 'Mật khẩu không đúng. Mật khẩu demo được lưu trên trình duyệt khi đăng ký.'
    return
  }

  try {
    const res = await api.post('/api/Customers/demo-login', {
      phone: loginForm.value.phone
    })

    await finishCustomerAuth(res.data)
    loginForm.value = { phone: '', password: '' }
  } catch (error) {
    const localCustomer = loadDemoCustomer(loginForm.value.phone.trim())
    if (localCustomer) {
      await finishCustomerAuth(localCustomer)
      loginForm.value = { phone: '', password: '' }
      return
    }

    authError.value = error.response?.data?.message || 'Không tìm thấy khách hàng. Vui lòng đăng ký.'
  }
}

function logout() {
  currentUser.value = null
  saveCustomerUser(null)
  checkout.value.customerId = null
  orders.value = []
  router.push('/')
}

async function saveAccountProfile() {
  accountSaveMessage.value = ''

  if (!currentUser.value) {
    openAuth('login')
    return
  }

  try {
    const res = await api.put(`/api/Customers/${currentUser.value.customerId}/profile`, {
      phone: accountForm.value.phone,
      fullName: accountForm.value.fullName,
      email: accountForm.value.email,
      address: accountForm.value.address
    })

    currentUser.value = {
      ...currentUser.value,
      fullName: res.data.fullName,
      email: res.data.email,
      address: res.data.address
    }

    saveCustomerUser(currentUser.value)
    initCheckoutShipping()
    accountSaveMessage.value = 'Đã cập nhật thông tin tài khoản.'
  } catch (error) {
    accountSaveMessage.value = error.response?.data?.message || 'Cập nhật thất bại.'
  }
}

async function lookupOrderByCode() {
  orderLookupError.value = ''

  if (!currentUser.value) {
    openAuth('login')
    return
  }

  if (!orderLookupCode.value.trim()) {
    orderLookupError.value = 'Vui lòng nhập mã đơn hàng.'
    return
  }

  try {
    const res = await api.get('/api/Orders/lookup', {
      params: {
        orderCode: orderLookupCode.value.trim(),
        phone: currentUser.value.phone
      }
    })

    orderLookupCode.value = ''
    await openOrderDetail(res.data)
  } catch (error) {
    orderLookupError.value = error.response?.data?.message || 'Không tìm thấy đơn hàng phù hợp.'
  }
}

function openStaffAuth() {
  staffAuthError.value = ''
  showStaffAuthModal.value = true
}

function closeStaffAuth() {
  showStaffAuthModal.value = false
  staffAuthError.value = ''
}

async function loginStaff() {
  staffAuthError.value = ''

  try {
    const res = await api.post('/api/auth/login', {
      username: staffLoginForm.value.username,
      role: staffLoginForm.value.role
    })

    setStaffToken(res.data.token)
    staffUser.value = {
      username: res.data.username,
      role: res.data.role
    }

    closeStaffAuth()
    await loadStaffData()
  } catch (error) {
    staffAuthError.value = error.response?.data?.message || 'Đăng nhập nhân viên thất bại.'
  }
}

function logoutStaff() {
  setStaffToken('')
  staffUser.value = null
  customers.value = []
  suppliers.value = []
  payments.value = []
  debts.value = []
  outboxMessages.value = []

  if (currentUser.value) {
    loadMyOrders()
  } else {
    orders.value = []
  }

  openPage('shop')
}

async function updateOrderStatus(order, status) {
  const orderId = order.orderId || order.id
  if (!orderId) return

  try {
    const res = await api.put(`/api/Orders/${orderId}/status`, { status })
    const updated = res.data
    alert('Đã cập nhật trạng thái đơn.')

    if (selectedOrder.value && Number(selectedOrder.value.orderId || selectedOrder.value.id) === Number(orderId)) {
      selectedOrder.value = { ...selectedOrder.value, ...updated }
    }

    if (staffUser.value) {
      await loadStaffData()
    } else if (currentUser.value) {
      await loadMyOrders()
    }
  } catch (error) {
    alert(error.response?.data?.message || 'Không cập nhật được trạng thái.')
  }
}

async function openOrderDetail(order, navigate = true) {
  const orderId = order.orderId || order.id
  if (!orderId) return

  selectedOrder.value = order
  activePage.value = 'orderDetail'
  orderDetailLoading.value = true

  if (navigate) {
    router.push(`/orders/${orderId}`)
  }

  try {
    const res = await api.get(`/api/Orders/${orderId}`)
    selectedOrder.value = res.data
  } catch (error) {
    console.error('Không tải được chi tiết đơn:', error)
  } finally {
    orderDetailLoading.value = false
  }
}

// ===== CART =====
function addToCart(product) {
  const availableStock = productStock(product)
  const stockLimit = productBaseStock(product)

  if (availableStock <= 0) {
    alert('Sản phẩm đã hết hàng.')
    return
  }

  const id = productId(product)
  const existed = cart.value.find(item => item.productId === id)

  if (existed) {
    if (existed.quantity + 1 > stockLimit) {
      alert('Số lượng mua không được vượt quá tồn kho.')
      return
    }

    existed.stock = stockLimit
    existed.unitPrice = productPrice(product)
    existed.quantity++
  } else {
    cart.value.push({
      productId: id,
      productCode: productCode(product),
      productName: productName(product),
      categoryName: productCategory(product),
      unitPrice: productPrice(product),
      quantity: 1,
      stock: stockLimit,
      image: productVisual(product)
    })
  }

  cartOpen.value = true
  saveCustomerCart(cart.value)
}

function removeFromCart(id) {
  cart.value = cart.value.filter(item => item.productId !== id)
  saveCustomerCart(cart.value)
}

function updateQuantity(item, value) {
  const quantity = Math.floor(Number(value))
  const stockLimit = cartStockLimit(item)
  item.stock = stockLimit

  if (!Number.isFinite(quantity) || quantity < 1) {
    item.quantity = 1
    return
  }

  if (quantity > stockLimit) {
    alert('Số lượng mua không được vượt quá tồn kho.')
    item.quantity = stockLimit
    return
  }

  item.quantity = quantity
  saveCustomerCart(cart.value)
}

function applyCheckoutShippingToUser() {
  if (!currentUser.value) return

  currentUser.value = {
    ...currentUser.value,
    fullName: checkoutShipping.value.fullName,
    phone: checkoutShipping.value.phone,
    address: checkoutShipping.value.address
  }

  saveCustomerUser(currentUser.value)
}

// ===== ORDER CHECKOUT =====
async function recalculateCheckout() {
  if (cart.value.length === 0) {
    alert('Giỏ hàng đang trống.')
    return
  }

  syncCheckoutAmounts()

  try {
    const res = await api.post('/api/Sales/calculate-total', {
      items: cart.value.map(item => ({
        productId: item.productId,
        quantity: Number(item.quantity)
      })),
      discountAmount: Number(checkout.value.discountAmount || 0)
    })
    const data = res.data?.data || res.data
    if (data?.finalAmount != null) {
      checkout.value.paidAmount = Math.min(
        Number(checkout.value.paidAmount || data.finalAmount),
        Number(data.finalAmount)
      )
    }
    alert(`Tổng: ${formatMoney(data?.totalAmount || cartTotal.value)} · Chiết khấu: ${formatMoney(data?.discountAmount || 0)} · Thanh toán: ${formatMoney(data?.finalAmount || finalAmount.value)}`)
  } catch (error) {
    alert(error.response?.data?.message || 'Không tính được tổng tiền.')
  }
}

function prepareCheckoutConfirmation() {
  if (cart.value.length === 0) {
    alert('Gio hang dang trong.')
    return
  }

  if (!currentUser.value) {
    alert('Vui long dang nhap hoac dang ky khach hang truoc khi dat hang.')
    openAuth('login')
    return
  }

  if (!checkoutShipping.value.address.trim()) {
    alert('Vui long nhap dia chi nhan hang.')
    return
  }

  const overStockItem = cart.value.find(item => Number(item.quantity || 0) > cartStockLimit(item))
  if (overStockItem) {
    alert(`${productName(overStockItem)} chỉ còn ${cartStockLimit(overStockItem)} sản phẩm trong kho.`)
    return
  }

  syncCheckoutAmounts()
  showCheckoutConfirmModal.value = true
}

function closeCheckoutConfirmModal() {
  if (!checkoutSubmitting.value) showCheckoutConfirmModal.value = false
}

async function confirmCheckout() {
  checkoutSubmitting.value = true
  await createOrder()
  checkoutSubmitting.value = false
}

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

  if (!checkoutShipping.value.address.trim()) {
    alert('Vui lòng nhập địa chỉ nhận hàng.')
    return
  }

  try {
    await api.put(`/api/Customers/${currentUser.value.customerId}/profile`, {
      phone: currentUser.value.phone,
      fullName: checkoutShipping.value.fullName || currentUser.value.fullName,
      email: currentUser.value.email || '',
      address: checkoutShipping.value.address
    })
    applyCheckoutShippingToUser()
  } catch (error) {
    alert(error.response?.data?.message || 'Không cập nhật được địa chỉ nhận hàng.')
    return
  }

  syncCheckoutAmounts()

  const payload = {
    customerId: Number(currentUser.value.customerId),
    discountAmount: Number(checkout.value.discountAmount || 0),
    paymentMethod: checkout.value.paymentMethod,
    paidAmount: Number(checkout.value.paidAmount ?? finalAmount.value),
    items: cart.value.map(item => ({
      productId: item.productId,
      quantity: Number(item.quantity)
    }))
  }

  try {
    const res = await api.post('/api/Sales/Checkout', payload)
    const createdOrder = res.data?.data || res.data

    alert(res.data?.message || 'Đặt hàng thành công.')

    cart.value = []
    saveCustomerCart([])
    cartOpen.value = false
    showCheckoutConfirmModal.value = false
    checkout.value = {
      customerId: currentUser.value.customerId,
      discountAmount: 0,
      paymentMethod: 'Cash',
      paidAmount: 0
    }

    await Promise.all([loadProducts(), loadMyOrders()])
    openPage('myOrders')

    if (createdOrder?.orderId) {
      await openOrderDetail(createdOrder)
    }
  } catch (error) {
    alert(error.response?.data?.message || 'Lỗi khi tạo đơn hàng.')
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

  try {
    if (editingCustomer.value) {
      const res = await api.put(`/api/Customers/${customerId(editingCustomer.value)}`, {
        fullName: payload.fullName,
        phone: payload.phone,
        email: payload.email,
        address: payload.address,
        status: 'Active'
      })
      Object.assign(editingCustomer.value, res.data)
    } else {
      const res = await api.post('/api/Customers', {
        fullName: payload.fullName,
        phone: payload.phone,
        email: payload.email,
        address: payload.address
      })
      customers.value.push(res.data)
    }
    await loadStaffData()
  } catch (error) {
    alert(error.response?.data?.message || 'Lưu khách hàng thất bại.')
    return
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

  try {
    if (editingSupplier.value) {
      const res = await api.put(`/api/Suppliers/${supplierId(editingSupplier.value)}`, {
        supplierName: payload.supplierName,
        contactPerson: payload.contactPerson,
        phone: payload.phone,
        email: payload.email,
        address: payload.address,
        status: payload.status
      })
      Object.assign(editingSupplier.value, res.data)
    } else {
      const res = await api.post('/api/Suppliers', {
        supplierCode: `NCC${String(getNextId(suppliers.value)).padStart(4, '0')}`,
        supplierName: payload.supplierName,
        contactPerson: payload.contactPerson,
        phone: payload.phone,
        email: payload.email,
        address: payload.address
      })
      suppliers.value.push(res.data)
    }
    await loadStaffData()
  } catch (error) {
    alert(error.response?.data?.message || 'Lưu nhà cung cấp thất bại.')
    return
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

  try {
    await api.post(`/api/Debts/${selectedDebt.value.id || selectedDebt.value.debtId}/pay`, {
      amount,
      paymentMethod: debtPayForm.value.paymentMethod
    })

    await loadStaffData()
    closeDebtModal()
  } catch (error) {
    alert(error.response?.data?.message || 'Ghi nhận trả nợ thất bại.')
  }
}

watch(cart, () => {
  saveCustomerCart(cart.value)
}, { deep: true })

watch(cartOpen, (isOpen) => {
  if (isOpen) {
    syncCartStock()
    initCheckoutShipping()
  }
})

watch(() => route.fullPath, () => {
  applyRoute()
})

onMounted(async () => {
  cart.value = loadCustomerCart()

  const savedCustomer = loadCustomerUser()
  if (savedCustomer) {
    currentUser.value = savedCustomer
    checkout.value.customerId = savedCustomer.customerId
    initCheckoutShipping()
  }

  const savedToken = getStaffToken()
  if (savedToken) {
    try {
      const payload = JSON.parse(atob(savedToken.split('.')[1]))
      staffUser.value = {
        username: payload.unique_name || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || 'staff',
        role: payload.role || 'Sales'
      }
      await loadStaffData()
    } catch {
      setStaffToken('')
    }
  }

  await loadProducts()
  syncCartStock()

  if (currentUser.value) {
    await loadMyOrders()
  }

  await applyRoute()
})

</script>

<template>
  <div class="app">
    <!-- HEADER -->
    <header class="site-header">
      <div class="header-top">
        <div class="header-top-inner">
          <div class="promo-marquee">
            <span>🔥 Flash Sale hôm nay - giảm đến 30%</span>
            <span>🚚 Miễn phí giao hàng cho đơn từ 500K</span>
            <span>🎁 Tặng voucher 50K cho khách hàng mới</span>
            <span>⚡ Mua nhanh trong khung giờ vàng 12:00 - 14:00</span>
            <span>🛒 Mua nhiều giảm thêm tại giỏ hàng</span>
            <span>🔥 Flash Sale hôm nay - giảm đến 30%</span>
            <span>🚚 Miễn phí giao hàng cho đơn từ 500K</span>
            <span>🎁 Tặng voucher 50K cho khách hàng mới</span>
            <span>⚡ Mua nhanh trong khung giờ vàng 12:00 - 14:00</span>
            <span>🛒 Mua nhiều giảm thêm tại giỏ hàng</span>
          </div>
        </div>
      </div>

      <div class="header-main">
        <button class="logo" @click="openPage('shop')">
          <span class="logo-icon">R</span>
          <span>RetailERP</span>
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
                @mousedown.prevent="chooseCategory(category)"
              >
                {{ category }}
              </button>
            </div>
          </div>
        </div>

        <button class="header-action cart-button" @click="openPage('cart')">
          🛒 Giỏ hàng
          <b v-if="cartTotalQuantity > 0">{{ cartTotalQuantity }}</b>
        </button>

        <div v-if="staffUser" class="user-actions">
          <button class="header-action" @click="openPage('orders')">
            🛠️ {{ staffUser.username }} ({{ staffUser.role }})
          </button>
          <button class="header-action logout-button" @click="logoutStaff">
            Thoát NV
          </button>
        </div>

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

        <button :class="{ active: isActive('myOrders') }" @click="openPage('myOrders')">
          📦 Đơn mua
        </button>

        <button :class="{ active: isActive('myOrders') }" @click="openPage('myOrders')">
          🔎 Tra cứu đơn
        </button>

      </nav>
    </header>

    <!-- SHOP PAGE -->
    <main v-if="activePage === 'shop'" class="page shop-page">
      <section class="home-layout">
        <aside class="category-list">
          <div class="side-title">
            <span>Danh mục</span>
            <b>{{ products.length }}</b>
          </div>

          <button
            v-for="category in categories"
            :key="category"
            :class="{ active: selectedCategory === category }"
            @click="chooseCategory(category)"
          >
            <span class="category-icon">{{ categoryIcon(category) }}</span>
            <b>{{ category }}</b>
            <em>{{ categoryCounts[category] || 0 }}</em>
          </button>
        </aside>

        <section class="hero-banner">
          <div class="hero-content">
            <p class="tag">RetailERP Market</p>
            <h1>Đặt hàng nhanh từ dữ liệu kho Nhóm 1</h1>
            <p>Danh mục luôn đồng bộ tồn kho, giỏ hàng kiểm tra số lượng và đơn bán được gửi sang hệ thống báo cáo sau khi đặt thành công.</p>

            <div class="hero-actions">
              <button @click="scrollToCatalog">Mua ngay</button>
              <button class="ghost-btn" @click="openPage('myOrders')">Tra cứu đơn</button>
            </div>

            <div class="hero-stats">
              <span><b>{{ products.length }}</b> sản phẩm</span>
              <span><b>{{ inStockProductCount }}</b> còn hàng</span>
              <span><b>{{ lowStockProductCount }}</b> sắp hết</span>
            </div>
          </div>

        </section>
      </section>

      <section class="market-showcase">
        <div class="showcase-tabs">
          <button
            v-for="tab in showcaseTabs"
            :key="tab.id"
            type="button"
            :class="{ active: activeShowcaseTab === tab.id }"
            @click="activeShowcaseTab = tab.id"
          >
            {{ tab.label }}
          </button>
        </div>

        <div class="showcase-categories">
          <button
            v-for="category in showcaseCategories"
            :key="category"
            type="button"
            :class="{ active: showcaseCategory === category }"
            @click="showcaseCategory = category"
          >
            {{ categoryIcon(category) }} {{ category }}
          </button>
        </div>

        <div v-if="activeShowcaseProducts.length === 0" class="showcase-empty">
          Chưa có sản phẩm phù hợp trong mục này.
        </div>

        <div v-else class="showcase-grid">
          <article v-for="product in activeShowcaseProducts" :key="productId(product)" class="showcase-card">
            <div class="showcase-image" :class="productTone(product)">
              <span class="showcase-badge">{{ showcaseProductBadge(product) }}</span>
              <span class="product-visual">{{ productVisual(product) }}</span>
            </div>

            <p>{{ productCategory(product) }}</p>
            <h3>{{ productName(product) }}</h3>
            <strong>{{ formatMoney(productPrice(product)) }}</strong>
            <span :class="['showcase-stock', productStockClass(product)]">
              {{ productStockLabel(product) }} · còn {{ productStock(product) }}
            </span>
            <button :disabled="productStock(product) <= 0" @click="addToCart(product)">
              {{ cartQuantityFor(product) ? 'Thêm nữa' : 'Thêm vào giỏ' }}
            </button>
          </article>
        </div>
      </section>

      <section id="product-catalog" class="section-head catalog-head">
        <div>
          <h2>Sản phẩm đang bán</h2>
          <p>
            {{ selectedCategory ? `Đang xem danh mục: ${selectedCategory}` : 'Hiển thị toàn bộ sản phẩm theo danh mục' }}
            · Tìm thấy {{ filteredProducts.length }} sản phẩm
          </p>
        </div>

        <div class="catalog-tools">
          <select v-model="productSort" aria-label="Sắp xếp sản phẩm">
            <option value="featured">Nổi bật</option>
            <option value="priceAsc">Giá thấp đến cao</option>
            <option value="priceDesc">Giá cao đến thấp</option>
            <option value="stockDesc">Tồn kho nhiều nhất</option>
          </select>

          <button class="outline-btn" @click="loadAll">
            Làm mới dữ liệu
          </button>
        </div>
      </section>

      <div v-if="catalogCategorySections.length === 0" class="cart-empty-state page-empty">
        <div class="empty-icon">🔎</div>
        <h3>Không tìm thấy sản phẩm</h3>
        <p>Thử đổi từ khóa tìm kiếm hoặc làm mới dữ liệu tồn kho.</p>
        <button @click="clearSearchText">Xem lại tất cả sản phẩm</button>
      </div>

      <section
        v-for="section in catalogCategorySections"
        :id="categorySectionId(section.category)"
        :key="section.category"
        class="category-product-section"
      >
        <div class="section-head category-product-head">
          <div>
            <h2>{{ categoryIcon(section.category) }} {{ section.category }}</h2>
            <p>{{ section.products.length }} sản phẩm trong danh mục này</p>
          </div>
        </div>

        <div class="product-grid">
          <article v-for="product in section.products" :key="productId(product)" class="product-card">
            <div class="product-image" :class="productTone(product)">
              <span :class="['product-badge', productStockClass(product)]">{{ productStockLabel(product) }}</span>
              <span class="product-visual">{{ productVisual(product) }}</span>
            </div>

            <div class="product-meta">
              <span>{{ categoryIcon(productCategory(product)) }} {{ productCategory(product) }}</span>
              <span>{{ productCode(product) }}</span>
            </div>

            <h3>{{ productName(product) }}</h3>

            <div class="price-row">
              <p class="price">{{ formatMoney(productPrice(product)) }}</p>
              <span v-if="cartQuantityFor(product)" class="cart-chip">Đã chọn {{ cartQuantityFor(product) }}</span>
            </div>

            <div class="stock-meter">
              <span :style="{ width: productStockPercent(product) + '%' }"></span>
            </div>

            <p class="stock" :class="productStock(product) > 0 ? 'in-stock' : 'out-stock'">
              {{ productStock(product) > 0 ? `Còn ${productStock(product)} sản phẩm có thể thêm` : 'Hết hàng hoặc đã chọn hết trong giỏ' }}
            </p>

            <button :disabled="productStock(product) <= 0" @click="addToCart(product)">
              {{ cartQuantityFor(product) ? 'Thêm nữa' : '+ Thêm vào giỏ' }}
            </button>
          </article>
        </div>
      </section>

      <div v-if="cartTotalQuantity > 0" class="floating-cart-summary">
        <div>
          <b>{{ cartTotalQuantity }} sản phẩm</b>
          <span>{{ formatMoney(cartTotal) }}</span>
        </div>
        <button @click="openPage('cart')">Xem giỏ</button>
      </div>
    </main>

    <!-- CART PAGE -->
    <main v-if="activePage === 'cart'" class="page">
      <section class="panel">
        <div class="panel-head">
          <div>
            <h2>Giỏ hàng</h2>
            <p>Trang giỏ hàng dành cho khách hàng đặt mua online.</p>
          </div>
          <button class="outline-btn" @click="openPage('shop')">Tiếp tục mua hàng</button>
        </div>

        <div v-if="cart.length === 0" class="cart-empty-state page-empty">
          <div class="empty-icon">🛒</div>
          <h3>Giỏ hàng đang trống</h3>
          <p>Hãy chọn sản phẩm từ trang bán hàng để bắt đầu đặt hàng.</p>
          <button @click="openPage('shop')">Về trang chủ</button>
        </div>

        <template v-else>
          <div class="cart-list page-cart-list">
            <div v-for="item in cart" :key="item.productId" class="cart-item">
              <div class="cart-icon">{{ item.image }}</div>
              <div class="cart-info">
                <div class="cart-row-head">
                  <h4>{{ item.productName }}</h4>
                  <button class="remove-link" @click="removeFromCart(item.productId)">Xóa</button>
                </div>
                <p class="cart-code">
                  {{ item.productCode }} · Đang chọn {{ item.quantity }} · Còn có thể thêm {{ cartRemainingForItem(item) }}
                </p>
                <div class="cart-row-bottom">
                  <b>{{ formatMoney(item.unitPrice) }}</b>
                  <div class="quantity-control">
                    <button @click="updateQuantity(item, item.quantity - 1)">−</button>
                    <input :value="item.quantity" type="number" min="1" :max="cartStockLimit(item)" @input="updateQuantity(item, $event.target.value)" />
                    <button @click="updateQuantity(item, item.quantity + 1)">+</button>
                  </div>
                </div>
                <div class="item-total">Thành tiền: <b>{{ formatMoney(item.unitPrice * item.quantity) }}</b></div>
              </div>
            </div>
          </div>

          <div class="checkout-box customer-checkout page-checkout">
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

            <div v-if="currentUser || staffUser" class="checkout-field">
              <label>Chiết khấu (VNĐ)</label>
              <input
                v-model.number="checkout.discountAmount"
                type="number"
                min="0"
                :max="cartTotal"
                placeholder="0"
                @input="syncCheckoutAmounts"
              />
              <button v-if="canManageSales" type="button" class="outline-btn small-top" @click="recalculateCheckout">
                Tính tổng (Sales API)
              </button>
            </div>

            <div v-if="currentUser" class="checkout-field">
              <label>Số tiền thanh toán trước (VNĐ)</label>
              <input
                v-model.number="checkout.paidAmount"
                type="number"
                min="0"
                :max="finalAmount"
                :placeholder="String(finalAmount)"
                @input="syncCheckoutAmounts"
              />
              <p v-if="debtAmount > 0" class="checkout-note debt-hint">
                Còn nợ sau đơn: <b>{{ formatMoney(debtAmount) }}</b> · hạn dự kiến {{ formatDate(expectedDebtDueDate) }}
              </p>
            </div>

            <div v-if="currentUser" class="shipping-form">
              <label>Họ tên người nhận</label>
              <input v-model="checkoutShipping.fullName" type="text" placeholder="Nhập họ tên" />
              <label>Số điện thoại</label>
              <input v-model="checkoutShipping.phone" type="text" placeholder="Nhập số điện thoại" />
              <label>Địa chỉ nhận hàng</label>
              <textarea v-model="checkoutShipping.address" rows="3" placeholder="Nhập địa chỉ nhận hàng"></textarea>
            </div>

            <div v-else class="checkout-note">
              Vui lòng đăng nhập hoặc đăng ký trước khi đặt hàng.
            </div>

            <div class="checkout-total-box">
              <div class="money-line"><span>Tổng tiền hàng</span><b>{{ formatMoney(cartTotal) }}</b></div>
              <div class="money-line"><span>Chiết khấu</span><b>- {{ formatMoney(checkout.discountAmount || 0) }}</b></div>
              <div class="money-line"><span>Thanh toán trước</span><b>{{ formatMoney(Number(checkout.paidAmount ?? finalAmount)) }}</b></div>
              <div v-if="debtAmount > 0" class="money-line"><span>Công nợ đơn này</span><b>{{ formatMoney(debtAmount) }}</b></div>
              <div class="money-line final"><span>Tổng cần thanh toán</span><b>{{ formatMoney(finalAmount) }}</b></div>
            </div>

            <button class="checkout-btn" @click="currentUser ? prepareCheckoutConfirmation() : openAuth('login')">
              {{ currentUser ? 'Đặt hàng' : 'Đăng nhập để đặt hàng' }}
            </button>
          </div>
        </template>
      </section>
    </main>

    <!-- ACCOUNT PAGE -->
    <main v-if="activePage === 'account'" class="page">
      <section class="panel">
        <h2>Tài khoản khách hàng</h2>
        <p v-if="!currentUser">Bạn cần đăng nhập để xem và cập nhật tài khoản.</p>

        <div v-else class="form-grid one account-form">
          <label>Họ tên</label>
          <input v-model="accountForm.fullName" type="text" />
          <label>Số điện thoại</label>
          <input v-model="accountForm.phone" type="text" disabled />
          <label>Email</label>
          <input v-model="accountForm.email" type="email" />
          <label>Địa chỉ nhận hàng</label>
          <textarea v-model="accountForm.address" rows="3"></textarea>
          <p v-if="accountSaveMessage" class="success-text">{{ accountSaveMessage }}</p>
          <button class="checkout-btn" @click="saveAccountProfile">Lưu thông tin</button>
        </div>
      </section>
    </main>

    <!-- MY ORDERS -->
    <main v-if="activePage === 'myOrders'" class="page">
      <section class="panel">
        <h2>Đơn hàng của tôi</h2>
        <p v-if="!currentUser">Bạn cần đăng nhập để xem đơn hàng của mình.</p>

        <div v-else class="lookup-box">
          <label>Tra cứu đơn theo mã</label>
          <div class="lookup-row">
            <input v-model="orderLookupCode" type="text" placeholder="Nhập mã đơn, ví dụ ORD000001" />
            <button class="primary-btn" @click="lookupOrderByCode">Tra cứu</button>
          </div>
          <p v-if="orderLookupError" class="error-text">{{ orderLookupError }}</p>
        </div>

        <div v-if="currentUser && myOrders.length === 0" class="cart-empty-state page-empty">
          <div class="empty-icon">📦</div>
          <h3>Bạn chưa có đơn hàng nào</h3>
          <p>Hãy chọn sản phẩm và đặt hàng để xem lịch sử mua tại đây.</p>
          <button @click="openPage('shop')">Mua sắm ngay</button>
        </div>

        <table v-else-if="currentUser">
          <thead>
            <tr>
              <th>Mã đơn</th>
              <th>Ngày tạo</th>
              <th>Thành tiền</th>
              <th>Đã trả</th>
              <th>Còn nợ</th>
              <th>Trạng thái đơn</th>
              <th>Thanh toán</th>
            </tr>
          </thead>

          <tbody>
            <tr v-for="order in myOrders" :key="order.id || order.orderId" class="clickable-row" @click="openOrderDetail(order)">
              <td>{{ order.orderCode || 'ORD' + (order.id || order.orderId) }}</td>
              <td>{{ order.orderDate ? new Date(order.orderDate).toLocaleString('vi-VN') : '' }}</td>
              <td>{{ formatMoney(orderTotalAmount(order)) }}</td>
              <td>{{ formatMoney(orderPaidAmount(order)) }}</td>
              <td>{{ formatMoney(orderDebtAmount(order)) }}</td>
              <td>
                <span :class="statusClass(order.orderStatus || order.status)">
                  {{ statusLabel(order.orderStatus || order.status) }}
                </span>
              </td>
              <td>
                <span :class="paymentStatusClass(order)">
                  {{ paymentStatusText(order) }}
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
            <tr v-for="order in orders" :key="order.id || order.orderId" class="clickable-row" @click="openOrderDetail(order)">
              <td>{{ order.orderCode || 'ORD' + (order.id || order.orderId) }}</td>
              <td>{{ order.customer?.name || order.customerName || 'ID: ' + order.customerId }}</td>
              <td>{{ order.orderDate ? new Date(order.orderDate).toLocaleString('vi-VN') : '' }}</td>
              <td>{{ formatMoney(orderTotalAmount(order)) }}</td>
              <td>{{ formatMoney(orderPaidAmount(order)) }}</td>
              <td>{{ formatMoney(orderDebtAmount(order)) }}</td>
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
                  {{ statusLabel(payment.paymentStatus || 'Paid') }}
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
              <th>Hạn thanh toán</th>
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
              <td>{{ debt.dueDate ? formatDate(debt.dueDate) : 'Chưa đặt' }}</td>
              <td>
                <span :class="statusClass(debt.debtStatus)">
                  {{ statusLabel(debt.debtStatus || 'Unpaid') }}
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

    <!-- ORDER DETAIL -->
    <main v-if="activePage === 'orderDetail'" class="page">
      <section class="panel">
        <div class="panel-head">
          <div>
            <h2>Chi tiết đơn hàng</h2>
            <p v-if="selectedOrder">
              Mã đơn: <b>{{ selectedOrder.orderCode || ('ORD' + (selectedOrder.orderId || selectedOrder.id)) }}</b>
            </p>
          </div>
          <button class="outline-btn" @click="openPage(currentUser ? 'myOrders' : (staffUser ? 'orders' : 'shop'))">
            Quay lại
          </button>
        </div>

        <p v-if="orderDetailLoading">Đang tải chi tiết đơn...</p>

        <template v-else-if="selectedOrder">
          <div class="order-detail-grid">
            <div>
              <span>Khách hàng</span>
              <b>{{ selectedOrder.customerName || ('ID: ' + selectedOrder.customerId) }}</b>
            </div>
            <div>
              <span>Ngày tạo</span>
              <b>{{ selectedOrder.orderDate ? new Date(selectedOrder.orderDate).toLocaleString('vi-VN') : '' }}</b>
            </div>
            <div>
              <span>Trạng thái đơn</span>
              <b>{{ statusLabel(selectedOrder.orderStatus || selectedOrder.status) }}</b>
            </div>
            <div>
              <span>Trạng thái thanh toán</span>
              <b :class="['detail-status', paymentStatusClass(selectedOrder)]">{{ paymentStatusText(selectedOrder) }}</b>
            </div>
            <div>
              <span>Phương thức thanh toán</span>
              <b>{{ paymentMethodLabel(selectedOrder.paymentMethod) }}</b>
            </div>
            <div>
              <span>Tổng tiền</span>
              <b>{{ formatMoney(orderTotalAmount(selectedOrder)) }}</b>
            </div>
            <div>
              <span>Đã thanh toán</span>
              <b>{{ formatMoney(orderPaidAmount(selectedOrder)) }}</b>
            </div>
            <div>
              <span>Còn phải trả</span>
              <b>{{ formatMoney(orderDebtAmount(selectedOrder)) }}</b>
            </div>
          </div>

          <div v-if="canViewWarehouse && String(selectedOrder.orderStatus || selectedOrder.status).toLowerCase() === 'pending'" class="order-detail-actions">
            <button class="checkout-btn" @click="updateOrderStatus(selectedOrder, 'Confirmed')">
              Xác nhận xuất kho
            </button>
          </div>

          <table>
            <thead>
              <tr>
                <th>Mã SP</th>
                <th>Tên sản phẩm</th>
                <th>Số lượng</th>
                <th>Đơn giá</th>
                <th>Thành tiền</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="item in (selectedOrder.items || [])" :key="item.orderDetailId || item.productId">
                <td>{{ item.productCode }}</td>
                <td>{{ item.productName }}</td>
                <td>{{ item.quantity }}</td>
                <td>{{ formatMoney(item.unitPrice) }}</td>
                <td>{{ formatMoney(item.subTotal || (item.unitPrice * item.quantity)) }}</td>
              </tr>
            </tbody>
          </table>
        </template>
      </section>
    </main>

    <!-- WAREHOUSE -->
    <main v-if="activePage === 'warehouse'" class="page">
      <section class="panel">
        <div class="panel-head">
          <div>
            <h2>Kho xuất hàng</h2>
            <p>Đối chiếu đơn hàng cần xuất và số lượng sản phẩm trong từng đơn.</p>
          </div>
        </div>

        <table>
          <thead>
            <tr>
              <th>Mã đơn</th>
              <th>Khách hàng</th>
              <th>Trạng thái</th>
              <th>Sản phẩm cần xuất</th>
              <th>Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="order in warehouseOrders" :key="order.orderId || order.id">
              <td>{{ order.orderCode || 'ORD' + (order.orderId || order.id) }}</td>
              <td>{{ order.customerName || order.customerId }}</td>
              <td>
                <span :class="statusClass(order.orderStatus || order.paymentStatus || order.status)">
                  {{ statusLabel(order.orderStatus || order.paymentStatus || order.status) }}
                </span>
              </td>
              <td>
                <div v-for="item in (order.items || [])" :key="item.orderDetailId || item.productId" class="warehouse-item">
                  {{ item.productName }} × {{ item.quantity }}
                </div>
              </td>
              <td class="warehouse-actions">
                <button class="small-btn" @click="openOrderDetail(order)">Đối chiếu</button>
                <button
                  v-if="canViewWarehouse && String(order.orderStatus || order.status).toLowerCase() === 'pending'"
                  class="small-btn primary"
                  @click="updateOrderStatus(order, 'Confirmed')"
                >
                  Xác nhận xuất
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

        <div class="stat-card">
          <span>Pending retry</span>
          <b>{{ pendingOutboxCount }}</b>
        </div>

        <div class="stat-card">
          <span>Đã gửi Nhóm 3</span>
          <b>{{ processedOutboxCount }}</b>
        </div>

        <div class="stat-card">
          <span>Gửi lỗi</span>
          <b>{{ failedOutboxCount }}</b>
        </div>
      </section>

      <section class="panel">
        <h2>Đồng bộ dữ liệu microservices</h2>
        <p>
          Trang này mô phỏng Nhóm 2 nhận stock.updated từ Nhóm 1 và phát order.created sang Nhóm 3.
        </p>
        <button class="outline-btn sync-refresh" @click="loadAll">Làm mới trạng thái</button>

        <table>
          <thead>
            <tr>
              <th>ID</th>
              <th>Event</th>
              <th>Trạng thái</th>
              <th>Retry</th>
              <th>Ngày tạo</th>
            </tr>
          </thead>

          <tbody>
            <tr v-for="message in outboxMessages" :key="message.id || message.outboxMessageId">
              <td>{{ message.id || message.outboxMessageId }}</td>
              <td>{{ message.eventName }}</td>
              <td>
                <span :class="statusClass(message.status)">
                  {{ message.status }}
                </span>
              </td>
              <td>{{ message.retryCount || 0 }}/5</td>
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

                <p class="cart-code">
                  {{ item.productCode }} · Đang chọn {{ item.quantity }} · Còn có thể thêm {{ cartRemainingForItem(item) }}
                </p>

                <div class="cart-row-bottom">
                  <b>{{ formatMoney(item.unitPrice) }}</b>

                  <div class="quantity-control">
                    <button @click="updateQuantity(item, item.quantity - 1)">−</button>
                    <input
                      :value="item.quantity"
                      type="number"
                      min="1"
                      :max="cartStockLimit(item)"
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
            <p class="checkout-note">
              Khách hàng chỉ cần chọn phương thức thanh toán và xác nhận đơn. Chiết khấu, xác nhận tiền đã trả và công nợ sẽ do hệ thống/nhân viên xử lý ở trang quản trị.
            </p>

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

            <div v-if="currentUser" class="shipping-form drawer-shipping">
              <b>Thông tin nhận hàng</b>
              <label>Họ tên</label>
              <input v-model="checkoutShipping.fullName" type="text" />
              <label>Số điện thoại</label>
              <input v-model="checkoutShipping.phone" type="text" />
              <label>Địa chỉ nhận hàng</label>
              <textarea v-model="checkoutShipping.address" rows="2" placeholder="Nhập địa chỉ nhận hàng"></textarea>
            </div>

            <button class="checkout-btn" @click="prepareCheckoutConfirmation">
              Đặt hàng
            </button>

            <button class="continue-btn" @click="cartOpen = false">
              Tiếp tục chọn sản phẩm
            </button>
          </div>
        </template>
      </aside>
    </div>

    <!-- STAFF AUTH MODAL -->
    <div v-if="showCheckoutConfirmModal" class="modal-backdrop">
      <div class="modal-card">
        <div class="drawer-head">
          <h2>Xác nhận đơn hàng</h2>
          <button class="close-btn" @click="closeCheckoutConfirmModal">✕</button>
        </div>

        <div class="confirm-layout">
          <div class="confirm-summary">
            <div class="money-line"><span>Số sản phẩm</span><b>{{ cartTotalQuantity }}</b></div>
            <div class="money-line"><span>Tổng tiền hàng</span><b>{{ formatMoney(cartTotal) }}</b></div>
            <div class="money-line"><span>Chiết khấu</span><b>- {{ formatMoney(checkout.discountAmount || 0) }}</b></div>
            <div class="money-line paid"><span>Thanh toán trước</span><b>{{ formatMoney(Number(checkout.paidAmount ?? finalAmount)) }}</b></div>
            <div v-if="debtAmount > 0" class="money-line debt"><span>Công nợ phát sinh</span><b>{{ formatMoney(debtAmount) }}</b></div>
            <div class="money-line final"><span>Tổng đơn</span><b>{{ formatMoney(finalAmount) }}</b></div>
          </div>

          <div class="checkout-note">
            <b>{{ checkoutShipping.fullName || currentUser?.fullName }}</b><br />
            {{ checkoutShipping.phone || currentUser?.phone }}<br />
            {{ checkoutShipping.address }}
          </div>

          <div v-if="debtAmount > 0" class="checkout-note debt-hint">
            Đơn này sẽ tạo công nợ tự động, hạn thanh toán dự kiến {{ formatDate(expectedDebtDueDate) }}.
          </div>
        </div>

        <div class="modal-actions">
          <button class="outline-btn" :disabled="checkoutSubmitting" @click="closeCheckoutConfirmModal">Kiểm tra lại</button>
          <button class="checkout-btn" :disabled="checkoutSubmitting" @click="confirmCheckout">
            {{ checkoutSubmitting ? 'Đang tạo đơn...' : 'Xác nhận đặt hàng' }}
          </button>
        </div>
      </div>
    </div>

    <div v-if="showStaffAuthModal" class="modal-backdrop">
      <div class="modal-card small-modal">
        <div class="drawer-head">
          <h2>Đăng nhập nhân viên</h2>
          <button class="close-btn" @click="closeStaffAuth">✕</button>
        </div>

        <div class="form-grid one">
          <label>Tên đăng nhập</label>
          <input v-model="staffLoginForm.username" type="text" placeholder="sales01" />

          <label>Vai trò</label>
          <select v-model="staffLoginForm.role">
            <option value="Sales">Sales</option>
            <option value="Admin">Admin</option>
            <option value="Warehouse">Warehouse</option>
          </select>

          <p v-if="staffAuthError" class="error-text">{{ staffAuthError }}</p>

          <button class="checkout-btn" @click="loginStaff">
            Đăng nhập nhân viên
          </button>
        </div>
      </div>
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

          <label>Mật khẩu demo</label>
          <input v-model="loginForm.password" type="password" placeholder="Nhập mật khẩu " />

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
          <input v-model="registerForm.password" type="password" placeholder="Tạo mật khẩu" />
          

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

        <div v-if="selectedDebt" class="checkout-note debt-hint">
          Còn phải thu: <b>{{ formatMoney(selectedDebt.remainingAmount || selectedDebt.debtAmount) }}</b>
          <span v-if="selectedDebt.dueDate"> · hạn {{ formatDate(selectedDebt.dueDate) }}</span>
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

    <section v-if="activePage === 'shop' && topManufacturers.length > 0" class="manufacturer-section footer-adjacent">
      <h2>Đội ngũ các nhà sản xuất và đối tác hàng đầu</h2>
      <div class="manufacturer-grid">
        <article v-for="partner in topManufacturers" :key="partner.name" class="manufacturer-card">
          <div class="manufacturer-logo">{{ partner.initials }}</div>
          <h3>{{ partner.name }}</h3>
          <p>{{ partner.primaryCategory }}</p>
          <span>{{ partner.productCount }} dòng sản phẩm</span>
        </article>
      </div>
    </section>

    <footer class="site-footer">
      <div class="footer-inner">
        <section class="footer-column support-column">
          <h3>Tổng đài hỗ trợ miễn phí</h3>
          <p>Mua hàng - bảo hành <b>12345.6789</b> (7h30 - 22h00)</p>
          <p>Khiếu nại <b>13578.3424</b> (8h00 - 21h30)</p>

          <h3>Phương thức thanh toán</h3>
          <div class="payment-icons">
            <img class="payment-logo" src="/payments/apple-pay-og.png" alt="Apple Pay" />
            <img class="payment-logo" src="/payments/vnpay-logo.webp" alt="VNPay" />
            <img class="payment-logo" src="/payments/momo_1.webp" alt="Momo" />
            <img class="payment-logo" src="/payments/onepay-logo.webp" alt="OnePay" />
            <img class="payment-logo" src="/payments/mpos-logo.webp" alt="mPOS" />
            <img class="payment-logo" src="/payments/zalopay-logo.webp" alt="ZaloPay" />
            <img class="payment-logo" src="/payments/kredivo-logo.webp" alt="Kredivo" />
            <img class="payment-logo" src="/payments/fundiin.webp" alt="Fundiin" />
          </div>

          <h3>Đăng ký nhận tin khuyến mại</h3>
          <div class="subscribe-box">
            <b>Nhận ngay voucher 10%</b>
            <span>Voucher sử dụng trong 24h cho khách hàng mới.</span>
          </div>
          <input type="email" placeholder="Nhập email của bạn" />
          <input type="text" placeholder="Nhập số điện thoại của bạn" />
          <label class="footer-check">
            <input type="checkbox" checked />
            Tôi đồng ý với điều khoản của RetailERP
          </label>
          <button class="subscribe-btn">Đăng ký ngay</button>
        </section>

        <section class="footer-column">
          <h3>Thông tin về chính sách</h3>
          <a href="#">Mua hàng và thanh toán Online</a>
          <a href="#">Mua hàng trả góp</a>
          <a href="#">Chính sách giao hàng</a>
          <a href="#">Chính sách đổi trả</a>
          <a href="#">Tra cứu bảo hành</a>
          <a href="#">Tra cứu hóa đơn điện tử</a>
          <a href="#">Thông tin hóa đơn mua hàng</a>
          <a href="#">Quy định bảo mật dữ liệu</a>
        </section>

        <section class="footer-column">
          <h3>Dịch vụ và thông tin khác</h3>
          <a href="#">Khách hàng doanh nghiệp</a>
          <a href="#">Ưu đãi thanh toán</a>
          <a href="#">Quy chế hoạt động</a>
          <a href="#">Chính sách bảo mật thông tin cá nhân</a>
          <a href="#">Chính sách bảo hành</a>
          <a href="#">Liên hệ hợp tác kinh doanh</a>
          <a href="#">Tuyển dụng</a>

          <h3>Mua sắm dễ dàng trên ứng dụng</h3>
          <div class="app-download">
            <div class="qr-box">QR</div>
            <div>
              <img class="app-badge-image" src="/payments/downloadANDROID.webp" alt="Tải trên Google Play" />
              <img class="app-badge-image" src="/payments/downloadiOS.webp" alt="Tải trên App Store" />
            </div>
          </div>
        </section>

        <section class="footer-column social-column">
          <h3>Kết nối với RetailERP</h3>
          <div class="social-links">
            <a href="#" aria-label="YouTube">▶</a>
            <a href="#" aria-label="Facebook">f</a>
            <a href="#" aria-label="Instagram">◎</a>
            <a href="#" aria-label="TikTok">♪</a>
            <a href="#" aria-label="Zalo">Zalo</a>
          </div>

          <h3>Website thành viên</h3>
          <p>Hệ thống bán hàng và quản lý đơn</p>
          <b class="brand-badge">RetailERP Sale</b>
          <p>Trung tâm hỗ trợ khách hàng</p>
          <b class="brand-badge care">RetailCare</b>
          <p>Kênh tin tức khuyến mại và công nghệ</p>
          <b class="brand-badge news">RetailNews</b>

        </section>
      </div>

      <div class="footer-tags">
        <a href="#">Gia dụng</a>
        <a href="#">Điện tử</a>
        <a href="#">Thời trang</a>
        <a href="#">Thực phẩm</a>
        <a href="#">Văn phòng phẩm</a>
        <a href="#">Trả góp</a>
        <a href="#">Flash Sale</a>
        <a href="#">Tra cứu đơn hàng</a>
      </div>

      <div class="footer-bottom">
        <span>Công ty TNHH RetailERP Demo - Order & Sales Service Nhóm 2</span>
        <span>Địa chỉ: Hà Nội, Việt Nam · Tổng đài: 1800.2044</span>
      </div>
    </footer>
  </div>
</template>

<style scoped>
:global(*) {
  box-sizing: border-box;
}

:global(body) {
  margin: 0;
  background: #f6f8fb;
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
  background: white;
  color: #172033;
  border-bottom: 1px solid #e7edf5;
  box-shadow: 0 10px 28px rgba(15, 23, 42, 0.08);
}

.header-top {
  background: #172033;
  color: #e5edf7;
  overflow: hidden;
}

.header-top-inner {
  max-width: 1180px;
  margin: 0 auto;
  height: 34px;
  display: flex;
  align-items: center;
  font-size: 12px;
  font-weight: 700;
  white-space: nowrap;
  overflow: hidden;
}

.promo-marquee {
  display: flex;
  align-items: center;
  gap: 34px;
  min-width: max-content;
  animation: promo-marquee 28s linear infinite;
}

.promo-marquee span {
  display: inline-flex;
  align-items: center;
  color: #f8fafc;
}

.header-top:hover .promo-marquee {
  animation-play-state: paused;
}

@keyframes promo-marquee {
  from {
    transform: translateX(0);
  }

  to {
    transform: translateX(-50%);
  }
}

.header-main {
  max-width: 1180px;
  margin: 0 auto;
  padding: 13px 12px 10px;
  display: grid;
  grid-template-columns: auto auto 1fr auto auto;
  align-items: center;
  gap: 12px;
}

.logo {
  border: none;
  background: transparent;
  color: #172033;
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
  background: #2f55d4;
  color: white;
}

.header-action {
  min-height: 40px;
  border: 1px solid #e1e8f2;
  border-radius: 10px;
  padding: 0 12px;
  background: #f5f8fc;
  color: #26324a;
  font-weight: 800;
  white-space: nowrap;
}

.header-action:hover {
  background: #eef4ff;
  border-color: #cfe0ff;
}

.search-box {
  height: 46px;
  display: flex;
  align-items: center;
  gap: 8px;
  background: white;
  color: #172033;
  border: 2px solid #2f55d4;
  border-radius: 12px;
  padding: 0 14px;
  box-shadow: 0 8px 20px rgba(47, 85, 212, .10);
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
  background: #172033;
  border-color: #172033;
  color: white;
}

.cart-button b {
  margin-left: 6px;
  background: #ffba49;
  color: #172033;
  padding: 2px 7px;
  border-radius: 999px;
}

.user-actions {
  display: flex;
  gap: 8px;
}

.logout-button {
  background: #fff1f2;
  border-color: #fecdd3;
  color: #be123c;
}

.quick-nav {
  max-width: 1180px;
  margin: 0 auto;
  padding: 0 12px 12px;
  display: flex;
  gap: 8px;
  overflow-x: auto;
}

.quick-nav button {
  min-height: 38px;
  border: 1px solid #e1e8f2;
  border-radius: 999px;
  padding: 0 16px;
  color: #475569;
  background: white;
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
  background: #f5f8fc;
  color: #172033;
}

.quick-nav button.active {
  background: #edf4ff;
  color: #1f3d99;
  border-color: #cfe0ff;
  box-shadow: none;
}

/* PAGE */
.page {
  max-width: 1180px;
  margin: 0 auto;
  padding: 16px 12px 60px;
}

.home-layout {
  display: grid;
  grid-template-columns: 230px 1fr;
  gap: 16px;
}

.category-list,
.hero-banner,
.member-card,
.panel,
.product-card,
.stat-card {
  background: white;
  border-radius: 12px;
  box-shadow: 0 12px 28px rgba(23, 32, 51, 0.08);
}

.category-list {
  padding: 12px;
  display: grid;
  align-content: start;
  gap: 8px;
}

.side-title {
  padding: 4px 4px 8px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  color: #172033;
  font-weight: 900;
}

.side-title b {
  min-width: 28px;
  height: 24px;
  border-radius: 999px;
  display: grid;
  place-items: center;
  background: #e8f7ef;
  color: #087647;
  font-size: 12px;
}

.category-list button {
  width: 100%;
  min-height: 46px;
  border: 1px solid transparent;
  border-radius: 10px;
  padding: 0 11px;
  display: grid;
  grid-template-columns: 34px 1fr auto;
  align-items: center;
  gap: 8px;
  background: transparent;
  color: #26324a;
  text-align: left;
  font-weight: 850;
}

.category-list button:hover,
.category-list button.active {
  background: #f0f7ff;
  border-color: #d8e6ff;
  color: #1f3d99;
}

.category-icon {
  width: 30px;
  height: 30px;
  border-radius: 8px;
  display: grid;
  place-items: center;
  background: #f6f8fb;
}

.category-list em {
  min-width: 24px;
  height: 22px;
  border-radius: 999px;
  display: grid;
  place-items: center;
  font-style: normal;
  background: #f1f5f9;
  color: #64748b;
  font-size: 12px;
}

.hero-banner {
  min-height: 320px;
  overflow: hidden;
  display: grid;
  grid-template-columns: minmax(0, 1fr);
  gap: 22px;
  padding: 28px;
  background:
    radial-gradient(circle at 78% 18%, rgba(255, 186, 73, .34), transparent 24%),
    linear-gradient(135deg, #ffffff 0%, #edf6ff 55%, #e9f7ef 100%);
}

.hero-content {
  padding: 4px 0;
  align-self: center;
}

.hero-content .tag {
  width: fit-content;
  padding: 7px 10px;
  border-radius: 999px;
  background: #fff7ed;
  color: #b45309;
  font-weight: 900;
  margin: 0 0 12px;
  font-size: 12px;
}

.hero-content h1 {
  max-width: 600px;
  margin: 0;
  font-size: 36px;
  line-height: 1.08;
  color: #172033;
}

.hero-content p {
  max-width: 620px;
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
  background: #2f55d4;
  color: white;
  font-weight: 900;
}

.ghost-btn {
  background: white !important;
  color: #1f3d99 !important;
  border: 1px solid #dbe6ff !important;
}

.hero-stats {
  margin-top: 24px;
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
}

.hero-stats span {
  min-width: 112px;
  padding: 10px 12px;
  border-radius: 10px;
  background: rgba(255, 255, 255, .74);
  color: #64748b;
  font-size: 12px;
  font-weight: 800;
}

.hero-stats b {
  display: block;
  color: #172033;
  font-size: 20px;
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

.market-showcase {
  margin: 16px 0 24px;
  padding: 0 14px 16px;
  border: 2px solid #5b8cff;
  border-radius: 12px;
  background: #eef5ff;
  box-shadow: 0 16px 34px rgba(31, 61, 153, .12);
}

.showcase-tabs {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  margin: -2px -16px 14px;
}

.showcase-tabs button {
  min-height: 56px;
  border: 1px solid #d9e6ff;
  border-top: none;
  background: white;
  color: #2385e8;
  font-size: 20px;
  font-weight: 950;
  text-transform: uppercase;
}

.showcase-tabs button:first-child {
  border-radius: 10px 0 0 0;
}

.showcase-tabs button:last-child {
  border-radius: 0 10px 0 0;
}

.showcase-tabs button.active {
  background: #eaf3ff;
  color: #172033;
  box-shadow: inset 0 3px 0 #2f55d4;
}

.showcase-categories {
  display: flex;
  gap: 8px;
  overflow-x: auto;
  padding: 0 0 12px;
}

.showcase-categories button {
  min-height: 34px;
  border: 1px solid #cfdcff;
  border-radius: 8px;
  padding: 0 12px;
  background: white;
  color: #172033;
  font-size: 12px;
  font-weight: 900;
  white-space: nowrap;
}

.showcase-categories button.active {
  background: #2f55d4;
  border-color: #2f55d4;
  color: white;
}

.showcase-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 12px;
}

.showcase-card {
  min-width: 0;
  padding: 10px;
  border-radius: 12px;
  background: white;
  box-shadow: 0 10px 24px rgba(23, 32, 51, .08);
  display: grid;
  grid-template-rows: auto auto auto auto auto 1fr;
  gap: 7px;
}

.showcase-image {
  position: relative;
  height: 132px;
  border-radius: 10px;
  display: grid;
  place-items: center;
  overflow: hidden;
}

.showcase-badge {
  position: absolute;
  top: 8px;
  left: 8px;
  border-radius: 999px;
  padding: 5px 8px;
  background: #e11d48;
  color: white;
  font-size: 11px;
  font-weight: 900;
}

.showcase-card p {
  margin: 2px 0 0;
  color: #64748b;
  font-size: 12px;
  font-weight: 900;
}

.showcase-card h3 {
  min-height: 40px;
  margin: 0;
  color: #172033;
  font-size: 15px;
  line-height: 1.35;
}

.showcase-card strong {
  color: #dc2626;
  font-size: 18px;
}

.showcase-stock {
  width: fit-content;
  border-radius: 999px;
  padding: 5px 8px;
  font-size: 12px;
  font-weight: 900;
}

.showcase-card button {
  align-self: end;
  border: none;
  border-radius: 10px;
  padding: 10px 12px;
  background: #172033;
  color: white;
  font-weight: 900;
}

.showcase-card button:disabled {
  background: #cbd5e1;
}

.showcase-empty {
  padding: 26px;
  border-radius: 12px;
  background: white;
  text-align: center;
  color: #64748b;
  font-weight: 900;
}

.section-head {
  margin: 22px 0 14px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.catalog-head {
  scroll-margin-top: 150px;
  gap: 16px;
}

.category-product-section {
  scroll-margin-top: 150px;
  margin-bottom: 28px;
}

.category-product-head {
  margin-top: 0;
  padding-top: 4px;
}

.section-head h2 {
  margin-bottom: 4px;
}

.section-head p {
  margin-top: 0;
  color: #6b7280;
}

.catalog-tools {
  display: flex;
  align-items: center;
  gap: 10px;
}

.catalog-tools select {
  min-height: 40px;
  border: 1px solid #dbe3ef;
  border-radius: 10px;
  padding: 0 12px;
  background: white;
  color: #172033;
  font-weight: 800;
}

.outline-btn {
  border: 1px solid #2f55d4;
  color: #2f55d4;
  background: white;
  border-radius: 10px;
  padding: 10px 14px;
  font-weight: 800;
}

.product-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 16px;
}

.product-card {
  padding: 12px;
  transition: .2s;
  border: 1px solid #edf1f7;
  display: grid;
  grid-template-rows: auto auto auto auto auto 1fr;
  min-width: 0;
}

.product-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 18px 36px rgba(23, 32, 51, 0.12);
}

.product-image {
  position: relative;
  height: 150px;
  border-radius: 10px;
  display: grid;
  place-items: center;
  overflow: hidden;
}

.product-image::after {
  content: '';
  position: absolute;
  inset: auto 18px 12px;
  height: 16px;
  border-radius: 999px;
  background: rgba(15, 23, 42, .08);
  filter: blur(3px);
}

.product-visual {
  position: relative;
  z-index: 1;
  font-size: 58px;
}

.tone-home {
  background: linear-gradient(135deg, #fff7ed, #fde68a);
}

.tone-tech {
  background: linear-gradient(135deg, #eff6ff, #bfdbfe);
}

.tone-food {
  background: linear-gradient(135deg, #f0fdf4, #bbf7d0);
}

.tone-fashion {
  background: linear-gradient(135deg, #fdf2f8, #fbcfe8);
}

.tone-office {
  background: linear-gradient(135deg, #f8fafc, #cbd5e1);
}

.tone-default {
  background: linear-gradient(135deg, #f8fafc, #e0f2fe);
}

.product-badge {
  position: absolute;
  z-index: 2;
  top: 10px;
  left: 10px;
  border-radius: 999px;
  padding: 6px 9px;
  font-size: 11px;
  font-weight: 900;
}

.stock-ready {
  background: #dcfce7;
  color: #166534;
}

.stock-low {
  background: #fef3c7;
  color: #92400e;
}

.stock-empty {
  background: #fee2e2;
  color: #b91c1c;
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
  margin: 10px 0 8px;
  color: #172033;
  line-height: 1.3;
}

.price-row {
  min-height: 32px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
}

.price {
  margin: 0;
  color: #3454d1;
  font-size: 20px;
  font-weight: 900;
}

.cart-chip {
  border-radius: 999px;
  padding: 5px 8px;
  background: #fff7ed;
  color: #c2410c;
  font-size: 11px;
  font-weight: 900;
  white-space: nowrap;
}

.stock-meter {
  height: 7px;
  border-radius: 999px;
  background: #eef2f7;
  overflow: hidden;
  margin: 12px 0 8px;
}

.stock-meter span {
  display: block;
  height: 100%;
  border-radius: inherit;
  background: linear-gradient(90deg, #22c55e, #2f55d4);
}

.stock {
  margin: 0 0 12px;
  font-weight: 800;
  font-size: 13px;
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
  align-self: end;
  background: #172033;
  color: white;
  font-weight: 900;
}

.product-card button:disabled {
  background: #cbd5e1;
  cursor: not-allowed;
}

.floating-cart-summary {
  position: fixed;
  left: 50%;
  bottom: 18px;
  transform: translateX(-50%);
  z-index: 90;
  width: min(560px, calc(100% - 24px));
  min-height: 58px;
  border-radius: 14px;
  padding: 9px 10px 9px 16px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 14px;
  background: #172033;
  color: white;
  box-shadow: 0 18px 40px rgba(15, 23, 42, .28);
}

.floating-cart-summary div {
  min-width: 0;
  display: grid;
  gap: 2px;
}

.floating-cart-summary span {
  color: #cbd5e1;
  font-size: 13px;
  font-weight: 800;
}

.floating-cart-summary button {
  border: none;
  border-radius: 10px;
  padding: 11px 16px;
  background: #ffba49;
  color: #172033;
  font-weight: 900;
  white-space: nowrap;
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

.clickable-row {
  cursor: pointer;
}

.clickable-row:hover {
  background: #f8fafc;
}

.order-detail-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 12px;
  margin-bottom: 18px;
}

.order-detail-grid div {
  padding: 12px;
  border-radius: 12px;
  background: #f8fafc;
}

.order-detail-grid span {
  display: block;
  color: #64748b;
  font-size: 13px;
  margin-bottom: 4px;
}

.detail-status {
  display: inline-flex;
  width: fit-content;
  align-items: center;
}

.warehouse-item {
  padding: 2px 0;
  color: #334155;
  font-size: 13px;
}

.page-empty {
  margin-top: 12px;
}

.page-cart-list {
  margin: 0 0 18px;
}

.page-checkout {
  margin: 0;
}

.lookup-box {
  margin: 16px 0;
  padding: 14px;
  border-radius: 14px;
  background: #f8fafc;
}

.lookup-row {
  display: flex;
  gap: 10px;
  margin-top: 8px;
}

.lookup-row input {
  flex: 1;
  min-height: 42px;
  border: 1px solid #d1d5db;
  border-radius: 12px;
  padding: 0 12px;
}

.shipping-form {
  display: grid;
  gap: 8px;
  padding: 12px;
  border-radius: 14px;
  background: #f8fafc;
}

.shipping-form label,
.drawer-shipping label {
  color: #475569;
  font-size: 13px;
  font-weight: 800;
}

.shipping-form input,
.shipping-form textarea,
.drawer-shipping input,
.drawer-shipping textarea {
  width: 100%;
  border: 1px solid #d1d5db;
  border-radius: 12px;
  padding: 10px 12px;
}

.account-form {
  max-width: 520px;
}

.success-text {
  background: #dcfce7;
  color: #166534;
  padding: 10px;
  border-radius: 10px;
}

.field-hint {
  color: #64748b;
  font-size: 12px;
}

.manufacturer-section {
  max-width: 1180px;
  margin: 22px auto 0;
  padding: 0 12px 30px;
}

.footer-adjacent {
  border-top: 1px solid #e5e7eb;
  padding-top: 24px;
}

.manufacturer-section h2 {
  margin: 0 0 16px;
  color: #172033;
  font-size: 28px;
}

.manufacturer-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 14px;
}

.manufacturer-card {
  min-height: 188px;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  padding: 18px;
  background: white;
  display: grid;
  place-items: center;
  text-align: center;
  box-shadow: 0 10px 26px rgba(23, 32, 51, .06);
}

.manufacturer-logo {
  width: 94px;
  height: 82px;
  border-radius: 14px;
  display: grid;
  place-items: center;
  background: linear-gradient(135deg, #eff6ff, #ecfdf5);
  color: #2f55d4;
  font-size: 28px;
  font-weight: 950;
}

.manufacturer-card h3 {
  margin: 8px 0 0;
  color: #172033;
  font-size: 16px;
}

.manufacturer-card p {
  margin: 0;
  color: #64748b;
  font-size: 13px;
  font-weight: 900;
}

.manufacturer-card span {
  color: #2f55d4;
  font-weight: 900;
}

.site-footer {
  margin-top: 36px;
  background: white;
  border-top: 1px solid #e5e7eb;
  color: #172033;
  font-size: 13px;
}

.footer-inner {
  max-width: 1180px;
  margin: 0 auto;
  padding: 28px 12px 24px;
  display: grid;
  grid-template-columns: 1.25fr 1fr 1.1fr 1fr;
  gap: 28px;
}

.footer-column {
  min-width: 0;
  display: grid;
  align-content: start;
  gap: 9px;
}

.footer-column h3 {
  margin: 0 0 6px;
  color: #172033;
  font-size: 14px;
}

.footer-column p {
  margin: 0;
  color: #475569;
  line-height: 1.5;
}

.footer-column a {
  color: #26324a;
  text-decoration: none;
}

.footer-column a:hover {
  color: #2f55d4;
  text-decoration: underline;
}

.payment-icons,
.social-links {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.payment-logo {
  width: 82px;
  height: 38px;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  padding: 5px;
  background: #f8fafc;
  object-fit: contain;
}

.subscribe-box {
  padding: 10px;
  border-radius: 10px;
  background: #fff1f2;
  color: #be123c;
}

.subscribe-box b,
.subscribe-box span {
  display: block;
}

.subscribe-box span {
  margin-top: 4px;
  color: #64748b;
}

.support-column input[type="email"],
.support-column input[type="text"] {
  width: 100%;
  min-height: 38px;
  border: 1px solid #d1d5db;
  border-radius: 8px;
  padding: 0 10px;
}

.footer-check {
  display: flex;
  gap: 6px;
  align-items: center;
  color: #64748b;
  font-size: 12px;
}

.subscribe-btn {
  border: none;
  border-radius: 8px;
  min-height: 40px;
  background: #e11d48;
  color: white;
  font-weight: 900;
  text-transform: uppercase;
}

.app-download {
  display: grid;
  grid-template-columns: 84px 1fr;
  gap: 10px;
  align-items: center;
}

.app-download > div:last-child {
  display: grid;
  gap: 7px;
}

.qr-box {
  width: 84px;
  height: 84px;
  border: 1px solid #cbd5e1;
  border-radius: 8px;
  display: grid;
  place-items: center;
  background:
    linear-gradient(90deg, #111827 8px, transparent 8px) 0 0 / 18px 18px,
    linear-gradient(#111827 8px, transparent 8px) 0 0 / 18px 18px,
    white;
  color: #e11d48;
  font-weight: 900;
}

.app-badge-image {
  width: 150px;
  height: 42px;
  object-fit: contain;
}

.social-links a {
  width: 30px;
  height: 30px;
  border-radius: 8px;
  display: grid;
  place-items: center;
  background: #f1f5f9;
  color: #172033;
  text-decoration: none;
  font-weight: 900;
}

.social-links a:nth-child(1) {
  background: #fee2e2;
  color: #dc2626;
}

.social-links a:nth-child(2) {
  background: #dbeafe;
  color: #2563eb;
}

.social-links a:nth-child(3) {
  background: #fce7f3;
  color: #db2777;
}

.social-links a:nth-child(4) {
  background: #111827;
  color: white;
}

.social-links a:nth-child(5) {
  width: auto;
  padding: 0 8px;
  background: #e0f2fe;
  color: #0284c7;
}

.brand-badge {
  width: fit-content;
  border-radius: 4px;
  padding: 5px 8px;
  background: #ef4444;
  color: white;
  font-size: 15px;
}

.brand-badge.care {
  background: #334155;
}

.brand-badge.news {
  background: #f97316;
}

.footer-tags {
  border-top: 1px solid #e5e7eb;
  max-width: 1180px;
  margin: 0 auto;
  padding: 16px 12px;
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  gap: 12px 22px;
}

.footer-tags a {
  color: #334155;
  text-decoration: none;
}

.footer-bottom {
  border-top: 1px solid #e5e7eb;
  padding: 14px 12px 22px;
  display: grid;
  gap: 4px;
  place-items: center;
  color: #64748b;
  text-align: center;
  font-size: 12px;
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

.debt-hint b {
  color: #b45309;
}

.small-top {
  margin-top: 8px;
}

.warehouse-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}

.order-detail-actions {
  margin-bottom: 16px;
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

.confirm-layout {
  display: grid;
  gap: 14px;
}

.confirm-summary {
  display: grid;
  gap: 8px;
  padding: 14px;
  border: 1px solid #e5e7eb;
  border-radius: 14px;
  background: #f8fafc;
}

.sync-refresh {
  width: fit-content;
  margin: 8px 0 16px;
}

button:disabled {
  cursor: not-allowed;
  opacity: .65;
}

/* RESPONSIVE */
@media (max-width: 1100px) {
  .header-main {
    grid-template-columns: auto 1fr auto;
  }

  .location-button {
    display: none;
  }

  .quick-nav {
    grid-template-columns: repeat(3, 1fr);
  }

  .home-layout {
    grid-template-columns: 1fr;
  }

  .hero-banner {
    grid-template-columns: 1fr;
  }

  .member-card {
    display: none;
  }

  .product-grid {
    grid-template-columns: repeat(3, 1fr);
  }

  .footer-inner {
    grid-template-columns: repeat(2, 1fr);
  }

  .showcase-grid,
  .manufacturer-grid {
    grid-template-columns: repeat(3, minmax(0, 1fr));
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

  .hero-banner {
    padding: 20px;
  }

  .showcase-tabs,
  .showcase-grid,
  .manufacturer-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .catalog-head {
    align-items: flex-start;
    flex-direction: column;
  }

  .catalog-tools {
    width: 100%;
    display: grid;
    grid-template-columns: 1fr;
  }

  .product-grid {
    grid-template-columns: repeat(2, 1fr);
  }

  .stats {
    grid-template-columns: 1fr;
  }

  .footer-inner {
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

  .hero-stats {
    display: grid;
    grid-template-columns: repeat(3, minmax(0, 1fr));
  }

  .hero-stats span {
    min-width: 0;
  }

  .showcase-tabs,
  .showcase-grid,
  .manufacturer-grid {
    grid-template-columns: 1fr;
  }

  .floating-cart-summary {
    bottom: 10px;
    border-radius: 12px;
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
