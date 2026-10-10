<script setup>
import { ref, onMounted } from 'vue'
import { useToast } from 'primevue/usetoast'
import { useConfirm } from 'primevue/useconfirm'
import Button from 'primevue/button'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Dialog from 'primevue/dialog'
import InputNumber from 'primevue/inputnumber'
import InputText from 'primevue/inputtext'
import Tag from 'primevue/tag'
import Textarea from 'primevue/textarea'
import ToggleSwitch from 'primevue/toggleswitch'
import { get, post, put, del } from '@/services/api'

// kind: shipping (có phí) | payment (có cờ trả góp)
const props = defineProps({ kind: { type: String, required: true } })
const isShip = props.kind === 'shipping'
const base = `/admin/checkout/${props.kind}`
const noun = isShip ? 'cách giao hàng' : 'cách thanh toán'

const toast = useToast()
const confirm = useConfirm()
const items = ref([])
const loading = ref(true)

const fail = (summary, err) => toast.add({ severity: 'error', summary, detail: err.message, life: 5000 })
const ok = summary => toast.add({ severity: 'success', summary, life: 2500 })
const money = v => (v ? v.toLocaleString('vi-VN') + ' ₫' : 'Miễn phí')

async function load() {
  try {
    items.value = await get(base)
  } catch (err) {
    fail(`Không tải được ${noun}`, err)
  } finally {
    loading.value = false
  }
}
onMounted(load)

const body = m => ({
  name: m.name.trim(),
  description: m.description?.trim() || null,
  isActive: m.isActive,
  ...(isShip ? { fee: m.fee ?? 0 } : { isInstallment: m.isInstallment }),
})

const dialog = ref(null)
const open = m => (dialog.value = m
  ? { ...m, description: m.description ?? '' }
  : { id: null, name: '', description: '', fee: 0, isInstallment: false, isActive: true })

async function save() {
  const d = dialog.value
  try {
    d.id ? await put(`${base}/${d.id}`, body(d)) : await post(base, body(d))
    ok(d.id ? 'Đã lưu' : `Đã thêm ${noun}`)
    dialog.value = null
    await load()
  } catch (err) {
    fail('Không lưu được', err)
  }
}

async function toggle(m, isActive) {
  try {
    await put(`${base}/${m.id}`, body({ ...m, isActive }))
    m.isActive = isActive
  } catch (err) {
    fail('Không đổi được', err)
  }
}

function remove(m) {
  confirm.require({
    header: `Xóa ${noun}`,
    message: `Xóa "${m.name}"?`,
    icon: 'pi pi-exclamation-triangle',
    acceptProps: { label: 'Xóa', severity: 'danger' },
    rejectProps: { label: 'Hủy', severity: 'secondary', outlined: true },
    accept: async () => {
      try {
        await del(`${base}/${m.id}`)
        ok('Đã xóa')
        await load()
      } catch (err) {
        fail('Không xóa được', err)
      }
    },
  })
}

async function move(i, d) {
  const list = items.value
  if (i + d < 0 || i + d >= list.length) return
  const ids = list.map(x => x.id)
  ;[ids[i], ids[i + d]] = [ids[i + d], ids[i]]
  try {
    await put(`${base}/order`, { ids })
    await load()
  } catch (err) {
    fail('Không đổi được thứ tự', err)
  }
}
</script>

