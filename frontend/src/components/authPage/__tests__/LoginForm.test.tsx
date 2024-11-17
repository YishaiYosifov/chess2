import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { Mock } from "vitest";

import {
    fillForm,
    submitForm,
    responseErrFactory,
    renderWithAuthContext,
} from "@/lib/testUtils/formUtils";

import { mockRouter } from "@/lib/testUtils/mocks";
import { authApi } from "@/lib/apiClient/client";
import constants from "@/lib/constants";
import LoginForm from "../LoginForm";

vi.mock("@/lib/apiClient/client");

describe("LoginForm", () => {
    const loginValues = {
        "Username / Email": "a",
        Password: "12345678",
    };

    it("should display the login form", () => {
        render(<LoginForm />);

        expect(
            screen.getByLabelText("Username / Email", { selector: "input" }),
        ).toBeInTheDocument();
        expect(
            screen.getByLabelText("Password", { selector: "input" }),
        ).toBeInTheDocument();
        expect(
            screen.getByRole("button", { name: "Log In" }),
        ).toBeInTheDocument();
    });

    it.each([
        [new Error(), constants.GENERIC_ERROR],
        [responseErrFactory(500), constants.GENERIC_ERROR],
        [responseErrFactory(401), "Wrong username / email / password"],
    ])(
        "should correctly handle submit failures",
        async (response, statusText) => {
            const { replace } = mockRouter();
            const mockLogin = authApi.login as Mock;
            mockLogin.mockRejectedValue(response);

            const user = userEvent.setup();
            render(<LoginForm />);

            await fillForm(user, loginValues);
            await submitForm(user);

            await waitFor(() => {
                const status = screen.getByTestId("formStatus");
                expect(status.textContent).toBe(statusText);
                expect(replace).not.toHaveBeenCalled();
            });
        },
    );

    it("should redirect when successful", async () => {
        const { replace } = mockRouter();
        const setHasAuthCookies = vi.fn();

        const user = userEvent.setup();
        renderWithAuthContext(<LoginForm />, { setHasAuthCookies });

        // check the button is disabled before entering information
        const loginButton = screen.getByText<HTMLButtonElement>("Log In");
        expect(loginButton.disabled).toBeTruthy();

        // fill and submit the form
        await fillForm(user, loginValues);
        await user.click(loginButton);

        expect(setHasAuthCookies).toHaveBeenCalledWith(true);
        expect(replace).toHaveBeenCalledWith("/");
    });
});
