import { render, screen } from "@testing-library/react";
import PlayPage from "../page";

describe("PlayPage", () => {
    it("should render play options and chessboard", () => {
        render(<PlayPage />);

        expect(screen.queryByTestId("chessboard")).toBeInTheDocument();
        expect(screen.queryByTestId("playOptions")).toBeInTheDocument();
    });
});
