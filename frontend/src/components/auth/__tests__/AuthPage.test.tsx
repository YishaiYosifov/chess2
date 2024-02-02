import { render, screen } from "@testing-library/react";

import AuthPage from "../AuthPage";

describe("AuthPage", () => {
    it("should render login form with link to signup", () => {
        render(<AuthPage login />);

        expect(screen.queryByTestId("loginForm")).toBeInTheDocument();
        expect(screen.queryByTestId("signupForm")).not.toBeInTheDocument();

        expect(screen.queryByTestId("signupLink")).toBeInTheDocument();
    });

    it("should render signup form with link to login", () => {
        render(<AuthPage signup />);
        expect(screen.queryByTestId("signupForm")).toBeInTheDocument();
        expect(screen.queryByTestId("loginForm")).not.toBeInTheDocument();

        expect(screen.queryByTestId("loginLink")).toBeInTheDocument();
    });
});
