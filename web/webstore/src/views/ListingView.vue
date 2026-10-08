<script setup>
import { ref, computed, watch, onMounted, onUnmounted } from 'vue'
import { useCatalogStore } from '@/stores/catalog'
import { pct } from '@/utils/format'
import { toList, toGroup } from '@/router/links'
import { useDrawer } from '@/composables/useDrawer'
import Select from 'primevue/select'
import Tree from 'primevue/tree'
import AppIcon from '@/components/AppIcon.vue'
import ProductCard from '@/components/ProductCard.vue'
import ProductCardSkeleton from '@/components/ProductCardSkeleton.vue'

const props = defineProps({
  cat: { type: String, default: null },
  nhom: { type: String, default: '' },
  loai: { type: String, default: '' },
  q: { type: String, default: '' },
  flash: { type: Boolean, default: false },
})
const catalog = useCatalogStore()

const PRICES = [
  { k: 'all', label: 'Tất cả', min: 0, max: Infinity },
  { k: 'a', label: 'Dưới 3 triệu', min: 0, max: 3e6 },
  { k: 'b', label: '3 – 10 triệu', min: 3e6, max: 10e6 },
  { k: 'c', label: '10 – 20 triệu', min: 10e6, max: 20e6 },
  { k: 'd', label: 'Trên 20 triệu', min: 20e6, max: Infinity },
]
const RATINGS = [[0, 'Tất cả'], [4.7, 'Từ 4.7 ★'], [4.8, 'Từ 4.8 ★']]
const SORTS = {
  pop: (a, b) => b.reviews - a.reviews,
  off: (a, b) => pct(b) - pct(a),
  asc: (a, b) => a.price - b.price,
  desc: (a, b) => b.price - a.price,
}
const SORT_OPTS = [
  { value: 'pop', label: 'Phổ biến nhất' },
  { value: 'off', label: 'Giảm nhiều nhất' },
  { value: 'asc', label: 'Giá thấp → cao' },
  { value: 'desc', label: 'Giá cao → thấp' },
]
// Ô sắp xếp (PrimeVue Select, unstyled): khung như ô nhập, danh sách sổ xuống dùng lại hiệu ứng menu của header
const SELECT_PT = {
  root: ({ state }) => ({ class: ['sel', { open: state.overlayVisible }] }),
  label: { class: 'sel-label' },
  dropdown: { class: 'sel-icon' },
  overlay: { class: 'sel-panel' },
  list: { class: 'sel-list' },
  option: ({ context }) => ({ class: ['sel-opt', { on: context.selected, focus: context.focused }] }),
  transition: { name: 'menu' },
}

const kind = ref('') // '' = tất cả · 'g:<nhóm>' = cả nhóm · 's:<loại>' = một loại
const price = ref('all')
const minR = ref(0)
const sort = ref('pop')

// Màn ≤1080 (khớp CSS): bộ lọc là ngăn kéo trượt từ phải, mở bằng nút "Bộ lọc"
const MD = '(max-width:1080px)'
const md = ref(window.matchMedia(MD).matches)
const open = ref(false)
const filtersEl = ref(null)
useDrawer(open, filtersEl)
let mq = null
function onMedia(e) {
  md.value = e.matches
  if (!e.matches) open.value = false
}
onMounted(() => {
  mq = window.matchMedia(MD)
  mq.addEventListener('change', onMedia)
})
onUnmounted(() => mq?.removeEventListener('change', onMedia))

