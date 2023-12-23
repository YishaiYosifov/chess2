import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";

import { FormField } from "../FormField";

describe("FormField", () => {
    it("should render FormField with label", () => {
        render(<FormField fieldName="testField" fieldLabel="Testing" />);

        expect(screen.getByLabelText("testField")).toBeInTheDocument();
        expect(screen.getByText("Testing")).toBeInTheDocument();
    });

    it("should render FormField without label", () => {
        render(<FormField fieldName="testField" />);

        expect(screen.queryByLabelText("testField")).toBeInTheDocument();
        expect(screen.queryByText("Test Label")).not.toBeInTheDocument();
    });

    it("should handles input change", async () => {
        const { container } = render(<FormField fieldName="testField" />);

        const input = container.querySelector("input")!;
        await userEvent.type(input, "Hello, World!");
        expect(input).toHaveValue("Hello, World!");
    });

    it("should handle children", () => {
        render(
            <FormField>
                <h1>child element</h1>
            </FormField>
        );
        expect(screen.getByText("child element")).toBeInTheDocument();
    });

    it("should handle validation", () => {
        const { container } = render(<FormField hasValidation />);
        const inputGroup = container.getElementsByClassName("input-group")[0];
        expect(inputGroup).toHaveClass("has-validation");
    });
});

// TODO!
describe.todo("FormikField", () => {});
