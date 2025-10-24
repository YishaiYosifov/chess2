import { notFound } from "next/navigation";
import dataOrThrow from "../dataOrThrow";

describe("dataOrThrow", () => {
    it("should return data when there is no error and data is defined", async () => {
        const mockData = { foo: "bar" };
        const promise = Promise.resolve({
            data: mockData,
            response: { status: 200 } as Response,
        });

        const result = await dataOrThrow(promise);
        expect(result).toEqual(mockData);
    });

    it("should call notFound when response status is 404", async () => {
        const promise = Promise.resolve({
            error: new Error("some error"),
            response: { status: 404 } as Response,
        });

        await expect(dataOrThrow(promise)).rejects.toThrow("notFound");
        expect(notFound).toHaveBeenCalled();
    });

    it("should throw the error when there is an error and status is not 404", async () => {
        const mockError = new Error("Something went wrong");
        const promise = Promise.resolve({
            error: mockError,
            response: { status: 500 } as Response,
        });

        await expect(dataOrThrow(promise)).rejects.toThrow(mockError);
    });

    it("should throw the error when data is undefined even if error is not provided", async () => {
        const promise = Promise.resolve({
            response: { status: 500 } as Response,
        });

        await expect(dataOrThrow(promise)).rejects.toBeUndefined();
    });
});
