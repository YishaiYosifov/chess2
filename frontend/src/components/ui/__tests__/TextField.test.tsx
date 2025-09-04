import { render, screen } from "@testing-library/react";
import InputField from "../InputField";

describe("InputField", () => {
    it("should render a default input element", () => {
        render(<InputField data-testid="testInput" />);

        const input = screen.getByTestId("testInput");
        expect(input).toBeInTheDocument();
        expect(input.tagName.toLowerCase()).toBe("input");
    });

    it("should render a custom component when 'as' prop is provided", () => {
        render(<InputField as="textarea" data-testid="testInput" />);

        const textarea = screen.getByTestId("testInput");
        expect(textarea).toBeInTheDocument();
        expect(textarea.tagName.toLowerCase()).toBe("textarea");
    });

    it("should apply the provided className in addition to default classes", () => {
        render(<InputField className="custom-class" data-testid="testInput" />);

        const input = screen.getByTestId("testInput");

        expect(input).toHaveClass("custom-class");
        expect(input).toHaveClass("bg-background/50");
    });

    it("should render the icon when 'icon' prop is provided", () => {
        render(<InputField icon={<span data-testid="testIcon">icon</span>} />);

        const icon = screen.getByTestId("testIcon");
        expect(icon).toBeInTheDocument();
        expect(icon.textContent).toBe("icon");
    });

    it("should forward other props to the underlying input element", () => {
        render(<InputField disabled data-testid="testInput" />);

        const input = screen.getByTestId("testInput");
        expect(input).toBeDisabled();
    });
});
