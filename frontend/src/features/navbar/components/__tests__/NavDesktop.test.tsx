import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";

import NavDesktop from "../NavDesktop";
import React from "react";
import useCollapseState from "../../hooks/useCollapseState";
import {
    SIDEBAR_COLLAPSED_CLS,
    SIDEBAR_EXPANDED_CLS,
} from "../../lib/sidebarWidth";

vi.mock("../NavItems");
vi.mock("../../hooks/useCollapseState");

describe("NavDesktop", () => {
    const useCollapseStateMock = vi.mocked(useCollapseState);
    const toggleCollapse = vi.fn();
    let isCollapsed = false;

    beforeEach(() => {
        useCollapseStateMock.mockImplementation(() => ({
            isCollapsed,
            toggleCollapse,
        }));
    });

    it("should render sidebar inside the desktop nav", () => {
        render(
            <NavDesktop
                hasAccessCookie={true}
                isCollapsedInitialState={false}
            />,
        );
        const sidebar = screen.getByTestId("sidebar");
        expect(sidebar).toBeInTheDocument();
    });

    it("should apply the expanded class when initial state is not collapsed", () => {
        render(
            <NavDesktop
                hasAccessCookie={true}
                isCollapsedInitialState={false}
            />,
        );
        const navDesktop = screen.getByTestId("navDesktop");
        expect(navDesktop).toHaveClass(SIDEBAR_EXPANDED_CLS);
    });

    it("should apply the collapsed class when initial state is collapsed", () => {
        isCollapsed = true;
        render(
            <NavDesktop
                hasAccessCookie={true}
                isCollapsedInitialState={true}
            />,
        );
        const navDesktop = screen.getByTestId("navDesktop");
        expect(navDesktop).toHaveClass(SIDEBAR_COLLAPSED_CLS);
    });

    it("should call toggleCollapse when collapse button is clicked", async () => {
        isCollapsed = false;
        const user = userEvent.setup();

        render(
            <NavDesktop
                hasAccessCookie={true}
                isCollapsedInitialState={true}
            />,
        );

        const button = screen.getByTestId("sidebarCollapseButton");
        await user.click(button);
        expect(toggleCollapse).toHaveBeenCalled();
    });
});
