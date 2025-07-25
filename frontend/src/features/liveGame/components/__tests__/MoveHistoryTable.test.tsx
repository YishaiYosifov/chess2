import { render, screen } from "@testing-library/react";
import { StoreApi } from "zustand";

import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import LiveChessStoreContext from "@/features/liveGame/contexts/liveChessContext";
import createLiveChessStore, {
    LiveChessStore,
} from "@/features/liveGame/stores/liveChessStore";
import MoveHistoryTable from "../MoveHistoryTable";
import { createFakePosition } from "@/lib/testUtils/fakers/positionFaker";
import {
    ChessboardState,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";
import ChessboardStoreContext from "@/features/chessboard/contexts/chessboardStoreContext";
import userEvent from "@testing-library/user-event";
import { createMoveOptions } from "@/features/chessboard/lib/moveOptions";
import { createFakeLegalMoveMap } from "@/lib/testUtils/fakers/chessboardFakers";
import { Position, ProcessedMoveOptions } from "@/types/tempModels";

describe("MoveHistoryTable", () => {
    let liveStore: StoreApi<LiveChessStore>;
    let chessboardStore: StoreApi<ChessboardState>;

    const emptyMoveOptions = createMoveOptions();
    let latestMoveOptions: ProcessedMoveOptions;

    beforeEach(() => {
        latestMoveOptions = {
            legalMoves: createFakeLegalMoveMap(),
            hasForcedMoves: false,
        };

        liveStore = createLiveChessStore(
            createFakeLiveChessStoreProps({
                positionHistory: [],
                latestMoveOptions,
            }),
        );
        chessboardStore = createChessboardStore();
    });

    function renderWithCtx() {
        return render(
            <LiveChessStoreContext.Provider value={liveStore}>
                <ChessboardStoreContext.Provider value={chessboardStore}>
                    <MoveHistoryTable />
                </ChessboardStoreContext.Provider>
            </LiveChessStoreContext.Provider>,
        );
    }

    function expectPosition(
        position: Position,
        moveOptions: ProcessedMoveOptions,
    ) {
        expect(chessboardStore.getState().pieces).toEqual(position.pieces);
        expect(chessboardStore.getState().moveOptions).toEqual(moveOptions);
    }

    it("should render an empty table when there are no moves", () => {
        renderWithCtx();
        const rows = screen.queryAllByRole("row");
        expect(rows.length).toBe(0);
    });

    it("should render a single row when there is one move", () => {
        liveStore.setState({
            positionHistory: [
                createFakePosition({ san: undefined }),
                createFakePosition({ san: "e4" }),
            ],
        });

        renderWithCtx();

        expect(screen.getByText("1.")).toBeInTheDocument();
        expect(screen.getByText("e4")).toBeInTheDocument();
    });

    it("should render multiple rows for multiple moves", () => {
        liveStore.setState({
            positionHistory: [
                createFakePosition({ san: undefined }),
                createFakePosition({ san: "e4" }),
                createFakePosition({ san: "e5" }),
                createFakePosition({ san: "Nf3" }),
                createFakePosition({ san: "Nc6" }),
            ],
        });

        renderWithCtx();

        expect(screen.getByText("1.")).toBeInTheDocument();
        expect(screen.getByText("2.")).toBeInTheDocument();
        expect(screen.getByText("e4")).toBeInTheDocument();
        expect(screen.getByText("e5")).toBeInTheDocument();
        expect(screen.getByText("Nf3")).toBeInTheDocument();
        expect(screen.getByText("Nc6")).toBeInTheDocument();
    });

    it("should apply alternating background color class for odd rows", () => {
        liveStore.setState({
            positionHistory: [
                createFakePosition({ san: undefined }),
                createFakePosition({ san: "e4" }),
                createFakePosition({ san: "e5" }),
                createFakePosition({ san: "Nf3" }),
                createFakePosition({ san: "Nf6" }),
            ],
        });

        renderWithCtx();

        const rows = screen.getAllByRole("row");
        expect(rows.length).toBe(2);

        expect(rows[0].className).not.toContain("bg-white/10");
        expect(rows[1].className).toContain("bg-white/10");
    });

    it("should update position using arrow keys", async () => {
        const move1 = createFakePosition({ san: undefined });
        const move2 = createFakePosition();
        const move3 = createFakePosition();

        liveStore.setState({
            latestMoveOptions: latestMoveOptions,
            viewingMoveNumber: 0,
            positionHistory: [move1, move2, move3],
        });

        const user = userEvent.setup();
        renderWithCtx();

        // go to move 2
        await user.keyboard("{ArrowRight}");
        expectPosition(move2, emptyMoveOptions);

        // go to move 3
        await user.keyboard("{ArrowRight}");
        expectPosition(move3, latestMoveOptions);

        // go back to move2
        await user.keyboard("{ArrowLeft}");
        expectPosition(move2, emptyMoveOptions);

        // jump to end
        await user.keyboard("{ArrowDown}");
        expectPosition(move3, latestMoveOptions);

        // jump to start
        await user.keyboard("{ArrowUp}");
        expectPosition(move1, emptyMoveOptions);
    });

    it("should update position when clicking on a move", async () => {
        const move1 = createFakePosition({ san: undefined });
        const move2 = createFakePosition({ san: "e4" });
        const move3 = createFakePosition({ san: "e5" });
        const move4 = createFakePosition({ san: "e6" });

        liveStore.setState({
            positionHistory: [move1, move2, move3, move4],
        });

        const user = userEvent.setup();
        renderWithCtx();

        await user.click(screen.getByText("e4"));
        expectPosition(move2, emptyMoveOptions);

        await user.click(screen.getByText("e5"));
        expectPosition(move3, emptyMoveOptions);

        await user.click(screen.getByText("e6"));
        expectPosition(move4, latestMoveOptions);
    });
});
