import { defineConfig } from "@hey-api/openapi-ts";

export default defineConfig({
    input: "http://127.0.0.1:5116/swagger/v1/swagger.json",
    output: "src/lib/client",
    plugins: ["@hey-api/client-fetch"],
});
