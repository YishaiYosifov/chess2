import useRematch from "@/features/liveGame/hooks/useRematch";
import createLiveChessStore, {
    LiveChessStore,
} from "@/features/liveGame/stores/liveChessStore";
import useMatchmaking from "@/features/lobby/hooks/useMatchmaking";
import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import { render, screen } from "@testing-library/react";
import { mock } from "vitest-mock-extended";
import { StoreApi } from "zustand";
import LiveChessStoreContext from "@/features/liveGame/contexts/liveChessContext";
import { GameResult } from "@/lib/apiClient";
import GameOverControls from "../GameOverControls";
import userEvent from "@testing-library/user-event";

vi.mock("@/features/liveGame/hooks/useRematch");
vi.mock("@/features/lobby/hooks/useMatchmaking");

describe("GameOverControls", () => {
    let liveChessStore: StoreApi<LiveChessStore>;

    const useMatchmakingMock = mock<ReturnType<typeof useMatchmaking>>({
        isSeeking: false,
    });
    const useRematchMock = mock<ReturnType<typeof useRematch>>({
        isRematchRequested: false,
        isRequestingRematch: false,
    });

    beforeEach(() => {
        liveChessStore = createLiveChessStore(
            createFakeLiveChessStoreProps({
                resultData: {
                    result: GameResult.ABORTED,
                    resultDescription: "test description",
                },
            }),
        );

        vi.mocked(useMatchmaking).mockReturnValue(useMatchmakingMock);
        vi.mocked(useRematch).mockReturnValue(useRematchMock);
    });

    it("should show new game when not seeking", () => {
        render(
            <LiveChessStoreContext.Provider value={liveChessStore}>
                <GameOverControls />
            </LiveChessStoreContext.Provider>,
        );

        const newGameButton = screen.getByTestId("gameOverControlsNewGame");
        expect(newGameButton).toHaveTextContent("New Game");
        expect(newGameButton).not.toHaveClass("animate-subtle-ping");
    });

    it("should show searching when seeking", () => {
        useMatchmakingMock.isSeeking = true;
        render(
            <LiveChessStoreContext.Provider value={liveChessStore}>
                <GameOverControls />
            </LiveChessStoreContext.Provider>,
        );

        const newGameButton = screen.getByTestId("gameOverControlsNewGame");
        expect(newGameButton).toHaveTextContent("Searching...");
        expect(newGameButton).toHaveClass("animate-subtle-ping");
    });

    it("should call toggleSeek when seeking new game", async () => {
        const user = userEvent.setup();
        render(
            <LiveChessStoreContext.Provider value={liveChessStore}>
                <GameOverControls />
            </LiveChessStoreContext.Provider>,
        );

        await user.click(screen.getByTestId("gameOverControlsNewGame"));

        expect(useMatchmakingMock.toggleSeek).toHaveBeenCalledOnce();
    });

    it("should show rematch when not requesting", async () => {
        render(
            <LiveChessStoreContext.Provider value={liveChessStore}>
                <GameOverControls />
            </LiveChessStoreContext.Provider>,
        );

        const rematchButton = screen.getByTestId("gameOverControlsRematch");
        expect(rematchButton).toHaveTextContent("Rematch");
        expect(rematchButton).not.toHaveClass("animate-subtle-ping");
    });

    it("should show animate when requesting rematch", async () => {
        useRematchMock.isRequestingRematch = true;
        render(
            <LiveChessStoreContext.Provider value={liveChessStore}>
                <GameOverControls />
            </LiveChessStoreContext.Provider>,
        );

        const rematchButton = screen.getByTestId("gameOverControlsRematch");
        expect(rematchButton).toHaveTextContent("Rematch");
        expect(rematchButton).toHaveClass("animate-subtle-ping");
    });

    it("should call toggleRematch when clicking rematch", async () => {
        const user = userEvent.setup();
        render(
            <LiveChessStoreContext.Provider value={liveChessStore}>
                <GameOverControls />
            </LiveChessStoreContext.Provider>,
        );

        await user.click(screen.getByTestId("gameOverControlsRematch"));

        expect(useRematchMock.toggleRematch).toHaveBeenCalledOnce();
    });

    it("should show rematch? when rematch is requested", () => {
        useRematchMock.isRematchRequested = true;
        render(
            <LiveChessStoreContext.Provider value={liveChessStore}>
                <GameOverControls />
            </LiveChessStoreContext.Provider>,
        );

        const rematchButton = screen.getByTestId("gameOverControlsRematch");
        expect(rematchButton).toHaveTextContent("Rematch?");
    });

    it("should call requestRematch when rematch is requested and accepted", async () => {
        useRematchMock.isRematchRequested = true;
        const user = userEvent.setup();
        render(
            <LiveChessStoreContext.Provider value={liveChessStore}>
                <GameOverControls />
            </LiveChessStoreContext.Provider>,
        );

        await user.click(screen.getByTestId("gameOverControlsRematch"));
        expect(useRematchMock.requestRematch).toHaveBeenCalledOnce();
    });
});
