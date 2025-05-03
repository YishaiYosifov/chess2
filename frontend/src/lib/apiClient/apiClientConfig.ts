import { type CreateClientConfig } from "./definition/client.gen";
import customFetch from "./fetchClient";

export const createClientConfig: CreateClientConfig = (config) => ({
    ...config,
    baseUrl: process.env.NEXT_PUBLIC_API_URL,
    credentials: "include",
    fetch: customFetch,
});
