import { render, screen } from "@testing-library/react";

import SignupForm from "../SignupForm";
import LoginForm from "../LoginForm";
import AuthPage from "../AuthPage";

describe("AuthPage", () => {
    it("should render login form with link to signup", () => {
        render(<AuthPage form={<LoginForm />} />);

        expect(screen.queryByTestId("loginForm")).toBeInTheDocument();
        expect(screen.queryByTestId("signupForm")).not.toBeInTheDocument();
    });

    it("should render signup form with link to login", () => {
        render(<AuthPage form={<SignupForm />} />);
        expect(screen.queryByTestId("signupForm")).toBeInTheDocument();
        expect(screen.queryByTestId("loginForm")).not.toBeInTheDocument();
    });
});
