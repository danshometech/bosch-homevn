// Gọi host BoschHomeVn.Admin qua đường dẫn tương đối /api (lúc dev Vite proxy sang host); controller của host gọi tiếp
// BoschHomeVn.Api. Cùng origin nên cookie đăng nhập (HttpOnly) tự gửi kèm. Lỗi: ném Error, message lấy từ ProblemDetails.
let onUnauthorized = () => {}
// router đăng ký: hết phiên (401) thì về trang đăng nhập
export const setUnauthorizedHandler = fn => (onUnauthorized = fn)

export async function api(method, path, body) {
  const init = { method, headers: { Accept: 'application/json' } }
  if (body instanceof FormData) {
    init.body = body
  } else if (body !== undefined) {
    init.headers['Content-Type'] = 'application/json'
    init.body = JSON.stringify(body)
  }

  const res = await fetch('/api' + path, init)
  if (res.status === 401 && !path.startsWith('/admin/auth/')) onUnauthorized()
  if (!res.ok) {
    let problem = null
    try { problem = await res.json() } catch { /* không có body JSON */ }
    throw Object.assign(new Error(problemMessage(problem, res.status)), { status: res.status })
  }
  return res.status === 204 ? null : res.json()
}

const problemMessage = (p, status) =>
  (p?.errors ? Object.values(p.errors).flat().join(' ') : p?.detail || p?.title) || `Lỗi ${status}`

export const get = path => api('GET', path)
export const post = (path, body) => api('POST', path, body)
export const put = (path, body) => api('PUT', path, body)
export const del = path => api('DELETE', path)

// Tải ảnh lên (folder: products | moments), trả về đường dẫn /media/...
export async function uploadImage(file, folder) {
  const form = new FormData()
  form.append('file', file)
  form.append('folder', folder)
  return (await post('/admin/media', form)).url
}

// Tải video lên (MP4 / WebM ≤ 50 MB), trả về đường dẫn /media/videos/…. Dùng XMLHttpRequest để báo % qua onProgress
// (fetch chưa có tiến độ upload).
export function uploadVideo(file, onProgress) {
  return new Promise((resolve, reject) => {
    const form = new FormData()
    form.append('file', file)
    const xhr = new XMLHttpRequest()
    xhr.open('POST', '/api/admin/media/video')
    xhr.setRequestHeader('Accept', 'application/json')
    xhr.upload.onprogress = e => e.lengthComputable && onProgress?.(Math.round((e.loaded / e.total) * 100))
    xhr.onload = () => {
      let body = null
      try { body = JSON.parse(xhr.responseText) } catch { /* không có body JSON */ }
      if (xhr.status >= 200 && xhr.status < 300) return resolve(body.url)
      if (xhr.status === 401) onUnauthorized()
      reject(Object.assign(new Error(problemMessage(body, xhr.status)), { status: xhr.status }))
    }
    xhr.onerror = () => reject(new Error('Mất kết nối khi tải video lên'))
    xhr.send(form)
  })
}
