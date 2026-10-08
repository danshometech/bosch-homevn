<script setup>
import { useTween } from '@/composables/useTween'
import { fmt } from '@/utils/format'

const props = defineProps({ value: { type: Number, required: true } })
const shown = useTween(() => props.value)
</script>

<template>
  <span class="tn">{{ fmt(shown) }}</span>
</template>
