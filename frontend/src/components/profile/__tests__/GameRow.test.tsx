import { render, screen } from "@testing-library/react";

import { createFinishedGame, gameMock, profileMock } from "@/mocks/mocks";
import { GameResult } from "@/client";

import GameRow from "../GameRow";

describe("GameRow", () => {
    it("should display the correct usernames", () => {
        render(<GameRow game={gameMock} viewingProfile={profileMock} />);

        expect(screen.getByTestId("gameRowUsernameWhite").textContent).toBe(
            gameMock.userWhite?.username
        );
        expect(screen.getByTestId("gameRowUsernameBlack").textContent).toBe(
            gameMock.userBlack?.username
        );
    });

    it.each([
        [GameResult.White, "1", "0"],
        [GameResult.Black, "0", "1"],
        [GameResult.Draw, "½", "½"],
    ])(
        "should correctly calculate the score of each player",
        (results, whiteScore, blackScore) => {
            const newGameMock = createFinishedGame({ results });
            render(<GameRow game={newGameMock} viewingProfile={profileMock} />);

            expect(screen.getByTestId("gameRowScoreWhite").textContent).toBe(
                whiteScore
            );
            expect(screen.getByTestId("gameRowScoreBlack").textContent).toBe(
                blackScore
            );
        }
    );

    it("should display the correct game link", () => {
        render(<GameRow game={gameMock} viewingProfile={profileMock} />);
        screen
            .getAllByTestId("gameRowLink")
            .forEach((gameLink) =>
                expect(gameLink.getAttribute("href")).toBe(
                    `/game/${gameMock.token}`
                )
            );
    });

    it("should display the correct date", () => {
        render(<GameRow game={gameMock} viewingProfile={profileMock} />);
        expect(screen.getByTestId("gameRowDate").textContent).toBe("Jan 1, 23");
    });
});
