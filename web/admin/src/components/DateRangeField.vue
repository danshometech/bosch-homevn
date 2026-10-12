<script setup>
import { computed } from 'vue'
import Button from 'primevue/button'
import DatePicker from 'primevue/datepicker'
import { addDays, startOfToday } from '@/utils/format'

// Chọn khoảng ngày [từ, đến], tối đa MAX_DAYS ngày (tính cả hai đầu), không quá hôm nay. Chọn xong ngày đầu thì lịch chỉ
// cho chọn ngày cuối trong vòng MAX_DAYS ngày quanh nó. v-model: [Date, Date] (đang chọn dở: [Date, null])
const MAX_DAYS = 31
const range = defineModel({ type: Array, default: () => [null, null] })
const today = startOfToday()

const picking = computed(() => !!range.value?.[0] && !range.value?.[1])
const minDate = computed(() => (picking.value ? addDays(range.value[0], -(MAX_DAYS - 1)) : null))
const maxDate = computed(() => {
  const cap = picking.value ? addDays(range.value[0], MAX_DAYS - 1) : today
  return cap < today ? cap : today
})

const y = today.getFullYear()
const m = today.getMonth()
const PRESETS = [
  ['7 ngày qua', [addDays(today, -6), today]],
  ['30 ngày qua', [addDays(today, -29), today]],
  ['Tháng này', [new Date(y, m, 1), today]],
  ['Tháng trước', [new Date(y, m - 1, 1), new Date(y, m, 0)]],
]
const isOn = ([from, to]) => +range.value?.[0] === +from && +range.value?.[1] === +to
</script>

<template>
  <div class="date-range">
    <DatePicker
      v-model="range"
      selection-mode="range"
      :manual-input="false"
      :min-date="minDate"
      :max-date="maxDate"
      date-format="dd/mm/yy"
      show-icon
      icon-display="input"
      placeholder="Chọn từ ngày – đến ngày"
      fluid
    />
    <div class="actions">
      <Button
        v-for="[label, r] in PRESETS"
        :key="label"
        :label="label"
        size="small"
        :severity="isOn(r) ? undefined : 'secondary'"
        :outlined="!isOn(r)"
        @click="range = [...r]"
      />
    </div>
    <small class="muted">Tối đa {{ MAX_DAYS }} ngày mỗi lần.</small>
  </div>
</template>
