import { defineConfig } from "@hey-api/openapi-ts";

export default defineConfig({
    input: "http://127.0.0.1:5116/swagger/v1/swagger.json",
    output: {
        path: "./src/lib/client",
        format: "prettier",
    },
    plugins: [
        {
            name: "@hey-api/client-next",
            runtimeConfigPath: "./src/lib/apiClientConfig.ts",
        },
    ],
});
