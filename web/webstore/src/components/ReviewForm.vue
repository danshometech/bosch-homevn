<script setup>
import { ref, reactive, computed, watch, onMounted, onBeforeUnmount } from 'vue'
import { useDrawer } from '@/composables/useDrawer'
import { replay } from '@/utils/motion'
import { shrinkImage } from '@/utils/image'
import { getCaptchaKey } from '@/services/orders'
import { submitReview } from '@/services/reviews'
import AppIcon from './AppIcon.vue'
import TurnstileBox from './TurnstileBox.vue'

const MAX_PHOTOS = 5
const MIN_TEXT = 20
const LABELS = ['', 'Rất tệ', 'Tệ', 'Bình thường', 'Tốt', 'Tuyệt vời']

const open = defineModel('open', { type: Boolean, default: false })
const props = defineProps({ product: { type: Object, required: true } })

const f = reactive({ rating: 0, content: '', name: '', phone: '' })
// Ảnh đã nén: { blob, url } (url = object URL để xem trước)
const photos = ref([])
const err = ref({})
const sendErr = ref('')
const sending = ref(false)
const sent = ref(false)
const reading = ref(false)
const fileInput = ref(null)
const ctrls = {}

const panel = ref(null)
useDrawer(open, panel)

// Chống spam như trang thanh toán: Turnstile chạy lúc bấm Gửi (captchaKey null = shop đang tắt)
const captchaKey = ref(null)
const captcha = ref(null)
onMounted(() => getCaptchaKey().then(k => (captchaKey.value = k)).catch(e => console.error(e)))

const hover = ref(0)
const shown = computed(() => hover.value || f.rating)

function clearErr(k) {
  if (!err.value[k]) return
  const { [k]: _, ...rest } = err.value
  err.value = rest
}

async function addPhotos(e) {
  const files = [...e.target.files].slice(0, MAX_PHOTOS - photos.value.length)
  e.target.value = ''
  reading.value = true
  try {
    for (const file of files) {
      try {
        const blob = await shrinkImage(file)
        photos.value.push({ blob, url: URL.createObjectURL(blob) })
      } catch (ex) {
        console.error(ex)
        sendErr.value = `Không đọc được ảnh ${file.name}, hãy chọn ảnh JPG hoặc PNG.`
      }
    }
  } finally {
    reading.value = false
  }
}
function removePhoto(i) {
  URL.revokeObjectURL(photos.value[i].url)
  photos.value.splice(i, 1)
}

function reset() {
  photos.value.forEach(x => URL.revokeObjectURL(x.url))
  Object.assign(f, { rating: 0, content: '', name: '', phone: '' })
  photos.value = []
  err.value = {}
  sendErr.value = ''
  sent.value = false
}
// Đóng sau khi đã gửi thì lần mở sau là form mới; đóng giữa chừng thì giữ nguyên nội dung đang viết
watch(open, v => !v && sent.value && reset())
onBeforeUnmount(() => photos.value.forEach(x => URL.revokeObjectURL(x.url)))

function validate() {
  const e = {}
  if (!f.rating) e.rating = 'Vui lòng chọn số sao'
  if (f.content.trim().length < MIN_TEXT) e.content = `Viết tối thiểu ${MIN_TEXT} ký tự để người mua sau tham khảo được`
  if (!f.name.trim()) e.name = 'Vui lòng nhập họ tên'
  if (!/^0\d{9}$/.test(f.phone.replace(/[\s.-]/g, ''))) e.phone = 'Số điện thoại gồm 10 số, bắt đầu bằng 0'
  err.value = e
  Object.keys(e).forEach(k => replay(ctrls[k], 'shake'))
  return !Object.keys(e).length
}

async function submit() {
  if (sending.value || reading.value) return
  sendErr.value = ''
  if (!validate()) return
  sending.value = true
  let token = null
  if (captchaKey.value) {
    try {
      token = await captcha.value.getToken()
    } catch (e) {
      console.error(e)
      sendErr.value = 'Chưa xác minh được bạn không phải robot, vui lòng bấm Gửi đánh giá lần nữa.'
      sending.value = false
      return
    }
  }
  try {
    await submitReview(props.product.id, {
      rating: f.rating,
      content: f.content.trim(),
      name: f.name.trim(),
      phone: f.phone.replace(/[\s.-]/g, ''),
      photos: photos.value.map(x => x.blob),
    }, token)
    sent.value = true
  } catch (e) {
    console.error(e)
    sendErr.value = e.problem ? e.message : 'Không gửi được đánh giá, vui lòng kiểm tra kết nối rồi thử lại.'
  } finally {
    sending.value = false
  }
}
</script>

