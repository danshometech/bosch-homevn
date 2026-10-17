import { put } from '@/services/api'

// Nút lên/xuống: đổi chỗ ngay trên danh sách đang hiện (tìm theo id) rồi lưu cả thứ tự.
// Các lần bấm liên tiếp được gửi lần lượt, nên lần sau không ghi đè lần trước.
export function useReorder(onError) {
  let queue = Promise.resolve()
  return function move(list, id, d, url) {
    const i = list.findIndex(x => x.id === id)
    const j = i + d
    if (i < 0 || j < 0 || j >= list.length) return
    ;[list[i], list[j]] = [list[j], list[i]]
    const ids = list.map(x => x.id)
    queue = queue.then(() => put(url, { ids })).catch(onError)
  }
}
