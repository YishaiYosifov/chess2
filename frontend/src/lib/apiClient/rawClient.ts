import type { ClientOptions } from "./definition/types.gen";
import { createClient, createConfig } from "./definition/client";
import baseClientConfig from "./baseClientConfig";

const config = {
    ...createConfig<ClientOptions>(),
    ...baseClientConfig,
    fetch,
};
const rawClient = createClient(config);
export default rawClient;
