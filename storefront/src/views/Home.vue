<script setup>
import { useRouter } from 'vue-router'

const router = useRouter()

const goToProducts = () => {
  router.push('/products')
}
</script>

<template>
  <div class="home animate-fade-in">
    <section class="hero text-center">
      <div class="container flex flex-col items-center justify-center gap-6">
        <h1 class="heading-1 text-primary">Chào mừng đến với Storefront</h1>
        <p class="heading-3 text-muted">Mua sắm thiết bị công nghệ với giá tốt nhất thị trường.</p>
        <button class="btn btn-primary" @click="goToProducts">
          Khám phá Sản phẩm ngay
        </button>
      </div>
    </section>

    <section class="features container grid grid-cols-1 md:grid-cols-3 gap-8">
      <div class="card p-6 text-center">
        <div class="icon">🚀</div>
        <h3 class="heading-3">Giao Hàng Siêu Tốc</h3>
        <p class="text-muted mt-2">Nhận hàng trong vòng 2h tại nội thành và 24h trên toàn quốc.</p>
      </div>
      <div class="card p-6 text-center">
        <div class="icon">🛡️</div>
        <h3 class="heading-3">Bảo Hành Chính Hãng</h3>
        <p class="text-muted mt-2">Cam kết 100% sản phẩm chính hãng với chế độ bảo hành tốt nhất.</p>
      </div>
      <div class="card p-6 text-center">
        <div class="icon">💸</div>
        <h3 class="heading-3">Giá Cạnh Tranh</h3>
        <p class="text-muted mt-2">Luôn có các chương trình khuyến mãi và mức giá tốt nhất cho bạn.</p>
      </div>
    </section>
  </div>
</template>

<style scoped>
.hero {
  padding: 6rem 0;
  background: linear-gradient(135deg, rgba(79, 70, 229, 0.1) 0%, rgba(15, 23, 42, 0.05) 100%);
  border-radius: var(--radius-2xl);
  margin: 2rem 0;
}

.features {
  padding: 4rem 0;
}

.icon {
  font-size: 3rem;
  margin-bottom: 1rem;
}

.p-6 {
  padding: 1.5rem;
}

.mt-2 {
  margin-top: 0.5rem;
}
</style>
