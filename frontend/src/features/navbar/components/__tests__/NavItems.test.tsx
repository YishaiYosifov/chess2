import { render, screen } from "@testing-library/react";

import { UpperNavItems, LowerNavItems } from "../NavItems";
import constants from "@/lib/constants";

describe("UpperNavItems", () => {
    it("should render with the correct href when not authenticated", () => {
        render(<UpperNavItems hasAccessCookie={false} />);

        expect(screen.getByText("Sign In").closest("a")).toHaveAttribute(
            "href",
            constants.PATHS.SIGNIN,
        );
        expect(screen.getByText("Play").closest("a")).toHaveAttribute(
            "href",
            constants.PATHS.PLAY,
        );
        expect(screen.getByText("Home").closest("a")).toHaveAttribute(
            "href",
            "/",
        );
        expect(screen.getByText("Quests").closest("a")).toHaveAttribute(
            "href",
            constants.PATHS.QUESTS,
        );
        expect(screen.getByText("Donate").closest("a")).toHaveAttribute(
            "href",
            constants.PATHS.DONATE,
        );

        expect(screen.queryByText("Profile")).not.toBeInTheDocument();
    });

    it("should render with the correct href when authenticated", () => {
        render(<UpperNavItems hasAccessCookie={true} />);

        expect(screen.getByText("Profile").closest("a")).toHaveAttribute(
            "href",
            constants.PATHS.PROFILE,
        );
        expect(screen.queryByText("Sign In")).not.toBeInTheDocument();
    });
});

describe("LowerNavItems", () => {
    it("should render with the correct href when not authenticated", () => {
        render(<LowerNavItems hasAccessCookie={false} />);

        expect(screen.queryByText("Guide")?.closest("a")).toHaveAttribute(
            "href",
            constants.PATHS.GUIDE,
        );
        expect(screen.queryByText("Settings")).not.toBeInTheDocument();
        expect(screen.queryByText("Logout")).not.toBeInTheDocument();
    });

    it("should render with the correct href when authenticated", () => {
        render(<LowerNavItems hasAccessCookie={true} />);

        expect(screen.queryByText("Guide")?.closest("a")).toHaveAttribute(
            "href",
            constants.PATHS.GUIDE,
        );
        expect(screen.getByText("Settings").closest("a")).toHaveAttribute(
            "href",
            constants.PATHS.SETTINGS_BASE,
        );
        expect(screen.getByText("Logout").closest("a")).toHaveAttribute(
            "href",
            constants.PATHS.LOGOUT,
        );
    });
});
