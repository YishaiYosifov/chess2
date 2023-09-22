import { setCsrfToken, setIsAuthed, useStore } from "@/app/store";
import { apiRequest } from "./common";

export async function login(
    selector: string | undefined,
    password: string | undefined
): Promise<{ status: number; response: Response }> {
    const response = await apiRequest("/auth/login", {
        json: {
            selector: selector,
            password: password,
        },
    });
    if (response.ok) setIsAuthed(true);

    return { status: response.status, response };
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
    const { csrfToken } = await response.json();
    setCsrfToken(csrfToken);

    return csrfToken;
}
