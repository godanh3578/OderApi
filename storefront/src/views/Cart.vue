<script setup>
import { ref, computed } from 'vue'
import axios from 'axios'

const API = 'http://160.250.132.117:3000'
const cart = ref(JSON.parse(localStorage.getItem('cart') || '[]'))

const checkoutForm = ref({
  customerName: '',
  phone: '',
  address: '',
  paymentMethod: 'Cash'
})

const cartTotal = computed(() => {
  return cart.value.reduce((sum, item) => sum + (item.unitPrice * item.quantity), 0)
})

const formatPrice = (price) => {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price)
}

const updateQuantity = (item, delta) => {
  const newQty = item.quantity + delta
  if (newQty < 1) return
  if (newQty > item.stock) {
    alert('Vượt quá số lượng tồn kho!')
    return
  }
  item.quantity = newQty
  saveCart()
}

const removeItem = (productId) => {
  cart.value = cart.value.filter(item => item.productId !== productId)
  saveCart()
}

const saveCart = () => {
  localStorage.setItem('cart', JSON.stringify(cart.value))
}

const checkout = async () => {
  if (cart.value.length === 0) {
    alert('Giỏ hàng trống!')
    return
  }
  if (!checkoutForm.value.customerName || !checkoutForm.value.phone || !checkoutForm.value.address) {
    alert('Vui lòng điền đầy đủ thông tin giao hàng!')
    return
  }

  // Find or create customer
  let customerId = 1 // Default fallback
  try {
    const custRes = await axios.post(`${API}/api/Customers`, {
      name: checkoutForm.value.customerName,
      phone: checkoutForm.value.phone,
      address: checkoutForm.value.address,
      email: '',
      debt: 0
    })
    customerId = custRes.data?.id || 1
  } catch (error) {
    console.warn('Could not create customer, using default ID 1')
  }

  // Create order
  const payload = {
    customerId: customerId,
    totalAmount: cartTotal.value,
    discountAmount: 0,
    finalAmount: cartTotal.value,
    paidAmount: checkoutForm.value.paymentMethod === 'Cash' ? 0 : cartTotal.value,
    debtAmount: checkoutForm.value.paymentMethod === 'Cash' ? cartTotal.value : 0,
    paymentMethod: checkoutForm.value.paymentMethod,
    paymentStatus: checkoutForm.value.paymentMethod === 'Cash' ? 'Pending' : 'Paid',
    orderStatus: 'Pending',
    status: 0,
    items: cart.value.map(item => ({
      productId: item.productId,
      productCode: item.productCode,
      productName: item.productName,
      quantity: item.quantity,
      unitPrice: item.unitPrice,
      subTotal: item.quantity * item.unitPrice
    }))
  }

  try {
    await axios.post(`${API}/api/Orders`, payload)
    alert('Đặt hàng thành công!')
    cart.value = []
    saveCart()
  } catch (error) {
    console.error('Lỗi khi đặt hàng:', error)
    alert('Có lỗi xảy ra khi đặt hàng. Chi tiết trong console.')
  }
}
</script>

