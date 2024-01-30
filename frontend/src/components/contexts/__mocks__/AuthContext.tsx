import { profileMock } from "@/mockUtils/profileMock";

export const useAuthedProfile = () => profileMock;
export const useAuthedContext = () => ({
    isAuthed: true,
    setIsAuthed: vi.fn(),
    setAuthedProfile: vi.fn(),
    authedProfile: profileMock,
});