<template>
  <div class="box">
    <div class="actions" style="margin-bottom:16px;align-items:center;justify-content:space-between;flex-wrap:nowrap">
      <div>
        <h2 style="margin:0 0 4px">{{ isShip ? 'Giao hàng' : 'Phương thức thanh toán' }}</h2>
        <p class="muted" style="margin:0;font-size:13px">
          <template v-if="isShip">Thứ tự ở đây là thứ tự trên trang thanh toán, mục đầu tiên được chọn sẵn. Phí cộng vào tổng đơn.</template>
          <template v-else>Thứ tự ở đây là thứ tự trên trang thanh toán. Cách "trả góp" chỉ hiện khi giỏ có sản phẩm cho trả góp, khách chọn số tháng.</template>
        </p>
      </div>
      <Button :label="isShip ? 'Thêm cách giao' : 'Thêm cách thanh toán'" icon="pi pi-plus" style="flex-shrink:0" @click="open(null)" />
    </div>
    <DataTable :value="items" :loading="loading" data-key="id" size="small">
      <template #empty><span class="muted">Chưa có {{ noun }} nào.</span></template>
      <Column :header="isShip ? 'Cách giao hàng' : 'Cách thanh toán'">
        <template #body="{ data }">
          <b>{{ data.name }}</b>
          <Tag v-if="data.isInstallment" value="Trả góp" severity="info" style="margin-left:8px" />
          <div v-if="data.description" class="muted" style="font-size:13px">{{ data.description }}</div>
        </template>
      </Column>
      <Column v-if="isShip" header="Phí" style="width:130px">
        <template #body="{ data }">{{ money(data.fee) }}</template>
      </Column>
      <Column header="Hiện trên site" style="width:120px">
        <template #body="{ data }">
          <ToggleSwitch :model-value="data.isActive" :aria-label="`Hiện ${data.name}`" @update:model-value="v => toggle(data, v)" />
        </template>
      </Column>
      <Column header="" style="width:200px">
        <template #body="{ data, index }">
          <Button icon="pi pi-arrow-up" text rounded aria-label="Lên" :disabled="index === 0" @click="move(index, -1)" />
          <Button icon="pi pi-arrow-down" text rounded aria-label="Xuống" :disabled="index === items.length - 1" @click="move(index, 1)" />
          <Button icon="pi pi-pencil" text rounded aria-label="Sửa" @click="open(data)" />
          <Button icon="pi pi-trash" text rounded severity="danger" aria-label="Xóa" @click="remove(data)" />
        </template>
      </Column>
    </DataTable>
  </div>

  <Dialog :visible="!!dialog" modal :header="dialog?.id ? `Sửa ${noun}` : `Thêm ${noun}`" style="width:min(480px, 92vw)"
          @update:visible="v => !v && (dialog = null)">
    <form v-if="dialog" class="rows" style="gap:14px" @submit.prevent="save">
      <div class="field">
        <label :for="`${kind}-name`">Tên</label>
        <InputText :id="`${kind}-name`" v-model="dialog.name" required maxlength="200" autofocus
                   :placeholder="isShip ? 'vd. Giao nhanh trong 24h' : 'vd. Chuyển khoản / QR ngân hàng'" />
      </div>
      <div class="field">
        <label :for="`${kind}-desc`">Mô tả ngắn (không bắt buộc)</label>
        <Textarea :id="`${kind}-desc`" v-model="dialog.description" rows="2" maxlength="300" auto-resize
                  :placeholder="isShip ? 'vd. Áp dụng nội thành HN, HCM, ĐN' : 'vd. VietQR · Xác nhận tự động'" />
        <small>Dòng chữ nhỏ dưới tên trên trang thanh toán.</small>
      </div>
      <div v-if="isShip" class="field">
        <label :for="`${kind}-fee`">Phí giao hàng</label>
        <InputNumber v-model="dialog.fee" :input-id="`${kind}-fee`" mode="currency" currency="VND" locale="vi-VN" :min="0" :max="1000000000" />
        <small>0 = Miễn phí.</small>
      </div>
      <div v-else class="field">
        <label class="check"><ToggleSwitch v-model="dialog.isInstallment" />Đây là trả góp</label>
        <small>Chỉ hiện khi giỏ có sản phẩm cho trả góp, khách chọn số tháng và xem tiền trả mỗi tháng.</small>
      </div>
      <label class="check"><ToggleSwitch v-model="dialog.isActive" />Hiện trên site</label>
      <div class="actions" style="justify-content:flex-end">
        <Button label="Hủy" severity="secondary" text @click="dialog = null" />
        <Button type="submit" label="Lưu" icon="pi pi-check" />
      </div>
    </form>
  </Dialog>
</template>
