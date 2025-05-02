import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import NavItem from "../NavItem";

describe("NavItem", () => {
    it("should renders children when not collapsed", () => {
        render(<NavItem href="/">Test Item</NavItem>);
        expect(screen.getByText("Test Item")).toBeInTheDocument();
    });

    it("should not render children when collapsed", () => {
        render(
            <NavItem isCollapsed href="/">
                Test Item
            </NavItem>,
        );
        expect(screen.queryByText("Test Item")).not.toBeInTheDocument();
    });

    it("should render an icon if provided", () => {
        render(
            <NavItem icon={<span>Icon</span>} href="/">
                Test Item
            </NavItem>,
        );
        expect(screen.getByText("Icon")).toBeInTheDocument();
    });

    it("should apply custom className", () => {
        render(
            <NavItem className="custom-class" href="/">
                Test Item
            </NavItem>,
        );
        const element = screen.getByText("Test Item");
        expect(element).toHaveClass("custom-class");
    });

    it("should use the default Link component if 'as' is not provided", () => {
        render(<NavItem href="/test">Test Item</NavItem>);
        const linkElement = screen.getByRole("link", { name: "Test Item" });
        expect(linkElement).toHaveAttribute("href", "/test");
    });

    it("should use a custom component when 'as' is provided", () => {
        const CustomComponent = vi.fn(
            ({ children }: { children: React.ReactNode }) => (
                <div>{children}</div>
            ),
        );
        render(<NavItem as={CustomComponent}>Test Item</NavItem>);
        expect(CustomComponent).toHaveBeenCalled();
        expect(screen.getByText("Test Item")).toBeInTheDocument();
    });

    it("should handle hover opacity transition", async () => {
        render(<NavItem href="/">Test Item</NavItem>);
        const element = screen.getByText("Test Item");
        expect(element).toHaveClass("hover:opacity-70");
    });
});
