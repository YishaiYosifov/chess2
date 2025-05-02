import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { Mock } from "vitest";

import {
    fillForm,
    submitForm,
    problemDetailsFactory,
    renderWithAuthContext,
} from "@/lib/testUtils/formUtils";

import { mockRouter } from "@/lib/testUtils/mocks";
import constants from "@/lib/constants";
import LoginForm from "../LoginForm";
import { signin } from "@/lib/apiClient";

vi.mock("@/lib/client/sdk.gen.ts");

describe("LoginForm", () => {
    const mockSignin = signin as Mock;
    const loginValues = {
        "Username / Email": "a",
        Password: "12345678",
    };

    it("should display the login form", () => {
        render(<LoginForm />);

        expect(
            screen.queryByLabelText("Username / Email", { selector: "input" }),
        ).toBeInTheDocument();
        expect(
            screen.queryByLabelText("Password", { selector: "input" }),
        ).toBeInTheDocument();
        expect(
            screen.queryByRole("button", { name: "Log In" }),
        ).toBeInTheDocument();
        expect(screen.queryByTestId("signupLink")).toBeInTheDocument();
    });

    it.each([
        [problemDetailsFactory(123), constants.GENERIC_ERROR],
        [problemDetailsFactory(500), constants.GENERIC_ERROR],
        [problemDetailsFactory(401), "Wrong username / email / password"],
    ])("should correctly handle submit failures", async (error, statusText) => {
        const { replace } = mockRouter();
        mockSignin.mockReturnValue({ error });

        const user = userEvent.setup();
        render(<LoginForm />);

        await fillForm(user, loginValues);
        await submitForm(user);

        await waitFor(() => {
            const status = screen.getByTestId("formStatus");
            expect(status.textContent).toBe(statusText);
            expect(replace).not.toHaveBeenCalled();
        });
    });

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
