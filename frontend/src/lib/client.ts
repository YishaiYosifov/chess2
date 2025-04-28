import { ProfileApi } from "./apiClient/apis/ProfileApi";
import { Configuration } from "./apiClient/runtime";
import { AuthApi } from "./apiClient/apis/AuthApi";

const BASE_PATH = process.env.NEXT_PUBLIC_API_URL!;
const config = new Configuration({
    basePath: BASE_PATH,
    credentials: "include",
});

export const authApi = new AuthApi(config);
export const profileApi = new ProfileApi(config);
