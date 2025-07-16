import { render, screen } from "@testing-library/react";
import { getGameResults, User } from "@/lib/apiClient";
import GameHistory from "../GameHistory";
import { createFakePagedGameSummaryResult } from "@/lib/testUtils/fakers/pagedGameSummaryResultFaker";
import { createFakeUser } from "@/lib/testUtils/fakers/userFaker";

vi.mock("@/lib/apiClient");

describe("GameHistory", () => {
    const getGameResultsMock = vi.mocked(getGameResults);
    let user: User;

    beforeEach(() => {
        user = createFakeUser();
    });

    it("renders initial game results", () => {
        const pagedResults = createFakePagedGameSummaryResult({
            count: 3,
            overrides: { page: 0, pageSize: 3, totalPages: 1 },
        });

        render(
            <GameHistory
                initialGameResults={pagedResults}
                profileViewpoint={user}
            />,
        );

        for (const game of pagedResults.items) {
            expect(screen.getByText(game.gameToken)).toBeInTheDocument();
        }
    });

    it("renders pagination buttons when there are multiple pages", () => {
        const pagedResults = createFakePagedGameSummaryResult({
            count: 9,
            overrides: { page: 1, pageSize: 3 },
        });

        render(
            <GameHistory
                initialGameResults={pagedResults}
                profileViewpoint={user}
            />,
        );

        expect(
            screen.getByRole("button", { name: "First" }),
        ).toBeInTheDocument();
        expect(screen.getByRole("button", { name: "‹" })).toBeInTheDocument();
        expect(screen.getByRole("button", { name: "›" })).toBeInTheDocument();
        expect(
            screen.getByRole("button", { name: "Last" }),
        ).toBeInTheDocument();
    });

    it("calls getGameResults when navigating to the next page", async () => {
        const initialPage = 1;
        const initialResults = createFakePagedGameSummaryResult({
            count: 9,
            overrides: { page: initialPage, pageSize: 3 },
        });

        const nextPage = 2;
        const nextResults = createFakePagedGameSummaryResult({
            count: 9,
            overrides: { page: nextPage, pageSize: 3 },
        });

        getGameResultsMock.mockResolvedValueOnce({ data: nextResults });

        render(
            <GameHistory
                initialGameResults={initialResults}
                profileViewpoint={user}
            />,
        );

        fireEvent.click(screen.getByRole("button", { name: "›" }));

        await waitFor(() =>
            expect(getGameResults).toHaveBeenCalledWith({
                path: { userId: mockUser.userId },
                query: { Page: nextPage, PageSize: 10 }, // Matches hardcoded PageSize in component
            }),
        );

        for (const game of nextResults.items) {
            expect(await screen.findByText(game.gameId)).toBeInTheDocument();
        }
    });

    it("disables pagination buttons while loading", async () => {
        let resolveFetch: (data: any) => void;
        const fetchPromise = new Promise((res) => (resolveFetch = res));

        (getGameResults as any).mockReturnValueOnce(fetchPromise);

        const pagedResults = createFakePagedGameSummaryResult({
            count: 9,
            overrides: { page: 1, pageSize: 3, totalPages: 3 },
        });

        render(
            <GameHistory
                initialGameResults={pagedResults}
                profileViewpoint={mockUser}
            />,
        );

        fireEvent.click(screen.getByRole("button", { name: "›" }));

        expect(screen.getByRole("button", { name: "›" })).toBeDisabled();

        resolveFetch!({
            data: createFakePagedGameSummaryResult({
                count: 9,
                overrides: { page: 2, pageSize: 3, totalPages: 3 },
            }),
        });

        await waitFor(() =>
            expect(
                screen.getByRole("button", { name: "›" }),
            ).not.toBeDisabled(),
        );
    });

    it("handles API errors gracefully", async () => {
        const initialResults = createFakePagedGameSummaryResult({
            count: 6,
            overrides: { page: 0, pageSize: 3, totalPages: 2 },
        });

        (getGameResults as any).mockResolvedValueOnce({
            error: "Network error",
        });

        render(
            <GameHistory
                initialGameResults={initialResults}
                profileViewpoint={mockUser}
            />,
        );

        fireEvent.click(screen.getByRole("button", { name: "›" }));

        await waitFor(() =>
            expect(getGameResults).toHaveBeenCalledWith({
                path: { userId: "123" },
                query: { Page: 1, PageSize: 10 },
            }),
        );

        // Ensure original page is still visible
        for (const game of initialResults.items) {
            expect(screen.getByText(game.gameId)).toBeInTheDocument();
        }
    });
});
