// 18990000 → "18.990.000₫"; không có giá → "—"
export const fmt = n => (n == null ? '—' : n.toLocaleString('vi-VN') + '₫')

export const fmtDate = d => (d ? new Date(d).toLocaleString('vi-VN', { dateStyle: 'short', timeStyle: 'short' }) : '—')

// Bỏ dấu tiếng Việt để tìm "may rua bat" vẫn ra "Máy rửa bát"
export const fold = s => (s || '').normalize('NFD').replace(/[̀-ͯ]/g, '').replace(/đ/g, 'd').replace(/Đ/g, 'D').toLowerCase()

export const STOCK = [
  { value: 'InStock', label: 'Còn hàng', severity: 'success' },
  { value: 'LowStock', label: 'Sắp hết', severity: 'warn' },
  { value: 'OutOfStock', label: 'Hết hàng', severity: 'danger' },
]
export const stockOf = value => STOCK.find(s => s.value === value) || STOCK[0]

// % giảm hiện thành nhãn "Giảm X%" trên site — tính từ giá gạch (oldPrice) so với giá bán; không có thì null
export const discountOf = p => (p.price != null && p.oldPrice > p.price ? Math.round((1 - p.price / p.oldPrice) * 100) : null)

// Sản phẩm đang hiện trên site bán hàng: có giá bán và đang bật hiển thị
export const onSale = p => p.price != null && p.isPublished

// Tông màu nền theo giờ của khoảnh khắc trang chủ (màu thật ở web/webstore/src/data/day.js)
export const TONES = {
  dawn: { label: 'Bình minh', color: 'oklch(0.95 0.022 75)' },
  morning: { label: 'Buổi sáng', color: 'oklch(0.965 0.032 92)' },
  day: { label: 'Ban ngày', color: 'oklch(0.965 0.016 220)' },
  noon: { label: 'Buổi trưa', color: 'oklch(0.985 0.004 240)' },
  dusk: { label: 'Chiều tối', color: 'oklch(0.9 0.045 50)' },
  night: { label: 'Ban đêm', color: 'oklch(0.2 0.03 265)' },
}

// Body gửi PUT/POST /api/admin/products từ một sản phẩm (dạng API trả về) — dùng cho sửa nhanh trên bảng
export const toProductRequest = p => ({
  model: p.model,
  name: p.name,
  typeId: p.typeId,
  series: p.series,
  price: p.price,
  oldPrice: p.oldPrice,
  dealerPrice: p.dealerPrice,
  stockStatus: p.stockStatus,
  stockQuantity: p.stockQuantity,
  stockNote: p.stockNote,
  color: p.color,
  origin: p.origin,
  warranty: p.warranty,
  isNew: p.isNew,
  isFlashSale: p.isFlashSale,
  isPublished: p.isPublished,
  imageUrl: p.imageUrl,
  galleryImages: p.galleryImages,
  videoUrl: p.videoUrl,
  videoPosterUrl: p.videoPosterUrl,
  highlights: p.highlights,
  specs: p.specs,
  allowInstallment: p.allowInstallment,
  installmentMonths: p.installmentMonths,
  installmentDisplayMonths: p.installmentDisplayMonths,
})

// Kỳ hạn trả góp admin chọn được cho sản phẩm
export const INSTALLMENT_MONTHS = [3, 6, 9, 12, 18, 24, 36]
export const monthly = (price, months) => Math.round(price / months / 1000) * 1000
