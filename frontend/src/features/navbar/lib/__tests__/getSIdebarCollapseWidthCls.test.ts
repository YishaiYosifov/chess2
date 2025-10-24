import getSidebarCollapseWidthCls, {
    SIDEBAR_COLLAPSED_CLS,
    SIDEBAR_EXPANDED_CLS,
} from "../sidebarWidth";

describe("getSidebarCollapseWidthCls", () => {
    it("should return the collapsed class when isCollapsed is true", () => {
        const result = getSidebarCollapseWidthCls(true);
        expect(result).toBe(SIDEBAR_COLLAPSED_CLS);
    });

    it("should return the expanded class when isCollapsed is false", () => {
        const result = getSidebarCollapseWidthCls(false);
        expect(result).toBe(SIDEBAR_EXPANDED_CLS);
    });
});
