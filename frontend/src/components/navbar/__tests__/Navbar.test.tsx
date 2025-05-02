import { render, screen } from "@testing-library/react";
import Navbar from "../Navbar";

describe("Navbar Component", () => {
    it("should render NavMobile and NavDesktop components", () => {
        render(<Navbar />);

        expect(screen.getByTestId("navbarMobile")).toBeInTheDocument();
        expect(screen.getByTestId("navbarDesktop")).toBeInTheDocument();
    });

    it("should passe isCollapsedInitialState prop to NavDesktop", () => {
        const isCollapsedInitialState = true;
        render(<Navbar isCollapsedInitialState={isCollapsedInitialState} />);

        const navDesktop = screen.getByTestId("navbarDesktop");
        expect(navDesktop).toHaveAttribute(
            "data-is-collapsed",
            `${isCollapsedInitialState}`,
        );
    });
});
