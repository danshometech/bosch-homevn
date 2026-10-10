<script setup>
import { ref, watch, onMounted, onBeforeUnmount } from 'vue'
import tinymce from 'tinymce'
import 'tinymce/models/dom'
import 'tinymce/themes/silver'
import 'tinymce/icons/default'
import 'tinymce/skins/ui/oxide/skin.js'
import 'tinymce/skins/ui/oxide/content.js'
import 'tinymce/skins/content/default/content.js'
import 'tinymce/plugins/autolink'
import 'tinymce/plugins/autoresize'
import 'tinymce/plugins/code'
import 'tinymce/plugins/fullscreen'
import 'tinymce/plugins/image'
import 'tinymce/plugins/link'
import 'tinymce/plugins/lists'
import 'tinymce/plugins/table'
import 'tinymce/plugins/wordcount'
import 'tinymce-i18n/langs8/vi'
import { uploadImage } from '@/services/api'

// TinyMCE tự host (GPL); ảnh chèn / dán vào được tải lên /media/<folder>
const html = defineModel({ type: String, default: '' })
const props = defineProps({ folder: { type: String, default: 'articles' } })
const el = ref(null)
const ready = ref(false)
let editor = null
let last = ''

const upload = blob => uploadImage(new File([blob.blob()], blob.filename(), { type: blob.blob().type }), props.folder)

onMounted(async () => {
  ;[editor] = await tinymce.init({
    target: el.value,
    license_key: 'gpl',
    language: 'vi',
    menubar: false,
    promotion: false,
    branding: false,
    plugins: 'autolink autoresize code fullscreen image link lists table wordcount',
    toolbar: 'undo redo | blocks | bold italic underline | alignleft aligncenter alignright | bullist numlist | link image table | removeformat code fullscreen',
    block_formats: 'Đoạn văn=p; Tiêu đề 2=h2; Tiêu đề 3=h3',
    min_height: 420,
    max_height: 900,
    relative_urls: false,
    convert_urls: false,
    image_caption: true,
    image_dimensions: false,
    images_upload_handler: upload,
    content_style: 'body{font-family:system-ui,sans-serif;font-size:15px;line-height:1.65;max-width:820px;margin:16px auto} img{max-width:100%;height:auto}',
  })
  if (!editor) return
  last = html.value || ''
  editor.setContent(last)
  editor.on('input change undo redo ExecCommand', () => {
    last = editor.getContent()
    html.value = last
  })
  ready.value = true
})

watch(html, v => {
  if (editor && v !== last) {
    last = v || ''
    editor.setContent(last)
  }
})

onBeforeUnmount(() => editor?.remove())
</script>

<template>
  <div class="rich-editor" :class="{ loading: !ready }">
    <textarea ref="el" />
  </div>
</template>

<style scoped>
.rich-editor.loading { min-height: 420px; border: 1px solid #e5e7eb; border-radius: 10px; background: #f9fafb; }
.rich-editor.loading textarea { visibility: hidden; }
</style>
