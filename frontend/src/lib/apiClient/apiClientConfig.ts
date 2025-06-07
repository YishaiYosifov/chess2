import { type CreateClientConfig } from "./definition/client.gen";
import authAwareFetch from "./authAwareFetch";

export const createClientConfig: CreateClientConfig = (config) => ({
    ...config,
    baseUrl: "/",
    credentials: "include",
    fetch: authAwareFetch,
});
