import { render, screen } from "@testing-library/react";

import { createFakePrivateUser } from "@/lib/testUtils/fakers/userFaker";
import SessionProvider from "@/features/auth/contexts/sessionContext";
import DailyQuestRankCard from "../DailyQuestRankCard";

describe("DailyQuestRankCard", () => {
    const userMock = createFakePrivateUser();

    it("should render the card with correct user info and quest points", () => {
        const questPoints = 15;
        const currentRank = 23;
        const totalPlayers = 456;

        render(
            <SessionProvider user={userMock}>
                <DailyQuestRankCard
                    questPoints={questPoints}
                    currentRank={currentRank}
                    totalPlayers={totalPlayers}
                />
            </SessionProvider>,
        );

        expect(
            screen.getByTestId("minimalProfileRowUsername"),
        ).toHaveTextContent(userMock.userName);
        expect(screen.getByTestId("dailyQuestRankPoints")).toHaveTextContent(
            `${questPoints} points`,
        );
        expect(screen.getByTestId("dailyQuestRankNumber")).toHaveTextContent(
            `#${currentRank}`,
        );

        const expectedPercentile = ((456 - 23) / 456) * 100;
        const progressFill = screen.getByTestId("progressBarFill");
        expect(progressFill.style.width).toBe(`${expectedPercentile}%`);

        expect(
            screen.getByTestId("dailyQuestRankPercentile"),
        ).toHaveTextContent(`That's top ${expectedPercentile.toFixed(1)}%!`);
    });

    it("should not render if not logged in", () => {
        const { container } = render(
            <SessionProvider user={null} fetchAttempted>
                <DailyQuestRankCard
                    questPoints={12}
                    currentRank={34}
                    totalPlayers={56}
                />
            </SessionProvider>,
        );
        expect(container).toBeEmptyDOMElement();
    });

    it("should calculate percentile correctly for top rank", () => {
        render(
            <SessionProvider user={userMock}>
                <DailyQuestRankCard
                    questPoints={200}
                    currentRank={1}
                    totalPlayers={100}
                />
            </SessionProvider>,
        );

        expect(
            screen.getByTestId("dailyQuestRankPercentile"),
        ).toHaveTextContent("That's top 99.0%!");
    });

    it("should calculate percentile correctly for last rank", () => {
        render(
            <SessionProvider user={userMock}>
                <DailyQuestRankCard
                    questPoints={50}
                    currentRank={100}
                    totalPlayers={100}
                />
            </SessionProvider>,
        );
        expect(
            screen.getByTestId("dailyQuestRankPercentile"),
        ).toHaveTextContent("That's top 0.0%!");
    });

    it("should handle 0 players", () => {
        render(
            <SessionProvider user={userMock}>
                <DailyQuestRankCard
                    questPoints={0}
                    currentRank={1}
                    totalPlayers={0}
                />
            </SessionProvider>,
        );

        expect(screen.getByTestId("dailyQuestRankNumber")).toHaveTextContent(
            "-",
        );
        expect(
            screen.getByTestId("dailyQuestRankPercentile"),
        ).toHaveTextContent("No players yet!");
    });
});
