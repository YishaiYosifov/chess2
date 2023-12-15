import { setIsAuthed } from "@/zustand/store";
import { apiRequest } from "./fetchUtils";

export async function login(
    username: string,
    password: string
): Promise<Response | null> {
    const loginForm = new FormData();
    loginForm.set("username", username);
    loginForm.set("password", password);

    const response = await apiRequest("/auth/login", {
        method: "POST",
        body: loginForm,
    });
    if (response && response.ok) setIsAuthed(true);

    return response;
}
