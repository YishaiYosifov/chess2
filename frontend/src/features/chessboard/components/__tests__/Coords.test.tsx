import { render, screen } from "@testing-library/react";
import { pointToStr } from "@/features/point/pointUtils";
import Coords from "../Coords";
import { StoreApi } from "zustand";
import {
    ChessboardStore,
    createChessboardStore,
} from "../../stores/chessboardStore";
import ChessboardStoreContext from "../../contexts/chessboardStoreContext";

describe("Coords", () => {
    let store: StoreApi<ChessboardStore>;

    beforeEach(() => {
        store = createChessboardStore();
    });

    const renderWithStore = () =>
        render(
            <ChessboardStoreContext.Provider value={store}>
                <Coords />
            </ChessboardStoreContext.Provider>,
        );

    it("should render all file labels", () => {
        renderWithStore();

        const files = "abcdefghij".split("");
        for (const file of files) {
            expect(screen.getByTestId(`coordsFile-${file}`)).toHaveTextContent(
                file,
            );
        }
    });

    it("should render all rank labels", () => {
        renderWithStore();

        for (let rank = 1; rank <= 10; rank++) {
            expect(screen.getByTestId(`coordsRank-${rank}`)).toHaveTextContent(
                rank.toString(),
            );
        }
    });

    it("should position file labels along the bottom row", () => {
        renderWithStore();

        const files = "abcdefghij".split("");
        files.forEach((file, x) => {
            expect(screen.getByTestId(`coordsFile-${file}`)).toHaveAttribute(
                "data-position",
                pointToStr({ x, y: 0 }),
            );
        });
    });

    it("should position rank labels along the right column", () => {
        renderWithStore();

        for (let y = 0; y < 10; y++) {
            const rank = y + 1;
            expect(screen.getByTestId(`coordsRank-${rank}`)).toHaveAttribute(
                "data-position",
                pointToStr({ x: 9, y }),
            );
        }
    });

    it("should apply alternating colors for file labels", () => {
        renderWithStore();

        const files = "abcdefghij".split("");
        files.forEach((file, x) => {
            const el = screen.getByTestId(`coordsFile-${file}`);
            if (x % 2 === 0) {
                expect(el.className).toContain("text-[#b58863]");
            } else {
                expect(el.className).toContain("text-[#f0d9b5]");
            }
        });
    });

    it("should apply alternating colors for rank labels", () => {
        renderWithStore();

        for (let y = 0; y < 10; y++) {
            const rank = y + 1;
            const el = screen.getByTestId(`coordsRank-${rank}`);
            if (y % 2 === 0) {
                expect(el.className).toContain("text-[#f0d9b5]");
            } else {
                expect(el.className).toContain("text-[#b58863]");
            }
        }
    });
});
