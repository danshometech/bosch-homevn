<script setup>
import CheckoutMethodsManager from '@/components/CheckoutMethodsManager.vue'
</script>

<template>
  <div class="page-h">
    <h1>Giao hàng &amp; thanh toán</h1>
  </div>
  <CheckoutMethodsManager kind="shipping" />
  <CheckoutMethodsManager kind="payment" style="margin-top:16px" />
</template>
