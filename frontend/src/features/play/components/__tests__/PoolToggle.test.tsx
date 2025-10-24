import { render, screen } from "@testing-library/react";
import PoolToggle from "../PoolToggle";
import userEvent from "@testing-library/user-event";
import React from "react";

describe("PoolToggle", () => {
    it("should render toggle button and rating labels", () => {
        render(<PoolToggle isRated={false} onToggle={vi.fn()} />);

        expect(screen.getByTestId("poolToggle")).toBeInTheDocument();
        expect(screen.getByText(/Casual/i)).toBeInTheDocument();
        expect(screen.getByText(/Rated/i)).toBeInTheDocument();
    });

    it("should position the slider correctly for isRated=false", () => {
        render(<PoolToggle isRated={false} onToggle={vi.fn()} />);
        const slider = screen.getByTestId("poolToggle")
            .firstChild as HTMLElement;
        expect(slider).toHaveClass("left-1");
    });

    it("should position the slider correctly for isRated=true", () => {
        render(<PoolToggle isRated={true} onToggle={vi.fn()} />);
        const slider = screen.getByTestId("poolToggle")
            .firstChild as HTMLElement;
        expect(slider).toHaveClass("left-[calc(100%-2.75rem)]");
    });

    it("should call onToggle with opposite value when clicked", async () => {
        const user = userEvent.setup();
        const onToggleMock = vi.fn();

        render(<PoolToggle isRated={false} onToggle={onToggleMock} />);
        const toggle = screen.getByTestId("poolToggle");

        await user.click(toggle);
        expect(onToggleMock).toHaveBeenCalledWith(true);

        await user.click(toggle);
        // still false prop, so each click sends opposite
        expect(onToggleMock).toHaveBeenCalledWith(true);
    });

    it("should toggle multiple times correctly", async () => {
        const user = userEvent.setup();

        let isRated = false;
        const handleToggle = (value: boolean) => {
            isRated = value;
            rerender(<PoolToggle isRated={isRated} onToggle={handleToggle} />);
        };

        const { rerender } = render(
            <PoolToggle isRated={isRated} onToggle={handleToggle} />,
        );
        const toggle = screen.getByTestId("poolToggle");

        await user.click(toggle);
        expect(isRated).toBe(true);

        await user.click(toggle);
        expect(isRated).toBe(false);
    });
});
