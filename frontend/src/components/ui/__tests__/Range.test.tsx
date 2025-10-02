import { render, screen } from "@testing-library/react";
import Range from "../Range";

describe("Range", () => {
    it("should render an input of type range", () => {
        render(<Range />);
        const input = screen.getByRole("slider");
        expect(input).toBeInTheDocument();
        expect(input).toHaveAttribute("type", "range");
    });

    it("applies default and custom className", () => {
        render(<Range className="custom-class" />);
        const input = screen.getByRole("slider");
        expect(input).toHaveClass("bg-primary");
        expect(input).toHaveClass("accent-secondary");
        expect(input).toHaveClass("custom-class");
    });

    it("passes other input props correctly", () => {
        render(<Range min={0} max={50} value={25} />);
        const input = screen.getByRole("slider");
        expect(input).toHaveAttribute("min", "0");
        expect(input).toHaveAttribute("max", "50");
        expect(input).toHaveValue("25");
    });
});
