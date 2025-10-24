import { render, screen } from "@testing-library/react";

import { createFakeGameSummary } from "@/lib/testUtils/fakers/gameSummaryFaker";
import { createFakeUser } from "@/lib/testUtils/fakers/userFaker";

import GameRow from "../GameRow";
import { GameResult, GameSummary, PublicUser } from "@/lib/apiClient";

describe("GameRow", () => {
    let gameSummaryMock: GameSummary;
    let userMock: PublicUser;

    beforeEach(() => {
        gameSummaryMock = createFakeGameSummary();
        userMock = createFakeUser();
    });

    it("should display the correct usernames", () => {
        render(
            <GameRow
                game={gameSummaryMock}
                profileViewpoint={userMock}
                index={0}
            />,
        );

        expect(screen.getByTestId("gameRowWhiteUsername").textContent).toBe(
            gameSummaryMock.whitePlayer?.userName,
        );
        expect(screen.getByTestId("gameRowBlackUsername").textContent).toBe(
            gameSummaryMock.blackPlayer?.userName,
        );
    });

    it.each([
        [GameResult.WHITE_WIN, "1", "0"],
        [GameResult.BLACK_WIN, "0", "1"],
        [GameResult.DRAW, "½", "½"],
    ])(
        "should correctly calculate the score of each player",
        (result, whiteScore, blackScore) => {
            gameSummaryMock.result = result;

            render(
                <GameRow
                    game={gameSummaryMock}
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
                game={gameSummaryMock}
                profileViewpoint={userMock}
                index={0}
            />,
        );
        screen
            .getAllByTestId("gameRowLink")
            .forEach((gameLink) =>
                expect(gameLink.getAttribute("href")).toBe(
                    `/game/${gameSummaryMock.gameToken}`,
                ),
            );
    });

    it("should display the correct date", () => {
        render(
            <GameRow
                game={gameSummaryMock}
                profileViewpoint={userMock}
                index={0}
            />,
        );

        const formattedDate = new Date(
            gameSummaryMock.createdAt,
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
