import { act, renderHook } from "@testing-library/react";
import useLocalPref from "../useLocalPref";

describe("useLocalPref", () => {
    const localStorageKey = "testKey";

    it("should initialize with the default value when localStorage is empty", () => {
        const { result } = renderHook(() =>
            useLocalPref<number>(localStorageKey, 42),
        );

        const [value] = result.current;
        expect(value).toBe(42);
    });

    it("should initialize with the value from localStorage if present", () => {
        localStorage.setItem(localStorageKey, JSON.stringify(100));

        const { result } = renderHook(() =>
            useLocalPref<number>(localStorageKey, 42),
        );

        const [value] = result.current;
        expect(value).toBe(100);
    });

    it("should update the state and localStorage when setNewValue is called", () => {
        const { result } = renderHook(() =>
            useLocalPref<string>(localStorageKey, "default"),
        );

        const [, setValue] = result.current;
        act(() => {
            setValue("newValue");
        });

        const [valueAfter] = result.current;
        expect(valueAfter).toBe("newValue");
        expect(localStorage.getItem(localStorageKey)).toBe(
            JSON.stringify("newValue"),
        );
    });

    it("should parse JSON correctly for objects", () => {
        const obj = { a: 1, b: "test" };
        localStorage.setItem(localStorageKey, JSON.stringify(obj));

        const { result } = renderHook(() =>
            useLocalPref<typeof obj>(localStorageKey, { a: 0, b: "" }),
        );

        const [value] = result.current;
        expect(value).toEqual(obj);
    });

    it("should overwrite localStorage when value is updated multiple times", () => {
        const { result } = renderHook(() =>
            useLocalPref<number>(localStorageKey, 1),
        );

        const [, setValue] = result.current;

        act(() => setValue(2));
        act(() => setValue(3));

        const [valueAfter] = result.current;
        expect(valueAfter).toBe(3);
        expect(localStorage.getItem(localStorageKey)).toBe("3");
    });
});
