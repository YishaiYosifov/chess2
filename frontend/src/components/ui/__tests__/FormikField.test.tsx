import { render, screen } from "@testing-library/react";
import { Formik, Form } from "formik";
import userEvent from "@testing-library/user-event";
import FormikField from "../../helpers/FormikField";

describe("FormikField", () => {
    it("should render the input field with the correct props", () => {
        render(
            <Formik initialValues={{ testField: "" }} onSubmit={() => {}}>
                <Form>
                    <FormikField
                        asInput="input"
                        name="testField"
                        placeholder="Enter text"
                    />
                </Form>
            </Formik>,
        );

        const input = screen.getByPlaceholderText("Enter text");
        expect(input).toBeInTheDocument();
        expect(input).toHaveAttribute("name", "testField");
    });

    it("should display an error message when there is a validation error", async () => {
        render(
            <Formik
                initialValues={{ testField: "" }}
                onSubmit={() => {}}
                validate={(values) => {
                    const errors: { testField?: string } = {};
                    if (!values.testField) {
                        errors.testField = "This field is required";
                    }
                    return errors;
                }}
            >
                <Form>
                    <FormikField asInput="input" name="testField" />
                    <button type="submit">Submit</button>
                </Form>
            </Formik>,
        );

        const user = userEvent.setup();
        const button = screen.getByText("Submit");
        await user.click(button);

        const errorMessage = screen.getByTestId("fieldError");
        expect(errorMessage).toBeInTheDocument();
        expect(errorMessage).toHaveTextContent("This field is required");
    });
});
