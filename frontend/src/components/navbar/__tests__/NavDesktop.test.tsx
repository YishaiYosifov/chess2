import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import Cookies from "js-cookie";

import constants from "@/lib/constants";

import NavDesktop from "../NavDesktop";
import React from "react";

vi.mock("js-cookie");
vi.mock("../NavItems");

describe("NavDesktop", () => {
    afterEach(() => {
        vi.clearAllMocks();
    });

    it("should render correctly when not collapsed", () => {
        render(<NavDesktop isCollapsedInitialState={false} />);

        expect(screen.getByAltText("Logo with text")).toBeInTheDocument();
        expect(screen.getByText("UpperNavItems")).toBeInTheDocument();
        expect(screen.getByText("LowerNavItems")).toBeInTheDocument();

        const collapseButton = screen.getByTestId("collapseButton");
        expect(collapseButton).toBeInTheDocument();
        expect(collapseButton).toHaveTextContent("Collapse");
    });

    it("should render correctly when collapsed", () => {
        render(<NavDesktop isCollapsedInitialState={true} />);

        expect(screen.getByAltText("Logo")).toBeInTheDocument();
        expect(screen.getByText("UpperNavItems")).toBeInTheDocument();
        expect(screen.getByText("LowerNavItems")).toBeInTheDocument();

        const collapseButton = screen.getByTestId("collapseButton");
        expect(collapseButton).toBeInTheDocument();
        expect(collapseButton).toHaveTextContent("");
    });

    it("should toggle collapse state and sets/removes cookie", async () => {
        const setCookieMock = vi.spyOn(Cookies, "set");
        const removeCookieMock = vi.spyOn(Cookies, "remove");
        const user = userEvent.setup();

        render(<NavDesktop isCollapsedInitialState={false} />);

        const collapseButton = screen.getByText("Collapse");
        await user.click(collapseButton);

        expect(setCookieMock).toHaveBeenCalledWith(
            constants.COOKIES.SIDEBAR_COLLAPSED,
            "1",
            expect.any(Object),
        );

        await user.click(collapseButton);

        expect(removeCookieMock).toHaveBeenCalledWith(
            constants.COOKIES.SIDEBAR_COLLAPSED,
        );
    });

    it("should apply correct width classes based on collapse state", () => {
        const { unmount } = render(
            <NavDesktop isCollapsedInitialState={false} />,
        );
        expect(screen.getByTestId("navbarDesktop")).toHaveClass("w-64");

        unmount();
        render(<NavDesktop isCollapsedInitialState={true} />);
        expect(screen.getByTestId("navbarDesktop")).toHaveClass("w-25");
    });

    it("should apply correct width classes based collapse button", async () => {
        const user = userEvent.setup();
        render(<NavDesktop isCollapsedInitialState={false} />);

        expect(screen.getByTestId("navbarDesktop")).toHaveClass("w-64");

        const collapseButton = screen.getByTestId("collapseButton");
        await user.click(collapseButton);
        expect(screen.getByTestId("navbarDesktop")).toHaveClass("w-25");

        await user.click(collapseButton);
        expect(screen.getByTestId("navbarDesktop")).toHaveClass("w-64");
    });
});
