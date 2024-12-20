import { render, screen } from "@testing-library/react";

import { FinishedGame, GameResult, User } from "@/lib/apiClient/models";
import { createFinishedGame } from "@/lib/testUtils/fakers/gameFaker";
import { createUser } from "@/lib/testUtils/fakers/userFaker";

import GameRow from "../GameRow";

describe("GameRow", () => {
    let finishedGameMock: FinishedGame;
    let userMock: User;

    beforeEach(() => {
        finishedGameMock = createFinishedGame();
        userMock = createUser();
    });

    it("should display the correct usernames", () => {
        render(
            <GameRow
                game={finishedGameMock}
                profileViewpoint={userMock}
                index={0}
            />,
        );

        expect(screen.getByTestId("gameRowUsernameWhite").textContent).toBe(
            finishedGameMock.userWhite?.username,
        );
        expect(screen.getByTestId("gameRowUsernameBlack").textContent).toBe(
            finishedGameMock.userBlack?.username,
        );
    });

    it.each([
        [GameResult.White, "1", "0"],
        [GameResult.Black, "0", "1"],
        [GameResult.Draw, "½", "½"],
    ])(
        "should correctly calculate the score of each player",
        (results, whiteScore, blackScore) => {
            finishedGameMock.results = results;

            render(
                <GameRow
                    game={finishedGameMock}
                    profileViewpoint={userMock}
                    index={0}
                />,
            );

            expect(screen.getByTestId("gameRowScoreWhite").textContent).toBe(
                whiteScore,
            );
            expect(screen.getByTestId("gameRowScoreBlack").textContent).toBe(
                blackScore,
            );
        },
    );

    it("should display the correct game link", () => {
        render(
            <GameRow
                game={finishedGameMock}
                profileViewpoint={userMock}
                index={0}
            />,
        );
        screen
            .getAllByTestId("gameRowLink")
            .forEach((gameLink) =>
                expect(gameLink.getAttribute("href")).toBe(
                    `/game/${finishedGameMock.token}`,
                ),
            );
    });

    it("should display the correct date", () => {
        render(
            <GameRow
                game={finishedGameMock}
                profileViewpoint={userMock}
                index={0}
            />,
        );

        const formattedDate = new Date(
            finishedGameMock.createdAt,
        ).toLocaleDateString("en-us", {
            month: "short",
            day: "numeric",
            year: "numeric",
        });
        expect(screen.getByTestId("gameRowDate").textContent).toBe(
            formattedDate,
        );
    });
});
