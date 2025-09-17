import { render, screen } from "@testing-library/react";
import LeaderboardResetCountdown from "../LeaderboardResetCountdown";

describe("LeaderboardResetCountdown", () => {
    beforeEach(() => {
        vi.useFakeTimers();
    });

    it("should render countdown correctly", () => {
        const now = new Date(Date.UTC(2025, 8, 13, 23, 59, 50)); // day 13 23:59:50
        vi.setSystemTime(now);

        render(<LeaderboardResetCountdown />);

        expect(
            screen.getByTestId("leaderboardResetCountdown"),
        ).toHaveTextContent("resets in 17:00:00:10");
    });
});
