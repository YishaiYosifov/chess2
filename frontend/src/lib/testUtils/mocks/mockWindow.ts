export function setWindowInnerWidth(value: number) {
    Object.defineProperty(window, "innerWidth", {
        writable: true,
        configurable: true,
        value,
    });
}
