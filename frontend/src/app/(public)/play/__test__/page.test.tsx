import { render, screen } from "@testing-library/react";
import PlayPage from "../page";
import { mockHubBuilder } from "@/lib/testUtils/mocks/mockSignalR";

vi.mock("@microsoft/signalr");

describe("PlayPage", () => {
    beforeEach(() => mockHubBuilder());

    it("should render the PlayPage with Chessboard and PlayOptions", () => {
        render(<PlayPage />);

        // Look for elements from the real components
        const chessboardElement = screen.getByTestId("chessboard");
        const playOptionsElement = screen.getByTestId("playOptions");

        expect(chessboardElement).toBeInTheDocument();
        expect(playOptionsElement).toBeInTheDocument();
    });
});
