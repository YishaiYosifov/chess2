import { render, screen, fireEvent } from "@testing-library/react";
import NavMobile from "../NavMobile";
import userEvent from "@testing-library/user-event";

vi.mock("../NavItems");

describe("NavMobile Component", () => {
    it("should render the mobile navbar with logo", () => {
        render(<NavMobile />);
        const navbar = screen.getByTestId("navbarMobile");
        const logo = screen.getByAltText("logo");

        expect(navbar).toBeInTheDocument();
        expect(logo).toBeInTheDocument();
    });

    it("should toggle the mobile menu when the button is clicked", async () => {
        const user = userEvent.setup();
        render(<NavMobile />);

        const toggleButton = screen.getByRole("button");
        const mobileNav = screen.getByTestId("navbarMobileOpened");

        // Initially, the mobile nav should be hidden
        expect(mobileNav).toHaveClass("hidden");

        // Click the toggle button to open the menu
        await user.click(toggleButton);
        expect(mobileNav).toHaveClass("flex");
        expect(mobileNav).not.toHaveClass("hidden");

        // Click the toggle button again to close the menu
        await user.click(toggleButton);
        expect(mobileNav).toHaveClass("hidden");
        expect(mobileNav).not.toHaveClass("flex");
    });

    it("should close the mobile menu when clicking inside the nav", () => {
        render(<NavMobile />);
        const toggleButton = screen.getByRole("button");
        const mobileNav = screen.getByTestId("navbarMobileOpened");

        // Open the menu
        fireEvent.click(toggleButton);
        expect(mobileNav).toHaveClass("flex");

        // Click inside the nav to close it
        fireEvent.click(mobileNav);
        expect(mobileNav).toHaveClass("hidden");
    });
});
