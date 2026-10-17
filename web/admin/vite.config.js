import { fileURLToPath, URL } from 'node:url'
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
  plugins: [vue()],
  publicDir: fileURLToPath(new URL('../webstore/public', import.meta.url)),
  resolve: {
    alias: { '@': fileURLToPath(new URL('./src', import.meta.url)) },
  },
  server: {
    port: 5174,
    proxy: { '/api': 'http://localhost:5082', '/media': 'http://localhost:5082' },
  },
  build: {
    outDir: fileURLToPath(new URL('../../src/BoschHomeVn.Admin/wwwroot', import.meta.url)),
    emptyOutDir: true,
  },
})
