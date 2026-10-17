<script setup>
import { ref, computed, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useToast } from 'primevue/usetoast'
import Button from 'primevue/button'
import Select from 'primevue/select'
import Tag from 'primevue/tag'
import Textarea from 'primevue/textarea'
import ToggleSwitch from 'primevue/toggleswitch'
import { get, put } from '@/services/api'
import { fmt, fmtDate, ORDER_STATUS, orderStatusOf } from '@/utils/format'
import { useOrderBadgeStore } from '@/stores/orders'

const props = defineProps({ code: { type: String, required: true } })
const router = useRouter()
const toast = useToast()
const badge = useOrderBadgeStore()

const order = ref(null)
const form = ref({ status: 'New', isPaid: false, note: '' })
const noteErr = ref(false)
const saving = ref(false)

const apply = o => {
  order.value = o
  form.value = { status: o.status, isPaid: o.isPaid, note: '' }
  noteErr.value = false
}

watch(() => props.code, async code => {
  try {
    apply(await get(`/admin/orders/${encodeURIComponent(code)}`))
  } catch (err) {
    toast.add({ severity: 'error', summary: 'Không tải được đơn hàng', detail: err.message, life: 5000 })
    router.replace({ name: 'orders' })
  }
}, { immediate: true })

const statusChanged = computed(() => order.value && form.value.status !== order.value.status)
const dirty = computed(() => order.value && (statusChanged.value || form.value.isPaid !== order.value.isPaid || !!form.value.note.trim()))

async function save() {
  if (statusChanged.value && !form.value.note.trim()) {
    noteErr.value = true
    return
  }
  saving.value = true
  try {
    apply(await put(`/admin/orders/${encodeURIComponent(props.code)}`, { ...form.value, note: form.value.note.trim() || null }))
    toast.add({ severity: 'success', summary: 'Đã cập nhật đơn', detail: `${order.value.code} · ${orderStatusOf(order.value.status).label}`, life: 2500 })
    badge.refresh()
  } catch (err) {
    toast.add({ severity: 'error', summary: 'Không lưu được', detail: err.message, life: 6000 })
  } finally {
    saving.value = false
  }
}
</script>

