import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";

import {
    fillForm,
    mockRouter,
    createFormRenderer,
} from "@/lib/utils/testUtils";
import SignupForm, { SignupFormValues } from "../SignupForm";
import { signup } from "@/lib/utils/authUtils";
import { Mock } from "vitest";

vi.mock("@/lib/utils/authUtils", () => ({ signup: vi.fn() }));

describe("SignupForm", () => {
    const signupMock = signup as Mock;
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

    it.each([null, { status: 500, json: vi.fn() }])(
        "should set status on unknown error",
        async (response) => {
            signupMock.mockResolvedValue(response);
            await renderAndFillSignup();

            expect(
                screen.getByText("Something went wrong.")
            ).toBeInTheDocument();
        }
    );

    it("should set errors on conflict", async () => {
        signupMock.mockResolvedValue({
            status: 409,
            json: () => ({ detail: { email: "this is a test error" } }),
        });

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
