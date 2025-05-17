import { render, screen } from "@testing-library/react";

import { mockCookies } from "@/lib/testUtils/mocks";
import constants from "@/lib/constants";
import Navbar from "../Navbar";

vi.mock("next/headers");

describe("Navbar Component", () => {
    it("should render NavMobile and NavDesktop components", async () => {
        const page = await Navbar();
        render(page);

        expect(screen.getByTestId("navbarMobile")).toBeInTheDocument();
        expect(screen.getByTestId("navbarDesktop")).toBeInTheDocument();
    });

    it.each([true, false])(
        "should pass isCollapsed from the cookie",
        async (isCollapsed) => {
            if (isCollapsed) mockCookies(constants.COOKIES.SIDEBAR_COLLAPSED);

            const page = await Navbar();
            render(page);

            const navDesktop = screen.getByTestId("navbarDesktop");
            expect(navDesktop).toHaveAttribute(
                "data-is-collapsed",
                isCollapsed.toString(),
            );
        },
    );

    it.each([true, false])(
        "should pass hasAccessCookie from the cookie",
        async (hasCookie) => {
            if (hasCookie) mockCookies(constants.COOKIES.IS_AUTHED);

            const page = await Navbar();
            render(page);

            const settingsLink = screen.queryAllByText("Settings")[0];
            const loginLink = screen.queryAllByText("Login")[0];
            if (hasCookie) {
                expect(settingsLink).toBeInTheDocument();
                expect(loginLink).toBeUndefined();
            } else {
                expect(settingsLink).toBeUndefined();
                expect(loginLink).toBeInTheDocument();
            }
        },
    );
});
