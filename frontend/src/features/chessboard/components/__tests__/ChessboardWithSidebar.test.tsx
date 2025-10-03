import { render, screen } from "@testing-library/react";
import ChessboardWithSidebar from "../ChessboardWithSidebar";

describe("ChessboardWithSidebar", () => {
    it("should render chessboard and aside content", () => {
        render(
            <ChessboardWithSidebar
                chessboard={<div data-testid="board" />}
                aside={<div data-testid="sidebar" />}
            />,
        );

        expect(screen.getByTestId("board")).toBeInTheDocument();
        expect(screen.getByTestId("sidebar")).toBeInTheDocument();
    });
});
