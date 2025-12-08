import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { usePathname } from "next/navigation";

import NavMobile from "../NavMobile";

describe("NavMobile", () => {
    const usePathnameMock = vi.mocked(usePathname);

    it("should render the mobile nav header and logo", () => {
        render(<NavMobile hasAccessCookie={true} />);
        const navMobile = screen.getByTestId("navMobile");

        expect(navMobile).toBeInTheDocument();
        const logo = screen.getByAltText("logo");
        expect(logo).toBeInTheDocument();
    });

    it("should have the sidebar hidden initially", () => {
        render(<NavMobile hasAccessCookie={true} />);

        const sidebarSlider = screen.getByTestId("sidebarSlider");
        expect(sidebarSlider).toHaveClass("-translate-x-full");
    });

    it("should open the sidebar when the toggle button is clicked", async () => {
        const user = userEvent.setup();
        render(<NavMobile hasAccessCookie={true} />);
        const toggleBtn = screen.getByTestId("sidebarToggle");

        await user.click(toggleBtn);

        const sidebarSlider = screen.getByTestId("sidebarSlider");
        expect(sidebarSlider).toHaveClass("translate-x-0");
    });

    it("should close the sidebar when clicking outside", async () => {
        const user = userEvent.setup();
        render(<NavMobile hasAccessCookie={true} />);
        const toggleBtn = screen.getByTestId("sidebarToggle");

        // open sidebar
        await user.click(toggleBtn);
        const sidebarSlider = screen.getByTestId("sidebarSlider");
        expect(sidebarSlider).toHaveClass("translate-x-0");

        // click outside
        await user.click(document.body);
        expect(sidebarSlider).toHaveClass("-translate-x-full");
    });

    it("should close the sidebar when pathname changes", async () => {
        usePathnameMock.mockReturnValue("/initial");
        const user = userEvent.setup();
        const { rerender } = render(<NavMobile hasAccessCookie={true} />);
        const toggleBtn = screen.getByTestId("sidebarToggle");

        // open sidebar
        await user.click(toggleBtn);
        const sidebarSlider = screen.getByTestId("sidebarSlider");
        expect(sidebarSlider).toHaveClass("translate-x-0");

        usePathnameMock.mockReturnValue("/other");
        rerender(<NavMobile hasAccessCookie={true} />);

        expect(sidebarSlider).toHaveClass("-translate-x-full");
    });

    it("should toggle the sidebar open and closed when clicking the button multiple times", async () => {
        const user = userEvent.setup();
        render(<NavMobile hasAccessCookie={true} />);

        const toggleBtn = screen.getByTestId("sidebarToggle");
        const sidebarSlider = screen.getByTestId("sidebarSlider");

        await user.click(toggleBtn);
        expect(sidebarSlider).toHaveClass("translate-x-0");

        await user.click(toggleBtn);
        expect(sidebarSlider).toHaveClass("-translate-x-full");
    });
});
