import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";

import { FormValues, mockRouter, renderForm } from "@/lib/utils/testUtils";
import SignupForm, { SignupFormValues } from "../SignupForm";
import { signup } from "@/lib/utils/authUtils";

jest.mock("@/lib/utils/authUtils", () => ({ signup: jest.fn() }));

describe("SignupForm", () => {
    const signupMock = signup as jest.Mock;
    const defaultFieldValues = {
        username: "a",
        email: "a@b.c",
        password: "fbf@d2AS",
    };

    const renderSignupForm = async (
        fieldValues: FormValues<SignupFormValues> = defaultFieldValues,
        doSubmit: boolean = true
    ) => renderForm(<SignupForm />, fieldValues, doSubmit);

    it("should display the signup form", () => {
        render(<SignupForm />);
        expect(screen.getByLabelText("username")).toBeInTheDocument();
        expect(screen.getByLabelText("email")).toBeInTheDocument();
        expect(screen.getByLabelText("password")).toBeInTheDocument();
        expect(screen.getByRole("form")).toBeInTheDocument();
    });

    it("should display errors on invalid fields", async () => {
        const { container } = await renderSignupForm(
            {
                username: "1",
                email: "a",
                password: "a",
            },
            false
        );

        const errors = container.querySelectorAll(".invalid-feedback");
        expect(errors).toHaveLength(3);
        for (const error of errors) expect(error.textContent).not.toBe("");
    });

    it.each([null, { status: 500, json: jest.fn() }])(
        "should set status on unknown error",
        async (response) => {
            signupMock.mockResolvedValue(response);
            await renderSignupForm();

            expect(
                screen.getByText("Something went wrong.")
            ).toBeInTheDocument();
        }
    );

    it("should set errors on conflict", async () => {
        signupMock.mockResolvedValue({ status: 409, json: jest.fn() });
        const user = userEvent.setup();

        render(<SignupForm />);
    });

    it("should redirect when successfull", async () => {
        const { push } = mockRouter();
        signupMock.mockResolvedValue({ ok: true });

        await renderSignupForm();
        expect(push).toHaveBeenCalledWith("/login");
    });
});
