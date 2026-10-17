import { fileURLToPath, URL } from 'node:url'
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: { '@': fileURLToPath(new URL('./src', import.meta.url)) },
  },
  server: {
    port: 5173,
    proxy: { '/api': 'http://localhost:5081', '/media': 'http://localhost:5081' },
  },
  build: {
    outDir: fileURLToPath(new URL('../../src/BoschHomeVn.WebStore/wwwroot', import.meta.url)),
    emptyOutDir: true,
  },
})
