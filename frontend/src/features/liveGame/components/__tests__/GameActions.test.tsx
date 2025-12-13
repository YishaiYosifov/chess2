import { render, screen } from "@testing-library/react";
import GameActions from "../GameActions";
import userEvent from "@testing-library/user-event";
import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";
import { StoreApi } from "zustand";
import ChessboardStoreContext from "@/features/chessboard/contexts/chessboardStoreContext";
import { act } from "react";
import { GameColor } from "@/lib/apiClient";

describe("GameActions", () => {
    let store: StoreApi<ChessboardStore>;

    beforeEach(() => {
        vi.useFakeTimers({ shouldAdvanceTime: true });
        store = createChessboardStore();
    });

    it("should show and hide the copied tooltip after 1.5 seconds", async () => {
        const user = userEvent.setup();

        render(
            <ChessboardStoreContext.Provider value={store}>
                <GameActions />
            </ChessboardStoreContext.Provider>,
        );

        const shareIcon = screen.getByTitle("Share");
        await user.click(shareIcon);

        expect(screen.getByText("Copied to Clipboard")).toHaveClass(
            "opacity-100",
        );

        await act(() => vi.advanceTimersByTime(1500));

        expect(screen.getByText("Copied to Clipboard")).toHaveClass(
            "opacity-0",
        );
    });

    it("should copy the current URL to clipboard when share is clicked", async () => {
        const writeTextMock = vi.fn();
        const user = userEvent.setup();
        Object.defineProperty(navigator, "clipboard", {
            value: {
                writeText: writeTextMock,
            },
        });

        render(
            <ChessboardStoreContext.Provider value={store}>
                <GameActions />
            </ChessboardStoreContext.Provider>,
        );

        const shareIcon = screen.getByTitle("Share");
        await user.click(shareIcon);

        expect(writeTextMock).toHaveBeenCalledWith(window.location.href);
    });

    it("should call flipBoard when the flip board icon is clicked", async () => {
        store.setState({ viewingFrom: GameColor.WHITE });
        const user = userEvent.setup();

        render(
            <ChessboardStoreContext.Provider value={store}>
                <GameActions />
            </ChessboardStoreContext.Provider>,
        );

        const flipIcon = screen.getByTitle("Flip Board");
        await user.click(flipIcon);

        expect(store.getState().viewingFrom).toBe(GameColor.BLACK);
    });
});
