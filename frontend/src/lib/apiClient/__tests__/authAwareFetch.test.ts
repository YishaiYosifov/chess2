import { createGuestUser, logout, refresh } from "../definition";
import { mockJsCookie } from "@/lib/testUtils/mocks/mockCookies";
import authAwareFetch from "../authAwareFetch";
import { navigate } from "@/actions/navigate";
import constants from "@/lib/constants";
import rawClient from "../rawClient";

vi.mock("js-cookie");
vi.mock("../definition/sdk.gen.ts");
vi.mock("@/actions/navigate");

const originalWindow = global.window;
describe("authAwareFetch", () => {
    const fetchMock = vi.fn();
    const refreshMock = vi.mocked(refresh);
    const createGuestUserMock = vi.mocked(createGuestUser);
    const logoutMock = vi.mocked(logout);
    const navigateMock = vi.mocked(navigate);

    const unauthorizedResponse = new Response("unauthorized", { status: 401 });
    const successfulResponse = new Response("ok", { status: 200 });

    beforeEach(() => {
        vi.stubGlobal("fetch", fetchMock);

        refreshMock.mockResolvedValue({
            data: undefined,
            response: new Response(),
        });
        createGuestUserMock.mockResolvedValue({
            data: undefined,
            response: new Response(),
        });
    });

    afterEach(() => {
        vi.stubGlobal("window", originalWindow);
    });

    it("should return response if status is not 401", async () => {
        fetchMock.mockResolvedValue(successfulResponse);

        const res = await authAwareFetch("https://localhost/api/data");

        expect(res).toBe(successfulResponse);
        expect(refreshMock).not.toHaveBeenCalled();
    });

    it("should return response if request is server-side (no window)", async () => {
        // simulate server environment
        vi.stubGlobal("window", undefined);
        fetchMock.mockResolvedValue(unauthorizedResponse);

        const res = await authAwareFetch("https://localhost/api/data");

        expect(res.status).toBe(401);
        expect(refreshMock).not.toHaveBeenCalled();
    });

    it("should refresh and retry requests on 401", async () => {
        mockJsCookie({ [constants.COOKIES.IS_LOGGED_IN]: "true" });
        const firstResponse = new Response(null, { status: 401 });
        const retryResponse = new Response("ok", { status: 200 });

        fetchMock
            .mockResolvedValueOnce(firstResponse) // initial 401
            .mockResolvedValueOnce(retryResponse); // retry

        const res = await authAwareFetch("https://localhost/api/data");

        expect(refreshMock).toHaveBeenCalledExactlyOnceWith({
            client: rawClient,
        });
        expect(res.status).toBe(200);
        expect(logoutMock).not.toHaveBeenCalled();
    });

    it("should create guest and retry requests on 401 when needed", async () => {
        const firstResponse = new Response(null, { status: 401 });
        const retryResponse = new Response("ok", { status: 200 });

        fetchMock
            .mockResolvedValueOnce(firstResponse)
            .mockResolvedValueOnce(retryResponse);

        await authAwareFetch("https://localhost/api/data");

        expect(createGuestUserMock).toHaveBeenCalledExactlyOnceWith({
            client: rawClient,
        });
    });

    it("should log out if ensureAuth fails", async () => {
        fetchMock.mockResolvedValueOnce(unauthorizedResponse);

        createGuestUserMock.mockResolvedValue({
            data: undefined,
            error: { errors: [] },
            response: new Response(),
        });

        await authAwareFetch("https://localhost/api/data");

        expect(logoutMock).toHaveBeenCalledOnce();
        expect(navigateMock).toHaveBeenCalledExactlyOnceWith(
            constants.PATHS.REGISTER,
        );
    });

    it("should log out if we fail to fetch after successful auth", async () => {
        fetchMock
            .mockResolvedValueOnce(unauthorizedResponse)
            .mockResolvedValueOnce(unauthorizedResponse);

        await authAwareFetch("https://localhost/api/data");

        expect(logoutMock).toHaveBeenCalledOnce();
        expect(navigateMock).toHaveBeenCalledExactlyOnceWith(
            constants.PATHS.REGISTER,
        );
    });
});
