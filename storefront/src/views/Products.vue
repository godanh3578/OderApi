<script setup>
import { ref, onMounted } from 'vue'
import axios from 'axios'

const API = 'http://160.250.132.117:3000'
const products = ref([])
const loading = ref(true)

const cart = ref(JSON.parse(localStorage.getItem('cart') || '[]'))

onMounted(async () => {
  try {
    const res = await axios.get(`${API}/api/ProductStockCaches`)
    products.value = Array.isArray(res.data) ? res.data : [
      {
        id: 1,
        productId: 1,
        productCode: 'SP001',
        productName: 'Laptop Gaming',
        sellingPrice: 25000000,
        quantityAvailable: 12,
        stockStatus: 'InStock',
        imageUrl: 'https://images.unsplash.com/photo-1603302576837-37561b2e2302?auto=format&fit=crop&w=500&q=80'
      },
      {
        id: 2,
        productId: 2,
        productCode: 'SP002',
        productName: 'Chuột Logitech',
        sellingPrice: 500000,
        quantityAvailable: 40,
        stockStatus: 'InStock',
        imageUrl: 'https://images.unsplash.com/photo-1527814050087-379381547949?auto=format&fit=crop&w=500&q=80'
      },
      {
        id: 3,
        productId: 3,
        productCode: 'SP003',
        productName: 'Bàn phím cơ',
        sellingPrice: 1200000,
        quantityAvailable: 0,
        stockStatus: 'OutOfStock',
        imageUrl: 'https://images.unsplash.com/photo-1595225476474-87563907a212?auto=format&fit=crop&w=500&q=80'
      }
    ] // Fallback mock data in case API fails
  } catch (error) {
    console.error('Error fetching products:', error)
    // Add mock data if API is unreachable
    products.value = [
      {
        id: 1,
        productId: 1,
        productCode: 'SP001',
        productName: 'Laptop Gaming',
        sellingPrice: 25000000,
        quantityAvailable: 12,
        stockStatus: 'InStock',
        imageUrl: 'https://images.unsplash.com/photo-1603302576837-37561b2e2302?auto=format&fit=crop&w=500&q=80'
      },
      {
        id: 2,
        productId: 2,
        productCode: 'SP002',
        productName: 'Chuột Logitech',
        sellingPrice: 500000,
        quantityAvailable: 40,
        stockStatus: 'InStock',
        imageUrl: 'https://images.unsplash.com/photo-1527814050087-379381547949?auto=format&fit=crop&w=500&q=80'
      }
    ]
  } finally {
    loading.value = false
  }
})

const addToCart = (product) => {
  if (product.quantityAvailable <= 0) {
    alert('Sản phẩm đã hết hàng!')
    return
  }

  const existingItem = cart.value.find(item => item.productId === product.productId)
  if (existingItem) {
    if (existingItem.quantity + 1 > product.quantityAvailable) {
      alert('Vượt quá số lượng tồn kho!')
      return
    }
    existingItem.quantity++
  } else {
    cart.value.push({
      productId: product.productId,
      productCode: product.productCode,
      productName: product.productName,
      unitPrice: product.sellingPrice,
      quantity: 1,
      stock: product.quantityAvailable,
      imageUrl: product.imageUrl || 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?auto=format&fit=crop&w=500&q=80'
    })
  }

  localStorage.setItem('cart', JSON.stringify(cart.value))
  alert(`Đã thêm ${product.productName} vào giỏ hàng!`)
}

const formatPrice = (price) => {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price)
}
</script>

<template>
  <div class="products-page animate-fade-in container">
    <h1 class="heading-2 mb-6">Sản phẩm nổi bật</h1>
    
    <div v-if="loading" class="text-center py-12">
      <p class="text-muted heading-3">Đang tải sản phẩm...</p>
    </div>

    <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
      <div v-for="product in products" :key="product.productId" class="card product-card">
        <div class="product-image">
          <img :src="product.imageUrl || 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?auto=format&fit=crop&w=500&q=80'" :alt="product.productName">
          <span v-if="product.quantityAvailable <= 0" class="badge badge-danger">Hết hàng</span>
          <span v-else class="badge badge-success">Còn {{ product.quantityAvailable }}</span>
        </div>
        <div class="product-info p-4">
          <p class="text-muted font-size-sm">{{ product.productCode }}</p>
          <h3 class="font-semibold text-lg mb-2 truncate">{{ product.productName }}</h3>
          <p class="text-primary font-bold heading-3 mb-4">{{ formatPrice(product.sellingPrice) }}</p>
          
          <button 
            class="btn btn-primary w-full" 
            :disabled="product.quantityAvailable <= 0"
            @click="addToCart(product)"
          >
            Thêm vào giỏ
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.products-page {
  padding: 3rem 1.5rem;
}

.mb-6 { margin-bottom: 1.5rem; }
.mb-4 { margin-bottom: 1rem; }
.mb-2 { margin-bottom: 0.5rem; }
.p-4 { padding: 1rem; }
.py-12 { padding-top: 3rem; padding-bottom: 3rem; }
.w-full { width: 100%; }
.truncate { white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }

.product-card {
  display: flex;
  flex-direction: column;
  height: 100%;
}

.product-image {
  position: relative;
  width: 100%;
  height: 200px;
  overflow: hidden;
  background-color: var(--color-background);
}

.product-image img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform var(--transition-normal);
}

.product-card:hover .product-image img {
  transform: scale(1.05);
}

.badge {
  position: absolute;
  top: 1rem;
  right: 1rem;
  padding: 0.25rem 0.75rem;
  border-radius: var(--radius-full);
  font-size: var(--font-size-xs);
  font-weight: 600;
  color: #fff;
}

.badge-success { background-color: var(--color-success); }
.badge-danger { background-color: var(--color-danger); }

.product-info {
  flex-grow: 1;
  display: flex;
  flex-direction: column;
}

.product-info .btn {
  margin-top: auto;
}

.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
  background-color: var(--color-text-muted);
}
</style>
