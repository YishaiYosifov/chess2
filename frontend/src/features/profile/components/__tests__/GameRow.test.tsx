import { render, screen } from "@testing-library/react";

import { createFakeFinishedGame } from "@/lib/testUtils/fakers/gameFaker";
import { createFakeUser } from "@/lib/testUtils/fakers/userFaker";

import GameRow from "../GameRow";
import { FinishedGame, GameResult } from "@/types/tempModels";
import { User } from "@/lib/apiClient";

describe("GameRow", () => {
    let finishedGameMock: FinishedGame;
    let userMock: User;

    beforeEach(() => {
        finishedGameMock = createFakeFinishedGame();
        userMock = createFakeUser();
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
            finishedGameMock.userWhite?.userName,
        );
        expect(screen.getByTestId("gameRowUsernameBlack").textContent).toBe(
            finishedGameMock.userBlack?.userName,
        );
    });

    it.each([
        [GameResult.WHITE_WIN, "1", "0"],
        [GameResult.BLACK_WIN, "0", "1"],
        [GameResult.DRAW, "½", "½"],
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
