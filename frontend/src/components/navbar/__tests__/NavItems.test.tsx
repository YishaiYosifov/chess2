import { render, screen } from "@testing-library/react";

import { UpperNavItems, LowerNavItems } from "../NavItems";
import AuthContextProvider from "@/contexts/authContext";

describe("NavItems", () => {
    it("renders UpperNavItems with unauthenticated links when not authenticated", () => {
        render(
            <AuthContextProvider hasAuthCookies={false}>
                <UpperNavItems />
            </AuthContextProvider>,
        );

        expect(screen.getByText("Login")).toBeInTheDocument();
        expect(screen.getByText("Signup")).toBeInTheDocument();
        expect(screen.getByText("Play")).toBeInTheDocument();
        expect(screen.getByText("Home")).toBeInTheDocument();
        expect(screen.getByText("Donate")).toBeInTheDocument();
    });

    it("renders UpperNavItems with authenticated links when authenticated", () => {
        render(
            <AuthContextProvider hasAuthCookies>
                <UpperNavItems />
            </AuthContextProvider>,
        );

        expect(screen.getByText("Profile")).toBeInTheDocument();
        expect(screen.getByText("Play")).toBeInTheDocument();
        expect(screen.getByText("Home")).toBeInTheDocument();
        expect(screen.getByText("Donate")).toBeInTheDocument();
    });

    it("renders LowerNavItems with authenticated links when authenticated", () => {
        render(
            <AuthContextProvider hasAuthCookies>
                <LowerNavItems />
            </AuthContextProvider>,
        );

        expect(screen.getByText("Settings")).toBeInTheDocument();
        expect(screen.getByText("Logout")).toBeInTheDocument();
    });

    it("does not render LowerNavItems when not authenticated", () => {
        render(
            <AuthContextProvider hasAuthCookies={false}>
                <LowerNavItems />
            </AuthContextProvider>,
        );

        expect(screen.queryByText("Settings")).not.toBeInTheDocument();
        expect(screen.queryByText("Logout")).not.toBeInTheDocument();
    });
});
