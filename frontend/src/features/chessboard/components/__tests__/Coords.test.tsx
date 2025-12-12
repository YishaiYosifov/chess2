import { render, screen } from "@testing-library/react";
import { pointToStr } from "@/features/point/pointUtils";
import Coords from "../Coords";
import { StoreApi } from "zustand";
import {
    ChessboardStore,
    createChessboardStore,
} from "../../stores/chessboardStore";
import ChessboardStoreContext from "../../contexts/chessboardStoreContext";
import { GameColor } from "@/lib/apiClient";

describe("Coords", () => {
    let store: StoreApi<ChessboardStore>;

    const files = ["a", "b", "c", "d", "e", "f", "g", "h", "i", "j"];
    const ranks = [10, 9, 8, 7, 6, 5, 4, 3, 2, 1];

    beforeEach(() => {
        store = createChessboardStore();
        store.setState({ boardDimensions: { width: 10, height: 10 } });
    });

    const renderWithStore = () =>
        render(
            <ChessboardStoreContext.Provider value={store}>
                <Coords />
            </ChessboardStoreContext.Provider>,
        );

    it("should render all file labels", () => {
        renderWithStore();

        for (const file of files) {
            expect(screen.getByTestId(`coordsFile-${file}`)).toHaveTextContent(
                file,
            );
        }
    });

    it("should render all rank labels", () => {
        renderWithStore();

        for (const rank of ranks) {
            expect(screen.getByTestId(`coordsRank-${rank}`)).toHaveTextContent(
                rank.toString(),
            );
        }
    });

    it.each([GameColor.WHITE, GameColor.BLACK])(
        "should position file labels along the top row",
        (viewingFrom) => {
            store.setState({ viewingFrom });
            renderWithStore();

            files.forEach((file, x) => {
                const expectedX = viewingFrom === GameColor.WHITE ? x : 9 - x;
                expect(
                    screen.getByTestId(`coordsFile-${file}`),
                ).toHaveAttribute(
                    "data-position",
                    pointToStr({ x: expectedX, y: 9 }),
                );
            });
        },
    );

    it.each([GameColor.WHITE, GameColor.BLACK])(
        "should position rank labels along the right column",
        (viewingFrom) => {
            store.setState({ viewingFrom });
            renderWithStore();

            ranks.forEach((rank, y) => {
                const expectedY = viewingFrom === GameColor.WHITE ? y : 9 - y;
                expect(
                    screen.getByTestId(`coordsRank-${rank}`),
                ).toHaveAttribute(
                    "data-position",
                    pointToStr({ x: 9, y: expectedY }),
                );
            });
        },
    );

    it("should apply alternating colors for file labels", () => {
        renderWithStore();

        files.forEach((file, x) => {
            const el = screen.getByTestId(`coordsFile-${file}`);
            if (x % 2 === 0) {
                expect(el).toHaveClass("text-[#e9e9d4]");
            } else {
                expect(el).toHaveClass("text-[#577298]");
            }
        });
    });

    it("should apply alternating colors for rank labels", () => {
        renderWithStore();

        ranks.forEach((rank, y) => {
            const el = screen.getByTestId(`coordsRank-${rank}`);
            if (y % 2 === 0) {
                expect(el).toHaveClass("text-[#e9e9d4]");
            } else {
                expect(el).toHaveClass("text-[#577298]");
            }
        });
    });

    it("should render 20 squares total", () => {
        renderWithStore();
        expect(screen.getAllByTestId(/coords(File|Rank)-/)).toHaveLength(20);
    });
});
