import { render, screen } from "@testing-library/react";

import { UpperNavItems, LowerNavItems } from "../NavItems";

describe("UpperNavItems", () => {
    it("should render with unauthenticated links when not authenticated", () => {
        render(<UpperNavItems hasAccessCookie={false} />);

        expect(screen.getByText("Login")).toBeInTheDocument();
        expect(screen.getByText("Signup")).toBeInTheDocument();
        expect(screen.getByText("Play")).toBeInTheDocument();
        expect(screen.getByText("Home")).toBeInTheDocument();
        expect(screen.getByText("Donate")).toBeInTheDocument();
    });

    it("should render with authenticated links when authenticated", () => {
        render(<UpperNavItems hasAccessCookie={true} />);

        expect(screen.getByText("Profile")).toBeInTheDocument();
        expect(screen.getByText("Play")).toBeInTheDocument();
        expect(screen.getByText("Home")).toBeInTheDocument();
        expect(screen.getByText("Donate")).toBeInTheDocument();
    });

    it("should render with the correct href when not authenticated", () => {
        render(<UpperNavItems hasAccessCookie={false} />);

        expect(screen.getByText("Login").closest("a")).toHaveAttribute(
            "href",
            "/login",
        );
        expect(screen.getByText("Signup").closest("a")).toHaveAttribute(
            "href",
            "/signup",
        );
        expect(screen.getByText("Play").closest("a")).toHaveAttribute(
            "href",
            "/play",
        );
        expect(screen.getByText("Home").closest("a")).toHaveAttribute(
            "href",
            "/",
        );
        expect(screen.getByText("Donate").closest("a")).toHaveAttribute(
            "href",
            "/donate",
        );
    });

    it("should render with the correct href when authenticated", () => {
        render(<UpperNavItems hasAccessCookie={true} />);

        expect(screen.getByText("Profile").closest("a")).toHaveAttribute(
            "href",
            "/profile",
        );
    });
});

describe("LowerNavItems", () => {
    it("should render LowerNavItems with authenticated links when authenticated", () => {
        render(<LowerNavItems hasAccessCookie={true} />);

        expect(screen.getByText("Settings")).toBeInTheDocument();
        expect(screen.getByText("Logout")).toBeInTheDocument();
    });

    it("should not render LowerNavItems when not authenticated", () => {
        render(<LowerNavItems hasAccessCookie={false} />);

        expect(screen.queryByText("Settings")).not.toBeInTheDocument();
        expect(screen.queryByText("Logout")).not.toBeInTheDocument();
    });

    it("should render with the correct href when authenticated", () => {
        render(<LowerNavItems hasAccessCookie={true} />);

        expect(screen.getByText("Settings").closest("a")).toHaveAttribute(
            "href",
            "/settings",
        );
        expect(screen.getByText("Logout").closest("a")).toHaveAttribute(
            "href",
            "/logout",
        );
    });
});