const activeCount = computed(() => (kind.value ? 1 : 0) + (price.value !== 'all' ? 1 : 0) + (minR.value ? 1 : 0))
function resetFilters() {
  kind.value = ''
  price.value = 'all'
  minR.value = 0
}
// Nhóm đang mở trong cây lọc ({ 'g:<nhóm>': true }), mỗi lúc chỉ một nhóm: vào trang thì thu gọn hết,
// chọn một dòng ngoài cùng thì chỉ mở dòng đó (nếu là nhóm) và đóng nhóm khác; chọn loại con thì giữ nguyên.
const expanded = ref({})
watch([() => props.cat, () => props.nhom, () => props.loai, () => props.q, () => props.flash], () => {
  kind.value = ''
  price.value = 'all'
  minR.value = 0
  expanded.value = {}
  open.value = false
})
watch(kind, k => {
  const node = nodes.value.find(n => n.key === (k || 'all'))
  if (node) expanded.value = node.children ? { [k]: true } : {}
})
// v-model cho Tree. Tree sửa thẳng vào object nó nhận rồi mới báo ra, nên đưa nó bản sao để còn so với trạng thái
// cũ: có nhóm vừa mở (bằng mũi tên / phím →) thì chỉ giữ nhóm đó.
const expandedKeys = computed({
  get: () => ({ ...expanded.value }),
  set: keys => {
    const opened = Object.keys(keys).find(k => !expanded.value[k])
    expanded.value = opened ? { [opened]: true } : keys
  },
})

const c = computed(() => props.cat && catalog.catOf(props.cat))
const sub = computed(() => catalog.subOf(props.cat, props.loai))
// Trang loại: nhóm chứa loại đó (để hiện trên breadcrumb); trang nhóm: nhóm theo ?nhom=
const group = computed(() => (sub.value ? catalog.groupOfSub(props.cat, sub.value) : catalog.groupOf(props.cat, props.nhom)))
// Sản phẩm của phạm vi đang xem (danh mục / từ khóa / flash sale) lấy từ GET /api/products mỗi khi phạm vi đổi
// (tìm không dấu làm ở server); loại / nhóm trên URL, bộ lọc giá, đánh giá và sắp xếp làm ngay trên trình duyệt
const products = ref([])
const loading = ref(true)
const failed = ref(false)
let request = 0
watch([() => props.cat, () => props.q, () => props.flash], async ([cat, q, flash]) => {
  const id = ++request // bỏ kết quả của lần gọi cũ nếu người dùng đã chuyển trang khác
  loading.value = true
  failed.value = false
  try {
    const list = await catalog.fetchProducts({ category: cat, q, flash })
    if (id === request) products.value = list
  } catch (err) {
    console.error(err)
    if (id === request) {
      products.value = []
      failed.value = true
    }
  } finally {
    if (id === request) loading.value = false
  }
}, { immediate: true })
const base = computed(() => products.value.filter(p => (sub.value ? p.sub === sub.value : !group.value || group.value.subs.includes(p.sub))))
// Bộ lọc "Loại sản phẩm" dạng cây: theo nhóm con của danh mục (Thiết bị đun nấu…),
// hoặc theo danh mục khi đang xem tất cả / tìm kiếm / flash sale. Chỉ hiện loại đang có sản phẩm.
const tree = computed(() => {
  const counts = {}
  base.value.forEach(p => (counts[p.sub] = (counts[p.sub] || 0) + 1))
  const node = (key, label, list) => {
    const opts = list.filter(s => counts[s]).map(s => ({ s, n: counts[s] }))
    return { key, label, opts, n: opts.reduce((a, x) => a + x.n, 0) }
  }
  const nodes = c.value?.groups ? c.value.groups.map(g => node(g.title, g.title, g.subs))
    : c.value ? [node(c.value.id, c.value.name, c.value.subs)]
    : catalog.cats.map(x => node(x.id, x.name, x.subs))
  return nodes.filter(x => x.opts.length)
})
const optCount = computed(() => tree.value.reduce((a, x) => a + x.opts.length, 0))
// Dữ liệu cho PrimeVue Tree; key trùng giá trị của `kind` ('all' thay cho ''). Có một nhóm thì chỉ liệt kê loại;
// nhóm chỉ có một loại thì hiện như một dòng thường, không lặp lại loại đó bên trong.
const nodes = computed(() => {
  const leaf = (key, label, n, grp = false) => ({ key, label, data: { n, grp } })
  const rest = tree.value.length > 1
    ? tree.value.map(g => g.opts.length > 1
      ? { ...leaf('g:' + g.key, g.label, g.n, true), children: g.opts.map(x => leaf('s:' + x.s, x.s, x.n)) }
      : leaf('g:' + g.key, g.label, g.n, true))
    : (tree.value[0]?.opts || []).map(x => leaf('s:' + x.s, x.s, x.n))
  return [leaf('all', 'Tất cả', base.value.length), ...rest]
})
// Chọn một như radio: bấm lại mục đang chọn thì Tree gửi {} (bỏ chọn) — bỏ qua để giữ nguyên lựa chọn
const selection = computed({
  get: () => ({ [kind.value || 'all']: true }),
  set: keys => {
    const k = Object.keys(keys)[0]
    if (k) kind.value = k === 'all' ? '' : k
  },
})
// ...và nếu đó là nhóm có mục con thì đóng/mở nhóm, giống bấm mũi tên
function reclick(node) {
  if (node.children) expanded.value = expanded.value[node.key] ? {} : { [node.key]: true }
}
// PrimeVue chạy unstyled (main.js): gắn class của site vào từng phần của Tree, CSS ở khối "cây lọc" trong styles.css
const TREE_PT = {
  rootChildren: { class: 'f-tree' },
  node: ({ context }) => ({ class: 'f-tree-item', 'aria-label': `${context.node.label}, ${context.node.data.n} sản phẩm` }),
  // part: nhóm đang chứa loại được chọn (chọn dở dang)
  nodeContent: ({ context }) => ({
    class: ['f-node', { grp: context.node.data.grp, on: context.selected, part: context.node.children?.some(x => x.key === kind.value) }],
  }),
  nodeToggleButton: ({ context }) => ({ class: ['f-tog', { shut: !context.expanded }], 'aria-hidden': 'true' }),
  nodeIcon: { class: 'f-tree-icon' }, // PrimeVue 4 luôn render chỗ đặt icon, kể cả khi node không có icon
  nodeLabel: { class: 'f-label' },
  nodeChildren: { class: 'f-tree f-tree-sub' },
}
const kindSubs = computed(() => {
  const [type, key] = [kind.value.slice(0, 2), kind.value.slice(2)]
  if (type === 'g:') return tree.value.find(n => n.key === key)?.opts.map(x => x.s) || []
  return type === 's:' ? [key] : null
})

