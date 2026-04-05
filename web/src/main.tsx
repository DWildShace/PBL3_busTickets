import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import "./main.css";
import App from "./App.tsx";
import ThemeProvider from "./controller/ThemeProvider.tsx";
import "@/locales/i18n";
import { configure } from "mobx";
import { BrowserRouter } from "react-router-dom";
import { client } from "./api/client.gen.ts";
import { getApiPing } from "./api/sdk.gen.ts";

configure({
    enforceActions: "always",
});

client.setConfig({
    baseUrl: import.meta.env.VITE_API_BASE_URL || "http://localhost:5026",
});

createRoot(document.getElementById("root")!).render(
    <StrictMode>
        <BrowserRouter>
            <ThemeProvider>
                <App />
            </ThemeProvider>
        </BrowserRouter>
    </StrictMode>,
);

// testing
(async () => {
    try {
        const time = Date.now();
        const r = await getApiPing();
        console.log("API Ping Response:", r.data, r);
        console.log("API Pong Time:", Date.now() - time + "ms");
    } catch (error) {
        console.error("API Ping Error:", error);
    }
})();
