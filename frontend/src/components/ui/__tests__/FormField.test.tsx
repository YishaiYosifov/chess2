import { render, screen } from "@testing-library/react";
import { Formik, Form } from "formik";
import userEvent from "@testing-library/user-event";
import FormField from "../FormField";

describe("FormField", () => {
    it("should render the child input with the correct props", () => {
        render(
            <Formik initialValues={{ testField: "" }} onSubmit={() => {}}>
                <Form>
                    <FormField name="testField">
                        <input data-testid="testInput" />
                    </FormField>
                </Form>
            </Formik>,
        );

        const input = screen.getByTestId<HTMLInputElement>("testInput");
        expect(input).toBeInTheDocument();
        expect(input.name).toBe("testField");
    });

    it("should render a label when provided", () => {
        render(
            <Formik initialValues={{ testField: "" }} onSubmit={() => {}}>
                <Form>
                    <FormField name="testField" label="test label">
                        <input data-testid="testInput" />
                    </FormField>
                </Form>
            </Formik>,
        );

        const input = screen.getByTestId("testInput");
        const label = screen.getByText<HTMLLabelElement>("test label");

        // Check that the label is linked to the input via the generated id
        expect(label).toBeInTheDocument();
        expect(input).toBeInTheDocument();
        expect(label.htmlFor).toBe(input.id);
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
                    <FormField name="testField">
                        <input />
                    </FormField>
                    <button type="submit" data-testid="testSubmit">
                        submit
                    </button>
                </Form>
            </Formik>,
        );

        await user.click(screen.getByTestId("testSubmit"));

        const errorMessage = await screen.findByTestId("fieldError");
        expect(errorMessage).toBeInTheDocument();
        expect(errorMessage).toHaveTextContent("This field is required");
    });

    it("should pass props correctly to a child icon element", () => {
        render(
            <Formik initialValues={{ testField: "" }} onSubmit={() => {}}>
                <Form>
                    <FormField name="testField">
                        <>
                            <input />
                            <span data-testid="icon" />
                        </>
                    </FormField>
                </Form>
            </Formik>,
        );

        expect(screen.getByTestId("icon")).toBeInTheDocument();
    });
});
