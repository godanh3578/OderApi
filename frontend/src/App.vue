<script setup>
import { computed, onMounted, ref, watch } from 'vue'
import { ShoppingBasket } from '@lucide/vue'
import { useRoute, useRouter } from 'vue-router'
import api, { getStaffToken, setStaffToken } from './api/client'
import {
  loadCustomerCart,
  loadCustomerUser,
  loadDemoCustomer,
  saveCustomerCart,
  saveCustomerUser,
  saveDemoCustomer,
  saveDemoPassword,
  verifyDemoPassword
} from './utils/customerStorage'

const router = useRouter()
const route = useRoute()

const pagePaths = {
  shop: '/',
  cart: '/cart',
  lookup: '/lookup',
  myOrders: '/my-orders',
  account: '/account',
  dashboard: '/manage/dashboard',
  orders: '/manage/orders',
  customers: '/manage/customers',
  suppliers: '/manage/suppliers',
  payments: '/manage/payments',
  debts: '/manage/debts',
  integration: '/manage/integration',
  warehouse: '/manage/warehouse'
}

const staffPages = ['dashboard', 'orders', 'customers', 'suppliers', 'payments', 'debts', 'integration', 'warehouse']
const staffPageTitles = {
  dashboard: 'Tổng quan bán hàng',
  orders: 'Quản lý đơn hàng',
  customers: 'Quản lý khách hàng',
  suppliers: 'Quản lý nhà cung cấp',
  payments: 'Thanh toán',
  debts: 'Công nợ',
  integration: 'Đồng bộ kho'
}

const demoProducts = [
  { productId: 1, productCode: 'GD001', productName: 'Nồi cơm điện Sunhouse 1.8L', categoryName: 'Gia dụng', sellingPrice: 650000, quantityAvailable: 12, stockStatus: 'InStock', manufacturerName: 'Sunhouse' },
  { productId: 2, productCode: 'GD002', productName: 'Máy xay sinh tố Philips', categoryName: 'Gia dụng', sellingPrice: 890000, quantityAvailable: 8, stockStatus: 'InStock', manufacturerName: 'Philips' },
  { productId: 3, productCode: 'GD003', productName: 'Quạt điện Panasonic', categoryName: 'Gia dụng', sellingPrice: 780000, quantityAvailable: 0, stockStatus: 'OutOfStock', manufacturerName: 'Panasonic' },
  { productId: 4, productCode: 'DT001', productName: 'Chuột Logitech M331', categoryName: 'Điện tử', sellingPrice: 320000, quantityAvailable: 30, stockStatus: 'InStock', manufacturerName: 'Logitech' },
  { productId: 5, productCode: 'DT002', productName: 'Bàn phím cơ AKKO', categoryName: 'Điện tử', sellingPrice: 1290000, quantityAvailable: 9, stockStatus: 'InStock', manufacturerName: 'AKKO' },
  { productId: 6, productCode: 'TP001', productName: 'Gạo ST25 túi 5kg', categoryName: 'Thực phẩm', sellingPrice: 185000, quantityAvailable: 25, stockStatus: 'InStock', manufacturerName: 'ST25' },
  { productId: 7, productCode: 'TT001', productName: 'Áo thun cotton nam', categoryName: 'Thời trang', sellingPrice: 150000, quantityAvailable: 18, stockStatus: 'InStock', manufacturerName: 'Đối tác Thời trang' },
  { productId: 8, productCode: 'VP001', productName: 'Bút bi Thiên Long hộp 20 cây', categoryName: 'Văn phòng phẩm', sellingPrice: 65000, quantityAvailable: 50, stockStatus: 'InStock', manufacturerName: 'Thiên Long' }
]

const LOCAL_ORDERS_KEY = 'retailerpLocalOrders'
const WALLET_STATE_KEY = 'retailerpWalletState'
const ACTIVITY_LOG_KEY = 'retailerpActivityLog'

const paymentMethods = [
  { value: 'Cash', label: 'Tiền mặt khi nhận hàng', note: 'Khách trả trực tiếp khi nhận hàng, đơn tạm ghi còn phải thu.' },
  { value: 'BankTransfer', label: 'Chuyển khoản ngân hàng', note: 'Xác nhận thanh toán đủ khi tạo đơn.' },
  { value: 'QR', label: 'Thanh toán QR', note: 'Thanh toán đủ qua mã QR của cửa hàng.' },
  { value: 'Wallet', label: 'Ví RetailERP', note: 'Trừ số dư ví, thiếu bao nhiêu sẽ ghi công nợ.' },
  { value: 'Deposit', label: 'Ứng cọc giữ đơn', note: 'Khách trả trước một phần để giữ hàng.' }
]

const memberTiers = [
  { name: 'Thường', minSpent: 0, rate: 0, className: 'basic', badge: 'TH' },
  { name: 'Bạc', minSpent: 2000000, rate: 2, className: 'silver', badge: 'B' },
  { name: 'Vàng', minSpent: 5000000, rate: 5, className: 'gold', badge: 'V' },
  { name: 'Kim cương', minSpent: 10000000, rate: 10, className: 'diamond', badge: 'KC' }
]

const products = ref([])
const productLoading = ref(false)
const productError = ref('')
const searchText = ref('')
const activeCategory = ref('')
const productSort = ref('popular')
const catalogPage = ref(1)
const productsPerPage = 12

const cart = ref(loadCustomerCart())
const currentUser = ref(loadCustomerUser())
const myOrders = ref([])
const myOrdersLoading = ref(false)
const selectedProduct = ref(null)
const walletTransactions = ref([])
const walletTopUpAmount = ref(200000)
const activityLogs = ref([])

const showAuthModal = ref(false)
const authMode = ref('login')
const authBusy = ref(false)
const authError = ref('')
const loginForm = ref({ phone: '', password: '' })
const registerForm = ref({ fullName: '', phone: '', email: '', address: '', password: '' })

const showStaffModal = ref(false)
const staffUser = ref(null)
const staffError = ref('')
const staffBusy = ref(false)
const staffLoginForm = ref({ username: 'sales01', role: 'Sales' })

const checkout = ref({
  voucher: 'NONE',
  paymentMethod: 'Cash',
  depositAmount: 0
})

const checkoutShipping = ref({
  fullName: '',
  phone: '',
  address: ''
})

const checkoutBusy = ref(false)
const checkoutMessage = ref('')

const orderLookup = ref({
  orderCode: '',
  phone: '',
  loading: false,
  error: '',
  result: null
})

const accountForm = ref({ fullName: '', phone: '', email: '', address: '' })
const accountProfile = ref({ gender: '', day: '', month: '', year: '' })
const accountEditing = ref({ email: false, phone: false })
const accountMessage = ref('')

const staffData = ref({
  orders: [],
  customers: [],
  suppliers: [],
  payments: [],
  debts: [],
  outbox: []
})
const staffLoading = ref(false)

const notice = ref({ type: '', message: '' })

const vouchers = [
  { code: 'NONE', label: 'Không dùng mã', type: 'none', value: 0 },
  { code: 'NEW10', label: 'Khách mới giảm 10%', type: 'percent', value: 10 },
  { code: 'SALE50', label: 'Giảm 50.000đ', type: 'fixed', value: 50000 }
]

vouchers.splice(
  0,
  vouchers.length,
  { code: 'NONE', label: 'Không dùng mã', type: 'none', value: 0, description: 'Thanh toán theo giá niêm yết.' },
  { code: 'NEW10', label: 'NEW10 - khách mới giảm 10%', type: 'percent', value: 10, max: 150000, description: 'Ưu đãi cho khách hàng mới.' },
  { code: 'SALE50', label: 'SALE50 - giảm 50.000đ', type: 'fixed', value: 50000, minAmount: 300000, description: 'Áp dụng cho đơn từ 300.000đ.' },
  { code: 'VIP5', label: 'VIP5 - hạng Vàng/Kim cương', type: 'percent', value: 5, minTier: 'Vàng', description: 'Mã riêng cho khách hàng thân thiết.' }
)

const activePage = computed(() => route.meta.page || 'shop')
const isStaffPage = computed(() => staffPages.includes(activePage.value))
const birthDays = Array.from({ length: 31 }, (_, index) => index + 1)
const birthMonths = Array.from({ length: 12 }, (_, index) => index + 1)
const birthYears = Array.from({ length: 70 }, (_, index) => new Date().getFullYear() - index)

const accountUsername = computed(() => {
  const phone = accountForm.value.phone || currentUser.value?.phone
  if (phone) return phone
  const name = accountForm.value.fullName || currentUser.value?.fullName || 'khachhang'
  return name.toLowerCase().replace(/\s+/g, '')
})

const maskedAccountEmail = computed(() => maskMiddle(accountForm.value.email || currentUser.value?.email, '@'))
const maskedAccountPhone = computed(() => maskTail(accountForm.value.phone || currentUser.value?.phone))

const categories = computed(() => {
  const map = new Map()
  for (const product of products.value) {
    if (productBaseStock(product) <= 0) continue
    const category = productCategory(product)
    map.set(category, (map.get(category) || 0) + 1)
  }
  return Array.from(map.entries())
    .map(([name, count], index) => ({ name, count, image: `/sarab/category-${(index % 6) + 1}.jpg` }))
    .sort((a, b) => a.name.localeCompare(b.name, 'vi'))
})

const filteredProducts = computed(() => {
  const keyword = searchText.value.trim().toLowerCase()
  const filtered = products.value.filter(product => {
    if (productStock(product) <= 0) return false
    if (activeCategory.value && productCategory(product) !== activeCategory.value) return false

    return !keyword ||
      productName(product).toLowerCase().includes(keyword) ||
      productCode(product).toLowerCase().includes(keyword) ||
      productCategory(product).toLowerCase().includes(keyword)
  })

  return filtered.sort((a, b) => {
    if (productSort.value === 'priceAsc') return productPrice(a) - productPrice(b)
    if (productSort.value === 'priceDesc') return productPrice(b) - productPrice(a)
    if (productSort.value === 'stockDesc') return productStock(b) - productStock(a)
    return productStock(b) - productStock(a) || productName(a).localeCompare(productName(b), 'vi')
  })
})

