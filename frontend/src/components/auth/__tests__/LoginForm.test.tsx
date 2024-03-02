import { render, screen, waitFor } from "@testing-library/react";
import { Mock } from "vitest";

import {
    fillForm,
    responseErrFactory,
    submitForm,
} from "@/lib/utils/testUtils";
import LoginForm, { LoginFormValues } from "../LoginForm";
import { mockRouter } from "@/mockUtils/mockRouter";
import constants from "@/lib/constants";
import { authApi } from "@/lib/apis";
import userEvent from "@testing-library/user-event";

vi.mock("@/lib/apis");

describe("LoginForm", () => {
    const loginValues = {
        username: "a",
        password: "b",
    };

    it("should display the login form", () => {
        render(<LoginForm />);
        expect(screen.queryByLabelText("username")).toBeInTheDocument();
        expect(screen.queryByLabelText("password")).toBeInTheDocument();
        expect(screen.queryByText("Log In")).toBeInTheDocument();
        expect(screen.queryByRole("form")).toBeInTheDocument();
    });

    it.each([
        [new Error(), constants.GENERIC_ERROR],
        [responseErrFactory(null, { status: 500 }), constants.GENERIC_ERROR],
        [
            responseErrFactory(null, { status: 401 }),
            "Wrong username / password",
        ],
    ])(
        "should correctly handle submit failures",
        async (response, statusText) => {
            const { replace } = mockRouter();

            const mockLogin = authApi.login as Mock;
            mockLogin.mockRejectedValue(response);
            render(<LoginForm />);
            submitForm();

            waitFor(() => {
                const status = screen.getByTestId("formStatus");
                expect(status.textContent).toBe(statusText);
                expect(replace).not.toHaveBeenCalled();
            });
        }
    );

    it("should redirect when successfull", async () => {
        const { replace } = mockRouter();

        const user = userEvent.setup();
        render(<LoginForm />);

        // check the button is disabled before entering information
        const loginButton = screen.getByText<HTMLButtonElement>("Log In");
        expect(loginButton.disabled).toBeTruthy();

        // fill and submit the form
        await fillForm(user, loginValues);
        await user.click(loginButton);

        expect(replace).toHaveBeenCalledWith("/");
    });
});
