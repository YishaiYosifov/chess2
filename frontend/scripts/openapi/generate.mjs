import { writeFileSync, rmSync, existsSync } from "fs";
import { execSync } from "child_process";

async function getOpenapiContent() {
    const response = await fetch("http://localhost:8000/openapi.json");
    return await response.json();
}

async function processOpenapiContent(openapiContent) {
    for (const pathData of Object.values(openapiContent.paths)) {
        for (const operation of Object.values(pathData)) {
            const tag = operation.tags[0];
            const operationId = operation.operationId;
            const toRemove = `${tag}-`;
            const newOperationId = operationId.substring(toRemove.length);
            operation.operationId = newOperationId;
        }
    }

    return openapiContent;
}

async function generateClient({
    outputFolder,
    template,
    additionalProperties,
}) {
    const openapiContent = await processOpenapiContent(
        await getOpenapiContent()
    );
    writeFileSync(
        "./scripts/openapi/openapi.json",
        JSON.stringify(openapiContent)
    );

    if (existsSync(outputFolder)) rmSync(outputFolder, { recursive: true });
    const additionalPropertiesFormatted = Object.entries(additionalProperties)
        .map(([property, value]) => `${property}=${value}`)
        .join(",");

    execSync(
        "npx openapi-generator-cli generate " +
            "-i ./scripts/openapi/openapi.json " +
            "-g typescript-fetch " +
            `-t ${template} ` +
            `-o  ${outputFolder} ` +
            `--additional-properties=${additionalPropertiesFormatted}`
    );
}

generateClient({
    outputFolder: "./src/apiClient",
    template: "openapiTemplate",
    additionalProperties: {
        stringEnums: true,
        supportsES6: true,
    },
});
