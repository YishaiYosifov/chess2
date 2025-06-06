import { defineConfig, defaultPlugins } from "@hey-api/openapi-ts";
process.env["NODE_TLS_REJECT_UNAUTHORIZED"] = "0";

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
            runtimeConfigPath: "./src/lib/apiClient/apiClientConfig.ts",
        },
        {
            name: "@hey-api/typescript",
            enums: "typescript",
            exportInlineEnums: true,
        },
    ],
});
