import { apiRequest } from "../utils/common";
import { useStore } from "../store";

const initialAuthState = {
    accessToken: null,
    refreshToken: null,

    csrfToken: null,
    csrfTokenCreatedAt: null,

    isAuthed: false,
};

export const createAuthSlice = (set) => ({
    ...initialAuthState,
});

export async function logout() {
    await apiRequest("/auth/logout");
    useStore.setState(initialAuthState, false, "LOGOUT");
}
