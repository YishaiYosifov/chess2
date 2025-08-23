import { render, screen } from "@testing-library/react";
import TextField from "../TextField";

describe("TextField", () => {
    it("should render an input with a label", () => {
        render(<TextField label="Username" />);
        expect(screen.getByLabelText("Username")).toBeInTheDocument();
        expect(screen.getByLabelText("Username").tagName).toBe("INPUT");
    });

    it("should render an input without a label using aria-label", () => {
        render(<TextField aria-label="Search" />);
        const element = screen.getByLabelText("Search");
        expect(element).toBeInTheDocument();
        expect(element.tagName).toBe("INPUT");
    });

    it("should render a textarea when as='textarea'", () => {
        render(<TextField as="textarea" label="Message" />);
        const element = screen.getByLabelText("Message");
        expect(element).toBeInTheDocument();
        expect(element.tagName).toBe("TEXTAREA");
    });

    it("should render an input with an icon", () => {
        const Icon = () => <span>Icon</span>;
        render(<TextField label="With Icon" icon={<Icon />} />);
        expect(screen.getByText("Icon")).toBeInTheDocument();
    });

    it("should apply custom className", () => {
        render(<TextField label="Custom Class" className="custom-class" />);
        const element = screen.getByLabelText("Custom Class");
        expect(element).toHaveClass("custom-class");
    });
});
