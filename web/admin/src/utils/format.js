export const fmt = n => (n == null ? '—' : n.toLocaleString('vi-VN') + '₫')

export const fmtDate = d => (d ? new Date(d).toLocaleString('vi-VN', { dateStyle: 'short', timeStyle: 'short', timeZone: 'Asia/Ho_Chi_Minh' }) : '—')

export const isoDate = d => `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
export const addDays = (d, n) => new Date(d.getFullYear(), d.getMonth(), d.getDate() + n)
export const startOfToday = () => {
  const vn = new Date(Date.now() + 7 * 3600_000)
  return new Date(vn.getUTCFullYear(), vn.getUTCMonth(), vn.getUTCDate())
}

export const fold = s => (s || '').normalize('NFD').replace(/[̀-ͯ]/g, '').replace(/đ/g, 'd').replace(/Đ/g, 'D').toLowerCase()

export const STOCK = [
  { value: 'InStock', label: 'Còn hàng', severity: 'success' },
  { value: 'LowStock', label: 'Sắp hết', severity: 'warn' },
  { value: 'OutOfStock', label: 'Hết hàng', severity: 'danger' },
]
export const stockOf = value => STOCK.find(s => s.value === value) || STOCK[0]

export const ORDER_STATUS = [
  { value: 'New', label: 'Mới', severity: 'warn' },
  { value: 'Confirmed', label: 'Đã xác nhận', severity: 'info' },
  { value: 'Shipping', label: 'Đang giao', severity: 'contrast' },
  { value: 'Completed', label: 'Hoàn tất', severity: 'success' },
  { value: 'Cancelled', label: 'Đã hủy', severity: 'danger' },
]
export const orderStatusOf = value => ORDER_STATUS.find(s => s.value === value) || ORDER_STATUS[0]

export const REVIEW_STATUS = [
  { value: 'Pending', label: 'Chờ duyệt', severity: 'warn' },
  { value: 'Approved', label: 'Đang hiện', severity: 'success' },
  { value: 'Hidden', label: 'Đã ẩn', severity: 'secondary' },
]
export const reviewStatusOf = value => REVIEW_STATUS.find(s => s.value === value) || REVIEW_STATUS[0]
// Nguồn đánh giá: khách gửi trên site, admin nhập, hoặc dữ liệu mẫu (seed, chỉ có trên máy dev)
export const reviewSourceOf = r => (r.createdBy === 'seed' ? 'Dữ liệu mẫu' : r.createdBy ? `Admin nhập (${r.createdBy})` : 'Khách gửi trên site')

export const discountOf =p => (p.price != null && p.oldPrice > p.price ? Math.round((1 - p.price / p.oldPrice) * 100) : null)

export const onSale = p => p.price != null && p.isPublished

export const TONES = {
  dawn: { label: 'Bình minh', color: 'oklch(0.95 0.022 75)' },
  morning: { label: 'Buổi sáng', color: 'oklch(0.965 0.032 92)' },
  day: { label: 'Ban ngày', color: 'oklch(0.965 0.016 220)' },
  noon: { label: 'Buổi trưa', color: 'oklch(0.985 0.004 240)' },
  dusk: { label: 'Chiều tối', color: 'oklch(0.9 0.045 50)' },
  night: { label: 'Ban đêm', color: 'oklch(0.2 0.03 265)' },
}

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
  isBestSeller: p.isBestSeller,
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
  soldCount: p.soldCount,
})

export const INSTALLMENT_MONTHS = [3, 6, 9, 12, 18, 24, 36]
export const monthly = (price, months) => Math.round(price / months / 1000) * 1000
