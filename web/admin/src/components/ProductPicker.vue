<script setup>
import { computed, ref } from 'vue'
import Button from 'primevue/button'
import Select from 'primevue/select'
import Tag from 'primevue/tag'
import { fmt, onSale } from '@/utils/format'

// Danh sách sản phẩm có thứ tự (id): chọn thêm từ ô tìm, đổi chỗ lên/xuống, bỏ.
// `exclude`: id đang dùng ở danh sách khác của cùng khoảnh khắc — một sản phẩm chỉ ở một chỗ.
const ids = defineModel({ type: Array, required: true })
const props = defineProps({
  products: { type: Array, required: true },
  exclude: { type: Array, default: () => [] },
  placeholder: { type: String, default: 'Thêm sản phẩm…' },
})

const byId = computed(() => new Map(props.products.map(p => [p.id, p])))
const options = computed(() => props.products
  .filter(p => !ids.value.includes(p.id) && !props.exclude.includes(p.id))
  .map(p => ({ value: p.id, label: `${p.model} — ${p.name}` })))
const adding = ref(null)

function add(id) {
  if (id) ids.value = [...ids.value, id]
  adding.value = null
}
function move(i, d) {
  const next = [...ids.value]
  ;[next[i], next[i + d]] = [next[i + d], next[i]]
  ids.value = next
}
const remove = i => (ids.value = ids.value.filter((_, j) => j !== i))
</script>

<template>
  <Select v-model="adding" :options="options" option-label="label" option-value="value" filter
          :placeholder="placeholder" fluid @update:model-value="add" />
  <div class="picker-list">
    <div v-for="(id, i) in ids" :key="id" class="picker-item">
      <img v-if="byId.get(id)?.imageUrl" class="thumb" :src="byId.get(id).imageUrl" alt="">
      <span v-else class="thumb empty"><i class="pi pi-image" /></span>
      <div class="name">
        {{ byId.get(id)?.name ?? id }}
        <small>{{ byId.get(id)?.model }} · {{ fmt(byId.get(id)?.price) }}</small>
      </div>
      <Tag v-if="byId.get(id) && !onSale(byId.get(id))" value="Không hiện trên site" severity="warn" />
      <Button icon="pi pi-arrow-up" text rounded aria-label="Lên" :disabled="i === 0" @click="move(i, -1)" />
      <Button icon="pi pi-arrow-down" text rounded aria-label="Xuống" :disabled="i === ids.length - 1" @click="move(i, 1)" />
      <Button icon="pi pi-times" text rounded severity="danger" aria-label="Bỏ" @click="remove(i)" />
    </div>
    <p v-if="!ids.length" class="muted" style="margin:4px 0">Chưa có sản phẩm.</p>
  </div>
</template>
