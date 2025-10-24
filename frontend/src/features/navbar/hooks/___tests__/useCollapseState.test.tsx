import { renderHook, act } from "@testing-library/react";
import Cookies from "js-cookie";
import useCollapseState from "../useCollapseState";
import { mockJsCookie } from "@/lib/testUtils/mocks/mockCookies";
import constants from "@/lib/constants";
import { setWindowInnerWidth } from "@/lib/testUtils/mocks/mockDom";

vi.mock("js-cookie");

describe("useCollapseState", () => {
    afterEach(() => {
        vi.clearAllMocks();
    });

    function mockIsCollapsedCookie(isCollapsed: boolean) {
        const cookieValue = isCollapsed ? "1" : undefined;
        mockJsCookie({
            [constants.COOKIES.SIDEBAR_COLLAPSED]: cookieValue,
        });
    }

    it("should use initial state on mount", () => {
        const { result } = renderHook(() => useCollapseState(false));
        expect(result.current.isCollapsed).toBe(false);
    });

    it("should set isCollapsed true on small screens", () => {
        setWindowInnerWidth(800);

        const { result } = renderHook(() => useCollapseState(false));
        act(() => {
            window.dispatchEvent(new Event("resize"));
        });

        expect(result.current.isCollapsed).toBe(true);
    });

    it("should read cookie value on larger screens", () => {
        mockIsCollapsedCookie(true);

        const { result } = renderHook(() => useCollapseState(false));
        act(() => {
            window.dispatchEvent(new Event("resize"));
        });

        expect(result.current.isCollapsed).toBe(true);
    });

    it("should toggle state and sets/removes cookie", () => {
        const setSpy = vi.spyOn(Cookies, "set");
        const removeSpy = vi.spyOn(Cookies, "remove");

        const { result } = renderHook(() => useCollapseState(false));

        // Collapse
        act(() => {
            result.current.toggleCollapse();
        });
        expect(result.current.isCollapsed).toBe(true);
        expect(setSpy).toHaveBeenCalled();

        // Expand
        act(() => {
            result.current.toggleCollapse();
        });
        expect(result.current.isCollapsed).toBe(false);
        expect(removeSpy).toHaveBeenCalled();
    });
});
