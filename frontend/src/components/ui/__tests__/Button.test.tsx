import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import Button from "../Button";

describe("Button Component", () => {
    it("should render the button with children", () => {
        render(<Button>Click Me</Button>);
        expect(screen.getByText("Click Me")).toBeInTheDocument();
    });

    it("should apply additional class names", () => {
        render(<Button className="custom-class">Click Me</Button>);
        const button = screen.getByText("Click Me");
        expect(button).toHaveClass("custom-class");
    });

    it("should handle click events", async () => {
        const handleClick = vi.fn();
        const user = userEvent.setup();

        render(<Button onClick={handleClick}>Click Me</Button>);
        const button = screen.getByText("Click Me");
        await user.click(button);
        expect(handleClick).toHaveBeenCalledTimes(1);
    });

    it("should render as disabled when the disabled prop is passed", () => {
        render(<Button disabled>Click Me</Button>);
        const button = screen.getByText("Click Me");
        expect(button).toBeDisabled();
    });

    it("should apply disabled styles when disabled", () => {
        render(<Button disabled>Click Me</Button>);
        const button = screen.getByText("Click Me");
        expect(button).toHaveClass("disabled:brightness-70");
        expect(button).toHaveClass("disabled:text-text/50");
    });
});
