import { render, screen } from "@testing-library/react";
import QuestLeaderboardResetCountdown from "../QuestLeaderboardResetCountdown";

describe("QuestLeaderboardResetCountdown", () => {
    beforeEach(() => {
        vi.useFakeTimers();
    });

    it("should render countdown correctly", () => {
        const now = new Date(Date.UTC(2025, 8, 13, 23, 59, 50)); // day 13 23:59:50
        vi.setSystemTime(now);

        render(<QuestLeaderboardResetCountdown />);

        expect(
            screen.getByTestId("leaderboardResetCountdown"),
        ).toHaveTextContent("resets in 17:00:00:10");
    });
});
