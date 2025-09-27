import { render, screen } from "@testing-library/react";
import ChessboardStoreContext from "../../contexts/chessboardStoreContext";
import {
    ChessboardStore,
    createChessboardStore,
} from "../../stores/chessboardStore";
import { StoreApi } from "zustand";
import { pointToStr, viewPoint } from "@/features/point/pointUtils";
import CoordSquare, { ChessSquareRef } from "../CoordSquare";
import React from "react";

describe("CoordSquare", () => {
    let store: StoreApi<ChessboardStore>;

    beforeEach(() => {
        store = createChessboardStore();
    });

    const renderWithStore = (ui: React.ReactNode) =>
        render(
            <ChessboardStoreContext.Provider value={store}>
                {ui}
            </ChessboardStoreContext.Provider>,
        );

    it("should render children", () => {
        renderWithStore(
            <CoordSquare
                position={viewPoint({ x: 0, y: 0 })}
                data-testid="coordSquare"
            >
                <span data-testid="child">henlo</span>
            </CoordSquare>,
        );

        expect(screen.getByTestId("child")).toHaveTextContent("henlo");
    });

    it("should set width and height based on board dimensions", () => {
        store.setState({
            boardDimensions: { width: 10, height: 10 },
        });

        renderWithStore(
            <CoordSquare
                position={viewPoint({ x: 0, y: 0 })}
                data-testid="coordSquare"
            />,
        );

        const el = screen.getByTestId("coordSquare");
        expect(el).toHaveStyle({
            width: "10%",
            height: "10%",
        });
    });

    it("should place element using transform at the correct position", () => {
        store.setState({
            boardDimensions: { width: 10, height: 10 },
        });

        renderWithStore(
            <CoordSquare
                position={viewPoint({ x: 3, y: 4 })}
                data-testid="coordSquare"
            />,
        );

        const el = screen.getByTestId("coordSquare");
        expect(el.getAttribute("style")).toContain("translate(");
    });

    it("should include data-position attribute", () => {
        const pos = viewPoint({ x: 2, y: 5 });
        renderWithStore(
            <CoordSquare position={pos} data-testid="coordSquare" />,
        );
        expect(screen.getByTestId("coordSquare")).toHaveAttribute(
            "data-position",
            pointToStr(pos),
        );
    });

    it("should merge custom className", () => {
        renderWithStore(
            <CoordSquare
                position={viewPoint({ x: 1, y: 1 })}
                className="custom"
                data-testid="coordSquare"
            />,
        );
        expect(screen.getByTestId("coordSquare").className).toContain("custom");
    });

    it("should merge custom style", () => {
        renderWithStore(
            <CoordSquare
                position={viewPoint({ x: 0, y: 0 })}
                style={{ backgroundColor: "red" }}
                data-testid="coordSquare"
            />,
        );
        expect(screen.getByTestId("coordSquare").style.backgroundColor).toBe(
            "red",
        );
    });

    it("should support ref methods", () => {
        const ref = React.createRef<ChessSquareRef>();
        renderWithStore(
            <CoordSquare
                position={viewPoint({ x: 0, y: 0 })}
                ref={ref}
                data-testid="coordSquare"
            />,
        );

        expect(ref.current).toBeDefined();
        expect(typeof ref.current?.updateDraggingOffset).toBe("function");
        expect(typeof ref.current?.getBoundingClientRect).toBe("function");
    });

    it("should update transform when updateDraggingOffset is called", () => {
        const ref = React.createRef<ChessSquareRef>();
        renderWithStore(
            <CoordSquare
                position={viewPoint({ x: 2, y: 2 })}
                ref={ref}
                data-testid="coordSquare"
            />,
        );

        const el = screen.getByTestId("coordSquare");
        const originalTransform = el.style.transform;

        ref.current?.updateDraggingOffset({ x: 15, y: -10 });
        expect(el.style.transform).not.toBe(originalTransform);
    });
});
