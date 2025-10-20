import {
    createFakeGuestUser,
    createFakePrivateUser,
} from "@/lib/testUtils/fakers/userFaker";
import { isAuthed, isGuest, isIdAuthed } from "../userGuard";

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

describe("isIdAuthed", () => {
    it("should return true for authed userId", () => {
        expect(isIdAuthed(crypto.randomUUID())).toBe(true);
    });

    it("should return false for guest userId", () => {
        expect(isIdAuthed("guest:" + crypto.randomUUID())).toBe(false);
    });

    it("should return false for null userId", () => {
        expect(isIdAuthed(null)).toBe(false);
    });
});
