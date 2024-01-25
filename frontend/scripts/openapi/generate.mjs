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

async function generateClient() {
    const openapiContent = await processOpenapiContent(
        await getOpenapiContent()
    );
    writeFileSync(
        "./scripts/openapi/openapi.json",
        JSON.stringify(openapiContent)
    );

    if (existsSync("./src/client")) rmSync("./src/client", { recursive: true });

    execSync(
        "npx openapi-generator-cli generate " +
            "-i ./scripts/openapi/openapi.json " +
            "-g typescript-fetch " +
            "-o ./src/client " +
            "--additional-properties=stringEnums=true"
    );
}

generateClient();
