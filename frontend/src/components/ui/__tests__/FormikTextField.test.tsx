import { render, screen } from "@testing-library/react";
import { Formik, Form } from "formik";
import userEvent from "@testing-library/user-event";
import FormikTextField from "../FormikField";

describe("FormikTextField", () => {
    it("should render the input field with the correct props", () => {
        render(
            <Formik initialValues={{ testField: "" }} onSubmit={() => {}}>
                <Form>
                    <FormikTextField
                        as="input"
                        name="testField"
                        placeholder="Enter text"
                    />
                </Form>
            </Formik>,
        );

        const input = screen.getByPlaceholderText(
            "Enter text",
        ) as HTMLInputElement;
        expect(input).toBeInTheDocument();
        expect(input.name).toBe("testField");
    });

    it("should render a label when provided", () => {
        render(
            <Formik initialValues={{ testField: "" }} onSubmit={() => {}}>
                <Form>
                    <FormikTextField
                        as="input"
                        name="testField"
                        label="Test Label"
                    />
                </Form>
            </Formik>,
        );

        expect(screen.getByText("Test Label")).toBeInTheDocument();
        expect(screen.getByLabelText("Test Label")).toBeInTheDocument();
    });

    it("should display an error message when validation fails", async () => {
        const user = userEvent.setup();

        render(
            <Formik
                initialValues={{ testField: "" }}
                onSubmit={() => {}}
                validate={(values) => {
                    const errors: { testField?: string } = {};
                    if (!values.testField)
                        errors.testField = "This field is required";
                    return errors;
                }}
            >
                <Form>
                    <FormikTextField as="input" name="testField" />
                    <button type="submit">Submit</button>
                </Form>
            </Formik>,
        );

        await user.click(screen.getByText("Submit"));

        const errorMessage = await screen.findByTestId("fieldError");
        expect(errorMessage).toBeInTheDocument();
        expect(errorMessage).toHaveTextContent("This field is required");
    });

    it("should render an icon when provided", () => {
        render(
            <Formik initialValues={{ testField: "" }} onSubmit={() => {}}>
                <Form>
                    <FormikTextField
                        as="input"
                        name="testField"
                        icon={<span data-testid="icon" />}
                    />
                </Form>
            </Formik>,
        );

        expect(screen.getByTestId("icon")).toBeInTheDocument();
    });
});
