import { render, screen } from "@testing-library/react";
import Selector from "../Selector";
import userEvent from "@testing-library/user-event";

describe("Selector", () => {
    const options = [
        { label: "Option 1", value: 1 },
        { label: "Option 2", value: 2 },
        { label: "Option 3", value: 3 },
    ];

    it("should render all options", () => {
        render(<Selector options={options} />);

        options.forEach((option) => {
            expect(screen.getByText(option.label)).toBeInTheDocument();
        });
    });

    it("should select the default value if provided", () => {
        render(<Selector options={options} defaultValue={2} />);

        const selectedButton = screen.getByText("Option 2");
        expect(selectedButton).toHaveAttribute("disabled");
        expect(selectedButton.className).toContain("border-secondary");
    });

    it("should select the first option if defaultValue is not provided", () => {
        render(<Selector options={options} />);

        const firstButton = screen.getByText("Option 1");
        expect(firstButton).toHaveAttribute("disabled");
        expect(firstButton.className).toContain("border-secondary");
    });

    it("should call onChange with the correct value when an option is clicked", async () => {
        const onChange = vi.fn();
        const user = userEvent.setup();
        render(<Selector options={options} onChange={onChange} />);

        const secondButton = screen.getByText("Option 2");
        await user.click(secondButton);

        expect(onChange).toHaveBeenCalledWith(2);
    });

    it("should update selected state when a different option is clicked", async () => {
        const user = userEvent.setup();
        render(<Selector options={options} />);

        const firstButton = screen.getByText("Option 1");
        const thirdButton = screen.getByText("Option 3");

        expect(firstButton).toHaveAttribute("disabled");
        expect(thirdButton).not.toHaveAttribute("disabled");

        await user.click(thirdButton);

        expect(thirdButton).toHaveAttribute("disabled");
        expect(firstButton).not.toHaveAttribute("disabled");
    });

    it("should fallback to first option if defaultValue does not exist in options", () => {
        render(<Selector options={options} defaultValue={999} />);

        const firstButton = screen.getByText("Option 1");
        expect(firstButton).toHaveAttribute("disabled");
    });
});
