import { render, screen } from "@testing-library/react";
import Selector from "../Selector";
import userEvent from "@testing-library/user-event";

describe("Selector", () => {
    const options = [
        { label: "Option 1", value: 1 },
        { label: "Option 2", value: 2 },
        { label: "Option 3", value: 3 },
    ];

    it("should render all options with correct labels", () => {
        render(<Selector options={options} data-testid="selector" />);

        options.forEach((option) => {
            const button = screen.getByTestId(`selector-${option.value}`);
            expect(button).toBeInTheDocument();
            expect(button.textContent).toBe(option.label);
        });
    });

    it("should select the default value if provided", () => {
        render(<Selector options={options} value={2} data-testid="selector" />);

        const container = screen.getByTestId("selector");
        expect(container).toHaveAttribute("data-selected", "2");

        const selectedButton = screen.getByTestId("selector-2");
        expect(selectedButton).toBeDisabled();
        expect(selectedButton.className).toContain("border-secondary");
    });

    it("should select the first option if defaultValue is not provided", () => {
        render(<Selector options={options} data-testid="selector" />);

        const container = screen.getByTestId("selector");
        expect(container).toHaveAttribute("data-selected", "1");

        const firstButton = screen.getByTestId("selector-1");
        expect(firstButton).toBeDisabled();
        expect(firstButton.className).toContain("border-secondary");
    });

    it("should call onChange with the correct value when an option is clicked", async () => {
        const onChange = vi.fn();
        const user = userEvent.setup();
        render(
            <Selector
                options={options}
                name="testName"
                onChange={onChange}
                data-testid="selector"
            />,
        );

        await user.click(screen.getByTestId("selector-2"));

        expect(onChange).toHaveBeenCalledWith({
            target: { name: "testName", value: 2 },
        });

        const container = screen.getByTestId("selector");
        expect(container).toHaveAttribute("data-selected", "2");
    });

    it("should update selected state when a different option is clicked", async () => {
        const user = userEvent.setup();
        render(<Selector options={options} data-testid="selector" />);

        const container = screen.getByTestId("selector");

        expect(container).toHaveAttribute("data-selected", "1");
        expect(screen.getByTestId("selector-3")).not.toBeDisabled();

        await user.click(screen.getByTestId("selector-3"));

        expect(container).toHaveAttribute("data-selected", "3");
        expect(screen.getByTestId("selector-3")).toBeDisabled();
        expect(screen.getByTestId("selector-1")).not.toBeDisabled();
    });

    it("should fallback to first option if defaultValue does not exist in options", () => {
        render(
            <Selector options={options} value={999} data-testid="selector" />,
        );

        const container = screen.getByTestId("selector");
        expect(container).toHaveAttribute("data-selected", "1");

        const firstButton = screen.getByTestId("selector-1");
        expect(firstButton).toBeDisabled();
    });
});
