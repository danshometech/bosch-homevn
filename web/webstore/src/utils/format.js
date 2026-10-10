export const fmt = n => n.toLocaleString('vi-VN') + '₫'
export const fmtDate = d => new Date(d).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' })
// % giảm so với giá gạch; sản phẩm không có giá gạch thì 0
export const pct = p => (p.old > p.price ? Math.round((1 - p.price / p.old) * 100) : 0)
// Thời hạn bảo hành từ DB ("BSH - 3 năm" → "3 năm"); không có thì dùng mức chung 24 tháng
export const warrantyOf = p => p.warranty?.replace(/^BSH\s*-\s*/i, '') || '24 tháng'
export const monthly = (total, months) => Math.round(total / months / 1000) * 1000

// Bỏ dấu tiếng Việt để tìm "may khoan" vẫn ra "Máy khoan"
export const fold = s => s.normalize('NFD').replace(/[̀-ͯ]/g, '').replace(/đ/g, 'd').replace(/Đ/g, 'D').toLowerCase()
export const slugify = s => fold(s).replace(/[^a-z0-9]+/g, '-').replace(/^-+|-+$/g, '')
