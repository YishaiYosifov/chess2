import { defineConfig, defaultPlugins } from "@hey-api/openapi-ts";

export default defineConfig({
    input: "https://localhost:7266/openapi/v1.json",
    output: {
        path: "./src/lib/apiClient/definition",
        format: "prettier",
    },
    logs: { file: false },
    plugins: [
        ...defaultPlugins,
        {
            name: "@hey-api/client-next",
            runtimeConfigPath: "@/lib/apiClient/apiClientConfig",
        },
        {
            name: "@hey-api/typescript",
            enums: {
                mode: "typescript",
            },
        },
    ],
    parser: {
        transforms: { enums: { mode: "root", name: "{{name}}" } },
    },
});
