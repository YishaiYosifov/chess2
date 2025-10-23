import { render, screen, within } from "@testing-library/react";

import {
    mockJsCookie,
    mockNextCookies,
} from "@/lib/testUtils/mocks/mockCookies";
import constants from "@/lib/constants";
import Navbar from "../Navbar";

vi.mock("next/headers");
vi.mock("js-cookie");

describe("Navbar Component", () => {
    it("should render NavMobile and NavDesktop components", async () => {
        const page = await Navbar();
        render(page);

        expect(screen.getByTestId("navMobile")).toBeInTheDocument();
        expect(screen.getByTestId("navDesktop")).toBeInTheDocument();
    });

    it.each([true, false])(
        "should pass isCollapsed from the cookie",
        async (isCollapsed) => {
            if (isCollapsed) {
                mockNextCookies(constants.COOKIES.SIDEBAR_COLLAPSED);
                mockJsCookie({ [constants.COOKIES.SIDEBAR_COLLAPSED]: "1" });
            }

            const page = await Navbar();
            render(page);

            const sidebar = within(
                screen.getByTestId("navDesktop"),
            ).getByTestId("sidebar");
            expect(sidebar).toHaveAttribute(
                "data-is-collapsed",
                isCollapsed.toString(),
            );
        },
    );

    it.each([true, false])(
        "should pass hasAccessCookie from the cookie",
        async (hasCookie) => {
            if (hasCookie) mockNextCookies(constants.COOKIES.IS_LOGGED_IN);

            const page = await Navbar();
            render(page);

            const settingsLink = screen.queryAllByText("Settings")[0];
            if (hasCookie) {
                expect(settingsLink).toBeInTheDocument();
            } else {
                expect(settingsLink).toBeUndefined();
            }
        },
    );
});
