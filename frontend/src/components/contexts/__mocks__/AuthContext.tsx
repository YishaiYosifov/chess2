import { profileMock } from "@/mockUtils/profileMock";

export const setIsAuthed = vi.fn();
export const setAuthedProfile = vi.fn();

export const useAuthedProfile = () => profileMock;
export const useAuthedContext = () => ({
    isAuthed: true,
    setIsAuthed,
    setAuthedProfile,
    authedProfile: profileMock,
});
