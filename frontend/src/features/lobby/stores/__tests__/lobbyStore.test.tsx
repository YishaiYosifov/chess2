import { act } from "@testing-library/react";
import useLobbyStore from "../lobbyStore";
import { PoolKeyStr } from "../../lib/types";
import { createFakeOngoingGame } from "@/lib/testUtils/fakers/ongoingGameFaker";

describe("lobbyStore", () => {
    beforeEach(() => {
        useLobbyStore.setState(useLobbyStore.getInitialState());
    });

    describe("clearSeeks", () => {
        it("should remove all seeks", () => {
            useLobbyStore.setState({
                seeks: new Set<PoolKeyStr>(["0-5+0", "1-3+2"]),
            });

            const { clearSeeks } = useLobbyStore.getState();
            act(() => clearSeeks());

            expect(useLobbyStore.getState().seeks.size).toBe(0);
        });
    });

    describe("addSeek", () => {
        it("should add a seek to the store", () => {
            const seekKey: PoolKeyStr = "0-10+0";

            const { addSeek } = useLobbyStore.getState();

            act(() => addSeek(seekKey));

            expect(useLobbyStore.getState().seeks.has(seekKey)).toBe(true);
        });
    });

    describe("removeSeek", () => {
        it("should remove a seek from the store", () => {
            const seekKey: PoolKeyStr = "0-10+0";
            useLobbyStore.setState({
                seeks: new Set<PoolKeyStr>([seekKey]),
            });

            const { removeSeek } = useLobbyStore.getState();
            act(() => removeSeek(seekKey));

            expect(useLobbyStore.getState().seeks.has(seekKey)).toBe(false);
        });
    });

    describe("setRequestedOpenSeek", () => {
        it("should set requestedOpenSeek to true", () => {
            const { setRequestedOpenSeek } = useLobbyStore.getState();

            act(() => setRequestedOpenSeek(true));

            expect(useLobbyStore.getState().requestedOpenSeek).toBe(true);
        });

        it("should set requestedOpenSeek to false", () => {
            useLobbyStore.setState({ requestedOpenSeek: true });

            const { setRequestedOpenSeek } = useLobbyStore.getState();
            act(() => setRequestedOpenSeek(false));

            expect(useLobbyStore.getState().requestedOpenSeek).toBe(false);
        });
    });

    describe("addOngoingGames", () => {
        it("should add new games to ongoingGames", () => {
            const game1 = createFakeOngoingGame();
            const game2 = createFakeOngoingGame();

            const { addOngoingGames } = useLobbyStore.getState();
            act(() => addOngoingGames([game1, game2]));

            expect(useLobbyStore.getState().ongoingGames).toEqual(
                new Map([
                    [game1.gameToken, game1],
                    [game2.gameToken, game2],
                ]),
            );
        });
    });

    describe("removeOngoingGame", () => {
        it("should remove a game from ongoingGames", () => {
            const gameTokenToRemove = "to remove";
            const gameToKeep = createFakeOngoingGame();

            useLobbyStore.setState({
                ongoingGames: new Map([
                    [gameToKeep.gameToken, gameToKeep],
                    [
                        gameTokenToRemove,
                        createFakeOngoingGame({ gameToken: gameTokenToRemove }),
                    ],
                ]),
            });

            const { removeOngoingGame } = useLobbyStore.getState();
            act(() => removeOngoingGame(gameTokenToRemove));

            expect(useLobbyStore.getState().ongoingGames).toEqual(
                new Map([[gameToKeep.gameToken, gameToKeep]]),
            );
        });
    });
});
