import { render, screen } from "@testing-library/react";
import PlayPage from "../page";

describe("PlayPage", () => {
    it("should render play options and chessboard", () => {
        render(<PlayPage />);

        expect(screen.getByTestId("chessboard")).toBeInTheDocument();
        expect(screen.getByTestId("playOptions")).toBeInTheDocument();
    });
});
