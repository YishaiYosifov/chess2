import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";

import NavDesktop from "../NavDesktop";
import React from "react";
import useCollapseState from "../useCollapseState";

vi.mock("../NavItems");
vi.mock("../useCollapseState");

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

    it("should render with collapsed state when isCollapsed is true", () => {
        isCollapsed = true;
        render(
            <NavDesktop isLoggedIn={true} isCollapsedInitialState={false} />,
        );

        const aside = screen.getByTestId("navDesktop");
        expect(aside).toHaveAttribute("data-is-collapsed", "true");
        expect(screen.getByAltText("Logo")).toBeInTheDocument();
        expect(screen.getByText("UpperNavItems")).toBeInTheDocument();
        expect(screen.getByText("LowerNavItems")).toBeInTheDocument();

        const collapseButton = screen.getByTestId("navDesktopCollapseButton");
        expect(collapseButton).toBeInTheDocument();
        expect(collapseButton).toHaveTextContent("");
    });

    it("should render with expanded state when isCollapsed is false", () => {
        isCollapsed = false;
        render(<NavDesktop isLoggedIn={true} isCollapsedInitialState={true} />);

        const aside = screen.getByTestId("navDesktop");
        expect(aside).toHaveAttribute("data-is-collapsed", "false");

        expect(screen.getByAltText("Logo with text")).toBeInTheDocument();
        expect(screen.getByText("UpperNavItems")).toBeInTheDocument();
        expect(screen.getByText("LowerNavItems")).toBeInTheDocument();

        const collapseButton = screen.getByTestId("navDesktopCollapseButton");
        expect(collapseButton).toBeInTheDocument();
        expect(collapseButton).toHaveTextContent("Collapse");
    });

    it("should call toggleCollapse when collapse button is clicked", async () => {
        isCollapsed = false;
        const user = userEvent.setup();
        render(<NavDesktop isLoggedIn={true} isCollapsedInitialState={true} />);
        const button = screen.getByTestId("navDesktopCollapseButton");
        await user.click(button);
        expect(toggleCollapse).toHaveBeenCalled();
    });

    it("should navigate to home when clicking on logo", () => {
        render(
            <NavDesktop isLoggedIn={false} isCollapsedInitialState={false} />,
        );

        expect(screen.getByTestId("navDesktopLogo")).toHaveAttribute(
            "href",
            "/",
        );
    });
});