const catalogPageCount = computed(() => Math.max(1, Math.ceil(filteredProducts.value.length / productsPerPage)))
const pagedProducts = computed(() => {
  const page = Math.min(catalogPage.value, catalogPageCount.value)
  const start = (page - 1) * productsPerPage
  return filteredProducts.value.slice(start, start + productsPerPage)
})
const catalogPages = computed(() => Array.from({ length: catalogPageCount.value }, (_, index) => index + 1))

const featuredProducts = computed(() => [...products.value]
  .filter(product => productStock(product) > 0)
  .sort((a, b) => productPrice(b) - productPrice(a))
  .slice(0, 4))

const cartTotal = computed(() => cart.value.reduce((sum, item) => sum + item.unitPrice * item.quantity, 0))
const walletBalance = computed(() => Number(currentUser.value?.walletBalance || 0))
const currentMemberTier = computed(() => memberTierFor(Number(currentUser.value?.totalSpent || 0)))
const nextMemberTier = computed(() => memberTiers.find(tier => tier.minSpent > Number(currentUser.value?.totalSpent || 0)) || null)
const tierProgressPercent = computed(() => {
  if (!nextMemberTier.value) return 100
  const spent = Number(currentUser.value?.totalSpent || 0)
  const currentMin = currentMemberTier.value.minSpent
  const target = nextMemberTier.value.minSpent
  return Math.max(0, Math.min(100, Math.round(((spent - currentMin) / (target - currentMin)) * 100)))
})
const selectedVoucher = computed(() => {
  const voucher = vouchers.find(item => item.code === checkout.value.voucher) || vouchers[0]
  return voucherAvailable(voucher) ? voucher : vouchers[0]
})
const voucherDiscountAmount = computed(() => {
  const voucher = selectedVoucher.value
  if (voucher.type === 'percent') {
    const amount = Math.round(cartTotal.value * voucher.value / 100)
    return voucher.max ? Math.min(amount, voucher.max) : amount
  }
  if (voucher.type === 'fixed') return Math.min(cartTotal.value, voucher.value)
  return 0
})
const tierDiscountAmount = computed(() => {
  const base = Math.max(0, cartTotal.value - voucherDiscountAmount.value)
  return Math.round(base * currentMemberTier.value.rate / 100)
})
const discountAmount = computed(() => Math.min(cartTotal.value, voucherDiscountAmount.value + tierDiscountAmount.value))
const finalAmount = computed(() => Math.max(0, cartTotal.value - discountAmount.value))
const selectedPaymentMethod = computed(() => paymentMethods.find(method => method.value === checkout.value.paymentMethod) || paymentMethods[0])
const paidAmount = computed(() => {
  const amount = finalAmount.value
  if (checkout.value.paymentMethod === 'BankTransfer' || checkout.value.paymentMethod === 'QR') return amount
  if (checkout.value.paymentMethod === 'Wallet') return Math.min(walletBalance.value, amount)
  if (checkout.value.paymentMethod === 'Deposit') return Math.min(Math.max(Number(checkout.value.depositAmount || 0), 0), amount)
  return 0
})
const debtAmount = computed(() => Math.max(0, finalAmount.value - paidAmount.value))
const paymentStatusPreview = computed(() => {
  if (finalAmount.value <= 0 || paidAmount.value >= finalAmount.value) return 'Paid'
  return paidAmount.value > 0 ? 'Partial' : 'Unpaid'
})
const orderStatusPreview = computed(() => paidAmount.value >= finalAmount.value ? 'Paid' : 'Debt')
const cartCount = computed(() => cart.value.reduce((sum, item) => sum + item.quantity, 0))
const relatedProducts = computed(() => {
  if (!selectedProduct.value) return []
  const category = productCategory(selectedProduct.value)
  return products.value
    .filter(product => productId(product) !== productId(selectedProduct.value) && productStock(product) > 0 && productCategory(product) === category)
    .slice(0, 4)
})
const staffOrdersForMetrics = computed(() => staffData.value.orders.length ? staffData.value.orders : myOrders.value)
const staffDashboard = computed(() => {
  const orders = staffOrdersForMetrics.value
  const paidOrders = orders.filter(order => paymentStatusFor(order) === 'Paid')
  const cancelledOrders = orders.filter(order => String(order.orderStatus || '').toLowerCase() === 'cancelled')
  const pendingOrders = orders.filter(order => ['pending', 'debt', 'confirmed'].includes(String(order.orderStatus || '').toLowerCase()))
  return {
    totalOrders: orders.length,
    revenue: paidOrders.reduce((sum, order) => sum + orderTotal(order), 0),
    pending: pendingOrders.length,
    paid: paidOrders.length,
    cancelled: cancelledOrders.length,
    debt: orders.reduce((sum, order) => sum + orderDebt(order), 0),
    wallet: staffData.value.customers.reduce((sum, customer) => sum + Number(customer.walletBalance || 0), 0) || walletBalance.value,
    topCustomer: topCustomerName(orders)
  }
})

function productId(product) {
  return Number(product.productId || product.id || product.productStockCacheId)
}

function productName(product) {
  return product.productName || product.name || 'Sản phẩm'
}

function productCode(product) {
  return product.productCode || `SP${String(productId(product)).padStart(3, '0')}`
}

function productCategory(product) {
  return product.categoryName || product.category || 'Khác'
}

function productPrice(product) {
  return Number(product.sellingPrice ?? product.price ?? product.unitPrice ?? 0)
}

function productBaseStock(product) {
  return Math.max(0, Number(product.quantityAvailable ?? product.stock ?? product.availableStock ?? 0))
}

function cartQuantityFor(product) {
  const line = cart.value.find(item => Number(item.productId) === productId(product))
  return line ? Number(line.quantity || 0) : 0
}

function productStock(product) {
  return Math.max(0, productBaseStock(product) - cartQuantityFor(product))
}

function productImage(product) {
  const id = productId(product)
  return `/sarab/menu-${((id - 1) % 6) + 1}.jpg`
}

function normalizeProduct(product) {
  return {
    ...product,
    productId: productId(product),
    productCode: productCode(product),
    productName: productName(product),
    categoryName: productCategory(product),
    sellingPrice: productPrice(product),
    quantityAvailable: productBaseStock(product),
    manufacturerName: product.manufacturerName || product.supplierName || 'Nhóm kho',
    stockStatus: product.stockStatus || (productBaseStock(product) > 0 ? 'InStock' : 'OutOfStock')
  }
}

function customerId(customer) {
  return Number(customer?.customerId || customer?.id || 0)
}

function customerName(customer) {
  return customer?.fullName || customer?.name || 'Khách hàng'
}

function memberTierFor(totalSpent) {
  return [...memberTiers]
    .reverse()
    .find(tier => Number(totalSpent || 0) >= tier.minSpent) || memberTiers[0]
}

function tierRank(name) {
  return memberTiers.findIndex(tier => tier.name === name)
}

function voucherAvailable(voucher) {
  if (!voucher || voucher.code === 'NONE') return true
  if (voucher.minAmount && cartTotal.value < voucher.minAmount) return false
  if (voucher.minTier && tierRank(currentMemberTier.value.name) < tierRank(voucher.minTier)) return false
  return true
}

function paymentMethodLabel(method) {
  return paymentMethods.find(item => item.value === method)?.label || method || 'Chưa chọn'
}

function backendPaymentMethod(method) {
  if (method === 'Wallet') return 'EWallet'
  if (method === 'Deposit') return 'Cash'
  return method || 'Cash'
}

function paymentStatusFor(order) {
  const raw = order?.paymentStatus || order?.PaymentStatus
  if (raw) return String(raw)
  const total = orderTotal(order)
  const paid = Number(order?.paidAmount || 0)
  if (total <= 0 || paid >= total) return 'Paid'
  return paid > 0 ? 'Partial' : 'Unpaid'
}

function orderItems(order) {
  return order?.items || order?.orderDetails || order?.details || []
}

function topCustomerName(orders) {
  const map = new Map()
  for (const order of orders) {
    const name = order.customerName || order.customerId || 'Khách lẻ'
    map.set(name, (map.get(name) || 0) + orderTotal(order))
  }
  return [...map.entries()].sort((a, b) => b[1] - a[1])[0]?.[0] || 'Chưa có'
}

function readJsonStorage(key, fallback) {
  try {
    const raw = localStorage.getItem(key)
    return raw ? JSON.parse(raw) : fallback
  } catch {
    return fallback
  }
}

function writeJsonStorage(key, value) {
  localStorage.setItem(key, JSON.stringify(value))
}

function walletKeyFor(user = currentUser.value) {
  return String(user?.customerId || user?.phone || 'guest')
}

function loadWalletState(user = currentUser.value) {
  if (!user) {
    walletTransactions.value = []
    return
  }

  const states = readJsonStorage(WALLET_STATE_KEY, {})
  const key = walletKeyFor(user)
  const state = states[key] || { balance: user.walletBalance ?? 500000, transactions: [] }
  currentUser.value = { ...currentUser.value, walletBalance: Number(state.balance || 0) }
  walletTransactions.value = state.transactions || []
  saveCustomerUser(currentUser.value)
}

function saveWalletState() {
  if (!currentUser.value) return
  const states = readJsonStorage(WALLET_STATE_KEY, {})
  states[walletKeyFor()] = {
    balance: Number(currentUser.value.walletBalance || 0),
    transactions: walletTransactions.value
  }
  writeJsonStorage(WALLET_STATE_KEY, states)
  saveCustomerUser(currentUser.value)
}

function addWalletTransaction(type, amount, note, orderCode = '') {
  walletTransactions.value.unshift({
    id: Date.now(),
    type,
    amount: Number(amount || 0),
    note,
    orderCode,
    createdAt: new Date().toISOString()
  })
  walletTransactions.value = walletTransactions.value.slice(0, 12)
  saveWalletState()
}

function addActivityLog(action, note, orderCode = '') {
  activityLogs.value.unshift({
    id: Date.now(),
    action,
    note,
    orderCode,
    actor: staffUser.value?.username || currentUser.value?.fullName || 'Khách hàng',
    createdAt: new Date().toISOString()
  })
  activityLogs.value = activityLogs.value.slice(0, 20)
  writeJsonStorage(ACTIVITY_LOG_KEY, activityLogs.value)
}

function formatMoney(value) {
  return Number(value || 0).toLocaleString('vi-VN') + 'đ'
}

