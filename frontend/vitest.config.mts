import tsconfigPaths from "vite-tsconfig-paths";
import { configDefaults, defineConfig } from "vitest/config";
import react from "@vitejs/plugin-react";

export default defineConfig({
    plugins: [react(), tsconfigPaths()],
    test: {
        setupFiles: ["vitest.env.ts", "vitest.setup.tsx"],
        exclude: [...configDefaults.exclude, "**/e2e-tests/**"],
        environment: "jsdom",
        globals: true,
        css: { modules: { classNameStrategy: "non-scoped" } },
    },
});
