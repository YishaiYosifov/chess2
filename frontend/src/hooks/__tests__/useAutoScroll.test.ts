import { renderHook } from "@testing-library/react";
import { createRef } from "react";
import useAutoScroll from "../useAutoScroll";

describe("useAutoScroll", () => {
    let scrollToMock: ReturnType<typeof vi.fn>;

    beforeEach(() => {
        scrollToMock = vi.fn();

        Object.defineProperty(HTMLElement.prototype, "scrollTo", {
            configurable: true,
            value: scrollToMock,
        });
    });

    it("should scroll on initial render if within 50px", () => {
        const ref = createRef<HTMLElement>();
        const el = document.createElement("div");

        Object.defineProperties(el, {
            scrollTop: { configurable: true, get: () => 151 },
            scrollHeight: { configurable: true, get: () => 300 },
            clientHeight: { configurable: true, get: () => 100 },
        });

        ref.current = el;

        renderHook(({ deps }) => useAutoScroll(ref, deps), {
            initialProps: { deps: [1] },
        });

        expect(scrollToMock).toHaveBeenCalledWith({
            top: 300,
            behavior: "smooth",
        });
    });

    it("should be able to scroll after the deps change", () => {
        const ref = createRef<HTMLElement>();
        const el = document.createElement("div");

        Object.defineProperties(el, {
            scrollTop: { configurable: true, get: () => 149 },
            scrollHeight: { configurable: true, get: () => 300 },
            clientHeight: { configurable: true, get: () => 100 },
        });

        ref.current = el;

        const { rerender } = renderHook(
            ({ deps }) => useAutoScroll(ref, deps),
            {
                initialProps: { deps: [1] },
            },
        );

        expect(scrollToMock).not.toHaveBeenCalled();

        Object.defineProperties(el, {
            scrollTop: { configurable: true, get: () => 151 },
        });

        rerender({ deps: [2] });

        expect(scrollToMock).toHaveBeenCalledWith({
            top: 300,
            behavior: "smooth",
        });
    });

    it("should not scroll if more than 50px from bottom", () => {
        const ref = createRef<HTMLElement>();
        const el = document.createElement("div");

        Object.defineProperties(el, {
            scrollTop: { configurable: true, get: () => 100 },
            scrollHeight: { configurable: true, get: () => 300 },
            clientHeight: { configurable: true, get: () => 100 },
        });

        ref.current = el;

        const { rerender } = renderHook(
            ({ deps }) => useAutoScroll(ref, deps),
            {
                initialProps: { deps: [1] },
            },
        );

        rerender({ deps: [2] });

        expect(scrollToMock).not.toHaveBeenCalled();
    });

    it("should do nothing if ref is null", () => {
        const ref = createRef<HTMLElement>();

        const { rerender } = renderHook(
            ({ deps }) => useAutoScroll(ref, deps),
            {
                initialProps: { deps: [1] },
            },
        );

        rerender({ deps: [2] });

        expect(scrollToMock).not.toHaveBeenCalled();
    });
});
