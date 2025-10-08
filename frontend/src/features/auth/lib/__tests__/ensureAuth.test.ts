import { createGuestUser, refresh } from "@/lib/apiClient";
import constants from "@/lib/constants";
import { mockJsCookie } from "@/lib/testUtils/mocks/mockCookies";
import ensureAuth from "../ensureAuth";
import rawClient from "@/lib/apiClient/rawClient";

vi.mock("js-cookie");
vi.mock("@/lib/apiClient/definition");

describe("ensureAuth", () => {
    const refreshMock = vi.mocked(refresh);
    const createGuestUserMock = vi.mocked(createGuestUser);

    beforeEach(() => {
        refreshMock.mockResolvedValue({
            data: undefined,
            response: new Response(),
        });
        createGuestUserMock.mockResolvedValue({
            data: undefined,
            response: new Response(),
        });
    });

    it("should call refresh when the user is logged in", async () => {
        mockJsCookie({ [constants.COOKIES.IS_LOGGED_IN]: "true" });

        const result = await ensureAuth();

        expect(refreshMock).toHaveBeenCalledWith({ client: rawClient });
        expect(result).toBe(true);
    });

    it("should call createGuestUser when the user is not logged in", async () => {
        const result = await ensureAuth();

        expect(createGuestUserMock).toHaveBeenCalledWith({ client: rawClient });
        expect(result).toBe(true);
    });

    it("should return false when refresh fails", async () => {
        mockJsCookie({ [constants.COOKIES.IS_LOGGED_IN]: "true" });
        refreshMock.mockResolvedValue({
            error: { errors: [] },
            data: undefined,
            response: new Response(),
        });

        const result = await ensureAuth();

        expect(result).toBe(false);
    });

    it("should return false when guest creation fails", async () => {
        createGuestUserMock.mockResolvedValue({
            error: { errors: [] },
            data: undefined,
            response: new Response(),
        });

        const result = await ensureAuth();

        expect(result).toBe(false);
    });

    it("should reuse the same promise when called concurrently", async () => {
        const promise1 = ensureAuth();
        const promise2 = ensureAuth();

        const [r1, r2] = await Promise.all([promise1, promise2]);

        expect(r1).toBe(true);
        expect(r2).toBe(true);
        expect(createGuestUserMock).toHaveBeenCalledExactlyOnceWith({
            client: rawClient,
        });
    });

    it("should create a new promise after the previous one resolves", async () => {
        const first = await ensureAuth();
        expect(first).toBe(true);

        const secondPromise = ensureAuth();
        expect(secondPromise).not.toBe(first);

        await expect(secondPromise).resolves.toBe(true);
    });
});