function formatDateTime(value) {
  if (!value) return ''
  return new Date(value).toLocaleString('vi-VN')
}

function maskMiddle(value, separator = '') {
  const text = String(value || '').trim()
  if (!text) return 'Chưa cập nhật'

  if (separator && text.includes(separator)) {
    const [first, ...rest] = text.split(separator)
    const domain = rest.join(separator)
    const visible = first.slice(0, 2)
    return `${visible}${'*'.repeat(Math.max(4, first.length - 2))}${separator}${domain}`
  }

  if (text.length <= 4) return `${text[0] || ''}***`
  return `${text.slice(0, 2)}${'*'.repeat(Math.max(4, text.length - 4))}${text.slice(-2)}`
}

function maskTail(value) {
  const text = String(value || '').trim()
  if (!text) return 'Chưa cập nhật'
  if (text.length <= 4) return `${text[0] || ''}***`
  return `${'*'.repeat(Math.max(6, text.length - 2))}${text.slice(-2)}`
}

function orderTotal(order) {
  return Number(order?.finalAmount ?? order?.totalAmount ?? 0)
}

function orderDebt(order) {
  return Math.max(0, Number(order?.debtAmount ?? order?.remainingAmount ?? 0))
}

function statusLabel(status) {
  const value = String(status || '').toLowerCase()
  const labels = {
    pending: 'Chờ xử lý',
    confirmed: 'Đã xác nhận',
    paid: 'Đã thanh toán',
    partial: 'Thanh toán một phần',
    debt: 'Còn công nợ',
    unpaid: 'Chưa thanh toán',
    cancelled: 'Đã hủy',
    refunded: 'Đã hoàn tiền',
    completed: 'Hoàn tất',
    failed: 'Thất bại',
    active: 'Đang hoạt động',
    inactive: 'Ngừng hoạt động',
    outofstock: 'Hết hàng'
  }
  return labels[value] || status || 'Không xác định'
}

function statusClass(status) {
  const value = String(status || '').toLowerCase()
  if (['paid', 'confirmed', 'completed', 'active', 'processed', 'refunded'].includes(value)) return 'ok'
  if (['pending', 'partial', 'debt', 'unpaid'].includes(value)) return 'warn'
  if (['cancelled', 'failed', 'blocked', 'outofstock'].includes(value)) return 'bad'
  return 'info'
}

function showNotice(message, type = 'ok') {
  notice.value = { message, type }
  window.setTimeout(() => {
    if (notice.value.message === message) notice.value = { type: '', message: '' }
  }, 3200)
}

function openPage(page) {
  if (['cart', 'myOrders', 'account'].includes(page) && !currentUser.value && page !== 'cart') {
    openAuth('login')
    return
  }

  if (staffPages.includes(page) && !staffUser.value) {
    openStaffAuth()
    return
  }

  router.push(pagePaths[page] || '/')
}

function scrollToCatalog() {
  document.getElementById('catalog')?.scrollIntoView({ behavior: 'smooth', block: 'start' })
}

async function loadProducts() {
  productLoading.value = true
  productError.value = ''

  try {
    const res = await api.get('/api/ProductStockCaches')
    const list = Array.isArray(res.data) ? res.data : []
    products.value = (list.length ? list : demoProducts).map(normalizeProduct)
  } catch (error) {
    products.value = demoProducts.map(normalizeProduct)
    productError.value = 'Chưa kết nối được Inventory Service, đang dùng dữ liệu demo.'
  } finally {
    syncCartStock()
    productLoading.value = false
  }
}

function syncCartStock() {
  const synced = []

  for (const item of cart.value) {
    const product = products.value.find(p => productId(p) === Number(item.productId))
    if (!product) continue

    const stock = productBaseStock(product)
    if (stock <= 0) continue

    item.productName = productName(product)
    item.productCode = productCode(product)
    item.categoryName = productCategory(product)
    item.unitPrice = productPrice(product)
    item.stock = stock
    item.image = productImage(product)
    item.quantity = Math.max(1, Math.min(Number(item.quantity || 1), stock))
    synced.push(item)
  }

  cart.value = synced
  saveCustomerCart(cart.value)
}

function addToCart(product) {
  const stock = productStock(product)
  if (stock <= 0) {
    showNotice('Sản phẩm này đã hết hàng trong kho.', 'bad')
    return
  }

  const id = productId(product)
  const existing = cart.value.find(item => Number(item.productId) === id)
  if (existing) {
    if (existing.quantity >= productBaseStock(product)) {
      showNotice('Số lượng mua không được vượt quá tồn kho.', 'bad')
      return
    }
    existing.quantity += 1
  } else {
    cart.value.push({
      productId: id,
      productCode: productCode(product),
      productName: productName(product),
      categoryName: productCategory(product),
      unitPrice: productPrice(product),
      quantity: 1,
      stock: productBaseStock(product),
      image: productImage(product)
    })
  }

  saveCustomerCart(cart.value)
  showNotice('Đã thêm vào giỏ hàng.')
}

function updateCartQuantity(item, value) {
  const quantity = Math.floor(Number(value || 1))
  const product = products.value.find(p => productId(p) === Number(item.productId))
  const limit = product ? productBaseStock(product) : Number(item.stock || 1)

  if (quantity > limit) {
    item.quantity = limit
    showNotice('Số lượng mua không được vượt quá tồn kho.', 'bad')
  } else {
    item.quantity = Math.max(1, quantity)
  }
  saveCustomerCart(cart.value)
}

function removeFromCart(id) {
  cart.value = cart.value.filter(item => Number(item.productId) !== Number(id))
  saveCustomerCart(cart.value)
}

function clearCart() {
  cart.value = []
  saveCustomerCart([])
}

function initCheckoutShipping() {
  checkoutShipping.value = {
    fullName: currentUser.value?.fullName || '',
    phone: currentUser.value?.phone || '',
    address: currentUser.value?.address || ''
  }
  if (!checkout.value.depositAmount) checkout.value.depositAmount = Math.round(finalAmount.value * 0.3)
}

function openAuth(mode = 'login') {
  authMode.value = mode
  authError.value = ''
  showAuthModal.value = true
}

function closeAuth() {
  showAuthModal.value = false
  authError.value = ''
}

function setCurrentCustomer(customer) {
  currentUser.value = {
    role: 'Customer',
    customerId: customerId(customer),
    fullName: customerName(customer),
    phone: customer.phone || '',
    email: customer.email || '',
    address: customer.address || '',
    currentDebt: Number(customer.currentDebt || 0),
    totalSpent: Number(customer.totalSpent || 0),
    walletBalance: Number(customer.walletBalance ?? currentUser.value?.walletBalance ?? 500000)
  }

  saveCustomerUser(currentUser.value)
  saveDemoCustomer(currentUser.value)
  loadWalletState(currentUser.value)
  initCheckoutShipping()
}

function createLocalDemoCustomer(profile) {
  return {
    customerId: Date.now(),
    fullName: profile.fullName,
    phone: profile.phone,
    email: profile.email || '',
    address: profile.address || '',
    currentDebt: 0,
    totalSpent: 0,
    walletBalance: 500000
  }
}

async function registerCustomer() {
  authBusy.value = true
  authError.value = ''

  if (!registerForm.value.fullName.trim() || !registerForm.value.phone.trim() || !registerForm.value.password.trim()) {
    authError.value = 'Vui lòng nhập họ tên, số điện thoại và mật khẩu.'
    authBusy.value = false
    return
  }

  try {
    const res = await api.post('/api/Customers', {
      fullName: registerForm.value.fullName.trim(),
      phone: registerForm.value.phone.trim(),
      email: registerForm.value.email.trim(),
      address: registerForm.value.address.trim()
    })

    saveDemoPassword(registerForm.value.phone.trim(), registerForm.value.password)
    setCurrentCustomer(res.data)
    registerForm.value = { fullName: '', phone: '', email: '', address: '', password: '' }
    closeAuth()
    showNotice('Đăng ký thành công.')
    await loadMyOrders()
  } catch (error) {
    const customer = createLocalDemoCustomer({
      fullName: registerForm.value.fullName.trim(),
      phone: registerForm.value.phone.trim(),
      email: registerForm.value.email.trim(),
      address: registerForm.value.address.trim()
    })
    saveDemoPassword(registerForm.value.phone.trim(), registerForm.value.password)
    setCurrentCustomer(customer)
    registerForm.value = { fullName: '', phone: '', email: '', address: '', password: '' }
    closeAuth()
    showNotice('Đăng ký demo local thành công. Backend chưa lưu được tài khoản này.')
    await loadMyOrders()
  } finally {
    authBusy.value = false
  }
}

async function loginCustomer() {
  authBusy.value = true
  authError.value = ''

  if (!loginForm.value.phone.trim()) {
    authError.value = 'Vui lòng nhập số điện thoại.'
    authBusy.value = false
    return
  }

  if (!verifyDemoPassword(loginForm.value.phone.trim(), loginForm.value.password)) {
    authError.value = 'Mật khẩu demo không đúng.'
    authBusy.value = false
    return
  }

  try {
    const res = await api.post('/api/Customers/demo-login', { phone: loginForm.value.phone.trim() })
    setCurrentCustomer(res.data)
    loginForm.value = { phone: '', password: '' }
    closeAuth()
    showNotice('Đăng nhập thành công.')
    await loadMyOrders()
  } catch (error) {
    const localCustomer = loadDemoCustomer(loginForm.value.phone.trim())
    if (localCustomer) {
      setCurrentCustomer(localCustomer)
      loginForm.value = { phone: '', password: '' }
      closeAuth()
      showNotice('Đăng nhập bằng dữ liệu demo local.')
      await loadMyOrders()
      return
    }
    authError.value = error.response?.data?.message || 'Không tìm thấy khách hàng. Vui lòng đăng ký.'
  } finally {
    authBusy.value = false
  }
}

function logoutCustomer() {
  currentUser.value = null
  saveCustomerUser(null)
  myOrders.value = []
  showNotice('Đã đăng xuất.')
  if (['myOrders', 'account'].includes(activePage.value)) openPage('shop')
}

function checkoutProfilePayload() {
  return {
    fullName: (checkoutShipping.value.fullName || currentUser.value?.fullName || '').trim(),
    phone: (checkoutShipping.value.phone || currentUser.value?.phone || '').trim(),
    email: currentUser.value?.email || '',
    address: checkoutShipping.value.address.trim()
  }
}

