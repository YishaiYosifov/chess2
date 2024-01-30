import { render, screen } from "@testing-library/react";
import { Mock } from "vitest";

import { createFormRenderer, responseErrFactory } from "@/lib/utils/testUtils";
import LoginForm, { LoginFormValues } from "../LoginForm";
import { mockRouter } from "@/mockUtils/mockRouter";
import constants from "@/lib/constants";
import { authApi } from "@/lib/apis";

vi.mock("@/lib/apis");

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
        [new Error(), constants.GENERIC_ERROR],
        [responseErrFactory(null, { status: 500 }), constants.GENERIC_ERROR],
        [
            responseErrFactory(null, { status: 401 }),
            "Wrong username / password",
        ],
    ])(
        "should correctly handle submit failures",
        async (response, statusText) => {
            const mockLogin = authApi.login as Mock;
            mockLogin.mockRejectedValue(response);
            await renderAndFillLogin();

            const status = screen.getByTestId("formStatus");
            expect(status.textContent).toBe(statusText);
        }
    );

    it("should redirect when successfull", async () => {
        const { replace } = mockRouter();

        const mockLogin = authApi.login as Mock;
        mockLogin.mockResolvedValue({});
        await renderAndFillLogin();

        expect(replace).toHaveBeenCalledWith("/");
    });
});
