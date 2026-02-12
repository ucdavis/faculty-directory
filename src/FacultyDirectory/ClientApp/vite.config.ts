import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import fs from 'fs';
import path from 'path';
import { homedir } from 'os';

// Get the backend URL from environment variables (set by ASP.NET Core SpaProxy)
const target = process.env.ASPNETCORE_HTTPS_PORT
  ? `https://localhost:${process.env.ASPNETCORE_HTTPS_PORT}`
  : process.env.ASPNETCORE_URLS
  ? process.env.ASPNETCORE_URLS.split(';')[0]
  : 'https://localhost:5001';

// Use ASP.NET Core development certificate
const baseFolder =
  process.env.APPDATA !== undefined && process.env.APPDATA !== ''
    ? `${process.env.APPDATA}/ASP.NET/https`
    : `${homedir()}/.aspnet/https`;

const certificateName = 'FacultyDirectory';
const certFilePath = path.join(baseFolder, `${certificateName}.pem`);
const keyFilePath = path.join(baseFolder, `${certificateName}.key`);

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  base: '/',
  resolve: {
    alias: {
      // Force reactstrap to use the ESM build
      'reactstrap': 'reactstrap/es',
    },
  },
  optimizeDeps: {
    include: ['reactstrap', 'opus-recorder'],
    esbuildOptions: {
      // Needed for proper handling of reactstrap's dependencies
      mainFields: ['module', 'main'],
    },
  },
  server: {
    port: 54921,
    host: true,
    https: {
      cert: fs.readFileSync(certFilePath),
      key: fs.readFileSync(keyFilePath),
    },
    proxy: {
      '^/api': {
        target: target,
        changeOrigin: true,
        secure: false,
        rewrite: (path) => path,
      },
      '/account/login': {
        target: target,
        changeOrigin: true,
        secure: false,
      },
      '/account/info': {
        target: target,
        changeOrigin: true,
        secure: false,
      },
      '/signin-oidc': {
        target: target,
        changeOrigin: true,
        secure: false,
      },
      '/signout-oidc': {
        target: target,
        changeOrigin: true,
        secure: false,
      },
      '/signout-callback-oidc': {
        target: target,
        changeOrigin: true,
        secure: false,
      },
    },
  },
  build: {
    outDir: 'dist',
    emptyOutDir: true,
  },
});