<template>
  <div class="cart-page animate-fade-in container">
    <h1 class="heading-2 mb-6">Giỏ hàng của bạn</h1>

    <div v-if="cart.length === 0" class="text-center py-12 card">
      <div class="text-4xl mb-4">🛒</div>
      <h3 class="heading-3 text-muted">Giỏ hàng đang trống</h3>
      <router-link to="/products" class="btn btn-primary mt-4">Tiếp tục mua sắm</router-link>
    </div>

    <div v-else class="grid grid-cols-1 lg:grid-cols-3 gap-8">
      <!-- Cart Items -->
      <div class="lg:col-span-2 flex flex-col gap-4">
        <div v-for="item in cart" :key="item.productId" class="card p-4 flex items-center gap-4">
          <img :src="item.imageUrl" :alt="item.productName" class="cart-img rounded">
          <div class="flex-grow">
            <h3 class="font-semibold">{{ item.productName }}</h3>
            <p class="text-muted font-size-sm">{{ item.productCode }}</p>
            <p class="text-primary font-bold">{{ formatPrice(item.unitPrice) }}</p>
          </div>
          <div class="flex items-center gap-2">
            <button class="qty-btn" @click="updateQuantity(item, -1)">-</button>
            <span class="w-8 text-center">{{ item.quantity }}</span>
            <button class="qty-btn" @click="updateQuantity(item, 1)">+</button>
          </div>
          <button class="remove-btn" @click="removeItem(item.productId)">🗑️</button>
        </div>
      </div>

      <!-- Checkout Summary -->
      <div class="card p-6 checkout-summary">
        <h3 class="heading-3 mb-4 border-b pb-2">Tổng quan đơn hàng</h3>
        <div class="flex justify-between mb-4">
          <span class="text-muted">Tổng tiền sản phẩm:</span>
          <span class="font-bold">{{ formatPrice(cartTotal) }}</span>
        </div>
        <div class="flex justify-between mb-6">
          <span class="text-muted">Phí giao hàng:</span>
          <span class="font-bold text-success">Miễn phí</span>
        </div>
        <div class="flex justify-between mb-6 pt-4 border-t">
          <span class="heading-3">Thành tiền:</span>
          <span class="heading-3 text-primary">{{ formatPrice(cartTotal) }}</span>
        </div>

        <h3 class="heading-3 mb-4 mt-8 border-b pb-2">Thông tin giao hàng</h3>
        <div class="flex flex-col gap-4 mb-6">
          <input type="text" v-model="checkoutForm.customerName" class="input-field" placeholder="Họ tên">
          <input type="tel" v-model="checkoutForm.phone" class="input-field" placeholder="Số điện thoại">
          <textarea v-model="checkoutForm.address" class="input-field" placeholder="Địa chỉ giao hàng" rows="3"></textarea>
          <select v-model="checkoutForm.paymentMethod" class="input-field">
            <option value="Cash">Thanh toán tiền mặt khi nhận hàng (COD)</option>
            <option value="BankTransfer">Chuyển khoản ngân hàng</option>
          </select>
        </div>

        <button class="btn btn-primary w-full" @click="checkout">Tiến hành Đặt hàng</button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.cart-page { padding: 3rem 1.5rem; }
.mb-6 { margin-bottom: 1.5rem; }
.mb-4 { margin-bottom: 1rem; }
.mt-4 { margin-top: 1rem; }
.mt-8 { margin-top: 2rem; }
.p-4 { padding: 1rem; }
.p-6 { padding: 1.5rem; }
.py-12 { padding-top: 3rem; padding-bottom: 3rem; }
.pb-2 { padding-bottom: 0.5rem; }
.pt-4 { padding-top: 1rem; }
.gap-2 { gap: 0.5rem; }
.gap-4 { gap: 1rem; }
.w-full { width: 100%; }
.w-8 { width: 2rem; }
.flex-grow { flex-grow: 1; }
.border-b { border-bottom: 1px solid var(--color-border); }
.border-t { border-top: 1px solid var(--color-border); }
.rounded { border-radius: var(--radius-md); }
.text-4xl { font-size: 3rem; }
.text-success { color: var(--color-success); }

@media (min-width: 1024px) {
  .lg\:col-span-2 { grid-column: span 2 / span 2; }
}

.cart-img {
  width: 80px;
  height: 80px;
  object-fit: cover;
}

.qty-btn {
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: var(--color-background);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-sm);
  font-weight: bold;
  transition: all var(--transition-fast);
}

.qty-btn:hover {
  background-color: var(--color-border);
}

.remove-btn {
  color: var(--color-danger);
  font-size: 1.25rem;
  padding: 0.5rem;
  border-radius: var(--radius-full);
  transition: background-color var(--transition-fast);
}

.remove-btn:hover {
  background-color: rgba(239, 68, 68, 0.1);
}

.checkout-summary {
  align-self: start;
  position: sticky;
  top: 2rem;
}
</style>
