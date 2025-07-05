import { privateProfileMock } from "@/mockUtils/profileMock";

export const setHasAuthCookies = vi.fn();
export const setAuthedProfile = vi.fn();

export const useAuthedProfile = () => privateProfileMock;
export const useAuthedContext = () => ({
    hasAuthCookies: true,
    setHasAuthCookies,
    setAuthedProfile,
    authedProfile: privateProfileMock,
});
