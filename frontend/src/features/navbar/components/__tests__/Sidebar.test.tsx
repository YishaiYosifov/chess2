import { render, screen } from "@testing-library/react";
import Sidebar from "../Sidebar";
import userEvent from "@testing-library/user-event";

vi.mock("../NavItems");

describe("Sidebar", () => {
    it("should render the sidebar with correct data attributes", () => {
        render(<Sidebar isCollapsed={false} hasAccessCookie={true} />);
        const sidebar = screen.getByTestId("sidebar");

        expect(sidebar).toBeInTheDocument();
        expect(sidebar).toHaveAttribute("data-is-collapsed", "false");
        expect(sidebar).toHaveAttribute("aria-label", "sidebar");
    });

    it("should reflect collapsed state in data attribute", () => {
        render(<Sidebar isCollapsed={true} hasAccessCookie={true} />);
        const sidebar = screen.getByTestId("sidebar");
        expect(sidebar.getAttribute("data-is-collapsed")).toBe("true");
    });

    it("should render the correct logo depending on collapsed state", () => {
        const { rerender } = render(
            <Sidebar isCollapsed={false} hasAccessCookie={true} />,
        );
        expect(screen.getByAltText("Logo with text")).toBeInTheDocument();

        rerender(<Sidebar isCollapsed={true} hasAccessCookie={true} />);
        expect(screen.getByAltText("Logo")).toBeInTheDocument();
    });

    it.each([true, false])(
        "should render UpperNavItems with correct prop",
        (hasAccessCookie) => {
            render(
                <Sidebar
                    isCollapsed={false}
                    hasAccessCookie={hasAccessCookie}
                />,
            );

            const upperNav = screen.getByTestId("upperNavItems");
            expect(upperNav).toBeInTheDocument();
            expect(upperNav).toHaveAttribute(
                "data-has-access-cookie",
                hasAccessCookie.toString(),
            );
        },
    );

    it.each([true, false])(
        "should render LowerNavItems with correct prop",
        (hasAccessCookie) => {
            render(
                <Sidebar
                    isCollapsed={false}
                    hasAccessCookie={hasAccessCookie}
                />,
            );

            const lowerNav = screen.getByTestId("lowerNavItems");
            expect(lowerNav).toBeInTheDocument();
            expect(lowerNav).toHaveAttribute(
                "data-has-access-cookie",
                hasAccessCookie.toString(),
            );
        },
    );

    it("should render the collapse button and call toggleCollapse on click", async () => {
        const toggleCollapse = vi.fn();
        const user = userEvent.setup();
        render(
            <Sidebar
                isCollapsed={false}
                hasAccessCookie={true}
                toggleCollapse={toggleCollapse}
            />,
        );
        const button = screen.getByTestId("sidebarCollapseButton");

        expect(button).toBeInTheDocument();
        await user.click(button);
        expect(toggleCollapse).toHaveBeenCalled();
    });
});
