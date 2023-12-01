import { setCsrfToken, setIsAuthed, useStore } from "@/zustand/store";
import { apiRequest } from "./fetchUtils";

interface Tokens {
    accessToken: string;
    refreshToken: string;
}

interface LoginData {
    status: number;
    data: Tokens;
}

export async function login(
    username: string,
    password: string
): Promise<LoginData> {
    const loginForm = new FormData();
    loginForm.set("username", username);
    loginForm.set("password", password);

    const response = await apiRequest("/auth/login", {
        body: loginForm,
    });
    if (response.ok) setIsAuthed(true);

    const data = (await response.json()).detail as Tokens;

    return { status: response.status, data };
}

export async function getCsrf(): Promise<string> {
    const { csrfTokenCreatedAt, csrfToken } = useStore.getState();

    const currentTimestamp = Date.now() / 1000;
    const didCsrfExpire = currentTimestamp - csrfTokenCreatedAt >= 3600;

    if (csrfToken && !didCsrfExpire) return csrfToken;
    else return await genCsrf();
}

async function genCsrf(): Promise<string> {
    const response = await apiRequest("/auth/gen-csrf", { method: "GET" });
    const { status, data } = await response.json();
    if (status != "success") throw Error("Failed to fetch CSRF");

    setCsrfToken(data);
    return data;
}
