import { render, screen } from "@testing-library/react";

import FormField from "../FormField";

describe("FormField", () => {
    it("should render FormField with label", () => {
        render(<FormField label="Testing" />);

        const label = screen.getByTestId("formFieldLabel");
        expect(label).toBeInTheDocument();
        expect(label.textContent).toBe("Testing");
    });

    it("should render FormField without label", () => {
        render(<FormField />);

        expect(screen.queryByTestId("formFieldLabel")).not.toBeInTheDocument();
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
