import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import Input, { PasswordInput } from "../Input";

describe("Input Component", () => {
    it("should render an input with a label", () => {
        render(<Input label="Username" />);
        expect(screen.getByLabelText("Username")).toBeInTheDocument();
    });

    it("should render an input without a label", () => {
        render(<Input aria-label="Search" />);
        expect(screen.getByLabelText("Search")).toBeInTheDocument();
    });

    it("should render an input with an icon", () => {
        const Icon = () => <span>Icon</span>;
        render(<Input label="With Icon" icon={<Icon />} />);
        expect(screen.getByText("Icon")).toBeInTheDocument();
    });

    it("should apply custom className", () => {
        render(<Input label="Custom Class" className="custom-class" />);
        const inputElement = screen.getByLabelText("Custom Class");
        expect(inputElement).toHaveClass("custom-class");
    });
});

describe("PasswordInput Component", () => {
    it("should render a password input by default", () => {
        render(<PasswordInput name="password" />);
        const inputElement = screen.getByLabelText("password");
        expect(inputElement).toHaveAttribute("type", "password");
    });

    it("should toggles password visibility", async () => {
        render(<PasswordInput name="password" />);
        const user = userEvent.setup();
        const inputElement = screen.getByLabelText("password");

        // Initially password type
        expect(inputElement).toHaveAttribute("type", "password");

        // Click to show password
        await user.click(screen.getByTestId("togglePasswordVisibility"));
        expect(inputElement).toHaveAttribute("type", "text");

        // Click to hide password
        await user.click(screen.getByTestId("togglePasswordVisibility"));
        expect(inputElement).toHaveAttribute("type", "password");
    });

    it("should render with a placeholder", () => {
        render(
            <PasswordInput name="password" placeholder="Enter your password" />,
        );
        expect(
            screen.getByPlaceholderText("Enter your password"),
        ).toBeInTheDocument();
    });
});
