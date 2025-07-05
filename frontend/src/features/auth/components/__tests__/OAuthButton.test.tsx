import { render, screen, fireEvent } from "@testing-library/react";
import OAuthButton from "../OAuthButton";
import React from "react";
import constants, { OAuthProvider } from "@/lib/constants";
import { mockRouter } from "@/lib/testUtils/mocks/mockRouter";

describe("OAuthButton", () => {
    const icon = <span data-testid="icon">Icon</span>;
    const loginText = "Login with Google";
    const oauthProvider = OAuthProvider.GOOGLE;

    it("should render the button with icon and text", () => {
        render(
            <OAuthButton
                icon={icon}
                loginText={loginText}
                oauthProvider={oauthProvider}
            />,
        );
        expect(screen.getByText(loginText)).toBeInTheDocument();
        expect(screen.getByTestId("icon")).toBeInTheDocument();
    });

    it("should apply custom className", () => {
        render(
            <OAuthButton
                icon={icon}
                loginText={loginText}
                oauthProvider={oauthProvider}
                className="custom-class"
            />,
        );
        const button = screen.getByRole("button");
        expect(button.className).toContain("custom-class");
    });

    it("should call router.push with correct URL on click", () => {
        const { push } = mockRouter();

        render(
            <OAuthButton
                icon={icon}
                loginText={loginText}
                oauthProvider={oauthProvider}
            />,
        );
        const button = screen.getByRole("button");
        fireEvent.click(button);

        expect(push).toHaveBeenCalledWith(`${constants.PATHS.OAUTH}google`);
    });

    it("should passe additional props to the button", () => {
        render(
            <OAuthButton
                icon={icon}
                loginText={loginText}
                oauthProvider={oauthProvider}
                data-testid="oauth-btn"
                disabled
            />,
        );
        const button = screen.getByTestId("oauth-btn");
        expect(button).toBeDisabled();
    });
});
