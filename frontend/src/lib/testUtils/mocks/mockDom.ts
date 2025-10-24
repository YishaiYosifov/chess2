import { Mock } from "vitest";

export function setWindowInnerWidth(value: number): void {
    Object.defineProperty(window, "innerWidth", {
        writable: true,
        configurable: true,
        value,
    });
}

const DEFAULT_RECT = {
    bottom: 0,
    height: 0,
    left: 0,
    right: 0,
    top: 0,
    width: 0,
} as DOMRect;

export function mockBoundingClientRect(
    elementTestIds: Record<string, DOMRect>,
): void {
    vi.spyOn(HTMLElement.prototype, "getBoundingClientRect").mockImplementation(
        function (this: HTMLElement) {
            const testId = this.dataset.testid;
            if (!testId) return DEFAULT_RECT;

            const rect = elementTestIds[testId];
            return rect ?? DEFAULT_RECT;
        },
    );
}

export function mockScrollTo(): Mock {
    const scrollToMock = vi.fn().mockImplementation(function (
        this: HTMLElement,
        { top }: { top: number },
    ) {
        this.scrollTop = top;
        this.dispatchEvent(new Event("scroll"));
    });

    Object.defineProperty(HTMLElement.prototype, "scrollTo", {
        configurable: true,
        value: scrollToMock,
    });
    return scrollToMock;
}
