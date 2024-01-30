import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { Mock } from "vitest";

import {
    createFormRenderer,
    fillForm,
    responseErrFactory,
} from "@/lib/utils/testUtils";
import constants from "@/lib/constants";

import SignupForm, { SignupFormValues } from "../SignupForm";
import { mockRouter } from "@/mockUtils/mockRouter";
import { authApi } from "@/lib/apis";

vi.mock("@/lib/apis");

describe("SignupForm", () => {
    const signupMock = authApi.signup as Mock;
    const defaultFieldValues = {
        username: "a",
        email: "a@b.c",
        password: "123456Aa",
    };

    const renderAndFillSignup = createFormRenderer<SignupFormValues>(
        <SignupForm />,
        defaultFieldValues
    );

    it("should display the signup form", () => {
        render(<SignupForm />);
        expect(screen.getByLabelText("username")).toBeInTheDocument();
        expect(screen.getByLabelText("email")).toBeInTheDocument();
        expect(screen.getByLabelText("password")).toBeInTheDocument();
        expect(screen.getByRole("form")).toBeInTheDocument();
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
            await renderAndFillSignup();

            expect(
                screen.getByText(constants.GENERIC_ERROR)
            ).toBeInTheDocument();
        }
    );

    it("should set errors on conflict", async () => {
        signupMock.mockRejectedValue(
            responseErrFactory(
                '{ "detail": { "email": "this is a test error" } }',
                { status: 409 }
            )
        );

        await renderAndFillSignup();
        expect(screen.getByText("this is a test error")).toBeInTheDocument();
    });

    it("should redirect when successfull", async () => {
        const { push } = mockRouter();
        signupMock.mockResolvedValue({ ok: true });

        await renderAndFillSignup();
        expect(push).toHaveBeenCalledWith("/login");
    });
});