const items = computed(() => {
  const r = PRICES.find(x => x.k === price.value)
  return base.value
    .filter(p => (!kindSubs.value || kindSubs.value.includes(p.sub)) && p.price >= r.min && p.price < r.max && (p.rating ?? 0) >= minR.value)
    .sort(SORTS[sort.value])
})
const title = computed(() => props.flash ? 'Flash sale' : props.q ? `Kết quả cho "${props.q}"`
  : sub.value || group.value?.title || (c.value ? c.value.name : 'Tất cả sản phẩm'))
</script>

<template>
  <div class="wrap">
    <div class="crumb">
      <RouterLink to="/">Trang chủ</RouterLink>/
      <template v-if="sub || group"><RouterLink :to="toList(cat)">{{ c.name }}</RouterLink>/</template>
      <template v-if="sub && group"><RouterLink :to="toGroup(cat, group.title)">{{ group.title }}</RouterLink>/</template>
      <span style="color:var(--ink)">{{ title }}</span>
    </div>
    <div class="pills">
      <RouterLink class="pill" :class="{ on: !cat && !flash && !q }" :to="toList()">Tất cả</RouterLink>
      <RouterLink v-for="x in catalog.cats" :key="x.id" class="pill" :class="{ on: cat === x.id }" :to="toList(x.id)">{{ x.short }}</RouterLink>
    </div>
    <div class="list">
      <Transition name="fade">
        <div v-if="md && open" class="filters-mask" @click="open = false" />
      </Transition>
      <aside
        ref="filtersEl"
        class="filters"
        :class="{ open }"
        :role="md ? 'dialog' : undefined"
        :aria-modal="md ? 'true' : undefined"
        aria-label="Bộ lọc"
      >
        <div class="drawer-h filters-h">
          <b>Bộ lọc</b>
          <button class="drawer-x" type="button" aria-label="Đóng bộ lọc" data-autofocus @click="open = false"><AppIcon name="close" /></button>
        </div>
        <div class="filters-body">
          <div v-if="optCount > 1" class="f-g">
            <h6 id="f-kind">Loại sản phẩm</h6>
            <Tree
              v-model:selectionKeys="selection"
              v-model:expandedKeys="expandedKeys"
              :value="nodes"
              selection-mode="single"
              aria-labelledby="f-kind"
              :pt="TREE_PT"
              @node-unselect="reclick"
            >
              <template #default="{ node }">
                <span class="f-radio" aria-hidden="true" />{{ node.label }}<small>{{ node.data.n }}</small>
              </template>
              <template #nodetoggleicon>
                <AppIcon name="chevron" :size="14" />
              </template>
            </Tree>
          </div>
          <div class="f-g">
            <h6>Khoảng giá</h6>
            <label v-for="r in PRICES" :key="r.k" class="chk"><input v-model="price" type="radio" name="price" :value="r.k">{{ r.label }}</label>
          </div>
          <div class="f-g">
            <h6>Đánh giá</h6>
            <label v-for="[k, l] in RATINGS" :key="k" class="chk"><input v-model="minR" type="radio" name="rating" :value="k">{{ l }}</label>
          </div>
        </div>
        <div class="drawer-f filters-f">
          <button class="btn btn-ghost" type="button" :disabled="!activeCount" @click="resetFilters">Xóa lọc</button>
          <button class="btn btn-primary" type="button" @click="open = false">Xem {{ items.length }} sản phẩm</button>
        </div>
      </aside>
      <div>
        <div class="toolbar">
          <div><h1>{{ title }}</h1><span class="muted" :style="{ visibility: loading ? 'hidden' : null }">{{ items.length }} sản phẩm</span></div>
          <div style="display:flex;gap:8px">
            <button class="btn btn-ghost btn-sm f-btn" type="button" aria-haspopup="dialog" :aria-expanded="open" @click="open = true">
              <AppIcon name="filter" :size="16" />Bộ lọc<span v-if="activeCount" class="f-count">{{ activeCount }}</span>
            </button>
            <Select v-model="sort" :options="SORT_OPTS" option-label="label" option-value="value" aria-label="Sắp xếp" :pt="SELECT_PT">
              <template #dropdownicon><AppIcon name="chevron" :size="16" /></template>
              <template #option="{ option, selected }">{{ option.label }}<AppIcon v-if="selected" name="check" :size="16" /></template>
            </Select>
          </div>
        </div>
        <div v-if="loading" class="grid" aria-busy="true" aria-label="Đang tải sản phẩm">
          <ProductCardSkeleton v-for="i in 8" :key="i" />
        </div>
        <div v-else-if="failed" class="box empty">
          <h3>Không tải được sản phẩm</h3>
          <p class="muted" style="margin-top:8px">Vui lòng thử lại sau ít phút hoặc gọi hotline 1900 6868.</p>
        </div>
        <div v-else-if="items.length" class="grid">
          <ProductCard v-for="p in items" :key="p.id" :p="p" :flash="flash" />
        </div>
        <div v-else-if="cat && !q && !base.length" class="box empty">
          <h3>Danh mục đang cập nhật sản phẩm</h3>
          <p class="muted" style="margin:8px 0 20px">Gọi hotline 1900 6868 để được tư vấn và báo giá {{ sub || group?.title || c.name }}.</p>
          <RouterLink class="btn btn-dark" :to="toList()">Xem tất cả sản phẩm</RouterLink>
        </div>
        <div v-else class="box empty">
          <h3>Không tìm thấy sản phẩm phù hợp</h3>
          <p class="muted" style="margin:8px 0 20px">Thử bỏ bớt bộ lọc hoặc tìm với từ khóa khác.</p>
          <RouterLink class="btn btn-dark" :to="toList()">Xem tất cả sản phẩm</RouterLink>
        </div>
      </div>
    </div>
  </div>
</template>
