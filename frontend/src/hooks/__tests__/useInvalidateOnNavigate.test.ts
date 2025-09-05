import { mockRouter } from "@/lib/testUtils/mocks/mockRouter";
import { renderHook } from "@testing-library/react";
import useInvalidateOnNavigate from "../useInvalidateOnNavigate";

describe("useInvalidateOnNavigate", () => {
    it("should not call router.refresh on mount", () => {
        const router = mockRouter();
        renderHook(() => useInvalidateOnNavigate());
        expect(router.refresh).not.toHaveBeenCalled();
    });

    it("should call router.refresh on unmount", () => {
        const router = mockRouter();
        const { unmount } = renderHook(() => useInvalidateOnNavigate());
        unmount();
        expect(router.refresh).toHaveBeenCalledTimes(1);
    });

    it("should only register one cleanup across re-renders", () => {
        const router = mockRouter();
        const { rerender, unmount } = renderHook(() =>
            useInvalidateOnNavigate(),
        );
        rerender();
        rerender();
        unmount();
        expect(router.refresh).toHaveBeenCalledTimes(1);
    });
});
