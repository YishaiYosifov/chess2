import { getResource, snakeToCamel } from "../fetchUtils";

describe("getResource", () => {
    let fetchMock: jest.SpyInstance;
    beforeEach(() => {
        fetchMock = jest.spyOn(global, "fetch");
    });
    afterEach(() => {
        jest.restoreAllMocks();
        jest.clearAllMocks();
    });

    it.each([
        [jest.fn(async () => ({ ok: false, text: () => "" }))],
        [
            jest.fn(async () => {
                throw new Error();
            }),
        ],
    ])("should fail correctly", async (fetch_fn) => {
        jest.spyOn(console, "error").mockImplementation();
        fetchMock.mockImplementation(fetch_fn);

        const dataProcessor = jest.fn((a) => a);
        const response = await getResource("test", { dataProcessor });

        expect(response).toBe(null);
        expect(console.error).toHaveBeenCalledTimes(1);
        expect(dataProcessor).not.toHaveBeenCalled();
    });

    it("should succeed and call the data processor", async () => {
        const data = "data";

        jest.spyOn(console, "error").mockImplementation();
        fetchMock.mockResolvedValue({ ok: true, json: () => data });

        const dataProcessor = jest.fn((a) => a);
        const response = await getResource("test", { dataProcessor });

        expect(response).toBe(data);
        expect(dataProcessor).toHaveBeenCalledTimes(1);
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
