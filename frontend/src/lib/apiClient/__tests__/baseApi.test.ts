import { MockInstance } from "vitest";

import { BaseAPI, type RequestOptions, type RequestContext } from "../baseApi";
import { ResponseError } from "@/lib/models/Errors";

vi.mock("fetch");

const BASE_PATH = "localhost";

describe.only("BaseAPI request", () => {
    let baseAPI: BaseAPI;
    let fetchSpy: MockInstance;

    const testPath = "/test";
    const testRequestOptions: RequestOptions = { method: "POST" };
    const expectedRequestContext: RequestContext = {
        url: BASE_PATH + "/test",
        init: {
            headers: {},
            credentials: "include",
            method: "POST",
        },
    };

    beforeEach(() => {
        baseAPI = new BaseAPI(BASE_PATH);
        fetchSpy = vi.spyOn(global, "fetch");
        fetchSpy.mockResolvedValue(new Response());
    });

    it("should make a request to the API endpoint and return the response", async () => {
        const expectedResponse = new Response("test");
        fetchSpy.mockResolvedValue(expectedResponse);

        const response = await baseAPI.request(testPath, testRequestOptions);
        expect(response).toEqual(expectedResponse);
    });

    it("should correctly use request options", () => {
        const body = { testing: "123" };
        const headers = { "X-TEST-HEADER": "test-value" };
        const requestOptions: RequestOptions = {
            body,
            headers,
            method: "PUT",
            cache: "force-cache",
        };

        const expectedRequestInit: RequestInit = {
            ...requestOptions,
            credentials: "include",
            body: JSON.stringify(body),
            headers: new Headers({
                ...headers,
                "Content-Type": "application/json",
            }),
        };

        baseAPI.request(testPath, requestOptions);

        expect(fetchSpy).toHaveBeenCalledWith(
            expectedRequestContext.url,
            expectedRequestInit,
        );
    });

    it.each([100, 199, 300, 400, 500])(
        "should raise an error for non 2xx response status",
        async (status) => {
            fetchSpy.mockResolvedValue({ status, json: async () => ({}) });
            await expect(
                async () => await baseAPI.request(testPath, testRequestOptions),
            ).rejects.toThrow(ResponseError);
        },
    );

    it("should correctly set proccess json data", async () => {
        const body = { testIng: 123, aB: ["a", "bC"] };
        const expectedBody = JSON.stringify(body);

        const options = { ...testRequestOptions, body };
        await baseAPI.request(testPath, options);

        const receivedHeaders = fetchSpy.mock.calls[0][1].headers as Headers;
        const receivedBody = fetchSpy.mock.calls[0][1].body as string;
        expect(receivedHeaders.get("Content-Type")).toBe("application/json");
        expect(receivedBody).toBe(expectedBody);
    });

    it("should correctly parse query params", async () => {
        const options = {
            ...testRequestOptions,
            query: { test: "ing", testing: "123" },
        };
        const expectedUrl = BASE_PATH + testPath + "?test=ing&testing=123";
        await baseAPI.request(testPath, options);

        const url = fetchSpy.mock.calls[0][0];
        expect(url).toBe(expectedUrl);
    });
});
