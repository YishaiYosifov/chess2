import { render, screen } from "@testing-library/react";
import PoolToggle, { PoolToggleRef } from "../PoolToggle";
import userEvent from "@testing-library/user-event";
import React, { useImperativeHandle, useRef } from "react";

describe("PoolToggle", () => {
    it("should render toggle button and rating labels", () => {
        render(<PoolToggle />);

        expect(screen.getByTestId("poolToggle")).toBeInTheDocument();
        expect(screen.getByText(/Casual/i)).toBeInTheDocument();
        expect(screen.getByText(/Rated/i)).toBeInTheDocument();
    });

    it("should toggle isRated value when clicked", async () => {
        const user = userEvent.setup();

        const ref = React.createRef<PoolToggleRef>();
        render(<PoolToggle ref={ref} />);

        const toggle = screen.getByTestId("poolToggle");

        expect(ref.current?.isRated).toBe(false);

        await user.click(toggle);
        expect(ref.current?.isRated).toBe(true);

        await user.click(toggle);
        expect(ref.current?.isRated).toBe(false);
    });
});
