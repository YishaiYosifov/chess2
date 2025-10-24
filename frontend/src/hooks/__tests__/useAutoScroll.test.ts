import { renderHook } from "@testing-library/react";
import { act, createRef } from "react";
import useAutoScroll from "../useAutoScroll";
import { Mock } from "vitest";
import { mockScrollTo } from "@/lib/testUtils/mocks/mockDom";

describe("useAutoScroll", () => {
    let scrollToMock: Mock;

    beforeEach(() => {
        scrollToMock = mockScrollTo();
    });

    function setupElement({
        scrollTop,
        scrollHeight,
        clientHeight,
        el,
    }: {
        scrollTop?: number;
        scrollHeight?: number;
        clientHeight?: number;
        el?: HTMLDivElement;
    } = {}) {
        scrollTop ??= 300;
        scrollHeight ??= 250;
        clientHeight ??= 50;

        el ??= document.createElement("div");
        el.scrollTop = scrollTop;

        Object.defineProperties(el, {
            scrollHeight: {
                configurable: true,
                get: () => scrollHeight,
            },
            clientHeight: {
                configurable: true,
                get: () => clientHeight,
            },
        });

        return el;
    }

    it("should scroll to bottom on initial render", () => {
        const ref = createRef<HTMLElement>();
        const el = setupElement({
            scrollHeight: 250,
        });
        ref.current = el;

        renderHook(({ deps }) => useAutoScroll(ref, deps), {
            initialProps: { deps: [1] },
        });

        expect(scrollToMock).toHaveBeenCalledWith({
            top: 250,
            behavior: "smooth",
        });
    });

    it("should not scroll if user has scrolled", async () => {
        const ref = createRef<HTMLElement>();
        const el = setupElement();
        ref.current = el;

        const { rerender } = renderHook(
            ({ deps }) => useAutoScroll(ref, deps),
            {
                initialProps: { deps: [1] },
            },
        );
        scrollToMock.mockClear();

        setupElement({
            scrollHeight: 300,
            scrollTop: 50,
            clientHeight: 100,
            el,
        });
        await act(() => el.dispatchEvent(new Event("scroll")));

        rerender({ deps: [2] });
        expect(scrollToMock).not.toHaveBeenCalled();
    });

    it("should scroll to bottom when deps change and still at bottom", () => {
        const ref = createRef<HTMLElement>();
        const el = setupElement({
            scrollTop: 200,
            scrollHeight: 300,
            clientHeight: 100,
        });
        ref.current = el;

        const { rerender } = renderHook(
            ({ deps }) => useAutoScroll(ref, deps),
            { initialProps: { deps: [1] } },
        );

        expect(scrollToMock).toHaveBeenCalledTimes(1);

        rerender({ deps: [2] });

        expect(scrollToMock).toHaveBeenCalledTimes(2);
    });

    it("should not auto-scroll if user has manually scrolled up", () => {
        const ref = createRef<HTMLElement>();
        const el = setupElement({
            scrollTop: 200,
            scrollHeight: 300,
            clientHeight: 100,
        });
        ref.current = el;

        const { rerender } = renderHook(
            ({ deps }) => useAutoScroll(ref, deps),
            {
                initialProps: { deps: [1] },
            },
        );

        act(() => {
            el.scrollTop = 100;
            el.dispatchEvent(new Event("scroll"));
        });

        scrollToMock.mockClear();

        rerender({ deps: [2] });

        expect(scrollToMock).not.toHaveBeenCalled();
    });

    it("should detect scrolling back to bottom and re-enable auto-scroll", () => {
        const ref = createRef<HTMLElement>();
        const el = setupElement({
            scrollTop: 100,
            scrollHeight: 300,
            clientHeight: 100,
        });
        ref.current = el;

        const { rerender } = renderHook(
            ({ deps }) => useAutoScroll(ref, deps),
            {
                initialProps: { deps: [1] },
            },
        );

        act(() => {
            el.scrollTop = 0;
            el.dispatchEvent(new Event("scroll"));
        });

        act(() => {
            el.scrollTop = 200;
            el.dispatchEvent(new Event("scroll"));
        });

        scrollToMock.mockClear();

        act(() => {
            rerender({ deps: [2] });
        });

        expect(scrollToMock).toHaveBeenCalled();
    });

    it("should do nothing if ref is null", () => {
        const ref = createRef<HTMLElement>();

        renderHook(({ deps }) => useAutoScroll(ref, deps), {
            initialProps: { deps: [1] },
        });

        expect(scrollToMock).not.toHaveBeenCalled();
    });
});
