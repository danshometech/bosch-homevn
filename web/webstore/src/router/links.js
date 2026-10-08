import { slugify } from '@/utils/format'

export const toList = cat => (cat ? { name: 'list', params: { cat } } : { name: 'list' })
export const toGroup = (cat, title) => ({ name: 'list', params: { cat }, query: { nhom: slugify(title) } })
export const toSub = (cat, sub) => ({ name: 'list', params: { cat }, query: { loai: slugify(sub) } })
export const toFlash = { name: 'list', query: { flash: '1' } }
export const toDetail = id => ({ name: 'detail', params: { id } })
