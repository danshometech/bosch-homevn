import { defineStore } from 'pinia'
import { ref } from 'vue'
import { api } from '@/services/api'

// Tin tức: chuyên mục tải một lần (GET /api/news/categories), bài viết mỗi trang tự gọi
export const useNewsStore = defineStore('news', () => {
  const categories = ref([])
  let pending = null

  function loadCategories() {
    pending ||= api('/news/categories')
      .then(list => (categories.value = list))
      .catch(err => {
        pending = null
        throw err
      })
    return pending
  }

  const catOf = id => categories.value.find(c => c.id === id)

  // GET /api/news/posts → { items, total, page, pageSize }
  const fetchPosts = (category, page, pageSize) => api('/news/posts', { category, page, pageSize })

  // GET /api/news/posts/{slug}; null nếu không có / chưa đăng
  async function fetchPost(slug) {
    try {
      return await api('/news/posts/' + encodeURIComponent(slug))
    } catch (err) {
      if (err.status === 404) return null
      throw err
    }
  }

  return { categories, loadCategories, catOf, fetchPosts, fetchPost }
})
