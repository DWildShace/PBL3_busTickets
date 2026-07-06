import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import tailwindcss from "@tailwindcss/vite";
import postcssImport from "postcss-import";
import path from "path";

// https://vite.dev/config/
export default defineConfig({
    base: process.env.GITHUB_ACTIONS ? "/PBL3_busTickets/" : "/",
    plugins: [react(), tailwindcss()],
    resolve: {
        alias: {
            "@": path.resolve(__dirname, "src"),
        },
    },
    css: {
        postcss: {
            plugins: [postcssImport()],
        },
    },
});
