import { render, screen } from "@testing-library/react";

import { createFormRenderer, mockRouter } from "@/lib/utils/testUtils";
import { login } from "@/lib/utils/authUtils";
import LoginForm, { LoginFormValues } from "../LoginForm";

jest.mock("@/lib/utils/authUtils", () => ({
    login: jest.fn(),
}));

describe("LoginForm", () => {
    const defaultFieldValues = {
        username: "a",
        password: "b",
    };

    const renderAndFillLogin = createFormRenderer<LoginFormValues>(
        <LoginForm />,
        defaultFieldValues
    );

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
            const mockLogin = login as jest.Mock;
            mockLogin.mockResolvedValue(response);
            await renderAndFillLogin();

            const status = screen.getByTestId("formStatus");
            expect(status.textContent).toBe(statusText);
        }
    );

    it("should redirect when successfull", async () => {
        const { replace } = mockRouter();

        const mockLogin = login as jest.Mock;
        mockLogin.mockResolvedValue({ ok: true });
        await renderAndFillLogin();

        expect(replace).toHaveBeenCalledWith("/");
    });
});
