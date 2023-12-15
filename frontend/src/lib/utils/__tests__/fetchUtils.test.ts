import { apiRequest, getResource, snakeToCamel } from "../fetchUtils";

describe("apiRequest", () => {
    let fetchMock: jest.SpyInstance;
    beforeEach(() => {
        fetchMock = jest.spyOn(global, "fetch");
    });

    it("should log an error when fetch raises error", async () => {
        fetchMock.mockRejectedValue(new Error());

        const response = await apiRequest("/test");
        expect(response).toBe(null);
        expect(console.error).toHaveBeenCalledTimes(1);
    });

    it("should include options in request", async () => {
        fetchMock.mockImplementation();

        const options = { headers: { testing: "123" } };

        await apiRequest("/test", options);
        expect(fetchMock).toHaveBeenCalledWith(
            `${process.env.NEXT_PUBLIC_API_URL}/test`,
            {
                credentials: "include",
                ...options,
            }
        );
    });

    it("should return request", async () => {
        fetchMock.mockResolvedValue("testing");

        const response = await apiRequest("/test");
        expect(response).toBe("testing");
    });
});

describe("getResource", () => {
    let fetchMock: jest.SpyInstance;
    const dataProcessor = jest.fn((a) => a);

    beforeEach(() => {
        fetchMock = jest.spyOn(global, "fetch");
    });

    it("should return null if fetch raises an error", async () => {
        fetchMock.mockRejectedValue(new Error());

        const response = await getResource("/test");
        expect(response).toBe(null);
    });

    it("should log an error when the response is not ok", async () => {
        fetchMock.mockResolvedValue({ ok: false, text: jest.fn() });

        const response = await getResource("/test", { dataProcessor });

        expect(response).toBe(null);
        expect(console.error).toHaveBeenCalledTimes(1);
        expect(dataProcessor).not.toHaveBeenCalled();
    });

    it("should succeed and call the data processor", async () => {
        const data = "data";

        fetchMock.mockResolvedValue({ ok: true, json: () => data });

        const response = await getResource("/test", { dataProcessor });

        expect(response).toBe(data);
        expect(dataProcessor).toHaveBeenCalledWith(data);
        expect(console.error).not.toHaveBeenCalled();
    });
});

describe("snakeToCamel", () => {
    it("should convert snake_case keys to camelCase", () => {
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

        const result = snakeToCamel(input);
        expect(result).toEqual(expectedOutput);
    });

    it("should handle null and non-object values", () => {
        const nullInput = null;
        const nonObjectInput = "not_an_object";

        expect(snakeToCamel(nullInput)).toEqual(nullInput);
        expect(snakeToCamel(nonObjectInput)).toEqual(nonObjectInput);
    });
});
