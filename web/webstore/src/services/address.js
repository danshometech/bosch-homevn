import { api } from '@/services/api'
import { fold } from '@/utils/format'

// Danh mục hành chính VN 2025 (34 tỉnh / thành → phường / xã) lấy qua host: GET /api/address/provinces và
// /api/address/provinces/{code}/wards (Api gọi provinces.open-api.vn, cache 24 giờ). Trình duyệt nhớ thêm 7 ngày;
// lỗi thì ném ra để form chuyển sang nhập tay. `search`: tên bỏ dấu để ô chọn tìm được khi gõ không dấu
const TTL = 7 * 24 * 3600 * 1000
const memo = new Map()
const withSearch = list => list.map(x => ({ ...x, search: fold(x.name) }))

function cached(key, path) {
  if (memo.has(key)) return memo.get(key)
  try {
    const saved = JSON.parse(localStorage.getItem(key))
    if (saved && Date.now() - saved.at < TTL && Array.isArray(saved.data) && saved.data.length) {
      const hit = Promise.resolve(withSearch(saved.data))
      memo.set(key, hit)
      return hit
    }
  } catch { /* private mode / dữ liệu hỏng */ }

  const job = api(path).then(data => {
    try { localStorage.setItem(key, JSON.stringify({ at: Date.now(), data })) } catch { /* storage đầy */ }
    return withSearch(data)
  })
  memo.set(key, job)
  job.catch(() => memo.delete(key))
  return job
}

export const getProvinces = () => cached('bh_addr_p', '/address/provinces')
export const getWards = code => cached(`bh_addr_w${code}`, `/address/provinces/${code}/wards`)
