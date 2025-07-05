import { render, screen } from "@testing-library/react";
import { Formik } from "formik";
import userEvent from "@testing-library/user-event";
import FormikSubmitButton from "../FormikSubmitButton";

describe("FormikSubmitButton", () => {
    const renderWithFormik = (
        props: React.ComponentProps<typeof FormikSubmitButton> = {},
    ) => {
        render(
            <Formik initialValues={{}} onSubmit={vi.fn()} initialStatus={null}>
                <FormikSubmitButton {...props}>Submit</FormikSubmitButton>
            </Formik>,
        );
    };

    it("renders with default type as 'submit'", () => {
        renderWithFormik();
        const button = screen.getByTestId("submitFormButton");
        expect(button).toHaveAttribute("type", "submit");
    });

    it("uses the provided type if specified", () => {
        renderWithFormik({ type: "button" });
        const button = screen.getByTestId("submitFormButton");
        expect(button).toHaveAttribute("type", "button");
    });

    it("disables the button when disabled prop is true", () => {
        renderWithFormik({ disabled: true });
        const button = screen.getByTestId("submitFormButton");
        expect(button).toBeDisabled();
    });

    it("displays the status message when status is set", () => {
        render(
            <Formik
                initialValues={{}}
                onSubmit={vi.fn()}
                initialStatus="Error occurred"
            >
                <FormikSubmitButton>Submit</FormikSubmitButton>
            </Formik>,
        );
        const statusMessage = screen.getByTestId("formStatus");
        expect(statusMessage).toHaveTextContent("Error occurred");
    });

    it("enables the button when form is dirty and valid", async () => {
        const user = userEvent.setup();
        render(
            <Formik
                initialValues={{}}
                onSubmit={vi.fn()}
                initialTouched={{}}
                initialErrors={{}}
                initialStatus={null}
                isSubmitting={false}
                validateOnMount={true}
            >
                {({ setFieldValue }) => (
                    <>
                        <FormikSubmitButton>Submit</FormikSubmitButton>
                        <button onClick={() => setFieldValue("field", "value")}>
                            Make Dirty
                        </button>
                    </>
                )}
            </Formik>,
        );

        const button = screen.getByTestId("submitFormButton");
        expect(button).toBeDisabled();

        const makeDirtyButton = screen.getByText("Make Dirty");
        await user.click(makeDirtyButton);

        expect(button).not.toBeDisabled();
    });
});
