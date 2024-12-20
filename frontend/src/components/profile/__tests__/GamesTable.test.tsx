import { render, screen } from "@testing-library/react";

import { createFinishedGame } from "@/lib/testUtils/fakers/gameFaker";
import { createUser } from "@/lib/testUtils/fakers/userFaker";
import { FinishedGame, User } from "@/lib/apiClient/models";
import GamesTable from "../GamesTable";

describe("GamesTable", () => {
    let gamesMock: FinishedGame[];
    let userMock: User;

    beforeEach(() => {
        userMock = createUser();
        gamesMock = Array.from({ length: 3 }, () =>
            createFinishedGame({
                userWhite: userMock,
                userBlack: createUser(),
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
                expectedGame.userWhite?.username,
            );

            expect(usernamesBlack[i].textContent).toBe(
                expectedGame.userBlack?.username,
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
