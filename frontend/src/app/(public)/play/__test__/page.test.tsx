import { render, screen } from "@testing-library/react";
import PlayPage from "../page";
import { mockHubBuilder } from "@/lib/testUtils/mocks/mockSignalR";
import flushMicrotasks from "@/lib/testUtils/flushMicrotasks";

vi.mock("@microsoft/signalr");

describe("PlayPage", () => {
    beforeEach(() => mockHubBuilder());

    it("should render the PlayPage with Chessboard and PlayOptions", async () => {
        render(<PlayPage />);
        await flushMicrotasks();

        // Look for elements from the real components
        const chessboardElement = screen.getByTestId("chessboard");
        const playOptionsElement = screen.getByTestId("playOptions");

        expect(chessboardElement).toBeInTheDocument();
        expect(playOptionsElement).toBeInTheDocument();
    });
});