async function updateBackendCustomer(customer, profile) {
  const id = customerId(customer)
  const phone = customer?.phone || profile.phone
  const res = await api.put(`/api/Customers/${id}/profile`, { ...profile, phone })
  setCurrentCustomer(res.data)
  return currentUser.value
}

async function findCustomerByPhone(phone) {
  if (!phone) return null
  try {
    const res = await api.post('/api/Customers/demo-login', { phone })
    return res.data
  } catch (error) {
    const localCustomer = loadDemoCustomer(phone)
    if (localCustomer) return localCustomer
    if (error.response?.status === 404) return null
    throw error
  }
}

async function ensureCheckoutCustomer() {
  const profile = checkoutProfilePayload()
  if (!profile.fullName) throw new Error('Vui lòng nhập họ tên người nhận.')
  if (!profile.phone) throw new Error('Vui lòng nhập số điện thoại.')
  if (!profile.address) throw new Error('Vui lòng nhập địa chỉ nhận hàng.')

  if (currentUser.value?.customerId) {
    try {
      return await updateBackendCustomer(currentUser.value, profile)
    } catch (error) {
      if (!error.response) {
        setCurrentCustomer({ ...currentUser.value, ...profile })
        return currentUser.value
      }
      if (![400, 404].includes(error.response?.status)) {
        throw new Error(error.response?.data?.message || 'Không cập nhật được địa chỉ nhận hàng.')
      }
    }
  }

  const existing = await findCustomerByPhone(profile.phone)
  if (existing) {
    try {
      return await updateBackendCustomer(existing, profile)
    } catch (error) {
      if (!error.response) {
        setCurrentCustomer({ ...existing, ...profile })
        return currentUser.value
      }
      throw error
    }
  }

  try {
    const res = await api.post('/api/Customers', profile)
    setCurrentCustomer(res.data)
    return currentUser.value
  } catch (error) {
    if (!error.response) {
      setCurrentCustomer(createLocalDemoCustomer(profile))
      return currentUser.value
    }
    throw error
  }
}

function loadLocalOrders(customer = currentUser.value) {
  if (!customer) return []
  const orders = readJsonStorage(LOCAL_ORDERS_KEY, [])
  const key = String(customer.customerId || customer.phone)
  return orders.filter(order => String(order.customerId) === key || String(order.customerPhone) === String(customer.phone))
}

function loadAllLocalOrders() {
  return readJsonStorage(LOCAL_ORDERS_KEY, [])
}

function saveLocalOrders(orders) {
  writeJsonStorage(LOCAL_ORDERS_KEY, orders)
}

function upsertLocalOrder(order) {
  const orders = readJsonStorage(LOCAL_ORDERS_KEY, [])
  const index = orders.findIndex(item => item.orderCode === order.orderCode)
  if (index >= 0) orders[index] = order
  else orders.unshift(order)
  saveLocalOrders(orders.slice(0, 50))
}

function mergeOrders(remoteOrders, localOrders) {
  const map = new Map()
  for (const order of [...remoteOrders, ...localOrders]) {
    map.set(order.orderCode || order.orderId || order.id, order)
  }
  return [...map.values()].sort((a, b) => new Date(b.orderDate || 0) - new Date(a.orderDate || 0))
}

function createLocalOrder(customer) {
  const orderCode = `ORD-DEMO${Date.now().toString().slice(-6)}`
  return {
    orderId: `local-${Date.now()}`,
    orderCode,
    customerId: String(customer.customerId || customer.phone),
    customerName: customer.fullName,
    customerPhone: customer.phone,
    orderDate: new Date().toISOString(),
    totalAmount: cartTotal.value,
    discountAmount: discountAmount.value,
    finalAmount: finalAmount.value,
    paidAmount: paidAmount.value,
    debtAmount: debtAmount.value,
    paymentMethod: checkout.value.paymentMethod,
    paymentStatus: paymentStatusPreview.value,
    orderStatus: orderStatusPreview.value,
    isLocalDemo: true,
    items: cart.value.map(item => ({
      productId: item.productId,
      productCode: item.productCode,
      productName: item.productName,
      categoryName: item.categoryName,
      quantity: item.quantity,
      unitPrice: item.unitPrice,
      subTotal: item.unitPrice * item.quantity
    }))
  }
}

function applyCheckoutSideEffects(order) {
  if (!currentUser.value) return

  if (checkout.value.paymentMethod === 'Wallet' && paidAmount.value > 0) {
    currentUser.value.walletBalance = Math.max(0, walletBalance.value - paidAmount.value)
    addWalletTransaction('pay', -paidAmount.value, 'Thanh toán đơn hàng bằng ví', order.orderCode)
  }

  currentUser.value.currentDebt = Math.max(0, Number(currentUser.value.currentDebt || 0) + orderDebt(order))
  currentUser.value.totalSpent = Number(currentUser.value.totalSpent || 0) + orderTotal(order)
  saveCustomerUser(currentUser.value)
  saveDemoCustomer(currentUser.value)
  saveWalletState()
  addActivityLog('order.created', `Tạo đơn ${paymentMethodLabel(order.paymentMethod)} - ${statusLabel(order.paymentStatus)}`, order.orderCode)
}

function topUpWallet() {
  if (!currentUser.value) {
    openAuth('login')
    return
  }

  const amount = Math.max(0, Number(walletTopUpAmount.value || 0))
  if (amount <= 0) {
    showNotice('Vui lòng nhập số tiền nạp ví hợp lệ.', 'bad')
    return
  }

  currentUser.value.walletBalance = walletBalance.value + amount
  addWalletTransaction('topup', amount, 'Nạp tiền vào ví RetailERP')
  addActivityLog('wallet.topup', `Khách nạp ví ${formatMoney(amount)}`)
  showNotice('Đã nạp tiền vào ví demo.')
}

function openProductDetail(product) {
  selectedProduct.value = product
}

function closeProductDetail() {
  selectedProduct.value = null
}

async function submitCheckout() {
  checkoutMessage.value = ''
  checkoutBusy.value = true
  syncCartStock()

  if (cart.value.length === 0) {
    checkoutMessage.value = 'Giỏ hàng đang trống hoặc sản phẩm đã hết hàng.'
    checkoutBusy.value = false
    return
  }

  if (!currentUser.value) {
    checkoutBusy.value = false
    openAuth('login')
    return
  }

  try {
    const customer = await ensureCheckoutCustomer()
    const payload = {
      customerId: customer.customerId,
      discountAmount: discountAmount.value,
      paymentMethod: backendPaymentMethod(checkout.value.paymentMethod),
      paidAmount: paidAmount.value,
      items: cart.value.map(item => ({
        productId: item.productId,
        quantity: item.quantity
      }))
    }

    const res = await api.post('/api/Sales/Checkout', payload)
    const order = { ...(res.data?.data || res.data), paymentMethod: checkout.value.paymentMethod }
    applyCheckoutSideEffects(order)
    clearCart()
    checkout.value = { voucher: 'NONE', paymentMethod: 'Cash', depositAmount: 0 }
    await Promise.all([loadProducts(), loadMyOrders()])
    showNotice(`Đặt hàng thành công ${order?.orderCode ? `- ${order.orderCode}` : ''}.`)
    openPage('myOrders')
  } catch (error) {
    if (!error.response) {
      const order = createLocalOrder(currentUser.value)
      upsertLocalOrder(order)
      applyCheckoutSideEffects(order)
      clearCart()
      checkout.value = { voucher: 'NONE', paymentMethod: 'Cash', depositAmount: 0 }
      await loadMyOrders()
      showNotice(`Đã tạo đơn demo local - ${order.orderCode}.`)
      openPage('myOrders')
      return
    }
    checkoutMessage.value = error.message || error.response?.data?.message || 'Lỗi khi tạo đơn hàng.'
  } finally {
    checkoutBusy.value = false
  }
}

async function loadMyOrders() {
  if (!currentUser.value?.customerId) return
  myOrdersLoading.value = true
  try {
    const res = await api.get('/api/Orders', { params: { customerId: currentUser.value.customerId } })
    myOrders.value = mergeOrders(Array.isArray(res.data) ? res.data : [], loadLocalOrders())
  } catch {
    myOrders.value = loadLocalOrders()
  } finally {
    myOrdersLoading.value = false
  }
}

async function lookupOrder() {
  orderLookup.value.loading = true
  orderLookup.value.error = ''
  orderLookup.value.result = null

  if (!orderLookup.value.orderCode.trim() || !orderLookup.value.phone.trim()) {
    orderLookup.value.error = 'Vui lòng nhập mã đơn hàng và số điện thoại.'
    orderLookup.value.loading = false
    return
  }

  try {
    const res = await api.get('/api/Orders/lookup', {
      params: {
        orderCode: orderLookup.value.orderCode.trim(),
        phone: orderLookup.value.phone.trim()
      }
    })
    orderLookup.value.result = res.data
  } catch (error) {
    orderLookup.value.error = error.response?.data?.message || 'Không tìm thấy đơn hàng phù hợp.'
  } finally {
    orderLookup.value.loading = false
  }
}

function initAccountForm() {
  accountForm.value = {
    fullName: currentUser.value?.fullName || '',
    phone: currentUser.value?.phone || '',
    email: currentUser.value?.email || '',
    address: currentUser.value?.address || ''
  }
  accountEditing.value = { email: false, phone: false }
}

function toggleAccountEdit(field) {
  accountEditing.value[field] = !accountEditing.value[field]
}

async function saveAccount() {
  accountMessage.value = ''
  if (!currentUser.value?.customerId) return

  if (!accountForm.value.fullName.trim()) {
    accountMessage.value = 'Vui lòng nhập tên khách hàng.'
    return
  }

  if (!accountForm.value.phone.trim()) {
    accountMessage.value = 'Vui lòng nhập số điện thoại.'
    accountEditing.value.phone = true
    return
  }

  try {
    const res = await api.put(`/api/Customers/${currentUser.value.customerId}/profile`, {
      fullName: accountForm.value.fullName.trim(),
      phone: accountForm.value.phone.trim(),
      email: accountForm.value.email.trim(),
      address: accountForm.value.address.trim()
    })
    setCurrentCustomer(res.data)
    accountEditing.value = { email: false, phone: false }
    accountMessage.value = 'Đã lưu thông tin tài khoản.'
  } catch (error) {
    accountMessage.value = error.response?.data?.message || 'Không lưu được thông tin.'
  }
}

