import { act, render, screen } from "@testing-library/react";
import DoubleClickIndicator, { DoubleClickRef } from "../DoubleClickIndicator";
import React from "react";

describe("DoubleClickIndicator", () => {
    let ref: React.RefObject<DoubleClickRef | null>;

    beforeEach(() => {
        ref = React.createRef<DoubleClickRef>();
        vi.useFakeTimers();
    });

    it("should initially does not render anything", () => {
        render(<DoubleClickIndicator ref={ref} />);
        expect(
            screen.queryByTestId("doubleClickIndicator"),
        ).not.toBeInTheDocument();
    });

    it("should trigger shows the indicator", async () => {
        render(<DoubleClickIndicator ref={ref} />);

        await act(async () => {
            await ref.current!.trigger();
        });

        expect(screen.getByTestId("doubleClickIndicator")).toBeInTheDocument();
    });

    it("should disappear after timeout", async () => {
        render(<DoubleClickIndicator ref={ref} />);

        await act(async () => {
            await ref.current!.trigger();
        });

        expect(screen.getByTestId("doubleClickIndicator")).toBeInTheDocument();

        act(() => {
            vi.advanceTimersByTime(700);
        });

        expect(
            screen.queryByTestId("doubleClickIndicator"),
        ).not.toBeInTheDocument();
    });

    it("should hide the indicator immediately when cancelling", async () => {
        render(<DoubleClickIndicator ref={ref} />);

        await act(async () => {
            await ref.current!.trigger();
        });

        expect(screen.getByTestId("doubleClickIndicator")).toBeInTheDocument();

        await act(async () => {
            await ref.current!.cancel();
        });

        expect(
            screen.queryByTestId("doubleClickIndicator"),
        ).not.toBeInTheDocument();
    });

    it("should cancel previous timers when triggering", async () => {
        render(<DoubleClickIndicator ref={ref} />);

        await act(async () => {
            await ref.current!.trigger();
        });
        const timerCountBefore = vi.getTimerCount();

        await act(async () => {
            await ref.current!.trigger();
        });
        const timerCountAfter = vi.getTimerCount();

        // should not create additional timers
        expect(timerCountAfter).toBe(timerCountBefore);
    });
});
