import { render, screen } from "@testing-library/react";

import { createFakeFinishedGame } from "@/lib/testUtils/fakers/gameFaker";
import { createFakeUser } from "@/lib/testUtils/fakers/userFaker";
import GamesTable from "../GamesTable";
import { FinishedGame } from "@/types/tempModels";
import { User } from "@/lib/apiClient";

describe("GamesTable", () => {
    let gamesMock: FinishedGame[];
    let userMock: User;

    beforeEach(() => {
        userMock = createFakeUser();
        gamesMock = Array.from({ length: 3 }, () =>
            createFakeFinishedGame({
                userWhite: userMock,
                userBlack: createFakeUser(),
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

        const usernamesWhite = screen.getAllByTestId("gameRowUsernameWhite");
        const usernamesBlack = screen.getAllByTestId("gameRowUsernameBlack");
        gamesMock.forEach((expectedGame, i) => {
            expect(usernamesWhite[i].textContent).toBe(
                expectedGame.userWhite?.userName,
            );

            expect(usernamesBlack[i].textContent).toBe(
                expectedGame.userBlack?.userName,
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
