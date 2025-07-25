import { render, screen } from "@testing-library/react";

import createLiveChessStore, {
    LiveChessStore,
} from "@/features/liveGame/stores/liveChessStore";
import { useGameEmitter } from "@/features/signalR/hooks/useSignalRHubs";
import GameControls from "../GameControls";
import userEvent from "@testing-library/user-event";
import LiveChessStoreContext from "@/features/liveGame/contexts/liveChessContext";
import { StoreApi } from "zustand";
import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import { GameResult } from "@/lib/apiClient";
import { createFakePosition } from "@/lib/testUtils/fakers/positionFaker";

vi.mock("@/features/signalR/hooks/useSignalRHubs");

describe("GameControls", () => {
    const useGameEmitterMock = vi.mocked(useGameEmitter);
    const sendGameEventMock = vi.fn();
    let store: StoreApi<LiveChessStore>;

    beforeEach(() => {
        store = createLiveChessStore(createFakeLiveChessStoreProps());
        useGameEmitterMock.mockReturnValue(sendGameEventMock);
    });

    it("should first render LiveGameControls with Abort", () => {
        store.setState({
            positionHistory: [createFakePosition()],
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameControls />
            </LiveChessStoreContext.Provider>,
        );

        expect(screen.getByText(/Abort/i)).toBeInTheDocument();
        expect(screen.getByText(/Draw/i)).toBeInTheDocument();
    });

    it("should render Resign if moveHistory has 2+ moves", () => {
        store.setState({
            positionHistory: [createFakePosition(), createFakePosition()],
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

        store.setState({ positionHistory: [] });

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
