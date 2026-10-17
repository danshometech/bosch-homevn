import { api, send } from './api'

// GET /api/products/{id}/reviews — { summary: { average, total, stars: { 1..5 }, withPhotos, verified }, items, page, pageSize, total }.
// params: { stars, photos, verified, sort: new | helpful | high | low, page, pageSize }
export const getReviews = (productId, params) => api(`/products/${encodeURIComponent(productId)}/reviews`, params)

// POST multipart: rating, content, name, phone, photos[] (kèm token Turnstile) — đánh giá chờ cửa hàng duyệt
export function submitReview(productId, { rating, content, name, phone, photos }, captchaToken) {
  const form = new FormData()
  form.append('rating', rating)
  form.append('content', content)
  form.append('name', name)
  form.append('phone', phone)
  photos.forEach((blob, i) => form.append('photos', blob, `anh-${i + 1}.jpg`))
  return send('POST', `/products/${encodeURIComponent(productId)}/reviews`, form, captchaToken ? { 'X-Captcha-Token': captchaToken } : {})
}

// Bấm / bỏ "Hữu ích" — trả { helpfulCount, voted }. Mỗi trình duyệt tính một lượt (cookie bh_vid do host cấp);
// `voted` của mỗi đánh giá có sẵn trong kết quả getReviews
export const markHelpful = (reviewId, on) => send(on ? 'POST' : 'DELETE', `/reviews/${reviewId}/helpful`)
