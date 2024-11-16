import {
    BlobApiResponse,
    JSONApiResponse,
    TextApiResponse,
    VoidApiResponse,
} from "../apiResponse";

describe("JSONApiResponse", () => {
    it.each([
        null,
        "not_an_object",
        ["test_test", "test_ing"],
        { testObject: "test" },
    ])("should parse the json body", async (input) => {
        const response = new Response(JSON.stringify(input));
        const apiResponse = new JSONApiResponse(response);
        expect(await apiResponse.value()).toEqual(input);
    });
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
