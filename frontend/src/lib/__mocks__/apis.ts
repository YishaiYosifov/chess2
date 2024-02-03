export const apiConfig = {
    basePath: process.env.NEXT_PUBLIC_API_URL,
};

export const authApi = { login: vi.fn(), signup: vi.fn() };
export const gameRequestApi = { startPoolGame: vi.fn(), cancel: vi.fn() };
export const settingsApi = { uploadProfilePicture: vi.fn() };
