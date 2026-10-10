import { defineStore } from 'pinia'
import { ref, reactive } from 'vue'
import { api } from '@/services/api'
import { slugify } from '@/utils/format'

// Danh mục cho menu/bộ lọc: { id, name, short, subs: [tên loại], groups: [{ title, subs }] | null }
// — nhóm (Thiết bị đun nấu…) chỉ có ở danh mục Thiết bị bếp
function toCategory(c) {
  const groups = []
  for (const t of c.types) {
    if (!t.group) continue
    let g = groups.find(x => x.title === t.group)
    if (!g) groups.push((g = { title: t.group, subs: [] }))
    g.subs.push(t.name)
  }
  return { id: c.id, name: c.name, short: c.shortName, subs: c.types.map(t => t.name), groups: groups.length ? groups : null }
}

// Sản phẩm theo dạng các component dùng: `cat` = id danh mục, `sub` = tên loại, `old` = giá gạch, `chips` = 3 thông số
// nổi bật, `stock` = số lượng tồn (có thể không có), `isNew` / `isFlash` = nhãn "Mới" / flash sale (admin bật),
// `inst` = trả góp 0% { months, displayMonths } hoặc null, `images` = ảnh chính + ảnh thêm, `video` = video giới thiệu.
// `specs` đổi thành object, giữ đúng thứ tự dòng.
const toProduct = p => ({
  id: p.id,
  model: p.model,
  name: p.name,
  cat: p.categoryId,
  sub: p.typeName,
  price: p.price,
  old: p.oldPrice,
  isNew: p.isNew,
  isFlash: p.isFlashSale,
  inst: p.installment,
  stock: p.stockQuantity,
  stockStatus: p.stockStatus,
  stockNote: p.stockNote,
  rating: p.rating,
  reviews: p.reviewCount,
  img: p.imageUrl,
  images: [p.imageUrl, ...(p.galleryImages ?? [])].filter(Boolean),
  video: p.videoUrl ?? null,
  videoPoster: p.videoPosterUrl ?? null,
  chips: p.highlights,
  specs: Object.fromEntries(p.specs.map(s => [s.name, s.value])),
  color: p.color,
  origin: p.origin,
  warranty: p.warranty,
})

const toLink = l => ({ label: l.label, to: l.url })

// Menu tải một lần cho mọi trang (GET /api/menu). Sản phẩm thì mỗi trang tự gọi API phần mình cần — trang chủ
// /api/home, danh sách /api/products?…, chi tiết /api/products/{id} — và store giữ lại theo id để giỏ hàng,
// so sánh tra cứu.
export const useCatalogStore = defineStore('catalog', () => {
  const cats = ref([])
  const flashCount = ref(0)
  const failed = ref(false)
  const home = ref(null)
  const cache = reactive(new Map())
  let menuPending = null
  let homePending = null

  const remember = list => list.map(p => {
    const x = toProduct(p)
    cache.set(x.id, x)
    return x
  })

  // Router chờ hàm này trước mỗi lần điều hướng; lỗi thì lần điều hướng sau thử lại
  function loadMenu() {
    menuPending ||= api('/menu')
      .then(m => {
        cats.value = m.categories.map(toCategory)
        flashCount.value = m.flashSaleCount
        failed.value = false
      })
      .catch(err => {
        console.error(err)
        failed.value = true
        menuPending = null
      })
    return menuPending
  }

  // Trang chủ: một lần gọi GET /api/home (khoảnh khắc kèm sản phẩm + tóm tắt flash sale), giữ lại cho lần quay lại
  function loadHome() {
    homePending ||= api('/home')
      .then(h => {
        home.value = {
          moments: h.moments.map(m => ({
            id: m.id,
            time: m.time,
            name: m.name,
            tone: m.tone,
            title: m.title,
            lead: m.lead,
            img: m.imageUrl,
            pos: m.imagePosition,
            still: m.stillImageUrl,
            alt: m.imageAlt,
            fact: m.fact,
            links: m.links.map(toLink),
            products: remember(m.products),
            extras: { title: m.extras.title, items: remember(m.extras.products), more: m.extras.more && toLink(m.extras.more) },
          })),
          flash: h.flashSale && {
            count: h.flashSale.count,
            maxOff: h.flashSale.maxDiscountPercent,
            subs: h.flashSale.types.map(t => t.toLowerCase()).join(', '),
          },
        }
      })
      .catch(err => {
        console.error(err)
        failed.value = true
        home.value = { moments: [], flash: null }
        homePending = null
      })
    return homePending
  }

  // GET /api/products — params: { category, q, flash, take }
  async function fetchProducts(params) {
    return remember(await api('/products', params))
  }

  // GET /api/products/{id}; null nếu không có / chưa bán
  async function fetchProduct(id) {
    try {
      return remember([await api('/products/' + encodeURIComponent(id))])[0]
    } catch (err) {
      if (err.status === 404) return null
      throw err
    }
  }

  // GET /api/products/{id}/article — HTML bài giới thiệu (đã lọc ở server); null nếu chưa có bài
  async function fetchArticle(id) {
    try {
      return (await api('/products/' + encodeURIComponent(id) + '/article')).html
    } catch (err) {
      if (err.status === 404) return null
      throw err
    }
  }

  // Bảo đảm đã có dữ liệu các mã (giỏ hàng): chỉ gọi API cho mã chưa có, trả về những mã còn đang bán
  async function ensure(ids) {
    const missing = ids.filter(id => !cache.has(id))
    for (let i = 0; i < missing.length; i += 50) {
      remember(await api('/products', { ids: missing.slice(i, i + 50).join(',') }))
    }
    return ids.filter(id => cache.has(id))
  }

  const prodOf = id => cache.get(id)
  const catOf = id => cats.value.find(c => c.id === id)
  // Tên loại sản phẩm trong danh mục, tra theo slug trên URL (?loai=bep-tu → 'Bếp từ')
  const subOf = (catId, slug) => catOf(catId)?.subs.find(s => slugify(s) === slug)
  // Nhóm con (vd. "Thiết bị đun nấu") — tra theo slug trên URL (?nhom=...) hoặc theo loại sản phẩm nằm trong nhóm
  const groupOf = (catId, slug) => catOf(catId)?.groups?.find(g => slugify(g.title) === slug)
  const groupOfSub = (catId, sub) => catOf(catId)?.groups?.find(g => g.subs.includes(sub))

  return {
    cats, flashCount, failed, home,
    loadMenu, loadHome, fetchProducts, fetchProduct, fetchArticle, ensure,
    prodOf, catOf, subOf, groupOf, groupOfSub,
  }
})
