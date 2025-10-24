import { render, screen } from "@testing-library/react";
import { logicalPoint, pointToStr } from "@/features/point/pointUtils";
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
            expected: { x: 2, y: 6 },
        },
        {
            viewingFrom: GameColor.BLACK,
            position: { x: 2, y: 3 },
            expected: { x: 7, y: 3 },
        },
        {
            viewingFrom: GameColor.WHITE,
            position: { x: 0, y: 0 },
            expected: { x: 0, y: 9 },
        },
        {
            viewingFrom: GameColor.BLACK,
            position: { x: 0, y: 0 },
            expected: { x: 9, y: 0 },
        },
        {
            viewingFrom: GameColor.WHITE,
            position: { x: 5, y: 7 },
            expected: { x: 5, y: 2 },
        },
        {
            viewingFrom: GameColor.BLACK,
            position: { x: 5, y: 7 },
            expected: { x: 4, y: 7 },
        },
    ])(
        "should render at the correct view position when viewingFrom=$viewingFrom and position=$position",
        ({ viewingFrom, position, expected }) => {
            store.setState({ viewingFrom });

            render(
                <ChessboardStoreContext.Provider value={store}>
                    <ChessSquare position={logicalPoint(position)} />
                </ChessboardStoreContext.Provider>,
            );

            const el = screen.getByTestId("chessSquare");
            expect(el.style.transform.replace(/\s/g, "")).toBe(
                `translate(clamp(0%,calc(${expected.x * 100}%+0px),900%),clamp(0%,calc(${expected.y * 100}%+0px),900%))`,
            );
        },
    );

    it("should set data-position correctly", () => {
        const position = logicalPoint({ x: 1, y: 2 });
        render(
            <ChessboardStoreContext.Provider value={store}>
                <ChessSquare position={position} />
            </ChessboardStoreContext.Provider>,
        );

        expect(screen.getByTestId("chessSquare")).toHaveAttribute(
            "data-position",
            pointToStr(position),
        );
    });
});
