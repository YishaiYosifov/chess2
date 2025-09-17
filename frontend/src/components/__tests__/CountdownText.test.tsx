import { act, render, screen } from "@testing-library/react";
import CountdownText from "../CountdownText";

describe("CountdownText", () => {
    beforeEach(() => {
        vi.useFakeTimers();
    });

    it("should render initial countdown correctly", () => {
        const now = new Date(Date.UTC(2025, 8, 13, 23, 59, 50)); // 23:59:50
        vi.setSystemTime(now);

        const getTimeUntil = () => new Date(Date.UTC(2025, 8, 14, 0, 0, 0)); // 00:00:00

        render(
            <CountdownText getTimeUntil={getTimeUntil}>
                {({ countdown }) => (
                    <span data-testid="countdownText">{countdown}</span>
                )}
            </CountdownText>,
        );

        expect(screen.getByTestId("countdownText")).toHaveTextContent(
            "00:00:10",
        );
    });

    it("should update countdown every second", () => {
        const now = new Date(Date.UTC(2025, 8, 13, 23, 59, 50));
        vi.setSystemTime(now);

        const getTimeUntil = () => new Date(Date.UTC(2025, 8, 14, 0, 0, 0)); // 10s away

        render(
            <CountdownText getTimeUntil={getTimeUntil}>
                {({ countdown }) => (
                    <span data-testid="countdownText">{countdown}</span>
                )}
            </CountdownText>,
        );

        act(() => {
            vi.advanceTimersByTime(3000);
        });

        expect(screen.getByTestId("countdownText")).toHaveTextContent(
            "00:00:07",
        );
    });

    it("should call onDateReached when time is up", () => {
        const now = new Date(Date.UTC(2025, 8, 13, 23, 59, 59));
        vi.setSystemTime(now);

        const getTimeUntil = vi.fn(
            () => new Date(Date.UTC(2025, 8, 14, 0, 0, 0)),
        ); // 1s away
        const onDateReached = vi.fn();

        render(
            <CountdownText
                getTimeUntil={getTimeUntil}
                onDateReached={onDateReached}
            >
                {({ countdown }) => (
                    <span data-testid="countdownText">{countdown}</span>
                )}
            </CountdownText>,
        );

        act(() => {
            vi.advanceTimersByTime(1000);
        });

        expect(onDateReached).toHaveBeenCalled();
        expect(getTimeUntil).toHaveBeenCalledTimes(2);
    });

    it("should render days when more than 24h away", () => {
        const now = new Date(Date.UTC(2025, 8, 13, 0, 0, 0));
        vi.setSystemTime(now);

        const getTimeUntil = () => new Date(Date.UTC(2025, 8, 15, 1, 2, 3)); // 2 days, 1:02:03

        render(
            <CountdownText getTimeUntil={getTimeUntil}>
                {({ countdown }) => (
                    <span data-testid="countdownText">{countdown}</span>
                )}
            </CountdownText>,
        );

        expect(screen.getByTestId("countdownText")).toHaveTextContent(
            "2:01:02:03",
        );
    });
});
