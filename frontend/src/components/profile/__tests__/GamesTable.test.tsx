import { render, screen } from "@testing-library/react";

import { createProfile, profileMock } from "@/mockUtils/profileMock";
import { createFinishedGame } from "@/mockUtils/gameMock";

import GamesTable from "../GamesTable";

const gamesMock = Array.from({ length: 3 }, (_, i) =>
    createFinishedGame({
        userWhite: createProfile({
            userId: 1,
            username: `test-user-white-${i}`,
        }),
        userBlack: createProfile({
            userId: 2,
            username: `test-user-black-${i}`,
        }),
    })
);

describe("GamesTable", () => {
    it("should render the correct table headers", () => {
        render(<GamesTable games={gamesMock} viewingProfile={profileMock} />);
        const expectedHeaders = ["Mode", "Players", "Results", "Date"];

        const headers = Array.from(
            screen.getByTestId("gamesTableHeader").children
        );

        expectedHeaders.forEach((expectedHeader, i) =>
            expect(headers[i].textContent).toBe(expectedHeader)
        );
    });

    it("should render game rows correctly", () => {
        render(<GamesTable games={gamesMock} viewingProfile={profileMock} />);

        const usernamesWhite = screen.getAllByTestId("gameRowUsernameWhite");
        const usernamesBlack = screen.getAllByTestId("gameRowUsernameBlack");
        gamesMock.forEach((expectedGame, i) => {
            expect(usernamesWhite[i].textContent).toBe(
                expectedGame.userWhite?.username
            );

            expect(usernamesBlack[i].textContent).toBe(
                expectedGame.userBlack?.username
            );
        });
    });

    it("should render empty rows when there are not enough games", () => {
        const fewerGames = gamesMock.slice(0, 2);
        render(<GamesTable games={fewerGames} viewingProfile={profileMock} />);

        const emptyRows = screen.getAllByTestId("emptyGamesTableRow");
        expect(emptyRows.length).toBe(3 - fewerGames.length);
    });
});
