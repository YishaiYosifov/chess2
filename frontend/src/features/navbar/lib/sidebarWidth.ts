export const SIDEBAR_EXPANDED_CLS = "w-64";
export const SIDEBAR_COLLAPSED_CLS = "w-25";

const getSidebarCollapseWidthCls = (isCollapsed: boolean) =>
    isCollapsed ? SIDEBAR_COLLAPSED_CLS : SIDEBAR_EXPANDED_CLS;
export default getSidebarCollapseWidthCls;
