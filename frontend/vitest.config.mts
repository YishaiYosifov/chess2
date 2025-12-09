import { configDefaults, defineConfig } from "vitest/config";
import tsconfigPaths from "vite-tsconfig-paths";
import react from "@vitejs/plugin-react";

export default defineConfig({
    plugins: [react(), tsconfigPaths()],
    test: {
        setupFiles: ["vitest.setup.tsx"],
        exclude: [...configDefaults.exclude, "**/e2e-tests/**"],
        environment: "jsdom",
        globals: true,
    },
});