<template>
  <form v-if="order" @submit.prevent="save">
    <div class="page-h">
      <div>
        <RouterLink class="crumb" :to="{ name: 'orders' }">← Đơn hàng</RouterLink>
        <h1 style="display:flex;align-items:center;gap:12px">
          <span class="mono">{{ order.code }}</span>
          <Tag :value="orderStatusOf(order.status).label" :severity="orderStatusOf(order.status).severity" />
        </h1>
        <p class="muted" style="margin:4px 0 0">Đặt lúc {{ fmtDate(order.createdAt) }}</p>
      </div>
    </div>

    <div class="edit-layout">
      <div>
        <section class="box">
          <h2>Sản phẩm ({{ order.items.reduce((s, i) => s + i.quantity, 0) }})</h2>
          <div class="order-lines">
            <div v-for="i in order.items" :key="i.productId" class="order-line">
              <img v-if="i.imageUrl" :src="i.imageUrl" alt="">
              <span v-else class="ph" />
              <div class="name">
                <RouterLink :to="{ name: 'product-edit', params: { id: i.productId } }">{{ i.name }}</RouterLink>
                <small class="muted mono">{{ i.model }}</small>
              </div>
              <span class="muted qty">{{ fmt(i.unitPrice) }} × {{ i.quantity }}</span>
              <b class="sum">{{ fmt(i.lineTotal) }}</b>
            </div>
          </div>
          <div class="order-totals">
            <div><span>Tạm tính</span><span>{{ fmt(order.subtotal) }}</span></div>
            <div><span>Phí giao hàng</span><span>{{ order.shippingFee ? fmt(order.shippingFee) : 'Miễn phí' }}</span></div>
            <div class="tot"><span>Tổng cộng</span><b>{{ fmt(order.total) }}</b></div>
          </div>
        </section>

        <section class="box">
          <h2>Người nhận</h2>
          <dl class="order-info">
            <dt>Họ tên</dt><dd>{{ order.customerName }}</dd>
            <dt>Điện thoại</dt><dd><a :href="'tel:' + order.customerPhone">{{ order.customerPhone }}</a></dd>
            <template v-if="order.customerEmail"><dt>Email</dt><dd><a :href="'mailto:' + order.customerEmail">{{ order.customerEmail }}</a></dd></template>
            <dt>Địa chỉ</dt><dd>{{ order.address }}</dd>
            <template v-if="order.customerNote"><dt>Ghi chú của khách</dt><dd style="white-space:pre-line">{{ order.customerNote }}</dd></template>
          </dl>
        </section>

        <section class="box">
          <h2>Lịch sử xử lý</h2>
          <ol class="timeline">
            <li v-for="(e, i) in order.events" :key="i">
              <div class="tl-h"><b>{{ e.actor || 'Khách hàng' }}</b><span class="muted">{{ fmtDate(e.createdAt) }}</span></div>
              <div class="tl-what">
                <template v-if="!e.actor">Đặt hàng</template>
                <template v-else-if="e.toStatus">
                  <Tag :value="orderStatusOf(e.fromStatus).label" :severity="orderStatusOf(e.fromStatus).severity" />
                  <i class="pi pi-arrow-right" />
                  <Tag :value="orderStatusOf(e.toStatus).label" :severity="orderStatusOf(e.toStatus).severity" />
                </template>
                <Tag v-if="e.isPaid != null" :value="e.isPaid ? 'Đã thu tiền' : 'Bỏ đánh dấu đã thu tiền'" :severity="e.isPaid ? 'success' : 'secondary'" />
                <template v-if="e.actor && !e.toStatus && e.isPaid == null">Ghi chú</template>
              </div>
              <p v-if="e.note" class="tl-note">{{ e.note }}</p>
            </li>
          </ol>
        </section>
      </div>

      <aside>
        <section class="box">
          <h2>Xử lý đơn</h2>
          <div class="rows" style="gap:14px">
            <div class="field">
              <label>Trạng thái</label>
              <Select v-model="form.status" :options="ORDER_STATUS" option-label="label" option-value="value">
                <template #option="{ option }"><Tag :value="option.label" :severity="option.severity" /></template>
              </Select>
            </div>
            <label class="check"><ToggleSwitch v-model="form.isPaid" />Đã thu tiền</label>
            <div class="field">
              <label for="note">Ghi chú<template v-if="statusChanged"> *</template></label>
              <Textarea
                id="note"
                v-model="form.note"
                rows="3"
                auto-resize
                maxlength="1000"
                :invalid="noteErr && !form.note.trim()"
                :placeholder="statusChanged ? 'Lý do / việc đã làm, vd. Đã gọi xác nhận, khách hẹn giao sau 17h' : 'vd. Khách hẹn giao sau 17h'"
                @input="noteErr = false"
              />
              <small v-if="noteErr && !form.note.trim()" style="color:#b91c1c">Đổi trạng thái cần nhập ghi chú.</small>
              <small v-else>{{ statusChanged ? 'Bắt buộc khi đổi trạng thái.' : '' }} Lưu vào lịch sử xử lý, khách không thấy.</small>
            </div>
            <Button type="submit" label="Lưu" icon="pi pi-check" :loading="saving" :disabled="!dirty" />
            <small class="muted">Cập nhật lần cuối {{ fmtDate(order.updatedAt) }}</small>
          </div>
        </section>
        <section class="box">
          <h2>Giao hàng &amp; thanh toán</h2>
          <dl class="order-info">
            <dt>Cách giao</dt><dd>{{ order.shippingMethodName }} · {{ order.shippingFee ? fmt(order.shippingFee) : 'Miễn phí' }}</dd>
            <dt>Thanh toán</dt><dd>{{ order.paymentMethodName }}<template v-if="order.installmentMonths"> · {{ order.installmentMonths }} tháng</template></dd>
          </dl>
        </section>
      </aside>
    </div>
  </form>
</template>
