import { render, screen } from "@testing-library/react";
import { logicalPoint } from "@/features/point/pointUtils";
import ChessSquare from "../ChessSquare";
import { createChessboardStore } from "../../stores/chessboardStore";
import ChessboardStoreContext from "../../contexts/chessboardStoreContext";
import { GameColor } from "@/lib/apiClient";

describe("ChessSquare", () => {
    let store = createChessboardStore();

    beforeEach(() => {
        store = createChessboardStore();
    });

    it.each([
        {
            viewingFrom: GameColor.WHITE,
            position: { x: 2, y: 3 },
            expected: "2,6",
        },
        {
            viewingFrom: GameColor.BLACK,
            position: { x: 2, y: 3 },
            expected: "7,3",
        },
        {
            viewingFrom: GameColor.WHITE,
            position: { x: 0, y: 0 },
            expected: "0,9",
        },
        {
            viewingFrom: GameColor.BLACK,
            position: { x: 0, y: 0 },
            expected: "9,0",
        },
        {
            viewingFrom: GameColor.WHITE,
            position: { x: 5, y: 7 },
            expected: "5,2",
        },
        {
            viewingFrom: GameColor.BLACK,
            position: { x: 5, y: 7 },
            expected: "4,7",
        },
    ])(
        "should render at the correct view position when viewingFrom=$viewingFrom and position=$position",
        ({ viewingFrom, position, expected }) => {
            store.setState({ viewingFrom });

            render(
                <ChessboardStoreContext.Provider value={store}>
                    <ChessSquare
                        position={logicalPoint(position)}
                        data-testid="square"
                    />
                </ChessboardStoreContext.Provider>,
            );

            const el = screen.getByTestId("square");
            expect(el.getAttribute("data-position")).toBe(expected);
        },
    );
});
