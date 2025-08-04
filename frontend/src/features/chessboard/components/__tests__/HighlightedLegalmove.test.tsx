import React from "react";
import { render, screen } from "@testing-library/react";
import ChessboardStoreContext from "@/features/chessboard/contexts/chessboardStoreContext";
import HighlightedLegalMove from "../HighlightedLegalMove";
import { StoreApi } from "zustand";
import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";
import { logicalPoint } from "@/lib/utils/pointUtils";

describe("HighlightedLegalMove", () => {
    let store: StoreApi<ChessboardStore>;

    beforeEach(() => {
        store = createChessboardStore();
    });

    it("renders without crashing", () => {
        render(
            <ChessboardStoreContext.Provider value={store}>
                <HighlightedLegalMove position={logicalPoint({ x: 1, y: 2 })} />
            </ChessboardStoreContext.Provider>,
        );

        const element = screen.getByTestId("highlightedLegalMove");
        expect(element).toBeInTheDocument();
    });

    it("should apply animation styling", () => {
        render(
            <ChessboardStoreContext.Provider value={store}>
                <HighlightedLegalMove position={logicalPoint({ x: 5, y: 6 })} />
            </ChessboardStoreContext.Provider>,
        );

        const element = screen.getByTestId("highlightedLegalMove");
        expect(element).toHaveClass("z-20");
        expect(element).toHaveClass("animate-[fadeIn_0.15s_ease-out]");
        expect(element).toHaveClass(
            "bg-[radial-gradient(rgba(0,0,0,0.25)_20%,_rgba(0,0,0,0)_23%)]",
        );
        expect(element).toHaveClass("hover:border-5");
        expect(element).toHaveClass("hover:border-white/50");
    });
});
