import { render, screen } from "@testing-library/react";
import { logicalPoint } from "@/features/point/pointUtils";
import userEvent from "@testing-library/user-event";
import {
    ChessboardStore,
    createChessboardStore,
} from "../../stores/chessboardStore";
import { StoreApi } from "zustand";
import ChessboardStoreContext from "../../contexts/chessboardStoreContext";
import IntermediateSquarePrompt from "../IntermediateSquarePrompt";
import { LogicalPoint } from "@/features/point/types";

describe("IntermediateSquarePrompt", () => {
    let store: StoreApi<ChessboardStore>;

    beforeEach(() => {
        store = createChessboardStore();
    });

    const renderComponent = () =>
        render(
            <ChessboardStoreContext.Provider value={store}>
                <IntermediateSquarePrompt />
            </ChessboardStoreContext.Provider>,
        );

    it("should render nothing when nextIntermediates is empty", () => {
        store.setState({ nextIntermediates: [] });
        renderComponent();
        expect(
            screen.queryByTestId("intermediateSquarePromptOverlay"),
        ).toBeNull();
    });

    it("should render overlay and squares when nextIntermediates has points", () => {
        const points = [
            logicalPoint({ x: 1, y: 1 }),
            logicalPoint({ x: 2, y: 2 }),
        ];
        store.setState({ nextIntermediates: points });

        renderComponent();

        expect(
            screen.getByTestId("intermediateSquarePromptOverlay"),
        ).toBeInTheDocument();

        const squares = screen.getAllByTestId("intermediateSquare");
        expect(squares).toHaveLength(points.length);
    });

    it("should call resolveNextIntermediate with the correct point when a square is clicked", async () => {
        const user = userEvent.setup();
        const points = [
            logicalPoint({ x: 1, y: 1 }),
            logicalPoint({ x: 2, y: 2 }),
        ];
        let resolvedPoint: LogicalPoint | null = null;
        store.setState({
            nextIntermediates: points,
            resolveNextIntermediate: (point) => (resolvedPoint = point),
        });

        renderComponent();

        const squares = screen.getAllByTestId("intermediateSquare");
        await user.click(squares[1]);

        expect(resolvedPoint).toEqual(points[1]);
    });

    it("should call resolveNextIntermediate(null) when overlay is clicked", async () => {
        const user = userEvent.setup();
        const points = [logicalPoint({ x: 1, y: 1 })];

        let resolvedPoint: LogicalPoint | null = logicalPoint({ x: 6, y: 9 });
        store.setState({
            nextIntermediates: points,
            resolveNextIntermediate: (point) => (resolvedPoint = point),
        });

        renderComponent();

        const overlay = screen.getByTestId("intermediateSquarePromptOverlay");
        await user.pointer({ target: overlay, keys: "[MouseLeft]" });

        expect(resolvedPoint).toBeNull();
    });
});
