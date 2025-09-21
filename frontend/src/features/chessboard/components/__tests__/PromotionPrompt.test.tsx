import { render, screen } from "@testing-library/react";
import ChessboardStoreContext from "../../contexts/chessboardStoreContext";
import PromotionPrompt from "../PromotionPrompt";
import {
    ChessboardStore,
    createChessboardStore,
} from "../../stores/chessboardStore";
import { StoreApi } from "zustand";
import { logicalPoint } from "@/features/point/pointUtils";
import { GameColor, PieceType } from "@/lib/apiClient";
import userEvent from "@testing-library/user-event";

describe("PromotionPrompt", () => {
    let store: StoreApi<ChessboardStore>;

    beforeEach(() => {
        store = createChessboardStore();
    });

    it("should not render when there is no pending promotion", () => {
        store.setState({ pendingPromotion: null });

        const { container } = render(
            <ChessboardStoreContext.Provider value={store}>
                <PromotionPrompt />
            </ChessboardStoreContext.Provider>,
        );

        expect(container.firstChild).toBeNull();
    });

    it("should render all promotion pieces", () => {
        store.setState({
            pendingPromotion: {
                at: logicalPoint({ x: 4, y: 6 }),
                piece: {
                    type: PieceType.PAWN,
                    color: GameColor.WHITE,
                    position: logicalPoint({ x: 4, y: 6 }),
                },
                pieces: [
                    PieceType.QUEEN,
                    PieceType.ROOK,
                    PieceType.BISHOP,
                    PieceType.HORSEY,
                ],
            },
        });

        render(
            <ChessboardStoreContext.Provider value={store}>
                <PromotionPrompt />
            </ChessboardStoreContext.Provider>,
        );

        const buttons = screen.getAllByTestId("promotionPiece");
        expect(buttons).toHaveLength(4);
    });

    it("should call resolvePromotion(null) when background is clicked", async () => {
        const resolvePromotion = vi.fn();
        store.setState({
            pendingPromotion: {
                at: logicalPoint({ x: 2, y: 5 }),
                piece: {
                    type: PieceType.PAWN,
                    color: GameColor.WHITE,
                    position: logicalPoint({ x: 2, y: 5 }),
                },
                pieces: [PieceType.QUEEN, PieceType.ROOK],
            },
            resolvePromotion,
        });

        const user = userEvent.setup();
        render(
            <ChessboardStoreContext.Provider value={store}>
                <PromotionPrompt />
            </ChessboardStoreContext.Provider>,
        );

        await user.pointer({
            keys: "[MouseLeft]",
            target: screen.getByTestId("promotionPromptOverlay"),
        });

        expect(resolvePromotion).toHaveBeenCalledWith(null);
    });

    it("should call resolvePromotion with the selected piece", async () => {
        const resolvePromotion = vi.fn();
        store.setState({
            pendingPromotion: {
                at: logicalPoint({ x: 1, y: 6 }),
                piece: {
                    type: PieceType.PAWN,
                    color: GameColor.WHITE,
                    position: logicalPoint({ x: 1, y: 6 }),
                },
                pieces: [PieceType.QUEEN, PieceType.BISHOP, PieceType.HORSEY],
            },
            resolvePromotion,
        });

        const user = userEvent.setup();
        render(
            <ChessboardStoreContext.Provider value={store}>
                <PromotionPrompt />
            </ChessboardStoreContext.Provider>,
        );

        const knightButton = screen.getAllByTestId("promotionPiece")[2];
        await user.click(knightButton);

        expect(resolvePromotion).toHaveBeenCalledWith(PieceType.HORSEY);
    });

    it("should position pieces above the pawn if it's in the lower half", () => {
        store.setState({
            pendingPromotion: {
                at: logicalPoint({ x: 2, y: 6 }),
                piece: {
                    type: PieceType.PAWN,
                    color: GameColor.WHITE,
                    position: logicalPoint({ x: 2, y: 6 }),
                },
                pieces: [PieceType.QUEEN, PieceType.ROOK],
            },
        });

        render(
            <ChessboardStoreContext.Provider value={store}>
                <PromotionPrompt />
            </ChessboardStoreContext.Provider>,
        );

        const [first, second] = screen.getAllByTestId("promotionPiece");

        expect(first).toHaveAttribute(
            "data-position",
            logicalPoint({ x: 2, y: 6 }).toString(),
        );
        expect(second).toHaveAttribute(
            "data-position",
            logicalPoint({ x: 2, y: 5 }).toString(),
        );
    });

    it("should position pieces below the pawn if it's in the upper half ", () => {
        store.setState({
            pendingPromotion: {
                at: logicalPoint({ x: 5, y: 2 }),
                piece: {
                    type: PieceType.PAWN,
                    color: GameColor.BLACK,
                    position: logicalPoint({ x: 5, y: 2 }),
                },
                pieces: [PieceType.QUEEN, PieceType.ROOK],
            },
        });

        render(
            <ChessboardStoreContext.Provider value={store}>
                <PromotionPrompt />
            </ChessboardStoreContext.Provider>,
        );

        const [first, second] = screen.getAllByTestId("promotionPiece");

        expect(first).toHaveAttribute(
            "data-position",
            logicalPoint({ x: 5, y: 2 }).toString(),
        );
        expect(second).toHaveAttribute(
            "data-position",
            logicalPoint({ x: 5, y: 3 }).toString(),
        );
    });
});