function openStaffAuth() {
  staffError.value = ''
  showStaffModal.value = true
}

function parseStaffToken(token) {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]))
    return {
      username: payload.unique_name || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || 'staff',
      role: payload.role || 'Sales'
    }
  } catch {
    return null
  }
}

async function loginStaff() {
  staffBusy.value = true
  staffError.value = ''

  try {
    const res = await api.post('/api/auth/login', staffLoginForm.value)
    setStaffToken(res.data.token)
    staffUser.value = {
      username: res.data.username || staffLoginForm.value.username,
      role: res.data.role || staffLoginForm.value.role
    }
    showStaffModal.value = false
    await loadStaffData()
    openPage('dashboard')
  } catch (error) {
    staffError.value = error.response?.data?.message || 'Không đăng nhập được nhân viên.'
  } finally {
    staffBusy.value = false
  }
}

function logoutStaff() {
  setStaffToken('')
  staffUser.value = null
  staffData.value = { orders: [], customers: [], suppliers: [], payments: [], debts: [], outbox: [] }
  if (isStaffPage.value) openPage('shop')
}

async function safeList(path) {
  try {
    const res = await api.get(path)
    return Array.isArray(res.data) ? res.data : []
  } catch {
    return []
  }
}

async function loadStaffData() {
  if (!staffUser.value) return
  staffLoading.value = true

  const [orders, customers, suppliers, payments, debts, outbox] = await Promise.all([
    safeList('/api/Orders'),
    safeList('/api/Customers'),
    safeList('/api/Suppliers'),
    safeList('/api/Payments'),
    safeList('/api/Debts'),
    safeList('/api/OutboxMessages')
  ])

  staffData.value = {
    orders: mergeOrders(orders, loadAllLocalOrders()),
    customers,
    suppliers,
    payments,
    debts,
    outbox
  }
  staffLoading.value = false
}

async function updateOrderStatus(order, status) {
  const id = order.orderId || order.id
  if (!id) return

  if (String(id).startsWith('local-') || order.isLocalDemo) {
    const updated = { ...order, orderStatus: status }
    if (status === 'Paid') {
      updated.paymentStatus = 'Paid'
      updated.paidAmount = orderTotal(order)
      updated.debtAmount = 0
    }
    upsertLocalOrder(updated)
    await loadMyOrders()
    await loadStaffData()
    addActivityLog('order.status', `Cập nhật trạng thái ${statusLabel(status)}`, updated.orderCode)
    showNotice('Đã cập nhật đơn demo local.')
    return
  }

  try {
    await api.put(`/api/Orders/${id}/status`, { status })
    await loadStaffData()
    await loadMyOrders()
    showNotice('Đã cập nhật trạng thái đơn hàng.')
  } catch (error) {
    showNotice(error.response?.data?.message || 'Không cập nhật được đơn hàng.', 'bad')
  }
}

function canCancelOrder(order) {
  const status = String(order?.orderStatus || '').toLowerCase()
  return !['cancelled', 'paid', 'completed'].includes(status)
}

function markOrderCancelled(order) {
  const updated = {
    ...order,
    orderStatus: 'Cancelled',
    paymentStatus: Number(order.paidAmount || 0) > 0 ? 'Refunded' : paymentStatusFor(order)
  }

  if (currentUser.value && order.paymentMethod === 'Wallet' && Number(order.paidAmount || 0) > 0) {
    currentUser.value.walletBalance = walletBalance.value + Number(order.paidAmount || 0)
    addWalletTransaction('refund', Number(order.paidAmount || 0), 'Hoàn tiền do hủy đơn', order.orderCode)
  }

  if (currentUser.value) {
    currentUser.value.currentDebt = Math.max(0, Number(currentUser.value.currentDebt || 0) - orderDebt(order))
    saveCustomerUser(currentUser.value)
    saveDemoCustomer(currentUser.value)
  }

  upsertLocalOrder(updated)
  addActivityLog('order.cancelled', 'Hủy đơn hàng', order.orderCode)
  return updated
}

async function cancelOrder(order, staff = false) {
  if (!canCancelOrder(order)) {
    showNotice('Đơn này không thể hủy ở trạng thái hiện tại.', 'bad')
    return
  }

  const id = order.orderId || order.id
  try {
    if (id && !String(id).startsWith('local-') && !order.isLocalDemo) {
      if (staff) {
        await api.put(`/api/Orders/${id}/cancel`)
      } else {
        await api.put(`/api/Orders/${id}/customer-cancel`, {
          phone: currentUser.value?.phone || order.customerPhone || ''
        })
      }
      await loadProducts()
      await loadStaffData()
      await loadMyOrders()
      showNotice('Đã hủy đơn hàng.')
      return
    }
    throw new Error('local')
  } catch (error) {
    if (staff && error.message !== 'local' && error.response) {
      showNotice(error.response?.data?.message || 'Không hủy được đơn hàng.', 'bad')
      return
    }
    markOrderCancelled(order)
    await loadProducts()
    await loadMyOrders()
    if (staff) await loadStaffData()
    showNotice('Đã cập nhật hủy đơn trong dữ liệu demo.')
  }
}

watch(cart, () => saveCustomerCart(cart.value), { deep: true })

watch(finalAmount, (amount) => {
  if (checkout.value.depositAmount > amount) {
    checkout.value.depositAmount = amount
  }
  if (checkout.value.paymentMethod === 'Deposit' && checkout.value.depositAmount === 0) {
    checkout.value.depositAmount = Math.round(amount * 0.3)
  }
})

watch(() => checkout.value.paymentMethod, method => {
  if (method === 'Deposit' && checkout.value.depositAmount === 0) {
    checkout.value.depositAmount = Math.round(finalAmount.value * 0.3)
  }
})

watch([searchText, activeCategory, productSort], () => {
  catalogPage.value = 1
})

watch(catalogPageCount, count => {
  if (catalogPage.value > count) catalogPage.value = count
})

watch(activePage, async page => {
  if (page === 'shop' || page === 'cart') {
    await loadProducts()
  }
  if (page === 'myOrders') {
    await loadMyOrders()
  }
  if (page === 'account') {
    initAccountForm()
  }
  if (staffPages.includes(page)) {
    if (!staffUser.value) openStaffAuth()
    else await loadStaffData()
  }
}, { immediate: false })

onMounted(async () => {
  const token = getStaffToken()
  const parsed = token ? parseStaffToken(token) : null
  if (parsed) staffUser.value = parsed
  else setStaffToken('')

  activityLogs.value = readJsonStorage(ACTIVITY_LOG_KEY, [])
  if (currentUser.value) {
    loadWalletState(currentUser.value)
    initCheckoutShipping()
  }
  await loadProducts()
  if (currentUser.value) await loadMyOrders()
  if (isStaffPage.value && !staffUser.value) openStaffAuth()
  if (staffUser.value && isStaffPage.value) await loadStaffData()
})
</script>

