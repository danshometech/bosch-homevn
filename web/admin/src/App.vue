<script setup>
import Toast from 'primevue/toast'
import ConfirmDialog from 'primevue/confirmdialog'
</script>

<template>
  <RouterView />
  <Toast position="bottom-right" />
  <ConfirmDialog />
</template>
