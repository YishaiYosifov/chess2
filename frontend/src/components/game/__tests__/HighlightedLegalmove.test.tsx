import React from "react";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { ChessProvider } from "@/contexts/chessStoreContext";
import { GameColor } from "@/lib/apiClient";
import HighlightedLegalMove from "../HighlightedLegalMove";
import { useChessStore } from "@/hooks/useChess";
import {
    createMove,
    createPiece,
} from "@/lib/testUtils/fakers/chessboardFakers";
import { LegalMoveMap, PieceID, PieceMap } from "@/types/tempModels";
import { pointToStr } from "@/lib/utils/pointUtils";

function PiecePositionProbe({ id }: { id: PieceID }) {
    const piecePosition = useChessStore(
        (state) => state.pieces.get(id)?.position,
    );

    return (
        <div data-testid="checker">
            {piecePosition && pointToStr(piecePosition)}
        </div>
    );
}

describe("HighlightedLegalMove", () => {
    it("renders without crashing", () => {
        render(
            <ChessProvider>
                <HighlightedLegalMove position={{ x: 1, y: 2 }} />
            </ChessProvider>,
        );

        const element = screen.getByTestId("highlightedLegalMove");
        expect(element).toBeInTheDocument();
    });

    it("calls moveSelectedPiece with correct position on click", async () => {
        const piece = createPiece();
        const move = createMove({ from: piece.position });
        const legalMoves: LegalMoveMap = new Map([
            [pointToStr(piece.position), [move]],
        ]);
        const pieces: PieceMap = new Map([["0", piece]]);

        const user = userEvent.setup();
        render(
            <ChessProvider
                viewingFrom={GameColor.WHITE}
                boardDimensions={{ width: 10, height: 10 }}
                selectedPieceId={"0"}
                legalMoves={legalMoves}
                pieces={pieces}
            >
                <HighlightedLegalMove position={move.to} />
                <PiecePositionProbe id={"0"} />
            </ChessProvider>,
        );

        const moveSpot = screen.getByTestId("highlightedLegalMove");
        await user.click(moveSpot);

        const checker = await screen.findByTestId("checker");
        expect(checker.textContent).toBe(pointToStr(move.to));
    });
});
