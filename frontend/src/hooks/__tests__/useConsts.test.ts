import { renderHook } from "@testing-library/react";
import useConst from "../useConst";

describe("useConst", () => {
    it("should initialize the value using the factory", () => {
        const factory = vi.fn(() => "initial value");
        const { result } = renderHook(() => useConst(factory));

        expect(result.current).toBe("initial value");
        expect(factory).toHaveBeenCalledTimes(1);
    });

    it("should not re-initialize on re-render", () => {
        const factory = vi.fn(() => Math.random());
        const { result, rerender } = renderHook(() => useConst(factory));
        const firstValue = result.current;

        rerender();
        rerender();

        expect(result.current).toBe(firstValue);
        expect(factory).toHaveBeenCalledTimes(1);
    });

    it("should preserve object identity across renders", () => {
        const factory = vi.fn(() => ({ value: 42 }));
        const { result, rerender } = renderHook(() => useConst(factory));
        const firstObject = result.current;

        rerender();

        expect(result.current).toBe(firstObject);
        expect(factory).toHaveBeenCalledTimes(1);
    });
});
