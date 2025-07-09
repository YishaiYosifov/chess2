import { render, screen } from "@testing-library/react";
import { StoreApi } from "zustand";

import { createFakeLiveChessStore } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import LiveChessStoreContext from "@/features/liveGame/contexts/liveChessContext";
import { LiveChessStore } from "@/features/liveGame/stores/liveChessStore";
import MoveHistoryTable from "../MoveHistoryTable";
import { createFakeMoveSnapshot } from "@/lib/testUtils/fakers/moveSnapshotFaker";

describe("MoveHistoryTable", () => {
    let store: StoreApi<LiveChessStore>;

    beforeEach(() => {
        store = createFakeLiveChessStore({ moveHistory: [] });
    });

    it("should render an empty table when there are no moves", () => {
        render(
            <LiveChessStoreContext.Provider value={store}>
                <MoveHistoryTable />
            </LiveChessStoreContext.Provider>,
        );

        const rows = screen.queryAllByRole("row");
        expect(rows.length).toBe(0);
    });

    it("should render a single row when there is one move", () => {
        store.setState({
            moveHistory: [createFakeMoveSnapshot({ san: "e4" })],
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <MoveHistoryTable />
            </LiveChessStoreContext.Provider>,
        );

        expect(screen.getByText("1.")).toBeInTheDocument();
        expect(screen.getByText("e4")).toBeInTheDocument();
    });

    it("should render multiple rows for multiple moves", () => {
        store.setState({
            moveHistory: [
                createFakeMoveSnapshot({ san: "e4" }),
                createFakeMoveSnapshot({ san: "e5" }),
                createFakeMoveSnapshot({ san: "Nf3" }),
                createFakeMoveSnapshot({ san: "Nc6" }),
            ],
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <MoveHistoryTable />
            </LiveChessStoreContext.Provider>,
        );

        expect(screen.getByText("1.")).toBeInTheDocument();
        expect(screen.getByText("2.")).toBeInTheDocument();
        expect(screen.getByText("e4")).toBeInTheDocument();
        expect(screen.getByText("e5")).toBeInTheDocument();
        expect(screen.getByText("Nf3")).toBeInTheDocument();
        expect(screen.getByText("Nc6")).toBeInTheDocument();
    });

    it("should apply alternating background color class for odd rows", () => {
        store.setState({
            moveHistory: [
                createFakeMoveSnapshot({ san: "e4" }),
                createFakeMoveSnapshot({ san: "e5" }),
                createFakeMoveSnapshot({ san: "Nf3" }),
                createFakeMoveSnapshot({ san: "Nf6" }),
            ],
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <MoveHistoryTable />
            </LiveChessStoreContext.Provider>,
        );

        const rows = screen.getAllByRole("row");
        expect(rows.length).toBe(2);

        expect(rows[0].className).not.toContain("bg-white/10");
        expect(rows[1].className).toContain("bg-white/10");
    });
});
