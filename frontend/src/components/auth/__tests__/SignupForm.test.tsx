import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { Mock } from "vitest";

import {
    fillForm,
    responseErrFactory,
    submitForm,
} from "@/lib/utils/testUtils";
import constants from "@/lib/constants";

import { mockRouter } from "@/mockUtils/mockRouter";
import SignupForm from "../SignupForm";
import { authApi } from "@/lib/apis";

vi.mock("@/lib/apis");

describe("SignupForm", () => {
    const signupMock = authApi.signup as Mock;
    const loginValues = {
        username: "a",
        email: "a@b.c",
        password: "123456Aa",
    };

    it("should display the signup form", () => {
        render(<SignupForm />);
        expect(screen.queryByLabelText("username")).toBeInTheDocument();
        expect(screen.queryByLabelText("email")).toBeInTheDocument();
        expect(screen.queryByLabelText("password")).toBeInTheDocument();
        expect(screen.queryByRole("form")).toBeInTheDocument();
    });

    it("should display errors on invalid fields", async () => {
        const user = userEvent.setup();

        const { container } = render(<SignupForm />);
        await fillForm(user, {
            username: "1",
            email: "a",
            password: "a",
        });

        const errors = container.querySelectorAll(".invalid-feedback");
        expect(errors).toHaveLength(3);
        for (const error of errors) expect(error.textContent).not.toBe("");
    });

    it.each([new Error(), responseErrFactory("{}", { status: 500 })])(
        "should set status on unknown error",
        async (response) => {
            signupMock.mockRejectedValue(response);
            render(<SignupForm />);
            submitForm();

            waitFor(() =>
                expect(
                    screen.queryByText(constants.GENERIC_ERROR)
                ).toBeInTheDocument()
            );
        }
    );

    it("should set errors on conflict", async () => {
        signupMock.mockRejectedValue(
            responseErrFactory(
                '{ "detail": { "email": "this is a test error" } }',
                { status: 409 }
            )
        );
        render(<SignupForm />);
        submitForm();

        waitFor(() =>
            expect(
                screen.queryByText("this is a test error")
            ).toBeInTheDocument()
        );
    });

    it("should redirect when successful", async () => {
        const { push } = mockRouter();

        const user = userEvent.setup();
        render(<SignupForm />);

        const signupButton = screen.getByText<HTMLButtonElement>("Sign Up");
        expect(signupButton.disabled).toBeTruthy();
        await fillForm(user, loginValues);
        await user.click(signupButton);

        expect(push).toHaveBeenCalledWith("/login");
    });
});
