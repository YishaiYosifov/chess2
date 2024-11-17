import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { Mock } from "vitest";

import {
    fillForm,
    responseErrFactory,
    submitForm,
} from "@/lib/testUtils/formUtils";

import constants from "@/lib/constants";

import { mockRouter } from "@/lib/testUtils/mocks";
import { authApi } from "@/lib/apiClient/client";
import SignupForm from "../SignupForm";

vi.mock("@/lib/apiClient/client");

describe("SignupForm", () => {
    const signupMock = authApi.signup as Mock;
    const signupValues = {
        Username: "a",
        Email: "a@b.c",
        Password: "12345678",
    };

    it("should display the signup form", () => {
        render(<SignupForm />);
        expect(screen.queryByLabelText("Username")).toBeInTheDocument();
        expect(screen.queryByLabelText("Email")).toBeInTheDocument();
        expect(screen.queryByLabelText("Password")).toBeInTheDocument();
    });

    it("should display errors on invalid fields", async () => {
        const user = userEvent.setup();

        render(<SignupForm />);
        await fillForm(user, {
            Username: "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
            Email: "a@",
            Password: "a",
        });

        const errors = screen.getAllByTestId("fieldError");
        expect(errors).toHaveLength(3);
        for (const error of errors) expect(error.textContent).not.toBe("");
    });

    it.each([
        [new Error(), constants.GENERIC_ERROR],
        [responseErrFactory(500), constants.GENERIC_ERROR],
    ])("should correctly handle submit failures", async (response) => {
        signupMock.mockRejectedValue(response);
        const user = userEvent.setup();

        render(<SignupForm />);

        await fillForm(user, signupValues);
        await submitForm(user);

        waitFor(() =>
            expect(screen.queryByTestId("formStatus")).toHaveValue(
                constants.GENERIC_ERROR,
            ),
        );
    });

    it("should set errors on conflict", async () => {
        signupMock.mockRejectedValue(
            responseErrFactory(
                409,
                {
                    code: "General.Conflict",
                    detail: "username conflict",
                    metadata: { relatedField: "username" },
                },
                {
                    code: "General.Conflict",
                    detail: "email conflict",
                    metadata: { relatedField: "email" },
                },
            ),
        );
        const user = userEvent.setup();

        render(<SignupForm />);
        await fillForm(user, signupValues);
        await submitForm(user);

        await waitFor(() => {
            const usernameConflict = screen.queryByText("username conflict");
            const emailConflict = screen.queryByText("email conflict");

            expect(usernameConflict).toBeInTheDocument();
            expect(emailConflict).toBeInTheDocument();
        });
    });

    it("should redirect when successful", async () => {
        const { push } = mockRouter();

        const user = userEvent.setup();
        render(<SignupForm />);

        const signupButton = screen.getByText<HTMLButtonElement>("Sign Up");
        expect(signupButton.disabled).toBeTruthy();
        await fillForm(user, signupValues);
        await user.click(signupButton);

        expect(push).toHaveBeenCalledWith("/login");
    });
});
