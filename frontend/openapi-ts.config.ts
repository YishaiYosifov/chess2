import { defineConfig, defaultPlugins } from "@hey-api/openapi-ts";

export default defineConfig({
    input: "http://127.0.0.1:5116/openapi/v1.json",
    output: {
        path: "./src/lib/apiClient/definition",
        format: "prettier",
    },
    plugins: [
        ...defaultPlugins,
        {
            name: "@hey-api/client-next",
            runtimeConfigPath: "./src/lib/apiClient/apiClientConfig.ts",
        },
        {
            name: "@hey-api/typescript",
            enums: "typescript",
            exportInlineEnums: true,
        },
    ],
});
