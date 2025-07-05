import { render, screen } from "@testing-library/react";

import { LiveChessStore } from "@/features/liveGame/stores/liveChessboardStore";
import { useGameEmitter } from "@/features/signalR/hooks/useSignalRHubs";
import GameControls from "../GameControls";
import { createMove } from "@/lib/testUtils/fakers/chessboardFakers";
import { GameResult } from "@/types/tempModels";
import userEvent from "@testing-library/user-event";
import { LiveChessStoreContext } from "@/features/liveGame/contexts/liveChessContext";
import { StoreApi } from "zustand";
import { createFakeLiveChessStore } from "@/lib/testUtils/fakers/liveChessStoreFaker";

vi.mock("@/features/signalR/hooks/useSignalRHubs");

describe("GameControls", () => {
    const useGameEmitterMock = vi.mocked(useGameEmitter);
    const sendGameEventMock = vi.fn();
    let store: StoreApi<LiveChessStore>;

    beforeEach(() => {
        store = createFakeLiveChessStore();
        useGameEmitterMock.mockReturnValue(sendGameEventMock);
    });

    it("should first render LiveGameControls with Abort", () => {
        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameControls />
            </LiveChessStoreContext.Provider>,
        );

        expect(screen.getByText(/Abort/i)).toBeInTheDocument();
        expect(screen.getByText(/Draw/i)).toBeInTheDocument();
    });

    it("should render Resign if moveHistory has 3+ moves", () => {
        store.setState({
            moveHistory: [createMove(), createMove(), createMove()],
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameControls />
            </LiveChessStoreContext.Provider>,
        );

        expect(screen.getByText(/Resign/i)).toBeInTheDocument();
        expect(screen.getByText(/Draw/i)).toBeInTheDocument();
    });

    it("should render GameOverControls when resultData exists", () => {
        store.setState({
            resultData: {
                result: GameResult.WHITE_WIN,
                resultDescription: "white won",
            },
        });
        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameControls />
            </LiveChessStoreContext.Provider>,
        );

        expect(screen.getByText(/New Game/i)).toBeInTheDocument();
        expect(screen.getByText(/Rematch/i)).toBeInTheDocument();
    });

    it("should call sendGameEvent when clicking Abort", async () => {
        const user = userEvent.setup();

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameControls />
            </LiveChessStoreContext.Provider>,
        );

        await user.click(screen.getByText(/Abort/i));
        expect(sendGameEventMock).toHaveBeenCalledWith(
            "EndGameAsync",
            store.getState().gameToken,
        );
    });
});
