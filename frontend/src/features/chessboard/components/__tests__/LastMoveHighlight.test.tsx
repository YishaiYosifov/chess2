import { StoreApi } from "zustand";
import {
    ChessboardStore,
    createChessboardStore,
} from "../../stores/chessboardStore";
import { logicalPoint, pointToStr } from "@/features/point/pointUtils";
import { render, screen } from "@testing-library/react";
import ChessboardStoreContext from "../../contexts/chessboardStoreContext";
import LastMoveHighlight from "../LastMoveHighlight";

describe("LastMoveHighlight", () => {
    let store: StoreApi<ChessboardStore>;

    beforeEach(() => {
        store = createChessboardStore();
    });

    it("should render from and to squares", () => {
        const fromPosition = logicalPoint({ x: 1, y: 2 });
        const toPosition = logicalPoint({ x: 3, y: 4 });
        store.setState({
            lastMove: {
                from: fromPosition,
                to: toPosition,
            },
        });

        render(
            <ChessboardStoreContext.Provider value={store}>
                <LastMoveHighlight />
            </ChessboardStoreContext.Provider>,
        );

        const fromSquare = screen.getByTestId("highlightedLastMoveFrom");
        expect(fromSquare).toBeInTheDocument();
        expect(fromSquare).toHaveAttribute(
            "data-position",
            pointToStr(fromPosition),
        );

        const toSquare = screen.getByTestId("highlightedLastMoveTo");
        expect(toSquare).toBeInTheDocument();
        expect(toSquare).toHaveAttribute(
            "data-position",
            pointToStr(toPosition),
        );
    });

    it("should not render anything if there is no last move", () => {
        store.setState({
            lastMove: undefined,
        });

        render(
            <ChessboardStoreContext.Provider value={store}>
                <LastMoveHighlight />
            </ChessboardStoreContext.Provider>,
        );

        expect(
            screen.queryByTestId("highlightedLastMoveFrom"),
        ).not.toBeInTheDocument();
        expect(
            screen.queryByTestId("highlightedLastMoveTo"),
        ).not.toBeInTheDocument();
    });
});
