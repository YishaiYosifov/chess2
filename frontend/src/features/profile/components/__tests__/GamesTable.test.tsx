import { render, screen } from "@testing-library/react";

import { createFakeGameSummary } from "@/lib/testUtils/fakers/gameSummaryFaker";
import { createFakeUser } from "@/lib/testUtils/fakers/userFaker";
import GamesTable from "../GamesTable";
import { GameSummary, PublicUser } from "@/lib/apiClient";
import {
    createFakePlayerSummary,
    createFakePlayerSummaryFromUser,
} from "@/lib/testUtils/fakers/playerSummaryFaker";

describe("GamesTable", () => {
    let gamesMock: GameSummary[];
    let userMock: PublicUser;

    beforeEach(() => {
        userMock = createFakeUser();
        gamesMock = Array.from({
            length: 3,
        }).map(() =>
            createFakeGameSummary({
                whitePlayer: createFakePlayerSummaryFromUser(userMock),
                blackPlayer: createFakePlayerSummary(),
            }),
        );
    });

    it("should render the correct table headers", () => {
        render(<GamesTable games={gamesMock} profileViewpoint={userMock} />);
        const expectedHeaders = ["Players", "Results", "Date"];

        const headers = Array.from(
            screen.getByTestId("gamesTableHeader").children,
        );

        expectedHeaders.forEach((expectedHeader, i) =>
            expect(headers[i].textContent).toBe(expectedHeader),
        );
    });

    it("should render game rows correctly", () => {
        render(<GamesTable games={gamesMock} profileViewpoint={userMock} />);

        const usernamesWhite = screen.getAllByTestId("gameRowWhiteUsername");
        const usernamesBlack = screen.getAllByTestId("gameRowBlackUsername");
        gamesMock.forEach((expectedGame, i) => {
            expect(usernamesWhite[i].textContent).toBe(
                expectedGame.whitePlayer?.userName,
            );

            expect(usernamesBlack[i].textContent).toBe(
                expectedGame.blackPlayer?.userName,
            );
        });
    });

    it("should render no games text when there are no games", () => {
        render(<GamesTable games={[]} profileViewpoint={userMock} />);

        const regularRow = screen.queryAllByTestId("gameRow");
        expect(regularRow).toHaveLength(0);

        const noGamesRow = screen.getByTestId("noGamesRow");
        expect(noGamesRow?.textContent).toBe(
            "This user hasn't played any games yet",
        );
    });
});
