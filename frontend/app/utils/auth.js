import { apiRequest } from "./common";
import { useStore } from "../store";

export async function login(selector, password) {
    const response = await apiRequest("/auth/login", {
        json: {
            selector: selector,
            password: password,
        },
    });
    const loginData = await response.json();
    if (response.ok) useStore.setState({ ...loginData, isAuthed: true });

    return { status: response.status, ...loginData };
}

export async function getCsrf() {
    let { csrfTokenCreatedAt, csrfToken } = useStore.getState();

    const currentTimestamp = Date.now() / 1000;
    const didCsrfExpire = currentTimestamp - csrfTokenCreatedAt >= 3600;

    if (csrfToken && !didCsrfExpire) return csrfToken;
    else return await genCsrf();
}

async function genCsrf() {
    const response = await apiRequest("/auth/gen-csrf", { method: "GET" });
    const { csrfToken } = await response.json();
    useStore.setState(
        {
            csrfToken,
            csrfTokenCreatedAt: new Date() / 1000,
        },
        false,
        "SET_CSRF"
    );

    return csrfToken;
}
