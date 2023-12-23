import { render, screen } from "@testing-library/react";

import AuthPage from "../AuthPage";

describe("AuthPage", () => {
    it("should render login form with link to signup", () => {
        render(<AuthPage login />);

        expect(screen.getByTestId("loginForm")).toBeInTheDocument();
        expect(screen.queryByTestId("signupForm")).not.toBeInTheDocument();

        expect(screen.getByTestId("signupLink")).toBeInTheDocument();
    });

    it("should render signup form with link to login", () => {
        render(<AuthPage signup />);
        expect(screen.getByTestId("signupForm")).toBeInTheDocument();
        expect(screen.queryByTestId("loginForm")).not.toBeInTheDocument();

        expect(screen.getByTestId("loginLink")).toBeInTheDocument();
    });
});
