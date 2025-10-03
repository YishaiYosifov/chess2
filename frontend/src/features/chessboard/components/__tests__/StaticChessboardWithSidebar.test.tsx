import { render, screen } from "@testing-library/react";
import StaticChessboardWithSidebar from "../StaticChessboardWithSidebar";

describe("StaticChessboardWithSidebar", () => {
    it("should render StaticChessboard and aside content", () => {
        render(
            <StaticChessboardWithSidebar
                aside={<div data-testid="sidebar">Sidebar content</div>}
            />,
        );

        expect(screen.getByTestId("chessboard")).toBeInTheDocument();
        expect(screen.getByTestId("sidebar")).toHaveTextContent(
            "Sidebar content",
        );
    });
});
