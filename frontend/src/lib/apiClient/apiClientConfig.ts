import { type CreateClientConfig } from "./definition/client.gen";
import authAwareFetch from "./authAwareFetch";

export const createClientConfig: CreateClientConfig = (config) => ({
    ...config,
    baseUrl: process.env.NEXT_PUBLIC_API_URL,
    credentials: "include",
    fetch: authAwareFetch,
});
