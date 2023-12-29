import {
    GameRequestsApi,
    Configuration,
    SettingsApi,
    ProfileApi,
    AuthApi,
} from "../client";

export const apiConfig = new Configuration({
    basePath: process.env.NEXT_PUBLIC_API_URL,
    credentials: "include",
});

export const gameRequestApi = new GameRequestsApi(apiConfig);
export const settingsApi = new SettingsApi(apiConfig);
export const profileApi = new ProfileApi(apiConfig);
export const authApi = new AuthApi(apiConfig);
