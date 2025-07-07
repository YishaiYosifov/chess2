import { type CreateClientConfig } from "./definition/client.gen";
import authAwareFetch from "./authAwareFetch";
import baseClientConfig from "./baseClientConfig";

export const createClientConfig: CreateClientConfig = (config) => ({
    ...config,
    ...baseClientConfig,
    fetch: authAwareFetch,
});