<template>
  <div class="app-shell">
    <div v-if="notice.message" :class="['toast', notice.type]">
      {{ notice.message }}
    </div>

    <div class="topbar" aria-label="Khuyến mãi đang chạy">
      <div class="sale-runner">
        <span>Flash Sale hôm nay - giảm đến 30%</span>
        <span>Miễn phí giao hàng cho đơn từ 500K</span>
        <span>Voucher 50K cho khách hàng mới</span>
        <span>Mua nhiều giảm thêm ngay trong giỏ hàng</span>
        <span>Deal giờ vàng 12:00 - 14:00</span>
        <span>Flash Sale hôm nay - giảm đến 30%</span>
        <span>Miễn phí giao hàng cho đơn từ 500K</span>
        <span>Voucher 50K cho khách hàng mới</span>
        <span>Mua nhiều giảm thêm ngay trong giỏ hàng</span>
        <span>Deal giờ vàng 12:00 - 14:00</span>
      </div>
    </div>

    <header class="shop-header">
      <button class="brand" type="button" @click="openPage('shop')">
        <span class="brand-icon">R</span>
        <span>
          <b>RetailERP</b>
          <small>Market</small>
        </span>
      </button>

      <nav class="main-nav">
        <button :class="{ active: activePage === 'shop' }" @click="openPage('shop')">Trang chủ</button>
        <button :class="{ active: activePage === 'lookup' }" @click="openPage('lookup')">Tra cứu đơn</button>
        <button :class="{ active: activePage === 'myOrders' }" @click="openPage('myOrders')">Đơn mua của tôi</button>
        <button :class="{ active: activePage === 'account' }" @click="openPage('account')">Tài khoản</button>
      </nav>

      <div class="header-search">
        <span>⌕</span>
        <input
          v-model="searchText"
          type="search"
          placeholder="Tìm sản phẩm trong kho..."
          @keyup.enter="openPage('shop')"
        />
      </div>

      <button class="cart-button" type="button" @click="openPage('cart')">
        <ShoppingBasket class="cart-icon" :size="18" :stroke-width="2.5" aria-hidden="true" />
        <span>Giỏ hàng</span>
        <b>{{ cartCount }}</b>
      </button>

      <button v-if="!currentUser" class="login-button" type="button" @click="openAuth('login')">
        Đăng nhập
      </button>
      <div v-else class="user-chip">
        <button type="button" @click="openPage('account')">{{ currentUser.fullName }}</button>
        <button type="button" @click="logoutCustomer">Thoát</button>
      </div>
    </header>

    <main v-if="activePage === 'shop'" class="shop-page">
      <section class="hero">
        <div class="hero-copy">
          <span class="eyebrow">Bán hàng & kho hàng</span>
          <h1>Mua hàng nhanh từ tồn kho đang có</h1>
          <p>
            Sản phẩm lấy từ Inventory Service, kiểm tra tồn kho khi thêm vào giỏ và tạo đơn bán hàng qua Order/Sales Service.
          </p>
          <div class="hero-actions">
            <button class="primary-btn" type="button" @click="scrollToCatalog">Mua ngay</button>
            <button class="ghost-btn" type="button" @click="openPage('lookup')">Tra cứu đơn</button>
          </div>
          <div class="hero-stats">
            <span><b>{{ products.length }}</b> sản phẩm</span>
            <span><b>{{ categories.length }}</b> danh mục</span>
            <span><b>{{ cartCount }}</b> trong giỏ</span>
          </div>
        </div>
        <div class="hero-art">
          <img src="/sarab/banner-img.jpg" alt="Sản phẩm nổi bật" />
          <div class="deal-card">
            <b>NEW10</b>
            <span>Giảm 10% cho đơn mới</span>
          </div>
        </div>
      </section>

      <section class="category-strip">
        <div class="section-heading">
          <span>Danh mục</span>
          <h2>Chọn nhóm sản phẩm</h2>
        </div>
        <div class="category-grid">
          <button
            v-for="category in categories"
            :key="category.name"
            :class="['category-card', { active: activeCategory === category.name }]"
            type="button"
            @click="activeCategory = activeCategory === category.name ? '' : category.name; scrollToCatalog()"
          >
            <img :src="category.image" alt="" />
            <b>{{ category.name }}</b>
            <span>{{ category.count }} sản phẩm</span>
          </button>
        </div>
      </section>

      <section class="featured-section">
        <div class="section-heading">
          <span>Gợi ý hôm nay</span>
          <h2>Sản phẩm nổi bật</h2>
        </div>
        <div class="featured-grid">
          <article v-for="product in featuredProducts" :key="productId(product)" class="featured-card clickable-card" @click="openProductDetail(product)">
            <img :src="productImage(product)" alt="" />
            <div>
              <small>{{ productCategory(product) }}</small>
              <h3>{{ productName(product) }}</h3>
              <b>{{ formatMoney(productPrice(product)) }}</b>
              <button type="button" @click.stop="addToCart(product)">Thêm nhanh</button>
            </div>
          </article>
        </div>
      </section>

      <section id="catalog" class="catalog-section">
        <div class="catalog-head">
          <div class="section-heading">
            <span>Sản phẩm</span>
            <h2>Danh sách đang còn trong kho</h2>
          </div>
          <div class="catalog-tools">
            <button v-if="activeCategory" class="ghost-btn small" type="button" @click="activeCategory = ''">
              Bỏ lọc {{ activeCategory }}
            </button>
            <select v-model="productSort">
              <option value="popular">Gợi ý</option>
              <option value="priceAsc">Giá thấp đến cao</option>
              <option value="priceDesc">Giá cao đến thấp</option>
              <option value="stockDesc">Tồn kho nhiều</option>
            </select>
          </div>
        </div>

        <p v-if="productError" class="soft-alert">{{ productError }}</p>
        <p v-if="productLoading" class="soft-alert">Đang tải sản phẩm từ Inventory Service...</p>

        <div v-if="filteredProducts.length === 0 && !productLoading" class="empty-state">
          <img src="/sarab/off-img.jpg" alt="" />
          <h3>Không có sản phẩm trong kho</h3>
          <p>Thử đổi từ khóa tìm kiếm hoặc chọn danh mục khác.</p>
        </div>

        <div v-else class="product-grid">
          <article v-for="product in pagedProducts" :key="productId(product)" class="product-card clickable-card" @click="openProductDetail(product)">
            <div class="product-image">
              <img :src="productImage(product)" alt="" />
              <span>{{ productStock(product) }} còn lại</span>
            </div>
            <div class="product-info">
              <small>{{ productCode(product) }} · {{ productCategory(product) }}</small>
              <h3>{{ productName(product) }}</h3>
              <p>{{ product.manufacturerName }}</p>
              <div class="product-row">
                <b>{{ formatMoney(productPrice(product)) }}</b>
                <button type="button" :disabled="productStock(product) <= 0" @click.stop="addToCart(product)">
                  Thêm vào giỏ
                </button>
              </div>
            </div>
          </article>
        </div>

        <div v-if="catalogPageCount > 1" class="catalog-pagination" aria-label="Phân trang sản phẩm">
          <button
            v-for="page in catalogPages"
            :key="page"
            :class="{ active: catalogPage === page }"
            type="button"
            :aria-label="`Trang ${page}`"
            @click="catalogPage = page; scrollToCatalog()"
          ></button>
        </div>
      </section>
    </main>

    <main v-else-if="activePage === 'cart'" class="page two-column-page">
      <section class="cart-panel">
        <div class="page-title">
          <span>Giỏ hàng</span>
          <h1>Kiểm tra sản phẩm trước khi thanh toán</h1>
        </div>

        <div v-if="cart.length === 0" class="empty-state compact">
          <h3>Giỏ hàng đang trống</h3>
          <p>Chọn sản phẩm còn trong kho để bắt đầu đặt hàng.</p>
          <button class="primary-btn" type="button" @click="openPage('shop')">Mua hàng</button>
        </div>

        <div v-else class="cart-list">
          <article v-for="item in cart" :key="item.productId" class="cart-line">
            <img :src="item.image" alt="" />
            <div>
              <small>{{ item.productCode }} · {{ item.categoryName }}</small>
              <h3>{{ item.productName }}</h3>
              <p>Còn trong kho: {{ item.stock }}</p>
            </div>
            <div class="qty-box">
              <button type="button" @click="updateCartQuantity(item, item.quantity - 1)">-</button>
              <input :value="item.quantity" type="number" min="1" :max="item.stock" @input="updateCartQuantity(item, $event.target.value)" />
              <button type="button" @click="updateCartQuantity(item, item.quantity + 1)">+</button>
            </div>
            <b>{{ formatMoney(item.unitPrice * item.quantity) }}</b>
            <button class="remove-btn" type="button" @click="removeFromCart(item.productId)">Xóa</button>
          </article>
        </div>
      </section>

      <aside class="checkout-panel">
        <h2>Thanh toán</h2>
        <p v-if="!currentUser" class="soft-alert">Bạn cần đăng nhập hoặc đăng ký trước khi đặt hàng.</p>

        <div class="checkout-fields">
          <label>
            Người nhận
            <input v-model="checkoutShipping.fullName" type="text" placeholder="Họ tên người nhận" />
          </label>
          <label>
            Số điện thoại
            <input v-model="checkoutShipping.phone" type="text" placeholder="Số điện thoại" />
          </label>
          <label>
            Địa chỉ nhận hàng
            <textarea v-model="checkoutShipping.address" rows="3" placeholder="Địa chỉ giao hàng"></textarea>
          </label>
          <label>
            Mã giảm giá
            <select v-model="checkout.voucher">
              <option v-for="voucher in vouchers" :key="voucher.code" :value="voucher.code" :disabled="!voucherAvailable(voucher)">
                {{ voucher.label }}
              </option>
            </select>
            <small>{{ selectedVoucher.description }}</small>
          </label>
          <label>
            Phương thức thanh toán
            <select v-model="checkout.paymentMethod">
              <option v-for="method in paymentMethods" :key="method.value" :value="method.value">
                {{ method.label }}
              </option>
            </select>
            <small>{{ selectedPaymentMethod.note }}</small>
          </label>
          <label v-if="checkout.paymentMethod === 'Deposit'">
            Số tiền ứng cọc
            <input v-model.number="checkout.depositAmount" type="number" min="0" :max="finalAmount" />
          </label>
        </div>

        <div class="money-box">
          <p><span>Tổng tiền hàng</span><b>{{ formatMoney(cartTotal) }}</b></p>
          <p><span>Voucher</span><b>- {{ formatMoney(voucherDiscountAmount) }}</b></p>
          <p><span>Ưu đãi hạng {{ currentMemberTier.name }}</span><b>- {{ formatMoney(tierDiscountAmount) }}</b></p>
          <p v-if="checkout.paymentMethod === 'Wallet'"><span>Số dư ví</span><b>{{ formatMoney(walletBalance) }}</b></p>
          <p><span>Đã thanh toán</span><b>{{ formatMoney(paidAmount) }}</b></p>
          <p v-if="debtAmount > 0"><span>Còn phải thu / công nợ</span><b>{{ formatMoney(debtAmount) }}</b></p>
          <p><span>Trạng thái thanh toán</span><b>{{ statusLabel(paymentStatusPreview) }}</b></p>
          <p class="final"><span>Cần thanh toán</span><b>{{ formatMoney(finalAmount) }}</b></p>
        </div>

        <p v-if="checkoutMessage" class="error-text">{{ checkoutMessage }}</p>
        <button class="primary-btn full" type="button" :disabled="checkoutBusy || cart.length === 0" @click="currentUser ? submitCheckout() : openAuth('login')">
          {{ checkoutBusy ? 'Đang tạo đơn...' : (currentUser ? 'Xác nhận đặt hàng' : 'Đăng nhập để đặt hàng') }}
        </button>
      </aside>
    </main>

    <main v-else-if="activePage === 'lookup'" class="page narrow-page">
      <div class="page-title">
        <span>Tra cứu</span>
        <h1>Tra cứu đơn hàng</h1>
      </div>
      <section class="panel">
        <div class="form-grid two">
          <label>
            Mã đơn hàng
            <input v-model="orderLookup.orderCode" type="text" placeholder="Ví dụ ORD000001" />
          </label>
          <label>
            Số điện thoại
            <input v-model="orderLookup.phone" type="text" placeholder="Số điện thoại đặt hàng" />
          </label>
        </div>
        <button class="primary-btn" type="button" :disabled="orderLookup.loading" @click="lookupOrder">
          {{ orderLookup.loading ? 'Đang tra cứu...' : 'Tra cứu' }}
        </button>
        <p v-if="orderLookup.error" class="error-text">{{ orderLookup.error }}</p>

        <div v-if="orderLookup.result" class="order-result">
          <h3>{{ orderLookup.result.orderCode }}</h3>
          <p>Ngày đặt: {{ formatDateTime(orderLookup.result.orderDate) }}</p>
          <p>Tổng tiền: <b>{{ formatMoney(orderTotal(orderLookup.result)) }}</b></p>
          <p>Thanh toán: <span :class="['status-pill', statusClass(paymentStatusFor(orderLookup.result))]">{{ statusLabel(paymentStatusFor(orderLookup.result)) }}</span></p>
          <p>Trạng thái: <span :class="['status-pill', statusClass(orderLookup.result.orderStatus)]">{{ statusLabel(orderLookup.result.orderStatus) }}</span></p>
        </div>
      </section>
    </main>

    <main v-else-if="activePage === 'myOrders'" class="page">
      <div class="page-title">
        <span>Lịch sử</span>
        <h1>Đơn mua của tôi</h1>
      </div>
      <section class="panel">
        <p v-if="myOrdersLoading" class="soft-alert">Đang tải đơn hàng...</p>
        <div v-else-if="myOrders.length === 0" class="empty-state compact">
          <h3>Chưa có đơn hàng</h3>
          <p>Các đơn đã đặt sẽ hiển thị tại đây.</p>
        </div>
        <div v-else class="table-wrap">
          <table>
            <thead>
              <tr>
                <th>Mã đơn</th>
                <th>Ngày đặt</th>
                <th>Tổng tiền</th>
                <th>Đã trả</th>
                <th>Công nợ</th>
                <th>Thanh toán</th>
                <th>Trạng thái đơn</th>
                <th>Thao tác</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="order in myOrders" :key="order.orderId || order.id">
                <td>{{ order.orderCode }}</td>
                <td>{{ formatDateTime(order.orderDate) }}</td>
                <td>{{ formatMoney(orderTotal(order)) }}</td>
                <td>{{ formatMoney(order.paidAmount) }}</td>
                <td>{{ formatMoney(orderDebt(order)) }}</td>
                <td>
                  <span :class="['status-pill', statusClass(paymentStatusFor(order))]">{{ statusLabel(paymentStatusFor(order)) }}</span>
                  <small>{{ paymentMethodLabel(order.paymentMethod) }}</small>
                </td>
                <td><span :class="['status-pill', statusClass(order.orderStatus)]">{{ statusLabel(order.orderStatus) }}</span></td>
                <td class="table-actions">
                  <button v-if="canCancelOrder(order)" type="button" @click="cancelOrder(order)">X Hủy</button>
                  <span v-else>✓ Đã xử lý</span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
    </main>

    <main v-else-if="activePage === 'account'" class="account-page">
      <aside class="account-sidebar">
        <button class="account-side-row" type="button">
          <span class="side-icon alert-icon">▯</span>
          <b>Thông Báo</b>
        </button>

        <div class="account-side-group">
          <button class="account-side-row active" type="button">
            <span class="side-icon user-icon">♙</span>
            <b>Tài Khoản Của Tôi</b>
          </button>
          <button class="account-sub active" type="button">Hồ Sơ</button>
          <button class="account-sub" type="button">Ngân Hàng</button>
          <button class="account-sub" type="button">Địa Chỉ</button>
          <button class="account-sub" type="button">Đổi Mật Khẩu</button>
          <button class="account-sub" type="button">Cài Đặt Thông Báo</button>
          <button class="account-sub" type="button">Những Thiết Lập Riêng Tư</button>
          <button class="account-sub" type="button">Thông Tin Cá Nhân</button>
        </div>

      </aside>

      <section class="account-profile-card">
        <header class="profile-heading">
          <h1>Hồ Sơ Của Tôi</h1>
          <p>Quản lý thông tin hồ sơ để bảo mật tài khoản</p>
        </header>

        <div class="account-summary-grid">
          <article :class="['member-card', currentMemberTier.className]">
            <span>Hạng thành viên</span>
            <div>
              <b>{{ currentMemberTier.name }}</b>
              <em>{{ currentMemberTier.badge }}</em>
            </div>
            <p>Ưu đãi tự động {{ currentMemberTier.rate }}% khi thanh toán.</p>
            <div class="tier-progress"><i :style="{ width: tierProgressPercent + '%' }"></i></div>
            <small v-if="nextMemberTier">Còn {{ formatMoney(nextMemberTier.minSpent - (currentUser?.totalSpent || 0)) }} để lên hạng {{ nextMemberTier.name }}</small>
            <small v-else>Bạn đang ở hạng cao nhất.</small>
          </article>

          <article class="wallet-card">
            <span>Ví RetailERP</span>
            <b>{{ formatMoney(walletBalance) }}</b>
            <div class="wallet-topup">
              <input v-model.number="walletTopUpAmount" type="number" min="0" />
              <button type="button" @click="topUpWallet">Nạp ví</button>
            </div>
          </article>

          <article class="debt-card">
            <span>Công nợ hiện tại</span>
            <b>{{ formatMoney(currentUser?.currentDebt || 0) }}</b>
            <p>Công nợ phát sinh khi thanh toán một phần hoặc ví không đủ số dư.</p>
          </article>
        </div>

        <div v-if="walletTransactions.length" class="wallet-history">
          <h3>Lịch sử giao dịch ví</h3>
          <div v-for="transaction in walletTransactions.slice(0, 4)" :key="transaction.id" class="wallet-transaction">
            <span>{{ transaction.note }}</span>
            <b :class="{ minus: transaction.amount < 0 }">{{ formatMoney(transaction.amount) }}</b>
            <small>{{ formatDateTime(transaction.createdAt) }}</small>
          </div>
        </div>

        <div class="profile-content">
          <form class="profile-form" @submit.prevent="saveAccount">
            <div class="profile-row readonly">
              <label>Tên đăng nhập</label>
              <div class="profile-value">{{ accountUsername }}</div>
            </div>

            <div class="profile-row">
              <label for="profile-name">Tên</label>
              <input id="profile-name" v-model="accountForm.fullName" type="text" />
            </div>

            <div class="profile-row">
              <label for="profile-email">Email</label>
              <div v-if="accountEditing.email" class="profile-edit-line">
                <input id="profile-email" v-model="accountForm.email" type="email" placeholder="Nhập email" />
                <button type="button" @click="toggleAccountEdit('email')">Xong</button>
              </div>
              <div v-else class="profile-value">
                <span>{{ maskedAccountEmail }}</span>
                <button type="button" @click="toggleAccountEdit('email')">Thay Đổi</button>
              </div>
            </div>

            <div class="profile-row">
              <label for="profile-phone">Số điện thoại</label>
              <div v-if="accountEditing.phone" class="profile-edit-line">
                <input id="profile-phone" v-model="accountForm.phone" type="text" placeholder="Nhập số điện thoại" />
                <button type="button" @click="toggleAccountEdit('phone')">Xong</button>
              </div>
              <div v-else class="profile-value">
                <span>{{ maskedAccountPhone }}</span>
                <button type="button" @click="toggleAccountEdit('phone')">Thay Đổi</button>
              </div>
            </div>

            <div class="profile-row">
              <label>Giới tính</label>
              <div class="radio-line">
                <label><input v-model="accountProfile.gender" type="radio" value="Nam" /> Nam</label>
                <label><input v-model="accountProfile.gender" type="radio" value="Nữ" /> Nữ</label>
                <label><input v-model="accountProfile.gender" type="radio" value="Khác" /> Khác</label>
              </div>
            </div>

            <div class="profile-row">
              <label>Ngày sinh</label>
              <div class="birthday-line">
                <select v-model="accountProfile.day">
                  <option value="">Ngày</option>
                  <option v-for="day in birthDays" :key="day" :value="day">{{ day }}</option>
                </select>
                <select v-model="accountProfile.month">
                  <option value="">Tháng</option>
                  <option v-for="month in birthMonths" :key="month" :value="month">Tháng {{ month }}</option>
                </select>
                <select v-model="accountProfile.year">
                  <option value="">Năm</option>
                  <option v-for="year in birthYears" :key="year" :value="year">{{ year }}</option>
                </select>
              </div>
            </div>

            <div class="profile-row address-row">
              <label>Địa chỉ</label>
              <textarea v-model="accountForm.address" rows="3"></textarea>
            </div>

            <div class="profile-actions">
              <button class="profile-save-btn" type="submit">Lưu</button>
              <p v-if="accountMessage">{{ accountMessage }}</p>
            </div>
          </form>

          <aside class="avatar-panel">
            <div class="avatar-preview">
              <span>♙</span>
            </div>
            <button type="button">Chọn Ảnh</button>
            <p>Dung lượng file tối đa 1 MB</p>
            <p>Định dạng: .JPEG, .PNG</p>
          </aside>
        </div>
      </section>
    </main>

    <main v-else-if="isStaffPage" class="page staff-page">
      <aside class="staff-sidebar">
        <h2>Nhân viên</h2>
        <p v-if="staffUser">{{ staffUser.username }} · {{ staffUser.role }}</p>
        <button :class="{ active: activePage === 'dashboard' }" @click="openPage('dashboard')">Tổng quan</button>
        <button :class="{ active: activePage === 'orders' }" @click="openPage('orders')">Đơn hàng</button>
        <button :class="{ active: activePage === 'customers' }" @click="openPage('customers')">Khách hàng</button>
        <button :class="{ active: activePage === 'suppliers' }" @click="openPage('suppliers')">Nhà cung cấp</button>
        <button :class="{ active: activePage === 'payments' }" @click="openPage('payments')">Thanh toán</button>
        <button :class="{ active: activePage === 'debts' }" @click="openPage('debts')">Công nợ</button>
        <button :class="{ active: activePage === 'integration' }" @click="openPage('integration')">Đồng bộ kho</button>
        <button class="ghost-btn small" type="button" @click="logoutStaff">Đăng xuất nhân viên</button>
      </aside>

      <section class="staff-content panel">
        <div class="panel-head">
          <div>
            <span class="eyebrow">Khu vực nhân viên</span>
            <h1>{{ staffPageTitles[activePage] || 'Quản lý' }}</h1>
          </div>
          <button class="ghost-btn small" type="button" :disabled="staffLoading" @click="loadStaffData">Tải lại</button>
        </div>

        <p v-if="staffLoading" class="soft-alert">Đang tải dữ liệu quản trị...</p>

        <div v-if="activePage === 'dashboard'" class="dashboard-panel">
          <div class="dashboard-grid">
          <article>
            <span>Tổng đơn hàng</span>
            <b>{{ staffDashboard.totalOrders }}</b>
          </article>
          <article>
            <span>Doanh thu đã thanh toán</span>
            <b>{{ formatMoney(staffDashboard.revenue) }}</b>
          </article>
          <article>
            <span>Đơn chờ xử lý</span>
            <b>{{ staffDashboard.pending }}</b>
          </article>
          <article>
            <span>Đơn đã thanh toán</span>
            <b>{{ staffDashboard.paid }}</b>
          </article>
          <article>
            <span>Đơn đã hủy</span>
            <b>{{ staffDashboard.cancelled }}</b>
          </article>
          <article>
            <span>Tổng công nợ</span>
            <b>{{ formatMoney(staffDashboard.debt) }}</b>
          </article>
          <article>
            <span>Số dư ví demo</span>
            <b>{{ formatMoney(staffDashboard.wallet) }}</b>
          </article>
          <article>
            <span>Khách mua nhiều nhất</span>
            <b>{{ staffDashboard.topCustomer }}</b>
          </article>
          </div>

          <div class="activity-panel">
          <h3>Nhật ký giao dịch gần đây</h3>
          <p v-if="activityLogs.length === 0" class="soft-alert">Chưa có thao tác nào được ghi nhận.</p>
          <div v-for="log in activityLogs.slice(0, 8)" :key="log.id" class="activity-row">
            <b>{{ log.action }}</b>
            <span>{{ log.note }}</span>
            <small>{{ log.actor }} · {{ formatDateTime(log.createdAt) }}</small>
          </div>
        </div>

        </div>

        <div v-else-if="activePage === 'orders'" class="table-wrap">
          <table>
            <thead><tr><th>Mã đơn</th><th>Khách</th><th>Tổng</th><th>Công nợ</th><th>Thanh toán</th><th>Trạng thái</th><th>Thao tác</th></tr></thead>
            <tbody>
              <tr v-for="order in staffData.orders" :key="order.orderId || order.id">
                <td>{{ order.orderCode }}</td>
                <td>{{ order.customerName || order.customerId }}</td>
                <td>{{ formatMoney(orderTotal(order)) }}</td>
                <td>{{ formatMoney(orderDebt(order)) }}</td>
                <td>
                  <span :class="['status-pill', statusClass(paymentStatusFor(order))]">{{ statusLabel(paymentStatusFor(order)) }}</span>
                  <small>{{ paymentMethodLabel(order.paymentMethod) }}</small>
                </td>
                <td><span :class="['status-pill', statusClass(order.orderStatus)]">{{ statusLabel(order.orderStatus) }}</span></td>
                <td class="table-actions">
                  <button @click="updateOrderStatus(order, 'Confirmed')">✓ Xác nhận</button>
                  <button @click="updateOrderStatus(order, 'Paid')">✓ Đã thanh toán</button>
                  <button v-if="canCancelOrder(order)" @click="cancelOrder(order, true)">X Hủy</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <div v-else-if="activePage === 'customers'" class="table-wrap">
          <table>
            <thead><tr><th>Mã KH</th><th>Họ tên</th><th>SĐT</th><th>Email</th><th>Công nợ</th></tr></thead>
            <tbody>
              <tr v-for="customer in staffData.customers" :key="customer.customerId || customer.id">
                <td>{{ customer.customerCode }}</td>
                <td>{{ customerName(customer) }}</td>
                <td>{{ customer.phone }}</td>
                <td>{{ customer.email }}</td>
                <td>{{ formatMoney(customer.currentDebt) }}</td>
              </tr>
            </tbody>
          </table>
        </div>

        <div v-else-if="activePage === 'suppliers'" class="table-wrap">
          <table>
            <thead><tr><th>Mã NCC</th><th>Tên</th><th>Liên hệ</th><th>Điện thoại</th><th>Email</th></tr></thead>
            <tbody>
              <tr v-for="supplier in staffData.suppliers" :key="supplier.supplierId || supplier.id">
                <td>{{ supplier.supplierCode }}</td>
                <td>{{ supplier.supplierName || supplier.name }}</td>
                <td>{{ supplier.contactPerson }}</td>
                <td>{{ supplier.phone }}</td>
                <td>{{ supplier.email }}</td>
              </tr>
            </tbody>
          </table>
        </div>

        <div v-else-if="activePage === 'payments'" class="table-wrap">
          <table>
            <thead><tr><th>Mã TT</th><th>Đơn</th><th>Phương thức</th><th>Số tiền</th><th>Ngày</th></tr></thead>
            <tbody>
              <tr v-for="payment in staffData.payments" :key="payment.paymentId || payment.id">
                <td>{{ payment.paymentCode }}</td>
                <td>{{ payment.orderId }}</td>
                <td>{{ payment.paymentMethod }}</td>
                <td>{{ formatMoney(payment.amount) }}</td>
                <td>{{ formatDateTime(payment.paymentDate) }}</td>
              </tr>
            </tbody>
          </table>
        </div>

        <div v-else-if="activePage === 'debts'" class="table-wrap">
          <table>
            <thead><tr><th>Khách</th><th>Đơn</th><th>Số nợ</th><th>Hạn trả</th><th>Trạng thái</th></tr></thead>
            <tbody>
              <tr v-for="debt in staffData.debts" :key="debt.debtId || debt.id">
                <td>{{ debt.customerName || debt.customerId }}</td>
                <td>{{ debt.orderId }}</td>
                <td>{{ formatMoney(debt.debtAmount) }}</td>
                <td>{{ formatDateTime(debt.dueDate) }}</td>
                <td><span :class="['status-pill', statusClass(debt.debtStatus)]">{{ statusLabel(debt.debtStatus) }}</span></td>
              </tr>
            </tbody>
          </table>
        </div>

        <div v-else class="integration-grid">
          <article>
            <h3>ProductStockCaches</h3>
            <p>Consume `stock.updated` để cập nhật tồn kho hiển thị cho khách.</p>
            <b>{{ products.length }}</b>
          </article>
          <article>
            <h3>Outbox order.created</h3>
            <p>Publish `order.created` sau khi đặt hàng thành công.</p>
            <b>{{ staffData.outbox.length }}</b>
          </article>
        </div>
      </section>
    </main>

    <div v-if="selectedProduct" class="modal-backdrop" @click.self="closeProductDetail">
      <section class="modal-card product-modal">
        <button class="modal-close" type="button" @click="closeProductDetail">×</button>
        <div class="product-detail-grid">
          <img :src="productImage(selectedProduct)" alt="" />
          <div>
            <span class="eyebrow">{{ productCode(selectedProduct) }} · {{ productCategory(selectedProduct) }}</span>
            <h2>{{ productName(selectedProduct) }}</h2>
            <p>{{ selectedProduct.manufacturerName }}</p>
            <b class="detail-price">{{ formatMoney(productPrice(selectedProduct)) }}</b>
            <div class="detail-facts">
              <span>Còn trong kho: <b>{{ productStock(selectedProduct) }}</b></span>
              <span>Trạng thái: <b>{{ productStock(selectedProduct) > 0 ? 'Còn hàng' : 'Hết hàng' }}</b></span>
              <span>Đồng bộ từ Nhóm 1</span>
            </div>
            <button class="primary-btn full" type="button" :disabled="productStock(selectedProduct) <= 0" @click="addToCart(selectedProduct); closeProductDetail()">
              {{ productStock(selectedProduct) > 0 ? 'Thêm vào giỏ hàng' : 'Sản phẩm đã hết hàng' }}
            </button>
          </div>
        </div>

        <div v-if="relatedProducts.length" class="related-products">
          <h3>Sản phẩm cùng danh mục</h3>
          <div>
            <button v-for="product in relatedProducts" :key="productId(product)" type="button" @click="selectedProduct = product">
              <img :src="productImage(product)" alt="" />
              <span>{{ productName(product) }}</span>
              <b>{{ formatMoney(productPrice(product)) }}</b>
            </button>
          </div>
        </div>
      </section>
    </div>

    <div v-if="showAuthModal" class="modal-backdrop">
      <section class="modal-card">
        <button class="modal-close" type="button" @click="closeAuth">×</button>
        <span class="eyebrow">Khách hàng</span>
        <h2>{{ authMode === 'login' ? 'Đăng nhập mua hàng' : 'Đăng ký tài khoản' }}</h2>

        <div v-if="authMode === 'login'" class="form-grid">
          <label>Số điện thoại<input v-model="loginForm.phone" type="text" /></label>
          <label>Mật khẩu demo<input v-model="loginForm.password" type="password" /></label>
          <button class="primary-btn full" type="button" :disabled="authBusy" @click="loginCustomer">
            {{ authBusy ? 'Đang đăng nhập...' : 'Đăng nhập' }}
          </button>
          <button class="link-btn" type="button" @click="authMode = 'register'; authError = ''">Chưa có tài khoản? Đăng ký</button>
        </div>

        <div v-else class="form-grid">
          <label>Họ tên<input v-model="registerForm.fullName" type="text" /></label>
          <label>Số điện thoại<input v-model="registerForm.phone" type="text" /></label>
          <label>Email<input v-model="registerForm.email" type="email" /></label>
          <label>Địa chỉ<input v-model="registerForm.address" type="text" /></label>
          <label>Mật khẩu demo<input v-model="registerForm.password" type="password" /></label>
          <button class="primary-btn full" type="button" :disabled="authBusy" @click="registerCustomer">
            {{ authBusy ? 'Đang đăng ký...' : 'Đăng ký' }}
          </button>
          <button class="link-btn" type="button" @click="authMode = 'login'; authError = ''">Đã có tài khoản? Đăng nhập</button>
        </div>

        <p v-if="authError" class="error-text">{{ authError }}</p>
      </section>
    </div>

    <div v-if="showStaffModal" class="modal-backdrop">
      <section class="modal-card small-modal">
        <button class="modal-close" type="button" @click="showStaffModal = false">×</button>
        <span class="eyebrow">Nhân viên</span>
        <h2>Đăng nhập quản trị</h2>
        <div class="form-grid">
          <label>Tên đăng nhập<input v-model="staffLoginForm.username" type="text" /></label>
          <label>
            Vai trò
            <select v-model="staffLoginForm.role">
              <option value="Sales">Sales</option>
              <option value="Admin">Admin</option>
              <option value="Warehouse">Warehouse</option>
            </select>
          </label>
          <button class="primary-btn full" type="button" :disabled="staffBusy" @click="loginStaff">
            {{ staffBusy ? 'Đang đăng nhập...' : 'Đăng nhập nhân viên' }}
          </button>
        </div>
        <p v-if="staffError" class="error-text">{{ staffError }}</p>
      </section>
    </div>
  </div>
</template>

<style scoped src="./App.css"></style>
