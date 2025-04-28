import { finished } from "stream/promises";
import { execSync } from "child_process";
import { createWriteStream } from "fs";
import { Readable } from "stream";

const SWAGGER_URL = "http://127.0.0.1:5116/swagger/v1/swagger.json";
const ADDITIONAL_PROPERTIES = ["useSingleRequestParameter=false"];

async function main() {
    const res = await fetch(SWAGGER_URL);
    const file = createWriteStream("swagger.json");
    await finished(Readable.fromWeb(res.body).pipe(file));

    const additionalProperties = ADDITIONAL_PROPERTIES.join(",");
    execSync(
        "npx @openapitools/openapi-generator-cli generate " +
            "-i swagger.json " +
            "-g typescript-fetch " +
            "-o src/lib/apiClient " +
            `--additional-properties=${additionalProperties} `,
        { stdio: "inherit" },
    );
}

main();
