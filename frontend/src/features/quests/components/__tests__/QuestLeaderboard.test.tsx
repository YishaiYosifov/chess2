import { render, screen } from "@testing-library/react";

import { createFakePagedUserQuestPoints } from "@/lib/testUtils/fakers/userQuestPointsFaker";
import { getQuestLeaderboard } from "@/lib/apiClient";
import QuestLeaderboard from "../QuestLeaderboard";
import userEvent from "@testing-library/user-event";

vi.mock("@/lib/apiClient/definition");

describe("QuestLeaderboard", () => {
    const getQuestLeaderboardMock = vi.mocked(getQuestLeaderboard);

    it("should render heading and initial leaderboard items", () => {
        const initialLeaderboard = createFakePagedUserQuestPoints();
        const firstItem = initialLeaderboard.items[0];

        render(<QuestLeaderboard initialLeaderboard={initialLeaderboard} />);

        expect(screen.getByTestId("questLeaderboardTitle")).toHaveTextContent(
            "Leaderboard",
        );
        expect(
            screen.getByTestId(
                `questLeaderboardRankDisplay-${firstItem.profile.userId}`,
            ),
        ).toBeInTheDocument();
        expect(
            screen.getByTestId(
                `questLeaderboardPoints-${firstItem.profile.userId}`,
            ),
        ).toHaveTextContent(`${firstItem.questPoints} points`);
        expect(screen.queryByText("No Players Yet")).not.toBeInTheDocument();
    });

    it("should apply podium colors and icons correctly", () => {
        const initialLeaderboard = createFakePagedUserQuestPoints();

        render(<QuestLeaderboard initialLeaderboard={initialLeaderboard} />);

        const podiumColors = ["bg-amber-400", "bg-slate-300", "bg-orange-400"];
        const podiumIcons = ["🥇", "🥈", "🥉"];

        initialLeaderboard.items.forEach((item, index) => {
            const rankEl = screen.getByTestId(
                `questLeaderboardRankDisplay-${item.profile.userId}`,
            );
            if (index < 3) {
                expect(rankEl).toHaveTextContent(podiumIcons[index]);
                expect(rankEl).toHaveClass(podiumColors[index]);
            } else {
                expect(rankEl).toHaveTextContent(`#${index + 1}`);
                expect(rankEl).toHaveClass("bg-text/70");
            }
        });
    });

    it("should fetch the next page when pagination is triggered", async () => {
        const user = userEvent.setup();
        const pageSize = 1;

        const firstPage = createFakePagedUserQuestPoints({
            pagination: { pageSize, totalCount: 2, page: 0 },
        });
        const secondPage = createFakePagedUserQuestPoints({
            pagination: { pageSize, totalCount: 2, page: 1 },
        });

        getQuestLeaderboardMock.mockResolvedValueOnce({
            data: secondPage,
            response: new Response(),
        });

        render(<QuestLeaderboard initialLeaderboard={firstPage} />);

        const nextBtn = screen.getByTestId("paginationNext");
        await user.click(nextBtn);

        expect(getQuestLeaderboardMock).toHaveBeenCalledWith({
            query: {
                Page: 1,
                PageSize: pageSize,
            },
        });

        const newItem = secondPage.items[0];
        expect(
            await screen.findByTestId(
                `questLeaderboardRankDisplay-${newItem.profile.userId}`,
            ),
        ).toBeInTheDocument();
    });

    it("should display no players text when there are no leaderboard spots", () => {
        const firstPage = createFakePagedUserQuestPoints({
            pagination: { totalCount: 0 },
        });

        render(<QuestLeaderboard initialLeaderboard={firstPage} />);

        expect(screen.getByText("No Players Yet")).toBeInTheDocument();
    });
});
