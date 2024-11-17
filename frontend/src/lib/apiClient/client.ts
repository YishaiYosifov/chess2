import { AuthApi } from "./apis/AuthApi";

const BASE_PATH = process.env.NEXT_PUBLIC_API_URL!;

export const authApi = new AuthApi(BASE_PATH);
