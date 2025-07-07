import authAwareFetch from "../authAwareFetch";
import { logout, refresh } from "../definition";
import { Mock } from "vitest";
import { navigate } from "@/actions/navigate";
import constants from "@/lib/constants";
import rawClient from "../rawClient";

vi.mock("../definition/sdk.gen.ts");
vi.mock("@/actions/navigate");
global.fetch = vi.fn();

describe("authAwareFetch", () => {
    const originalWindow = global.window;
    const fetchMock = fetch as Mock;
    const refreshMock = refresh as Mock;

    const unauthorizedResponse = new Response("unauthorized", { status: 401 });
    const successfulResponse = new Response("ok", { status: 200 });

    afterEach(() => {
        global.window = originalWindow;
    });

    it("should return response if status is not 401", async () => {
        fetchMock.mockResolvedValue(successfulResponse);

        const res = await authAwareFetch("https://localhost/api/data");

        expect(res).toBe(successfulResponse);
        expect(refresh).not.toHaveBeenCalled();
    });

    it("should return response if request is server-side (no window)", async () => {
        // Simulate server environment
        // @ts-expect-error shut up eslint
        delete global.window;
        fetchMock.mockResolvedValue(unauthorizedResponse);

        const res = await authAwareFetch("https://localhost/api/data");

        expect(res.status).toBe(401);
        expect(refresh).not.toHaveBeenCalled();
    });

    it("refreshes and retries queued requests on 401", async () => {
        const firstResponse = new Response(null, { status: 401 });
        const retryResponse = new Response("ok", { status: 200 });

        refreshMock.mockResolvedValue({ error: null });

        fetchMock
            .mockResolvedValueOnce(firstResponse) // initial 401
            .mockResolvedValueOnce(retryResponse); // retry

        const res = await authAwareFetch("https://localhost/api/data");

        expect(refresh).toHaveBeenCalledWith({ client: rawClient });
        expect(res.status).toBe(200);
        expect(logout).not.toHaveBeenCalled();
    });

    it("should log out if refresh fails", async () => {
        fetchMock
            .mockResolvedValueOnce(unauthorizedResponse)
            .mockResolvedValueOnce(unauthorizedResponse);

        refreshMock.mockResolvedValue({ error: "error" });

        await authAwareFetch("https://localhost/api/data");

        expect(logout).toHaveBeenCalled();
        expect(navigate).toHaveBeenCalledWith(constants.PATHS.LOGIN);
    });

    it("should queue multiple 401 requests and retries them after refresh", async () => {
        refreshMock.mockResolvedValue({ error: null });

        fetchMock
            .mockResolvedValueOnce(unauthorizedResponse) // req1
            .mockResolvedValueOnce(unauthorizedResponse) // req2
            .mockResolvedValueOnce(successfulResponse) // retry1
            .mockResolvedValueOnce(successfulResponse); // retry2

        const promise1 = authAwareFetch("https://localhost/api/data1");
        const promise2 = authAwareFetch("https://localhost/api/data2");

        const [res1, res2] = await Promise.all([promise1, promise2]);

        expect(refresh).toHaveBeenCalledTimes(1);
        expect(res1.status).toBe(200);
        expect(res2.status).toBe(200);
    });
});
