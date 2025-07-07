import type { ClientOptions } from "./definition/types.gen";
import { createClient, createConfig } from "@hey-api/client-next";
import baseClientConfig from "./baseClientConfig";

const config = {
    ...createConfig<ClientOptions>(),
    ...baseClientConfig,
    fetch,
};
const rawClient = createClient(config);
export default rawClient;
