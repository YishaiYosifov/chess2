import React from "react";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import ChessboardStoreContext from "@/features/chessboard/contexts/chessboardStoreContext";
import { GameColor } from "@/lib/apiClient";
import HighlightedLegalMove from "../HighlightedLegalMove";
import { useChessboardStore } from "@/features/chessboard/hooks/useChessboard";
import {
    createFakeMove,
    createFakePiece,
} from "@/lib/testUtils/fakers/chessboardFakers";
import { LegalMoveMap, PieceID, PieceMap } from "@/types/tempModels";
import { pointToStr } from "@/lib/utils/pointUtils";
import { StoreApi } from "zustand";
import {
    ChessboardState,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";

function PiecePositionProbe({ id }: { id: PieceID }) {
    const piecePosition = useChessboardStore(
        (state) => state.pieces.get(id)?.position,
    );

    return (
        <div data-testid="checker">
            {piecePosition && pointToStr(piecePosition)}
        </div>
    );
}

describe("HighlightedLegalMove", () => {
    let store: StoreApi<ChessboardState>;

    beforeEach(() => {
        store = createChessboardStore();
    });

    it("renders without crashing", () => {
        render(
            <ChessboardStoreContext.Provider value={store}>
                <HighlightedLegalMove position={{ x: 1, y: 2 }} />
            </ChessboardStoreContext.Provider>,
        );

        const element = screen.getByTestId("highlightedLegalMove");
        expect(element).toBeInTheDocument();
    });

    it("calls moveSelectedPiece with correct position on click", async () => {
        const piece = createFakePiece();
        const move = createFakeMove({ from: piece.position });
        const legalMoves: LegalMoveMap = new Map([
            [pointToStr(piece.position), [move]],
        ]);
        const pieces: PieceMap = new Map([["0", piece]]);
        store.setState({
            viewingFrom: GameColor.WHITE,
            selectedPieceId: "0",
            legalMoves,
            pieces,
        });

        const user = userEvent.setup();
        render(
            <ChessboardStoreContext.Provider value={store}>
                <HighlightedLegalMove position={move.to} />
                <PiecePositionProbe id={"0"} />
            </ChessboardStoreContext.Provider>,
        );

        const moveSpot = screen.getByTestId("highlightedLegalMove");
        await user.click(moveSpot);

        const checker = await screen.findByTestId("checker");
        expect(checker.textContent).toBe(pointToStr(move.to));
    });
});
