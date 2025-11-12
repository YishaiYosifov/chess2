import { render, screen } from "@testing-library/react";

import { createFakePrivateUser } from "@/lib/testUtils/fakers/userFaker";
import SessionProvider from "@/features/auth/contexts/sessionContext";
import WinStreakProfileStats from "../WinStreakProfile";
import { MyWinStreakStats } from "@/lib/apiClient";

describe("WinStreakProfileStats", () => {
    const userMock = createFakePrivateUser();

    it("should render the card with correct user info and streaks", () => {
        const stats: MyWinStreakStats = {
            rank: 10,
            highestStreak: 100,
            currentStreak: 10,
        };
        const totalPlayers = 100;

        render(
            <SessionProvider user={userMock}>
                <WinStreakProfileStats
                    stats={stats}
                    totalPlayers={totalPlayers}
                />
            </SessionProvider>,
        );

        const streakText = screen.getByTestId("winStreakProfileStatsStreaks");
        expect(streakText).toHaveTextContent(
            `Highest Streak: ${stats.highestStreak}`,
        );
        expect(streakText).toHaveTextContent(
            `Current Streak: ${stats.currentStreak}`,
        );

        expect(
            screen.getByTestId("minimalProfileRowUsername"),
        ).toHaveTextContent(userMock.userName);
        expect(screen.getByTestId("rankDisplayNumber")).toHaveTextContent(
            `#${stats.rank}`,
        );
        expect(screen.getByTestId("rankDisplayPercentile")).toHaveTextContent(
            `That's top 90.0%!`,
        );
    });

    it("should not render if not logged in", () => {
        const { container } = render(
            <SessionProvider user={null} fetchAttempted>
                <WinStreakProfileStats
                    stats={{ rank: 1, highestStreak: 2, currentStreak: 3 }}
                    totalPlayers={56}
                />
            </SessionProvider>,
        );
        expect(container).toBeEmptyDOMElement();
    });
});
