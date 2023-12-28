import {
    AuthApi,
    Configuration,
    GameRequestsApi,
    ProfileApi,
    SettingsApi,
} from "../client";

const config = new Configuration({ basePath: process.env.NEXT_PUBLIC_API_URL });

export const gameRequestApi = new GameRequestsApi(config);
export const settingsApi = new SettingsApi(config);
export const profileApi = new ProfileApi(config);
export const authApi = new AuthApi(config);
