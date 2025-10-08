import {
    createFakeGuestUser,
    createFakePrivateUser,
} from "@/lib/testUtils/fakers/userFaker";
import { isAuthed, isGuest } from "../userGuard";

describe("isAuthed", () => {
    it("should return true for authed", () => {
        expect(isAuthed(createFakePrivateUser())).toBe(true);
    });

    it("should return false for guest", () => {
        expect(isAuthed(createFakeGuestUser())).toBe(false);
    });

    it("should return false for null", () => {
        expect(isAuthed(null)).toBe(false);
    });
});

describe("isGuest", () => {
    it("should return true for guest", () => {
        expect(isGuest(createFakeGuestUser())).toBe(true);
    });

    it("should return false for authed", () => {
        expect(isGuest(createFakePrivateUser())).toBe(false);
    });

    it("should return true for null", () => {
        expect(isGuest(null)).toBe(true);
    });
});
