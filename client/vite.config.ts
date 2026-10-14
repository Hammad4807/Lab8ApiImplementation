import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    // Always use port 5173, because the API's CORS policy only allows http://localhost:5173
    port: 5173,
    strictPort: true,
  },
})
