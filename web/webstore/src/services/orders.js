import { api, post } from './api'

// Site key của ô xác minh Cloudflare Turnstile; null = shop đang tắt chống spam
export const getCaptchaKey = async () => (await api('/orders/captcha')).siteKey

// POST /api/orders (kèm token Turnstile) — trả { code, total }. Server kiểm tra lại giá / tồn kho / cách giao,
// sai thì ném Error kèm thông báo
export const createOrder = (order, captchaToken) =>
  post('/orders', order, captchaToken ? { 'X-Captcha-Token': captchaToken } : {})
