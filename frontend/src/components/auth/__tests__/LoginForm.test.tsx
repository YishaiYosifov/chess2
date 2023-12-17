import userEvent, { UserEvent } from "@testing-library/user-event";
import { render, screen } from "@testing-library/react";

import {
    FormValues,
    fillForm,
    mockRouter,
    submitForm,
} from "@/lib/utils/testUtils";
import { login } from "@/lib/utils/authUtils";
import LoginForm, { LoginFormValues } from "../LoginForm";

jest.mock("@/lib/utils/authUtils", () => ({
    login: jest.fn(),
}));

describe("LoginForm", () => {
    const fillLoginForm = async (
        user: UserEvent,
        fieldValues: FormValues<LoginFormValues> = {
            username: "a",
            password: "b",
        }
    ): Promise<void> => fillForm(user, fieldValues);

    it("should display the login form", () => {
        render(<LoginForm />);
        expect(screen.getByLabelText("username")).toBeInTheDocument();
        expect(screen.getByLabelText("password")).toBeInTheDocument();
        expect(screen.getByTestId("submitForm")).toBeInTheDocument();
        expect(screen.getByRole("form")).toBeInTheDocument();
    });

    it.each([
        [null, "Something went wrong."],
        [{ status: 500, text: jest.fn() }, "Something went wrong."],
        [{ status: 401 }, "Wrong username / password"],
    ])(
        "should correctly handle submit failures",
        async (response, statusText) => {
            const user = userEvent.setup();

            const mockLogin = login as jest.Mock;
            mockLogin.mockResolvedValue(response);

            render(<LoginForm />);
            await fillLoginForm(user);
            await submitForm(user);

            const status = screen.getByTestId("formStatus");
            expect(status.textContent).toBe(statusText);
        }
    );

    it("should redirect when successfull", async () => {
        const user = userEvent.setup();
        const { replace } = mockRouter();

        const mockLogin = login as jest.Mock;
        mockLogin.mockResolvedValue({ ok: true });

        render(<LoginForm />);
        await fillLoginForm(user);
        await submitForm(user);
        expect(replace).toHaveBeenCalledWith("/");
    });
});
