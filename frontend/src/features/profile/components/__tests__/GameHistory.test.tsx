import { render, screen, waitFor } from "@testing-library/react";
import {
    GameSummary,
    getGameResults,
    PagedResultOfGameSummaryDto,
    User,
} from "@/lib/apiClient";
import GameHistory from "../GameHistory";
import { createFakePagedGameSummary } from "@/lib/testUtils/fakers/pagedGameSummaryFaker";
import { createFakeUser } from "@/lib/testUtils/fakers/userFaker";
import { problemDetailsFactory } from "@/lib/testUtils/formUtils";
import userEvent from "@testing-library/user-event";
import constants from "@/lib/constants";

vi.mock("@/lib/apiClient");

describe("GameHistory", () => {
    const getGameResultsMock = vi.mocked(getGameResults);
    let userMock: User;

    beforeEach(() => {
        userMock = createFakeUser();
    });

    it("should render initial games and pagination buttons", () => {
        const initialGameResults = createFakePagedGameSummary({
            pagination: { page: 0, pageSize: 5, totalCount: 15 },
        });

        render(
            <GameHistory
                initialGameResults={initialGameResults}
                profileViewpoint={userMock}
            />,
        );

        testGameRowsVisible(initialGameResults.items);
        expect(screen.getByTestId("paginationPage0")).toBeInTheDocument();
        expect(screen.getByTestId("paginationPage1")).toBeInTheDocument();
        expect(screen.getByTestId("paginationPage2")).toBeInTheDocument();
    });

    it("should fetch the next page when pagination button is clicked and update games", async () => {
        const initialGameResults = createFakePagedGameSummary({
            pagination: { page: 0, pageSize: 5, totalCount: 15 },
        });

        const newPageResults = createFakePagedGameSummary({
            pagination: { page: 1, pageSize: 5, totalCount: 15 },
        });

        getGameResultsMock.mockResolvedValue({
            data: newPageResults,
            response: new Response(),
        });

        render(
            <GameHistory
                initialGameResults={initialGameResults}
                profileViewpoint={userMock}
            />,
        );

        const page2Button = screen.getByTestId("paginationPage1");
        await userEvent.click(page2Button);

        testGameRowsVisible(newPageResults.items);
        expect(getGameResultsMock).toHaveBeenCalledWith({
            path: { userId: userMock.userId },
            query: { Page: 1, PageSize: initialGameResults.pageSize },
        });
    });

    it("should disable pagination buttons while loading", async () => {
        const initialGameResults = createFakePagedGameSummary();

        type GetGameResultsReturnType = {
            data: PagedResultOfGameSummaryDto;
            response: Response;
        };

        let resolveFetch!: (value: GetGameResultsReturnType) => void;
        const fetchPromise = new Promise<GetGameResultsReturnType>(
            (resolve) => {
                resolveFetch = resolve;
            },
        );
        getGameResultsMock.mockReturnValue(fetchPromise);

        render(
            <GameHistory
                initialGameResults={initialGameResults}
                profileViewpoint={userMock}
            />,
        );

        // click and disable
        expect(screen.getByTestId("paginationPage1")).not.toBeDisabled();
        await userEvent.click(screen.getByTestId("paginationPage1"));
        expect(screen.getByTestId("paginationPage1")).toBeDisabled();

        // resolve fetch and reenable
        resolveFetch({ data: initialGameResults, response: new Response() });
        await waitFor(() => expect(getGameResultsMock).toHaveBeenCalled());
        expect(screen.getByTestId("paginationPage1")).not.toBeDisabled();
    });

    it("should handle API errors gracefully", async () => {
        const initialResults = createFakePagedGameSummary({
            pagination: { totalCount: 20 },
        });

        getGameResultsMock.mockResolvedValueOnce({
            data: undefined,
            error: problemDetailsFactory(400),
            response: new Response(),
        });

        const user = userEvent.setup();
        render(
            <GameHistory
                initialGameResults={initialResults}
                profileViewpoint={userMock}
            />,
        );

        await user.click(screen.getByTestId("paginationNext"));

        expect(getGameResults).toHaveBeenCalledWith({
            path: { userId: userMock.userId },
            query: {
                Page: 1,
                PageSize: constants.PAGINATION_PAGE_SIZE.GAME_SUMMARY,
            },
        });

        testGameRowsVisible(initialResults.items);
    });

    function testGameRowsVisible(games: GameSummary[]) {
        for (const game of games) {
            expect(
                screen.getByTestId(`gameRow-${game.gameToken}`),
            ).toBeInTheDocument();
        }
    }
});
