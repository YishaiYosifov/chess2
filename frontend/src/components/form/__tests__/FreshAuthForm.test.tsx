import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import FreshAuthForm from "../FreshAuthForm";

import { verifyPasswordMock } from "../__mocks__/PasswordVerificationModal";

vi.mock("../PasswordVerificationModal");
vi.mock("../CollapsibleForm");

describe("FreshAuthForm", () => {
    const FormContents = () => <input data-testid="testFormInput" />;

    it("should render password verification modal inside a collapsible form", () => {
        render(
            <FreshAuthForm>
                <FormContents />
            </FreshAuthForm>
        );

        expect(
            screen.queryByTestId("passwordVerificationModal")
        ).toBeInTheDocument();
        expect(screen.queryByTestId("collapsibleForm")).toBeInTheDocument();
        expect(screen.queryByTestId("testFormInput")).toBeInTheDocument();
    });

    it.each([true, false])(
        "does not submit when password verification fails",
        async (passwordVerified) => {
            const user = userEvent.setup();
            verifyPasswordMock.mockReturnValue(passwordVerified);

            const onSubmit = vi.fn();
            render(
                <FreshAuthForm onSubmit={onSubmit}>
                    <FormContents />
                </FreshAuthForm>
            );

            const submitButton = screen.getByTestId("collapsibleFormSubmit");
            await user.click(submitButton);

            if (passwordVerified) expect(onSubmit).toHaveBeenCalledOnce();
            else expect(onSubmit).not.toHaveBeenCalledOnce();
        }
    );
});