<template>
  <Transition name="modal" :duration="{ enter: 260, leave: 180 }">
    <div v-if="open" class="modal-bg" @click="open = false">
      <div ref="panel" class="modal rv-modal" role="dialog" aria-modal="true" aria-labelledby="rvf-h" @click.stop>
        <div class="rv-modal-h">
          <h2 id="rvf-h">Viết đánh giá</h2>
          <button class="drawer-x" type="button" aria-label="Đóng" data-autofocus @click="open = false"><AppIcon name="close" /></button>
        </div>

        <form v-if="!sent" class="rv-form" novalidate @submit.prevent="submit">
          <div class="rv-form-body">
            <div class="rv-prod">
              <img v-if="product.img" :src="product.img" alt="">
              <div><b>{{ product.name }}</b><small>Mã: <span class="mono">{{ product.model }}</span></small></div>
            </div>

            <fieldset :ref="el => (ctrls.rating = el)" class="rv-rate" @mouseleave="hover = 0">
              <legend>Bạn chấm sản phẩm này mấy sao?</legend>
              <div class="rv-rate-stars">
                <button
                  v-for="n in 5"
                  :key="n"
                  type="button"
                  :class="{ on: n <= shown }"
                  :aria-label="`${n} sao: ${LABELS[n]}`"
                  :aria-pressed="n === f.rating"
                  @mouseenter="hover = n"
                  @click="f.rating = n; clearErr('rating')"
                ><AppIcon name="star" :size="36" /></button>
              </div>
              <span class="rv-rate-label" :class="{ e: err.rating && !shown }" aria-live="polite">{{ shown ? LABELS[shown] : err.rating || 'Chạm vào sao để chấm điểm' }}</span>
            </fieldset>

            <label :ref="el => (ctrls.content = el)" class="fld" :class="{ err: err.content }">
              Cảm nhận của bạn
              <textarea
                v-model="f.content"
                maxlength="1000"
                rows="5"
                placeholder="Sản phẩm dùng thế nào? Giao hàng, lắp đặt ra sao? Chia sẻ để người mua sau tham khảo nhé."
                :aria-invalid="!!err.content"
                @input="clearErr('content')"
              />
              <span class="rv-count">
                <Transition name="err" mode="out-in">
                  <span v-if="err.content" class="e">{{ err.content }}</span>
                  <span v-else>Tối thiểu {{ MIN_TEXT }} ký tự</span>
                </Transition>
                <span>{{ f.content.length }}/1000</span>
              </span>
            </label>

            <div class="rv-photos-f">
              <span class="rv-photos-h">Thêm ảnh thực tế <span class="muted">(không bắt buộc)</span></span>
              <div class="rv-photos-row">
                <div v-for="(x, i) in photos" :key="x.url" class="rv-photo-x">
                  <img :src="x.url" :alt="`Ảnh ${i + 1}`">
                  <button type="button" :aria-label="`Bỏ ảnh ${i + 1}`" @click="removePhoto(i)"><AppIcon name="close" :size="12" /></button>
                </div>
                <button v-if="photos.length < MAX_PHOTOS" type="button" class="rv-photo-add" :disabled="reading" @click="fileInput.click()">
                  <AppIcon :name="reading ? 'spinner' : 'plus'" :size="20" :class="{ spin: reading }" />{{ reading ? 'Đang xử lý' : 'Thêm ảnh' }}
                </button>
                <small class="muted">Tối đa {{ MAX_PHOTOS }} ảnh</small>
              </div>
              <input ref="fileInput" type="file" accept="image/jpeg,image/png,image/webp" multiple hidden @change="addPhotos">
            </div>

            <div class="form-g">
              <label :ref="el => (ctrls.name = el)" class="fld" :class="{ err: err.name }">
                Họ và tên
                <input v-model="f.name" maxlength="100" autocomplete="name" placeholder="Nguyễn Văn A" :aria-invalid="!!err.name" @input="clearErr('name')">
                <Transition name="err"><span v-if="err.name" class="e">{{ err.name }}</span></Transition>
              </label>
              <label :ref="el => (ctrls.phone = el)" class="fld" :class="{ err: err.phone }">
                Số điện thoại
                <input v-model="f.phone" maxlength="20" type="tel" inputmode="tel" autocomplete="tel" placeholder="09xx xxx xxx" :aria-invalid="!!err.phone" @input="clearErr('phone')">
                <Transition name="err"><span v-if="err.phone" class="e">{{ err.phone }}</span></Transition>
              </label>
            </div>
            <p class="rv-privacy muted">
              Số điện thoại chỉ dùng để đối chiếu đơn hàng, không hiển thị công khai. Nếu trùng số đã đặt mua sản phẩm này,
              đánh giá sẽ có nhãn “Đã mua hàng”.
            </p>

            <TurnstileBox v-if="captchaKey" ref="captcha" :site-key="captchaKey" />
            <Transition name="err">
              <p v-if="sendErr" class="order-err" role="alert"><AppIcon name="info" :size="18" />{{ sendErr }}</p>
            </Transition>
          </div>

          <div class="rv-form-f">
            <span class="muted">Đánh giá sẽ hiển thị sau khi cửa hàng duyệt.</span>
            <button class="btn btn-ghost" type="button" @click="open = false">Hủy</button>
            <button class="btn btn-primary" type="submit" :disabled="sending || reading">
              <AppIcon v-if="sending" name="spinner" :size="18" class="spin" />{{ sending ? 'Đang gửi' : 'Gửi đánh giá' }}
            </button>
          </div>
        </form>

        <div v-else class="rv-sent">
          <span class="rv-sent-ic"><AppIcon name="check" :size="30" class="draw" /></span>
          <h3>Cảm ơn bạn đã đánh giá!</h3>
          <p>Đánh giá của bạn đã được gửi và sẽ hiển thị trên trang sản phẩm sau khi cửa hàng duyệt.</p>
          <button class="btn btn-ghost" type="button" @click="open = false">Đóng</button>
        </div>
      </div>
    </div>
  </Transition>
</template>
