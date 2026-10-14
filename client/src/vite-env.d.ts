/// <reference types="vite/client" />

// Tells TypeScript which VITE_ variables exist in .env files
interface ImportMetaEnv {
  readonly VITE_API_URL?: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
