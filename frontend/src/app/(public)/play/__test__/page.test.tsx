import { render, screen } from "@testing-library/react";
import PlayPage from "../page";

describe("PlayPage", () => {
    it("should render the PlayPage with Chessboard and PlayOptions", () => {
        render(<PlayPage />);

        // Look for elements from the real components
        const chessboardElement = screen.getByTestId("chessboard");
        const playOptionsElement = screen.getByTestId("playOptions");

        expect(chessboardElement).toBeInTheDocument();
        expect(playOptionsElement).toBeInTheDocument();
    });
});
