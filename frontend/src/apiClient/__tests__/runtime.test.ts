import { MockInstance } from "vitest";
import {
    BaseAPI,
    BlobApiResponse,
    JSONApiResponse,
    TextApiResponse,
    VoidApiResponse,
    type RequestOpts,
    type RequestContext,
} from "../runtime";
import { BASE_PATH, Configuration } from "../apiConfig";

vi.mock("fetch");

describe("BaseAPI request", () => {
    let baseAPI: BaseAPI;
    let fetchSpy: MockInstance;

    const requestOpts: RequestOpts = { path: "/test", method: "POST" };
    const defaultRequestContext: RequestContext = {
        url: BASE_PATH + "/test",
        init: {
            headers: {},
            credentials: "include",
            method: "POST",
        },
    };

    beforeEach(() => {
        baseAPI = new BaseAPI();
        fetchSpy = vi.spyOn(global, "fetch");
        fetchSpy.mockResolvedValue(new Response());
    });

    it("should make a request to the API endpoint and return the response", async () => {
        const expectedResponse = new Response("test");
        fetchSpy.mockResolvedValue(expectedResponse);

        const response = await baseAPI.request(requestOpts);
        expect(response).toEqual(expectedResponse);
    });

    it("should correctly prioritize init overrides", () => {
        const requestInit: RequestInit = {
            headers: new Headers({ "test-header": "test-value" }),
            credentials: "same-origin",
            cache: "force-cache",
        };

        const body = { testing: "123" };
        baseAPI.request({ ...requestOpts, body }, requestInit);

        expect(fetchSpy).toHaveBeenCalledWith(defaultRequestContext.url, {
            method: requestOpts.method,
            body: JSON.stringify(body),
            ...requestInit,
        });

        // Headers in initOverrides should override everything
        expect(
            fetchSpy.mock.calls[0][1].headers["Content-Type"]
        ).toBeUndefined();
    });

    it.each([100, 199, 300, 400, 500])(
        "should raise an error for non 2xx response status",
        async (status) => {
            fetchSpy.mockResolvedValue({ status });
            await expect(
                async () => await baseAPI.request(requestOpts)
            ).rejects.toThrow("Response returned an error code");
        }
    );

    it("should correctly set headers for json data", async () => {
        const options = { ...requestOpts, body: { testing: "123" } };
        await baseAPI.request(options);

        const headers = fetchSpy.mock.calls[0][1].headers;
        expect(headers["Content-Type"]).toBe("application/json");
    });

    it("should correctly parse query params", async () => {
        const options = {
            ...requestOpts,
            query: { test: "ing", testing: "123" },
        };
        const expectedUrl = BASE_PATH + "/test?test=ing&testing=123";
        await baseAPI.request(options);

        const url = fetchSpy.mock.calls[0][0];
        expect(url).toBe(expectedUrl);
    });

    it("should correctly call middlewares", async () => {
        // modify the request context in the pre request
        const preRequestModifiedContext = { url: "test", init: {} };
        const preRequest = vi.fn().mockResolvedValue(preRequestModifiedContext);

        // modify the response in the post request
        const postRequestModifiedResponse = new Response("test");
        const postRequest = vi
            .fn()
            .mockResolvedValue(postRequestModifiedResponse);

        baseAPI = new BaseAPI(new Configuration({ preRequest, postRequest }));
        const response = await baseAPI.request(requestOpts);
        expect(response).toBe(postRequestModifiedResponse);

        expect(preRequest).toHaveBeenCalledWith(defaultRequestContext);
        expect(postRequest).toHaveBeenCalledWith(
            preRequestModifiedContext,
            expect.objectContaining(new Response())
        );
    });

    it("should correctly call onError", async () => {
        const error = new Error();
        fetchSpy.mockRejectedValue(error);

        const onError = vi.fn();
        baseAPI = new BaseAPI(new Configuration({ onError }));
        await expect(
            async () => await baseAPI.request(requestOpts)
        ).rejects.toThrow();

        expect(onError).toHaveBeenCalledWith(defaultRequestContext, error);
    });

    it("should not raise an error when response is modified in onError", async () => {
        const error = new Error();
        fetchSpy.mockRejectedValue(error);

        const onErrorModifedResponse = new Response("test");
        const onError = vi.fn().mockResolvedValue(onErrorModifedResponse);

        baseAPI = new BaseAPI(new Configuration({ onError }));
        const response = await baseAPI.request(requestOpts);

        expect(response).toBe(onErrorModifedResponse);
    });
});

describe("JSONApiResponse", () => {
    it("should convert snake_case keys to camelCase", async () => {
        const input = {
            snake_case: "value",
            nested_object: {
                another_key: "nested_value",
                deep_nested_object: {
                    deep_key: "deep_value",
                },
            },
            array_of_objects: [
                { array_key: "array_value_1" },
                { array_key: "array_value_2" },
            ],
        };

        const expectedOutput = {
            snakeCase: "value",
            nestedObject: {
                anotherKey: "nested_value",
                deepNestedObject: {
                    deepKey: "deep_value",
                },
            },
            arrayOfObjects: [
                { arrayKey: "array_value_1" },
                { arrayKey: "array_value_2" },
            ],
        };
        const response = new Response(JSON.stringify(input));

        const apiResponse = new JSONApiResponse(response);
        expect(await apiResponse.value()).toEqual(expectedOutput);
    });

    it.each([null, "not_an_object", ["test_test", "test_ing"]])(
        "should not do anything to non object values",
        async (input) => {
            const response = new Response(JSON.stringify(input));
            const apiResponse = new JSONApiResponse(response);
            expect(await apiResponse.value()).toEqual(input);
        }
    );
});

describe("VoidApiResponse", () => {
    it("should return undefined", async () => {
        const response = new Response("testing");
        const apiResponse = new VoidApiResponse(response);
        expect(await apiResponse.value()).toBeUndefined();
    });
});

describe("TextApiResponse", () => {
    it("should return the text of the response", async () => {
        const text = '{"test": "ing"}';
        const response = new Response(text);
        const apiResponse = new TextApiResponse(response);
        expect(await apiResponse.value()).toBe(text);
    });
});

describe("BlobApiResponse", () => {
    it("should return a blob", async () => {
        const blob = "test";

        const response = new Response(blob);
        const apiResponse = new BlobApiResponse(response);
        const receivedBlob = await apiResponse.value();

        expect(receivedBlob.size).toBe(blob.length);
        expect(receivedBlob.type).toBe("text/plain;charset=utf-8");
        expect(await receivedBlob.text()).toBe(blob);
    });
});
