import { act, render, screen } from "@testing-library/react";

import useLiveChessboardStore from "@/stores/liveChessboardStore";
import { useGameEmitter } from "@/hooks/signalR/useSignalRHubs";
import GameControls from "../GameControls";
import { createMove } from "@/lib/testUtils/fakers/chessboardFakers";
import { GameResult } from "@/types/tempModels";
import userEvent from "@testing-library/user-event";

vi.mock("@/hooks/signalR/useSignalRHubs");

describe("GameControls", () => {
    const useGameEmitterMock = vi.mocked(useGameEmitter);
    const sendGameEventMock = vi.fn();

    const token = "testtoken";

    beforeEach(() => {
        useGameEmitterMock.mockReturnValue(sendGameEventMock);
        act(() =>
            useLiveChessboardStore.setState(
                {
                    ...useLiveChessboardStore.getInitialState(),
                    gameToken: token,
                },
                true,
            ),
        );
    });

    it("should first render LiveGameControls with Abort", () => {
        render(<GameControls />);

        expect(screen.getByText(/Abort/i)).toBeInTheDocument();
        expect(screen.getByText(/Draw/i)).toBeInTheDocument();
    });

    it("should render Resign if moveHistory has 3+ moves", () => {
        act(() =>
            useLiveChessboardStore.setState({
                moveHistory: [createMove(), createMove(), createMove()],
            }),
        );

        render(<GameControls />);

        expect(screen.getByText(/Resign/i)).toBeInTheDocument();
        expect(screen.getByText(/Draw/i)).toBeInTheDocument();
    });

    it("should render GameOverControls when resultData exists", () => {
        act(() =>
            useLiveChessboardStore.setState({
                resultData: {
                    result: GameResult.WHITE_WIN,
                    resultDescription: "white won",
                },
            }),
        );

        render(<GameControls />);

        expect(screen.getByText(/New Game/i)).toBeInTheDocument();
        expect(screen.getByText(/Rematch/i)).toBeInTheDocument();
    });

    it("should call sendGameEvent when clicking Abort", async () => {
        const user = userEvent.setup();

        render(<GameControls />);

        await user.click(screen.getByText(/Abort/i));
        expect(sendGameEventMock).toHaveBeenCalledWith("EndGameAsync", token);
    });
});
