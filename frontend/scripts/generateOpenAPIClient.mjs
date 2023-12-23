import OpenAPI from "openapi-typescript-codegen";

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

    OpenAPI.generate({
        input: openapiContent,
        output: "./src/client",
        client: "fetch",
        useOptions: true,
    });
}

generateClient();
