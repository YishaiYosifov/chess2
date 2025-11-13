import { render, screen, within } from "@testing-library/react";

import { createFakePagedWinStreak } from "@/lib/testUtils/fakers/winStreakFaker";
import WinStreakLeaderboard from "../WinStreakLeaderboard";
import constants from "@/lib/constants";
import userEvent from "@testing-library/user-event";
import { getWinStreakLeaderboard } from "@/lib/apiClient";

vi.mock("@/lib/apiClient/definition");

describe("WinStreakLeaderboard", () => {
    const getWinStreakLeaderboardMock = vi.mocked(getWinStreakLeaderboard);

    it("should render heading and initial leaderboard items", () => {
        const initialLeaderboard = createFakePagedWinStreak();
        const firstItem = initialLeaderboard.items[0];

        render(
            <WinStreakLeaderboard initialLeaderboard={initialLeaderboard} />,
        );

        expect(
            screen.getByTestId(`leaderboardItem-${firstItem.profile.userId}`),
        ).toBeInTheDocument();

        const firstItemElement = screen.getAllByTestId(
            "winStreakLeaderboardItem",
        )[0];
        expect(firstItemElement).toHaveTextContent(
            `${firstItem.highestStreakGameTokens.length} Win Streak`,
        );
        expect(within(firstItemElement).getAllByRole("link")).toHaveLength(
            firstItem.highestStreakGameTokens.length,
        );
        for (let i = 0; i < firstItem.highestStreakGameTokens.length; i++) {
            const token = firstItem.highestStreakGameTokens[i];
            const link = within(firstItemElement).getByRole("link", {
                name: `Game #${i + 1}`,
            });
            expect(link).toHaveAttribute(
                "href",
                `${constants.PATHS.GAME}/${token}`,
            );
        }
        expect(screen.queryByText("No Players Yet")).not.toBeInTheDocument();
    });

    it("should fetch the next page when pagination is triggered", async () => {
        const user = userEvent.setup();
        const pageSize = 1;

        const firstPage = createFakePagedWinStreak({
            pagination: { pageSize, totalCount: 2, page: 0 },
        });
        const secondPage = createFakePagedWinStreak({
            pagination: { pageSize, totalCount: 2, page: 1 },
        });

        getWinStreakLeaderboardMock.mockResolvedValueOnce({
            data: secondPage,
            response: new Response(),
        });

        render(<WinStreakLeaderboard initialLeaderboard={firstPage} />);

        const nextBtn = screen.getByTestId("paginationNext");
        await user.click(nextBtn);

        expect(getWinStreakLeaderboardMock).toHaveBeenCalledWith({
            query: {
                Page: 1,
                PageSize: pageSize,
            },
        });

        const newItem = secondPage.items[0];
        expect(
            await screen.findByTestId(
                `leaderboardItem-${newItem.profile.userId}`,
            ),
        ).toBeInTheDocument();
    });

    it("should display no players text when there are no leaderboard spots", () => {
        const firstPage = createFakePagedWinStreak({
            pagination: { totalCount: 0 },
        });

        render(<WinStreakLeaderboard initialLeaderboard={firstPage} />);

        expect(screen.getByText("No Players Yet")).toBeInTheDocument();
    });
});
