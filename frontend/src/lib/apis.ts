import { AuthApi } from "@/lib/apiClient";

const BASE_PATH = process.env.NODE_PUBLIC_API_URL!;

export const authApi = new AuthApi(BASE_PATH);
