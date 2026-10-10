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
