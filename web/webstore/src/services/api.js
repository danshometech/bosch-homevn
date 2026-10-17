// Gọi host BoschHomeVn.WebStore qua đường dẫn tương đối /api (lúc dev Vite proxy sang host, vite.config.js);
// controller của host gọi tiếp BoschHomeVn.Api để lấy dữ liệu — Vue không gọi thẳng Api.
// `params`: query string, bỏ qua giá trị rỗng / null / false; mảng thành nhiều tham số cùng tên. Lỗi HTTP ném Error có `status`.
export async function api(path, params) {
  const search = new URLSearchParams()
  for (const [k, v] of Object.entries(params || {})) {
    for (const x of [v].flat()) if (x != null && x !== '' && x !== false) search.append(k, x)
  }
  const qs = search.toString()
  const url = '/api' + path + (qs ? '?' + qs : '')
  const res = await fetch(url, { headers: { Accept: 'application/json' } })
  if (!res.ok) throw Object.assign(new Error(`GET ${url} → ${res.status}`), { status: res.status })
  return res.json()
}

// POST JSON. Lỗi: Error có `status`, `message` lấy từ ProblemDetails của server (thông báo tiếng Việt) nếu có
export const post = (path, body, headers = {}) => send('POST', path, body, headers)

// Gửi JSON hoặc FormData (ảnh) bằng method bất kỳ, lỗi như post
export async function send(method, path, body, headers = {}) {
  const form = body instanceof FormData
  const res = await fetch('/api' + path, {
    method,
    headers: { Accept: 'application/json', ...(body !== undefined && !form ? { 'Content-Type': 'application/json' } : {}), ...headers },
    body: body === undefined || form ? body : JSON.stringify(body),
  })
  if (!res.ok) {
    let p = null
    try { p = await res.json() } catch { /* không có body JSON */ }
    const message = (p?.errors ? Object.values(p.errors).flat().join(' ') : p?.detail || p?.title) || `${method} /api${path} → ${res.status}`
    throw Object.assign(new Error(message), { status: res.status, problem: !!p })
  }
  return res.json()
}
